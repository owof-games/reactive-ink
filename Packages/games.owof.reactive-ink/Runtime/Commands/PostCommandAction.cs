namespace ReactiveInk.Commands
{
    public enum PostCommandActionType
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
        ///         cref="ICommandProcessor{TValue,TParam}.Execute(CommandInfo{TValue}, CommandProcessorContext{TValue,TParam}, System.Threading.CancellationToken)" />
        ///     when a choice is taken and the method is async.
        /// </summary>
        /// <param name="commandProcessor">The command processor this result refers to.</param>
        /// <param name="index">The index of the choice to take.</param>
        /// <returns>An action representing the choice taken.</returns>
        public static PostCommandAction TakeChoice<TValue, TParam>(
            this ICommandProcessor<TValue, TParam> commandProcessor, int index)
        {
            return new PostCommandAction(commandProcessor.CommandName, PostCommandActionType.Choice, index);
        }

        /// <summary>
        ///     Create a return value for
        ///     <see
        ///         cref="ICommandProcessor{TValue,TParam}.Execute(CommandInfo{TValue}, CommandProcessorContext{TValue,TParam}, System.Threading.CancellationToken)" />
        ///     when no choice is taken and the method is async.
        /// </summary>
        /// <param name="commandProcessor">The command processor this result refers to.</param>
        /// <returns>An action representing a continue operation.</returns>
        public static PostCommandAction Continue<TValue, TParam>(
            this ICommandProcessor<TValue, TParam> commandProcessor)
        {
            return new PostCommandAction(commandProcessor.CommandName, PostCommandActionType.Continue, -1);
        }

        /// <summary>
        ///     Create a return value for
        ///     <see
        ///         cref="ICommandProcessor{TValue,TParam}.Execute(CommandInfo{TValue}, CommandProcessorContext{TValue,TParam}, System.Threading.CancellationToken)" />
        ///     when nothing must be done as a result of the command and the method is async.
        /// </summary>
        /// <param name="commandProcessor">The command processor this result refers to.</param>
        /// <returns>An action representing a continue operation.</returns>
        public static PostCommandAction DoNothing<TParam, TResult>(
            this ICommandProcessor<TParam, TResult> commandProcessor)
        {
            return new PostCommandAction(commandProcessor.CommandName, PostCommandActionType.DoNothing, -1);
        }
    }
}