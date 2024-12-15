using Cysharp.Threading.Tasks;

namespace ReactiveInk.Commands
{
    internal enum PostCommandActionType
    {
        DoNothing,
        Continue,
        Choice
    }

    /// <summary>
    ///     The action to execute after a command has been completed: take a choice, continue or do nothing. See also
    ///     <see cref="PostCommandActionExtensions" /> for methods that create this struct.
    /// </summary>
    public readonly struct PostCommandAction
    {
        internal int ChoiceIndex { get; }

        internal PostCommandActionType Type { get; }

        internal string CommandName { get; }

        internal PostCommandAction(string commandName, PostCommandActionType type, int choiceIndex)
        {
            CommandName = commandName;
            Type = type;
            ChoiceIndex = choiceIndex;
        }
    }

    public static class PostCommandActionExtensions
    {
        /// <summary>
        ///     Create a return value for
        ///     <see
        ///         cref="ICommandProcessor{TValue}.Execute(CommandInfo{TValue}, CommandProcessorContext, System.Threading.CancellationToken)" />
        ///     when a choice is taken and the method is not async.
        /// </summary>
        /// <param name="commandProcessor">The command processor this result refers to.</param>
        /// <param name="index">The index of the choice to take.</param>
        /// <returns>A completed task representing the choice taken.</returns>
        public static UniTask<PostCommandAction> TakeChoiceAsync<T>(this ICommandProcessor<T> commandProcessor,
            int index)
        {
            var completionSource = new UniTaskCompletionSource<PostCommandAction>();
            completionSource.TrySetResult(new PostCommandAction(commandProcessor.CommandName,
                PostCommandActionType.Choice, index));
            return completionSource.Task;
        }

        /// <summary>
        ///     Create a return value for
        ///     <see
        ///         cref="ICommandProcessor{TValue}.Execute(CommandInfo{TValue}, CommandProcessorContext, System.Threading.CancellationToken)" />
        ///     when a choice is taken and the method is async.
        /// </summary>
        /// <param name="commandProcessor">The command processor this result refers to.</param>
        /// <param name="index">The index of the choice to take.</param>
        /// <returns>An action representing the choice taken.</returns>
        public static PostCommandAction TakeChoice<T>(this ICommandProcessor<T> commandProcessor, int index)
        {
            return new PostCommandAction(commandProcessor.CommandName, PostCommandActionType.Choice, index);
        }

        /// <summary>
        ///     Create a return value for
        ///     <see
        ///         cref="ICommandProcessor{TValue}.Execute(CommandInfo{TValue}, CommandProcessorContext, System.Threading.CancellationToken)" />
        ///     when no choice is taken and the method is not async.
        /// </summary>
        /// <param name="commandProcessor">The command processor this result refers to.</param>
        /// <returns>A completed task representing a continue operation.</returns>
        public static UniTask<PostCommandAction> ContinueAsync<T>(this ICommandProcessor<T> commandProcessor)
        {
            var completionSource = new UniTaskCompletionSource<PostCommandAction>();
            completionSource.TrySetResult(new PostCommandAction(commandProcessor.CommandName,
                PostCommandActionType.Continue, -1));
            return completionSource.Task;
        }

        /// <summary>
        ///     Create a return value for
        ///     <see
        ///         cref="ICommandProcessor{TValue}.Execute(CommandInfo{TValue}, CommandProcessorContext, System.Threading.CancellationToken)" />
        ///     when no choice is taken and the method is async.
        /// </summary>
        /// <param name="commandProcessor">The command processor this result refers to.</param>
        /// <returns>An action representing a continue operation.</returns>
        public static PostCommandAction Continue<T>(this ICommandProcessor<T> commandProcessor)
        {
            return new PostCommandAction(commandProcessor.CommandName, PostCommandActionType.Continue, -1);
        }

        /// <summary>
        ///     Create a return value for
        ///     <see
        ///         cref="ICommandProcessor{TValue}.Execute(CommandInfo{TValue}, CommandProcessorContext, System.Threading.CancellationToken)" />
        ///     when nothing must be done as a result of the command and the method is not async.
        /// </summary>
        /// <param name="commandProcessor">The command processor this result refers to.</param>
        /// <returns>A completed task representing a continue operation.</returns>
        public static UniTask<PostCommandAction> DoNothingAsync<T>(this ICommandProcessor<T> commandProcessor)
        {
            var completionSource = new UniTaskCompletionSource<PostCommandAction>();
            completionSource.TrySetResult(new PostCommandAction(commandProcessor.CommandName,
                PostCommandActionType.DoNothing, -1));
            return completionSource.Task;
        }

        /// <summary>
        ///     Create a return value for
        ///     <see
        ///         cref="ICommandProcessor{TValue}.Execute(CommandInfo{TValue}, CommandProcessorContext, System.Threading.CancellationToken)" />
        ///     when nothing must be done as a result of the command and the method is async.
        /// </summary>
        /// <param name="commandProcessor">The command processor this result refers to.</param>
        /// <returns>An action representing a continue operation.</returns>
        public static PostCommandAction DoNothing<T>(this ICommandProcessor<T> commandProcessor)
        {
            return new PostCommandAction(commandProcessor.CommandName, PostCommandActionType.DoNothing, -1);
        }
    }
}