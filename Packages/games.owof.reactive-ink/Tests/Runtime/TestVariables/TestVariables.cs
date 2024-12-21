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
            using var engine = new ReactiveInkEngine(GetJson());

            engine.ObservedVariableNames.Should()
                .BeEquivalentTo("intVariable", "stringVariable", "divertVariable");
        }

        // Test observation of a single variable
        [Test]
        public async Task SingleVariableObservation()
        {
            using var engine = new ReactiveInkEngine(GetJson());

            using var variableValues = engine.GetVariableObservable("var").ToLiveList();
            variableValues
                .Should().ContainSingle()
                .Which.Should().HaveValue(1);

            await engine.Continue();
            variableValues.Should().ContainSingle();

            await engine.Continue();
            variableValues.Should().HaveCount(2)
                .And.Subject.Last().Should().HaveValue(2);

            await engine.Continue();
            variableValues.Should().HaveCount(3)
                .And.Subject.Last().Should().HaveValue(9);
        }

        // Test observation of multiple variables at the same time
        [Test]
        public async Task MultipleVariablesObservation()
        {
            using var engine = new ReactiveInkEngine(GetJson());

            using var intValues = engine.GetVariableObservable("varInt").ToLiveList();
            using var stringValues = engine.GetVariableObservable("varString").ToLiveList();
            intValues.Should().ContainSingle()
                .Which.Should().HaveValue(1);
            stringValues.Should().ContainSingle()
                .Which.Should().HaveValue("hi");

            await engine.Continue();
            intValues.Should().ContainSingle();
            stringValues.Should().ContainSingle();

            await engine.Continue();
            intValues.Should().HaveCount(2)
                .And.Subject.Last().Should().HaveValue(2);
            stringValues.Should().ContainSingle();

            await engine.Continue();
            intValues.Should().HaveCount(2);
            stringValues.Should().HaveCount(2)
                .And.Subject.Last().Should().HaveValue("hello");
        }
    }
}