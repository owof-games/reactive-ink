#nullable enable
using System;
using System.Collections.Generic;
using Ink.Runtime;

namespace ReactiveInk
{
    public class StoryStep
    {
        /// <summary>
        ///     Create a new story type from the current type of a story.
        /// </summary>
        /// <param name="story">The story to read the data from.</param>
        public StoryStep(Story story)
        {
            Text = story.currentText.Trim();

            Choices = new StoryChoice[story.currentChoices.Count];
            for (var i = 0; i < Choices.Length; i++) Choices[i] = new StoryChoice(story.currentChoices[i]);

            CanContinue = story.canContinue;

            Tags = story.currentTags == null || story.currentTags.Count == 0
                ? Array.Empty<string>()
                : story.currentTags.AsReadOnly();

            FlowName = string.IsNullOrEmpty(story.currentFlowName) ? null : story.currentFlowName;
        }

        /// <summary>
        ///     The text to display in this type.
        /// </summary>
        public string Text { get; }

        /// <summary>
        ///     The list of choices available in this type.
        /// </summary>
        public StoryChoice[] Choices { get; }

        /// <summary>
        ///     Whether the story can continue from this step (it's not the end, and there are no choices).
        /// </summary>
        public bool CanContinue { get; }

        /// <summary>
        ///     The tags at this story step.
        /// </summary>
        public IEnumerable<string> Tags { get; }

        /// <summary>
        ///     The name of the flow, or <c>null</c> if this is the default flow.
        /// </summary>
        public string? FlowName { get; }
    }
}