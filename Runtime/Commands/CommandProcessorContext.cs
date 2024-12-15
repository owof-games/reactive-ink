namespace ReactiveInk.Commands
{
    /// <summary>
    ///     The context in which a command processor runs.
    /// </summary>
    public struct CommandProcessorContext
    {
        /// <summary>
        ///     The story to read the commands from.
        /// </summary>
        public StoryStep StoryStep { get; }

        internal CommandProcessorContext(StoryStep storyStep)
        {
            StoryStep = storyStep;
        }
    }
}