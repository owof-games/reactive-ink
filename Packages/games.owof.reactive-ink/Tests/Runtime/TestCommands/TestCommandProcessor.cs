using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using R3;
using ReactiveInk.Commands;

namespace ReactiveInk.Tests.Tests.Runtime.TestCommands
{
    public class TestCommandProcessor<TValue, TResult> : ICommandProcessor<TValue, TResult>
    {
        private readonly float _delay;
        private readonly object _returnValue;
        private readonly TimeProvider _timeProvider;

        public TestCommandProcessor(List<CommandInfo<TValue>> commandInfosList, float delay = -1,
            TimeProvider timeProvider = null, string name = "command", bool registerAsExternal = false,
            object returnValue = null)
        {
            _delay = delay;
            _timeProvider = timeProvider;
            _returnValue = returnValue;
            CommandInfosList = commandInfosList;
            CommandName = name;
            RegisterAsExternalFunction = registerAsExternal;
        }

        private List<CommandInfo<TValue>> CommandInfosList { get; }
        public string CommandName { get; }
        public bool RegisterAsExternalFunction { get; }

        public async UniTask Execute(CommandInfo<TValue> commandInfo,
            CommandProcessorContext<TValue, TResult> context, CancellationToken cancellationToken)
        {
            if (_delay > 0)
            {
                var obs = _timeProvider != null
                    ? Observable.Timer(TimeSpan.FromSeconds(_delay), _timeProvider)
                    : Observable.Timer(TimeSpan.FromSeconds(_delay));
                await obs.LastAsync(cancellationToken);
            }

            CommandInfosList.Add(commandInfo);

            if (_returnValue != null) context.SetResult((TResult)_returnValue);

            context.PostCommandAction = this.DoNothing();
        }
    }
}