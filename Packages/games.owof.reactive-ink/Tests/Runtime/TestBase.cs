using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using FluentAssertions;
using Ink;
using R3;
using UnityEditor;
using UnityEngine;

namespace ReactiveInk.Tests.Tests.Runtime
{
    public class TestBase
    {
        /// <summary>
        ///     Get the JSON string found in the ink file at given sub-path of Tests/Runtime, using the class name as extra
        ///     directory path.
        /// </summary>
        /// <param name="memberName">The name of the ink file to retrieve.</param>
        /// <returns>The compiled JSON of the ink file.</returns>
        protected string GetJson([CallerMemberName] string memberName = "")
        {
            var filename = $"{memberName}.txt";
            var assetPath = $"Packages/games.owof.reactive-ink/Tests/Runtime/{GetType().Name}/{filename}";
            var inkFile = AssetDatabase.LoadAssetAtPath<TextAsset>(assetPath);
            inkFile.Should().NotBeNull("The resource {0} should be present in the filesystem", assetPath);
            var compiler = new Compiler(inkFile.text);
            var story = compiler.Compile();
            return story.ToJson();
        }

        protected class StoryStepsAsyncReader : IDisposable
        {
            private readonly Channel<StoryStep> _channel;
            private readonly IDisposable _subscriptionDisposable;

            public StoryStepsAsyncReader(ReactiveInkEngine engine)
            {
                _channel = Channel.CreateUnbounded<StoryStep>(new UnboundedChannelOptions
                {
                    SingleReader = true,
                    SingleWriter = true
                });
                _subscriptionDisposable = engine.StorySteps.SubscribeAwait(
                    async (item, token) => { await _channel.Writer.WriteAsync(item, token); }, AwaitOperation.Parallel);
            }

            public void Dispose()
            {
                _subscriptionDisposable?.Dispose();
            }

            /// <summary>
            ///     Read the next story step, waiting for it if it's not ready yet.
            /// </summary>
            /// <param name="cancellationToken"></param>
            /// <returns></returns>
            public async ValueTask<StoryStep> ReadAsync(
                CancellationToken cancellationToken = default)
            {
                var value = await _channel.Reader.ReadAsync(cancellationToken);
                return value;
            }

            /// <summary>
            ///     Tries to read the next story step, not waiting for it.
            /// </summary>
            /// <param name="step"></param>
            /// <returns>Whether there was a story step to read</returns>
            public bool TryRead(out StoryStep step)
            {
                return _channel.Reader.TryRead(out step);
            }
        }
    }
}