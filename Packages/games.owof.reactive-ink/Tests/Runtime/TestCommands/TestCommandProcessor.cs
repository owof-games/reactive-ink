using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using R3;
using ReactiveInk.Commands;

namespace ReactiveInk.Tests.Tests.Runtime.TestCommands
{
    public class TestCommandProcessor<T> : ICommandProcessor<T>
    {
        private readonly float _delay;
        private readonly TimeProvider _timeProvider;

        public TestCommandProcessor(List<CommandInfo<T>> commandInfosList, float delay = -1,
            TimeProvider timeProvider = null, string name = "command", bool registerAsExternal = false)
        {
            _delay = delay;
            _timeProvider = timeProvider;
            CommandInfosList = commandInfosList;
            CommandName = name;
            RegisterAsExternalFunction = registerAsExternal;
        }

        private List<CommandInfo<T>> CommandInfosList { get; }
        public string CommandName { get; }
        public bool RegisterAsExternalFunction { get; }

        public async UniTask<PostCommandAction> Execute(CommandInfo<T> commandInfo,
            CommandProcessorContext context, CancellationToken cancellationToken)
        {
            if (_delay > 0)
            {
                var obs = _timeProvider != null
                    ? Observable.Timer(TimeSpan.FromSeconds(_delay), _timeProvider)
                    : Observable.Timer(TimeSpan.FromSeconds(_delay));
                await obs.LastAsync(cancellationToken);
            }

            CommandInfosList.Add(commandInfo);

            return await this.ContinueAsync();
        }
    }
}