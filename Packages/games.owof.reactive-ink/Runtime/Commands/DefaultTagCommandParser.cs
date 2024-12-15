#nullable enable
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace ReactiveInk.Commands
{
    public class DefaultTagCommandParser : ICommandParser
    {
        private static readonly IReadOnlyDictionary<string, string> EmptyDictionary =
            new ReadOnlyDictionary<string, string>(new Dictionary<string, string>());

        public IEnumerable<CommandInfo<string>> ParseCommand(CommandParserContext commandParserContext)
        {
            return from tag in commandParserContext.StoryStep.Tags
                let parts = tag.Split(':')
                where commandParserContext.KnownCommands.Contains(parts[0])
                select new CommandInfo<string>(parts[0], EmptyDictionary,
                    new ArraySegment<string>(parts, 1, parts.Length - 1));
        }
    }
}