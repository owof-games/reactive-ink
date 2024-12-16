#nullable enable
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using Ink.Runtime;
using R3;
using ReactiveInk.Commands;
using UnityEngine;
using Object = Ink.Runtime.Object;

namespace ReactiveInk
{
    /// <summary>
    ///     A reactive ink engine.
    /// </summary>
    public class ReactiveInkEngine : IDisposable
    {
        private static readonly List<PostCommandAction> EmptyActions = new();

        /// <summary>
        ///     A static, empty dictionary.
        /// </summary>
        private static readonly Dictionary<PostCommandActionType, List<PostCommandAction>>
            EmptyResultsByType = new();

        private static readonly IReadOnlyDictionary<string, object> EmptyPositionalParameters =
            new Dictionary<string, object>();

        /// <summary>
        ///     The available command parsers.
        /// </summary>
        private readonly IEnumerable<ICommandParser> _commandParsers;

        /// <summary>
        ///     All the known value commands, indexed by name.
        /// </summary>
        private readonly string[] _externalFunctionsToUnbind;

        /// <summary>
        ///     Maximum number of milliseconds that the Ink engine can run every frame.
        /// </summary>
        private readonly float _maxMillisecondsPerFrame;

        /// <summary>
        ///     The Ink engine runtime story.
        /// </summary>
        private readonly Story _story;

        /// <summary>
        ///     The subject used to create story states.
        /// </summary>
        private readonly Subject<StoryStep> _storyStates;

        /// <summary>
        ///     A cache containing the names of all the string commands.
        /// </summary>
        private readonly string[] _stringCommandNames;

        /// <summary>
        ///     All the known string commands, indexed by name.
        /// </summary>
        private readonly Dictionary<string, ICommandProcessor<string, Unit>> _stringCommands;

        /// <summary>
        ///     A bag to contain all disposables produced by the engine which must be disposed when it gets out of scope.
        /// </summary>
        private DisposableBag _disposableBag;

        /// <summary>
        ///     Create a new reactive ink engine.
        /// </summary>
        /// <param name="text">The JSON text content of the story.</param>
        /// <param name="storyActions">A stream of actions that the engine will react to.</param>
        /// <param name="commandParsers">The parsers used to find commands in the story.</param>
        /// <param name="stringCommands">All the command processors that use string parameters.</param>
        /// <param name="valueCommands">All the command processors that use value parameters.</param>
        /// <param name="maxMillisecondsPerFrame">
        ///     Maximum number of milliseconds that the Ink engine can run every
        ///     frame (by default, half of a 60fps frame duration).
        /// </param>
        public ReactiveInkEngine(string text,
            Observable<StoryAction> storyActions,
            IEnumerable<ICommandParser>? commandParsers = null,
            IEnumerable<ICommandProcessor<string, Unit>>? stringCommands = null,
            IEnumerable<ICommandProcessor<object, object>>? valueCommands = null,
            float maxMillisecondsPerFrame = 1000.0f / 60 / 2)
        {
            _story = new Story(text);

            _maxMillisecondsPerFrame = maxMillisecondsPerFrame;

            storyActions.SubscribeAwait(OnStoryAction, Debug.LogError, _ => { }).AddTo(ref _disposableBag);

            _storyStates = new Subject<StoryStep>().AddTo(ref _disposableBag);
            StorySteps = _storyStates.AsObservable();

            _commandParsers = commandParsers ?? Array.Empty<ICommandParser>();
            _stringCommands =
                (stringCommands ?? Array.Empty<ICommandProcessor<string, Unit>>()).ToDictionary(
                    command => command.CommandName, command => command);
            _stringCommandNames = _stringCommands.Values.Select(command => command.CommandName).ToArray();
            if (valueCommands != null)
            {
                var names = new List<string>();
                foreach (var command in valueCommands.Where(command => command.RegisterAsExternalFunction))
                {
                    _story.BindExternalFunctionGeneral(command.CommandName, args => ExternalFunction(command, args));
                    names.Add(command.CommandName);
                }

                _externalFunctionsToUnbind = names.ToArray();
            }
            else
            {
                _externalFunctionsToUnbind = Array.Empty<string>();
            }

            InitializeVariables();
        }

        /// <summary>
        ///     Stream of story states as they are produced by running the story.
        /// </summary>
        public Observable<StoryStep> StorySteps { get; }

