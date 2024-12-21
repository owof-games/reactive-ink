using System.Runtime.CompilerServices;
using FluentAssertions;
using Ink;
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
    }
}