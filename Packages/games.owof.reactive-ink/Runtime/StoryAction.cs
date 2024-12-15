#nullable enable

namespace ReactiveInk
{
    /// <summary>
    ///     An action to make the story progress.
    /// </summary>
    public struct StoryAction
    {
        /// <summary>
        ///     A continue action, if <c>null</c>, or the choice to pick, if not null.
        /// </summary>
        public int? Choice { get; private set; }

        /// <summary>
        ///     The flow in which to execute this action; default flow if <c>null</c>.
        /// </summary>
        public string? Flow { get; private set; }

        /// <summary>
        ///     Create a new story action.
        /// </summary>
        /// <param name="choice"></param>
        /// <param name="flow"></param>
        private StoryAction(int? choice, string? flow)
        {
            Choice = choice;
            Flow = flow;
        }

        /// <summary>
        ///     Create a story action that continues.
        /// </summary>
        /// <param name="flow">The flow where to execute the continue; if <c>null</c>, the default flow.</param>
        /// <returns>The story action.</returns>
        public static StoryAction Continue(string? flow = null)
        {
            return new StoryAction
            {
                Choice = null,
                Flow = flow
            };
        }

        /// <summary>
        ///     Create a story action that picks a choice (without continuing on that choice).
        /// </summary>
        /// <param name="index">Index of the chosen <see cref="Ink.Runtime.Choice" />.</param>
        /// <param name="flow">The flow where to execute the continue; if <c>null</c>, the default flow.</param>
        /// <returns>The story action.</returns>
        public static StoryAction Choose(int index, string? flow = null)
        {
            return new StoryAction
            {
                Choice = index,
                Flow = flow
            };
        }
    }
}