        public void Dispose()
        {
            _disposableBag.Dispose();
            foreach (var name in _externalFunctionsToUnbind) _story.UnbindExternalFunction(name);
        }

        /// <summary>
        ///     Handles a single story action (choice or continue).
        /// </summary>
        /// <param name="action">The action to handle.</param>
        /// <param name="token">The cancellation token.</param>
        private async ValueTask OnStoryAction(StoryAction action, CancellationToken token)
        {
            if (action.Choice is { } choiceIndex)
                _story.ChooseChoiceIndex(choiceIndex);
            else
                for (;;)
                {
                    // spread execution of continue over multiple frames within the maximum budget of
                    // _maxMillisecondsPerFrame every frame
                    await ContinueRespectingTimeBudget(token);

                    // create the current story step
                    var storyStep = new StoryStep(_story);

                    // run all the commands found at this step and extract the results according to the type
                    var actionsByType = await RunCommands(storyStep, token);

                    // execute the command results and decide if we must repeat this loop (e.g.: some command requested to
                    // continue)
                    if (ExecutePostCommandActions(actionsByType, storyStep)) continue;

                    // no commands requested to continue: finish processing and notify the new story step
                    _storyStates.OnNext(storyStep);
                    break;
                }
        }

        /// <summary>
        ///     Perform a continue operation on the story, never using more than _maxMillisecondsPerFrame milliseconds
        ///     every frame, and continuing over multiple frames if needed.
        /// </summary>
        private async UniTask ContinueRespectingTimeBudget(CancellationToken token)
        {
            for (;;)
            {
                token.ThrowIfCancellationRequested();
                _story.ContinueAsync(_maxMillisecondsPerFrame);
                if (_story.asyncContinueComplete) break;

                await UniTask.NextFrame();
            }
        }

        /// <summary>
        ///     Run all the commands found on the given line.
        /// </summary>
        /// <param name="token"></param>
        /// <param name="storyStep"></param>
        /// <returns></returns>
        private async ValueTask<Dictionary<PostCommandActionType, List<PostCommandAction>>>
            RunCommands(StoryStep storyStep, CancellationToken token)
        {
            // get all the command info from all the parsers
            var commandContext = new CommandParserContext(storyStep, _stringCommandNames);
            var allCommandInfo = _commandParsers
                .SelectMany(commandParser => commandParser.ParseCommand(commandContext))
                .ToList();
            if (allCommandInfo.Count == 0)
                // short circuit for the lines where there are no commands (likely the majority of the lines)
                return EmptyResultsByType;

            // process the commands and create an array of results
            var actionsList = await allCommandInfo
                .ToObservable()
                .SelectAwait(async (commandInfo, cancellationToken) =>
                {
                    var commandProcessor = _stringCommands[commandInfo.CommandName];
                    var commandProcessorContext =
                        new CommandProcessorContext<string, Unit>(commandProcessor, storyStep);
                    await commandProcessor
                        .Execute(commandInfo, commandProcessorContext, cancellationToken);
                    return commandProcessorContext.PostCommandAction;
                })
                .ToArrayAsync(token);

            // map the results into a dictionary, grouped by result type
            return actionsList
                .Where(action => action.Type != PostCommandActionType.DoNothing)
                .GroupBy(action => action.Type)
                .ToDictionary(
                    group => group.Key,
                    group => group.ToList());
        }

        /// <summary>
        ///     Execute the results returned by the commands.
        /// </summary>
        /// <param name="actionsByType">All the actions resulting from the commands, grouped by type.</param>
        /// <param name="storyStep">The current story step.</param>
        /// <returns>Whether we must repeat the continue loop, because at least one command required to.</returns>
        /// <exception cref="InvalidOperationException">If the commands returned a result incompatible with the current state.</exception>
        private bool ExecutePostCommandActions(
            Dictionary<PostCommandActionType, List<PostCommandAction>> actionsByType,
            StoryStep storyStep)
        {
            {
                var choiceActions = actionsByType.GetValueOrDefault(PostCommandActionType.Choice, EmptyActions);
                var continueActions = actionsByType.GetValueOrDefault(PostCommandActionType.Continue, EmptyActions);

                // execute choice/continue requests
                if (storyStep.Choices.Length == 0)
                {
                    if (choiceActions.Count > 0)
                        throw new InvalidOperationException(
                            $"The following commands asked to take a choice when there is no choice: {JoinCommandNames(choiceActions)}");

                    if (continueActions.Count > 0)
                        // there are no choices, and at least one command asked to continue
                        return true;
                }
                else
                {
                    if (continueActions.Count > 0)
                        throw new InvalidOperationException(
                            $"The following commands asked to continue when there is a choice: {JoinCommandNames(continueActions)}");

                    if (choiceActions.Count > 1)
                        throw new InvalidOperationException(
                            $"Multiple commands tried to select the choice to take: {JoinCommandNames(choiceActions)}");

                    if (choiceActions.Count == 1)
                    {
                        // exactly one command has asked to take a choice: take it and continue
                        _story.ChooseChoiceIndex(choiceActions[0].ChoiceIndex);
                        return true;
                    }
                }

                // no command either requested to take a choice or continue
                return false;
            }

            string JoinCommandNames(IEnumerable<PostCommandAction> results)
            {
                return string.Join(", ", results.Select(result => result.CommandName));
            }
        }

