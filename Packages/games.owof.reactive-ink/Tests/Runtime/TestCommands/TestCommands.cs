using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Time.Testing;
using NUnit.Framework;
using R3;
using ReactiveInk.Commands;

namespace ReactiveInk.Tests.Tests.Runtime.TestCommands
{
    public class TestCommands : TestBase
    {
        /*
         * LINE COMMANDS
         */

        [Test]
        public async Task NoArgumentsCommand()
        {
            // create engine with default command line parser and a (non-waiting) command
            using var storyActions = new Subject<StoryAction>();
            var commandInfos = new List<CommandInfo<string>>();
            using var engine = new ReactiveInkEngine(
                GetJson(),
                storyActions,
                new[] { new DefaultLineCommandParser() },
                new[] { new TestCommandProcessor<string, Unit>(commandInfos) }
            );
            using var storySteps = new StoryStepsAsyncReader(engine);

            // check no commands have executed
            commandInfos.Should().BeEmpty("no command has executed yet.");

            // make engine advance
            storyActions.OnNext(StoryAction.Continue());
            await storySteps.ReadAsync();

            // check that a single command has executed
            var commandInfo = commandInfos.Should().ContainSingle().Which;
            commandInfo.NamedParametersNames.Should().BeEmpty();
            commandInfo.PositionalParametersCount.Should().Be(0);
        }

        [Test]
        public async Task NoArgumentsAsyncCommand()
        {
            // create engine with default command line parser and a (non-waiting) command
            using var storyActions = new Subject<StoryAction>();
            var commandInfos = new List<CommandInfo<string>>();
            var fakeTimeProvider = new FakeTimeProvider();
            using var engine = new ReactiveInkEngine(
                GetJson(),
                storyActions,
                new[] { new DefaultLineCommandParser() },
                new[] { new TestCommandProcessor<string, Unit>(commandInfos, 2, fakeTimeProvider) }
            );
            using var storySteps = new StoryStepsAsyncReader(engine);

            // check no commands have executed
            commandInfos.Should().BeEmpty("no command has executed yet.");

            // make engine advance
            storyActions.OnNext(StoryAction.Continue());
            storySteps.TryRead(out _).Should().Be(false, "the command is waiting");
            commandInfos.Should().BeEmpty("no command has executed yet.");

            // wait half the time and check that the engine didn't advance yet
            fakeTimeProvider.Advance(TimeSpan.FromSeconds(1));
            storySteps.TryRead(out _).Should().Be(false, "the command is still waiting after 1 second");
            commandInfos.Should().BeEmpty("no command has executed yet.");

            // advance to the full waiting time and check there's a single command execution
            fakeTimeProvider.Advance(TimeSpan.FromSeconds(1));
            await storySteps.ReadAsync();
            var commandInfo = commandInfos.Should().ContainSingle().Which;
            commandInfo.NamedParametersNames.Should().BeEmpty();
            commandInfo.PositionalParametersCount.Should().Be(0);
        }

        [Test]
        public async Task ArgumentsCommand()
        {
            // create engine with default command line parser and a (non-waiting) command
            using var storyActions = new Subject<StoryAction>();
            var commandInfos = new List<CommandInfo<string>>();
            using var engine = new ReactiveInkEngine(
                GetJson(),
                storyActions,
                new[] { new DefaultLineCommandParser() },
                new[] { new TestCommandProcessor<string, Unit>(commandInfos) }
            );
            using var storySteps = new StoryStepsAsyncReader(engine);

            // make engine advance
            storyActions.OnNext(StoryAction.Continue());
            await storySteps.ReadAsync();

            // check arguments of the command
            var commandInfo = commandInfos.Should().ContainSingle().Which;
            commandInfo.NamedParametersNames.Should().BeEquivalentTo(new[] { "param1", "param2" },
                "The command has parameters 'param1' and 'param2'");
            commandInfo.PositionalParametersCount.Should()
                .Be(0, "Default line command parses no positional parameters");
            commandInfo["param1"].Should().Be("value1", "'param1' has value 'value1'");
            commandInfo["param2"].Should().Be("value 2", "'param2' has value 'value 2'");
            commandInfo.TryGetParameter("param1", out var value1).Should().BeTrue("param1 is present");
            value1.Should().Be("value1", "'param1' has value 'value1'");
            commandInfo.TryGetParameter("param2", out var value2).Should().BeTrue("param2 is present");
            value2.Should().Be("value 2", "'param2' has value 'value 2'");
            commandInfo.TryGetParameter("param3", out _).Should().BeFalse("there is no param3");
            commandInfo.TryGetParameter(0, out _).Should().BeFalse("there are no positional parameters");
        }

        /*
         * TAG COMMANDS
         */

        [Test]
        public async Task TagSingleCommand()
        {
            // create engine with default command line parser and a (non-waiting) command
            using var storyActions = new Subject<StoryAction>();
            var commandInfos = new List<CommandInfo<string>>();
            using var engine = new ReactiveInkEngine(
                GetJson(),
                storyActions,
                new[] { new DefaultTagCommandParser() },
                new[] { new TestCommandProcessor<string, Unit>(commandInfos) }
            );
            using var storySteps = new StoryStepsAsyncReader(engine);

            // check no commands have executed
            commandInfos.Should().BeEmpty("no command has executed yet.");

            // make engine advance
            storyActions.OnNext(StoryAction.Continue());
            await storySteps.ReadAsync();

            // check that a single command has executed
            var commandInfo = commandInfos.Should().ContainSingle().Which;
            commandInfo.NamedParametersNames.Should().BeEmpty();
            commandInfo.PositionalParametersCount.Should().Be(0);
        }

