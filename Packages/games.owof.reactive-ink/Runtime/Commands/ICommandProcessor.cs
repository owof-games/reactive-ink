using System.Threading;
using Cysharp.Threading.Tasks;

namespace ReactiveInk.Commands
{
    /// <summary>
    ///     A processor to execute commands.
    /// </summary>
    /// <typeparam name="TValue">The type of parameter values for this processor.</typeparam>
    public interface ICommandProcessor<TValue>
    {
        /// <summary>
        ///     Name of the command. The interpretation of what the name is changes according to the fact if this command
        ///     is a line command, a tag command, or a function command.
        /// </summary>
        public string CommandName { get; }

        /// <summary>
        ///     Whether this command processor must also be registered as an external function. Only command processors
        ///     taking <see cref="Ink.Runtime.Value" /> parameters can be registered as external functions.
        /// </summary>
        public bool RegisterAsExternalFunction { get; }

        /// <summary>
        ///     Execute the command.
        /// </summary>
        /// <param name="commandInfo">The info about the command to execute.</param>
        /// <param name="context">The context this command executes in.</param>
        /// <param name="cancellationToken"></param>
        /// <returns>
        ///     A task that represents the completion of the execution. The value represents the action to take
        ///     after the command execution.
        ///     <see cref="PostCommandActionExtensions.TakeChoiceAsyncAsync{T}" />,
        ///     <see cref="PostCommandActionExtensions.ContinueAsyncAsync{T}" /> and
        ///     <see cref="PostCommandActionExtensions.DoNothingAsyncAsync{T}" /> are helpers that can be used to
        ///     immediately produce this return value if there are no async operations needed.
        /// </returns>
        UniTask<PostCommandAction> Execute(CommandInfo<TValue> commandInfo, CommandProcessorContext context,
            CancellationToken cancellationToken);
    }
}