#nullable enable
using System.Diagnostics.CodeAnalysis;

namespace ReactiveInk.Commands
{
    /// <summary>
    ///     The context in which a command processor runs.
    /// </summary>
    public class CommandProcessorContext<TParam, TResult>
    {
        private bool _hasResult;
        private TResult? _result;

        internal CommandProcessorContext(ICommandProcessor<TParam, TResult> command, StoryStep storyStep)
        {
            StoryStep = storyStep;
            PostCommandAction = command.DoNothing();
            _hasResult = false;
            _result = default;
        }

        /// <summary>
        ///     The story to read the commands from.
        /// </summary>
        public StoryStep StoryStep { get; }

        public PostCommandAction PostCommandAction { get; set; }

        public void SetResult(TResult result)
        {
            _result = result;
            _hasResult = true;
        }

        public bool TryGetResult([NotNullWhen(true)] out TResult? result)
        {
            if (_hasResult)
            {
                result = _result!;
                return true;
            }
            else
            {
                result = default;
                return false;
            }
        }
    }
}