        [Test]
        public async Task TagMultiCommand()
        {
            // create engine with default command line parser and a (non-waiting) command
            using var storyActions = new Subject<StoryAction>();
            var commandInfos = new List<CommandInfo<string>>();
            using var engine = new ReactiveInkEngine(
                GetJson(),
                storyActions,
                new[] { new DefaultTagCommandParser() },
                new[]
                {
                    new TestCommandProcessor<string, Unit>(commandInfos),
                    new TestCommandProcessor<string, Unit>(commandInfos, name: "command2")
                }
            );
            using var storySteps = new StoryStepsAsyncReader(engine);

            // check no commands have executed
            commandInfos.Should().BeEmpty("no command has executed yet.");

            // make engine advance
            storyActions.OnNext(StoryAction.Continue());
            await storySteps.ReadAsync();

            // check that two commands have executed
            commandInfos.Should().HaveCount(2);
            commandInfos[0].NamedParametersNames.Should().BeEmpty();
            commandInfos[0].PositionalParametersCount.Should().Be(0);
            commandInfos[1].NamedParametersNames.Should().BeEmpty();
            commandInfos[1].PositionalParametersCount.Should().Be(0);
        }

        [Test]
        public async Task TagCommandWithParameters()
        {
            // create engine with default command line parser and a (non-waiting) command
            using var storyActions = new Subject<StoryAction>();
            var commandInfos = new List<CommandInfo<string>>();
            using var engine = new ReactiveInkEngine(
                GetJson(),
                storyActions,
                new[] { new DefaultTagCommandParser() },
                new[] { new TestCommandProcessor<string, Unit>(commandInfos) }
            );
            using var storySteps = new StoryStepsAsyncReader(engine);

            // make engine advance
            storyActions.OnNext(StoryAction.Continue());
            await storySteps.ReadAsync();

            // check that a single command has executed
            var commandInfo = commandInfos.Should().ContainSingle().Which;
            commandInfo.NamedParametersNames.Should().BeEmpty();
            commandInfo.PositionalParametersCount.Should().Be(2);
            commandInfo[0].Should().Be("value1", "first parameter has value 'value1'");
            commandInfo[1].Should().Be("value2", "second parameter has value 'value2'");
            commandInfo.TryGetParameter(0, out var value1).Should().BeTrue("first parameter is present");
            value1.Should().Be("value1", "First parameter has value 'value1'");
            commandInfo.TryGetParameter(1, out var value2).Should().BeTrue("param2 is present");
            value2.Should().Be("value2", "Second parameter has value 'value2'");
            commandInfo.TryGetParameter(2, out _).Should().BeFalse("there is no third parameter");
            commandInfo.TryGetParameter("param", out _).Should().BeFalse("there are no named parameters");
        }

        /*
         * EXTERNAL FUNCTIONS
         */

        [Test]
        public async Task ExternalFunctionCommand()
        {
            // create engine with default command line parser and a (non-waiting) command
            using var storyActions = new Subject<StoryAction>();
            var commandInfos = new List<CommandInfo<object>>();
            using var engine = new ReactiveInkEngine(
                GetJson(),
                storyActions,
                new ICommandParser[] { },
                valueCommands: new[]
                    { new TestCommandProcessor<object, object>(commandInfos, registerAsExternal: true) }
            );
            using var storySteps = new StoryStepsAsyncReader(engine);

            // check no commands have executed
            commandInfos.Should().BeEmpty("no command has executed yet.");

            // make engine advance
            storyActions.OnNext(StoryAction.Continue());
            await storySteps.ReadAsync();

            // check exactly one command has been executed
            var commandInfo = commandInfos.Should().ContainSingle().Which;
            commandInfo.NamedParametersNames.Should().BeEmpty();
            commandInfo.PositionalParametersCount.Should().Be(0);
        }

        [Test]
        public async Task ExternalFunctionCommandWithArgs()
        {
            // create engine with default command line parser and a (non-waiting) command
            using var storyActions = new Subject<StoryAction>();
            var commandInfos = new List<CommandInfo<object>>();
            using var engine = new ReactiveInkEngine(
                GetJson(),
                storyActions,
                new ICommandParser[] { },
                valueCommands: new[]
                    { new TestCommandProcessor<object, object>(commandInfos, registerAsExternal: true) }
            );
            using var storySteps = new StoryStepsAsyncReader(engine);

            // check no commands have executed
            commandInfos.Should().BeEmpty("no command has executed yet.");

            // make engine advance
            storyActions.OnNext(StoryAction.Continue());
            await storySteps.ReadAsync();

            // check exactly one command has been executed
            var commandInfo = commandInfos.Should().ContainSingle().Which;
            commandInfo.NamedParametersNames.Should().BeEmpty();
            commandInfo.PositionalParametersCount.Should().Be(2);
            commandInfo[0].Should().Be("hello");
            commandInfo[1].Should().Be(3);
        }

        [Test]
        public async Task ExternalFunctionCommandWithReturnValue()
        {
            // create engine with default command line parser and a (non-waiting) command
            using var storyActions = new Subject<StoryAction>();
            var commandInfos = new List<CommandInfo<object>>();
            using var engine = new ReactiveInkEngine(
                GetJson(),
                storyActions,
                new ICommandParser[] { },
                valueCommands: new[]
                    { new TestCommandProcessor<object, object>(commandInfos, registerAsExternal: true, returnValue: 9) }
            );
            using var storySteps = new StoryStepsAsyncReader(engine);

            // check no commands have executed
            commandInfos.Should().BeEmpty("no command has executed yet.");

            // make engine advance
            storyActions.OnNext(StoryAction.Continue());
            var line = await storySteps.ReadAsync();
            line.Text.Should().Be("x is 9.");

            // check exactly one command has been executed
            commandInfos.Should().ContainSingle();
        }
    }
}