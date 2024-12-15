using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;
using R3;

namespace ReactiveInk.Tests.Tests.Runtime.TestVariables
{
    public class TestVariables : TestBase
    {
        // Test the observed variable names
        [Test]
        public void VariableNames()
        {
            using var engine = new ReactiveInkEngine(GetJson(), Observable.Never<StoryAction>());

            engine.ObservedVariableNames.Should()
                .BeEquivalentTo("intVariable", "stringVariable", "divertVariable");
        }

        // Test observation of a single variable
        [Test]
        public async Task SingleVariableObservation()
        {
            using var storyActions = new Subject<StoryAction>();
            using var engine = new ReactiveInkEngine(GetJson(), storyActions);
            using var storySteps = new StoryStepsAsyncReader(engine);

            using var variableValues = engine.GetVariableObservable("var").ToLiveList();
            variableValues
                .Should().ContainSingle()
                .Which.Should().HaveValue(1);

            storyActions.OnNext(StoryAction.Continue());
            await storySteps.ReadAsync();
            variableValues.Should().ContainSingle();

            storyActions.OnNext(StoryAction.Continue());
            await storySteps.ReadAsync();
            variableValues.Should().HaveCount(2)
                .And.Subject.Last().Should().HaveValue(2);

            storyActions.OnNext(StoryAction.Continue());
            await storySteps.ReadAsync();
            variableValues.Should().HaveCount(3)
                .And.Subject.Last().Should().HaveValue(9);
        }

        // Test observation of multiple variables at the same time
        [Test]
        public async Task MultipleVariablesObservation()
        {
            using var storyActions = new Subject<StoryAction>();
            using var engine = new ReactiveInkEngine(GetJson(), storyActions);
            using var storySteps = new StoryStepsAsyncReader(engine);

            using var intValues = engine.GetVariableObservable("varInt").ToLiveList();
            using var stringValues = engine.GetVariableObservable("varString").ToLiveList();
            intValues.Should().ContainSingle()
                .Which.Should().HaveValue(1);
            stringValues.Should().ContainSingle()
                .Which.Should().HaveValue("hi");

            storyActions.OnNext(StoryAction.Continue());
            await storySteps.ReadAsync();
            intValues.Should().ContainSingle();
            stringValues.Should().ContainSingle();

            storyActions.OnNext(StoryAction.Continue());
            await storySteps.ReadAsync();
            intValues.Should().HaveCount(2)
                .And.Subject.Last().Should().HaveValue(2);
            stringValues.Should().ContainSingle();

            storyActions.OnNext(StoryAction.Continue());
            await storySteps.ReadAsync();
            intValues.Should().HaveCount(2);
            stringValues.Should().HaveCount(2)
                .And.Subject.Last().Should().HaveValue("hello");
        }
    }
}