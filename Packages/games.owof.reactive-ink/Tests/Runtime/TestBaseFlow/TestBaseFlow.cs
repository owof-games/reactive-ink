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
        public async Task Continue()
        {
            using var engine = new ReactiveInkEngine(GetJson());
            using var list = engine.StorySteps.ToLiveList();
            list.Should().BeEmpty("Should return no story steps before continuing.");

            await engine.Continue();
            list.Should().ContainSingle("After the first continue there's a single line.");
            list[0].Text.Should().BeEquivalentTo("First line.");
        }

        // Test Choice
        [TestCase(0)]
        [TestCase(1)]
        public async Task Choose(int choiceIndex)
        {
            // initialize the data structures
            using var engine = new ReactiveInkEngine(GetJson());

            // advance to the choice
            var state = await engine.Continue();
            state.Choices.Should().HaveCount(2, "There are two choices available.");
            state.Choices[0].Text.Should().BeEquivalentTo("Entry 1");
            state.Choices[1].Text.Should().BeEquivalentTo("Entry 2");

            // pick the choice
            var choice = state.Choices[choiceIndex];
            engine.TakeChoice(choice.Index);
            {
                // confirm that is not enough to pick the choice in order to advance
                var cts = new CancellationTokenSource();
                cts.CancelAfter(300);
                // ReSharper disable once AccessToDisposedClosure
                Func<Task> readStoryStep = async () => await engine.StorySteps.FirstAsync(cts.Token);
                await readStoryStep.Should().ThrowAsync<OperationCanceledException>();
            }
            // continue and check the choice was taken
            var state2 = await engine.Continue();
            state2.Text.Should().BeEquivalentTo(choice.Text);
        }
    }
}