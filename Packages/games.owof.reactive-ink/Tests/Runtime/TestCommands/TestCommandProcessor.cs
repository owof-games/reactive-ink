using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using R3;
using ReactiveInk.Commands;

namespace ReactiveInk.Tests.Tests.Runtime.TestCommands
{
    public class TestCommandProcessor : ICommandProcessor<string>
    {
        private readonly float _delay;
        private readonly TimeProvider _timeProvider;

        public TestCommandProcessor(List<CommandInfo<string>> commandInfosList, float delay = -1,
            TimeProvider timeProvider = null)
        {
            _delay = delay;
            _timeProvider = timeProvider;
            CommandInfosList = commandInfosList;
        }

        private List<CommandInfo<string>> CommandInfosList { get; }
        public string CommandName => "command";
        public bool RegisterAsExternalFunction => false;

        public async UniTask<PostCommandAction> Execute(CommandInfo<string> commandInfo,
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