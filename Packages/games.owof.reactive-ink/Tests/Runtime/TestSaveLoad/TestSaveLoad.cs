using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;
using R3;

namespace ReactiveInk.Tests.Tests.Runtime.TestSaveLoad
{
    public class TestSaveLoad : TestBase
    {
        /*
         * x=0, y=2, First Line.
         * x=2, y=2, Second Line.
         * x=2, y=3, Third line.
         */
        [Test]
        public async Task TestJson()
        {
            // create engine with default command line parser and a (non-waiting) command
            using var storyActions = new Subject<StoryAction>();
            using var engine = new ReactiveInkEngine(GetJson(), storyActions);
            using var storySteps = new StoryStepsAsyncReader(engine);

            // saving state
            var save0 = engine.ToJson();

            // make engine advance (First line.)
            storyActions.OnNext(StoryAction.Continue());
            var line1 = (await storySteps.ReadAsync()).Text;
            var save1 = engine.ToJson();

            // make engine advance (First line.)
            storyActions.OnNext(StoryAction.Continue());
            var line2 = (await storySteps.ReadAsync()).Text;
            var save2 = engine.ToJson();

            // make engine advance (First line.)
            storyActions.OnNext(StoryAction.Continue());
            var line3 = (await storySteps.ReadAsync()).Text;
            var save3 = engine.ToJson();

            // sanity checks
            line1.Should().NotBe(line2);
            line1.Should().NotBe(line3);
            line2.Should().NotBe(line3);

            // check contents
            engine.FromJson(save0);
        }
    }
}