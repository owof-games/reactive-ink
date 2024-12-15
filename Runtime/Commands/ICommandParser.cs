#nullable enable
using System.Collections.Generic;

namespace ReactiveInk.Commands
{
    /// <summary>
    ///     A command parser is an object that can find commands in the current type of the story (considering the line
    ///     and/or the tags).
    /// </summary>
    public interface ICommandParser
    {
        /// <summary>
        ///     Parses all the commands it finds from the current story type.
        /// </summary>
        /// <param name="commandParserContext">The context where to find commands from.</param>
        /// <returns>All the commands found in the current type.</returns>
        IEnumerable<CommandInfo<string>> ParseCommand(CommandParserContext commandParserContext);
    }
}