        private static object? ExternalFunction(ICommandProcessor<object, object> commandProcessor, object[] args)
        {
            var commandInfo = new CommandInfo<object>(commandProcessor.CommandName, EmptyPositionalParameters,
                args.ToArray());
            var commandProcessorContext =
                new CommandProcessorContext<object, object>(commandProcessor, new StoryStep());
            var task = commandProcessor
                .Execute(commandInfo, commandProcessorContext, CancellationToken.None);

            if (task.Status == UniTaskStatus.Pending) throw new Exception("external functions cannot be asynchronous");
            return commandProcessorContext.TryGetResult(out var result) ? result : null;
        }

        #region variables

        /// <summary>
        ///     A map between (global) variable names and an observable (with replay 1) that produces its values.
        /// </summary>
        private readonly Dictionary<string, Observable<Value>> _variableObservables = new();

        /// <summary>
        ///     Initialization logic for managing the variables.
        /// </summary>
        private void InitializeVariables()
        {
            // create a stream with all the changes happening in variables
            var changes = Observable.FromEvent<VariablesState.VariableChanged, (string VariableName, Object NewValue)>(
                    action => (variableName, newValue) => action((variableName, newValue)),
                    handler => _story.variablesState.variableChangedEvent += handler,
                    handler => _story.variablesState.variableChangedEvent -= handler)
                .Publish();
            changes.Connect().AddTo(ref _disposableBag);

            // produce observables for all the variables
            foreach (var variableName in _story.variablesState) CreateVariableObservable(variableName, changes);
        }

        /// <summary>
        ///     Helper method for InitializeVariables that takes care of generating the observable for a single variable.
        /// </summary>
        /// <param name="variableName">The variable to generate the </param>
        /// <param name="changes">The observable with the changes for all the variables.</param>
        private void CreateVariableObservable(string variableName,
            Observable<(string VariableName, Object NewValue)> changes)
        {
            // if the initial value of a variable cannot be obtained, just ignore the variable
            if (!TryGetValueFromObject(_story.variablesState.GetVariableWithName(variableName),
                    out var initialValue))
                return;

            var connectable =
                changes
                    // return a value only if it's about this variable, and it's possible to extract its value 
                    .Select(change =>
                        change.VariableName != variableName ? null :
                        TryGetValueFromObject(change.NewValue, out var value) ? value : null)
                    .WhereNotNull()
                    .Prepend(initialValue)
                    .Replay(1);
            _variableObservables[variableName] = connectable.AsObservable();
            connectable.Connect().AddTo(ref _disposableBag);
        }

        private static bool TryGetValueFromObject(Object obj, [NotNullWhen(true)] out Value? value)
        {
            if (obj is Value newValue)
            {
                value = newValue;
                return true;
            }

            value = null;
            return false;
        }

        /// <summary>
        ///     Get the stream of changes for a variable.
        /// </summary>
        /// <param name="variableName">Name of the variable to observe.</param>
        /// <returns>An observable (with replay 1) of values for the given variable.</returns>
        public Observable<Value> GetVariableObservable(string variableName)
        {
            return _variableObservables[variableName];
        }

        /// <summary>
        ///     The names of all the variables currently observed.
        /// </summary>
        public IEnumerable<string> ObservedVariableNames => _variableObservables.Keys;

        #endregion

        // TODO: external functions return values

        // TODO: save/load

        // TODO: EvaluateFunction

        // TODO: debug mode

        // TODO: error handling

        // TODO: syntax checks?

        // TODO: list helpers?
    }
}