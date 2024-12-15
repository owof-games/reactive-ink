using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;
using R3;

namespace ReactiveInk.Tests.Tests.Runtime.TestBaseFlow
{
    public class TestBaseFlow : TestBase
    {
        // Test ContinueAsync for a single-line story.
        [Test]
        public void Continue()
        {
            using var storyActions = new Subject<StoryAction>();
            using var engine = new ReactiveInkEngine(GetJson(), storyActions);
            using var list = engine.StorySteps.ToLiveList();
            list.Should().BeEmpty("Should return no story steps before continuing.");

            storyActions.OnNext(StoryAction.Continue());
            list.Should().ContainSingle("After the first continue there's a single line.");
            list[0].Text.Should().BeEquivalentTo("First line.");
        }

        // Test Choice
        [TestCase(0)]
        [TestCase(1)]
        public async Task Choose(int choiceIndex)
        {
            // initialize the data structures
            using var storyActions = new Subject<StoryAction>();
            using var engine = new ReactiveInkEngine(GetJson(), storyActions);
            using var storySteps = new StoryStepsAsyncReader(engine);

            // advance to the choice
            storyActions.OnNext(StoryAction.Continue());
            var state = await storySteps.ReadAsync();
            state.Choices.Should().HaveCount(2, "There are two choices available.");
            state.Choices[0].Text.Should().BeEquivalentTo("Entry 1");
            state.Choices[1].Text.Should().BeEquivalentTo("Entry 2");

            // pick the choice
            var choice = state.Choices[choiceIndex];
            storyActions.OnNext(StoryAction.Choose(choice.Index));
            // confirm that is not enough to pick the choice in order to advance
            var cts = new CancellationTokenSource();
            cts.CancelAfter(300);
            // ReSharper disable once AccessToDisposedClosure
            Func<Task> readStoryStep = async () => await storySteps.ReadAsync(cts.Token);
            await readStoryStep.Should().ThrowAsync<OperationCanceledException>();
            // continue and check the choice was taken
            storyActions.OnNext(StoryAction.Continue());
            var state2 = await storySteps.ReadAsync();
            state2.Text.Should().BeEquivalentTo(choice.Text);
        }
    }
}