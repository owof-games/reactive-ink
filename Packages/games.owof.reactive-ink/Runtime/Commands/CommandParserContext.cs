using System.Collections.Generic;

namespace ReactiveInk.Commands
{
    /// <summary>
    ///     The context of a command parser to extract commands from.
    /// </summary>
    public struct CommandParserContext
    {
        /// <summary>
        ///     The story to read the commands from.
        /// </summary>
        public StoryStep StoryStep { get; }

        /// <summary>
        ///     The names of all the currently known commands.
        /// </summary>
        public IEnumerable<string> KnownCommands { get; }

        internal CommandParserContext(StoryStep storyStep, IEnumerable<string> knownCommands)
        {
            StoryStep = storyStep;
            KnownCommands = knownCommands;
        }
    }
}