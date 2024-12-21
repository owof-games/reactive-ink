using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;

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
            using var engine = new ReactiveInkEngine(GetJson());

            // saving state
            var save0 = engine.ToJson();

            // make engine advance (First line.)
            var line1 = await engine.Continue();
            var save1 = engine.ToJson();

            // make engine advance (First line.)
            var line2 = await engine.Continue();
            var save2 = engine.ToJson();

            // make engine advance (First line.)
            var line3 = await engine.Continue();
            var save3 = engine.ToJson();

            // sanity checks
            line1.Text.Should().NotBe(line2.Text);
            line1.Text.Should().NotBe(line3.Text);
            line2.Text.Should().NotBe(line3.Text);

            // check contents
            engine.FromJson(save0);
        }
    }
}