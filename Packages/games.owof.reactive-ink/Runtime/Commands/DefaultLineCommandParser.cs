#nullable enable
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine.Assertions;

namespace ReactiveInk.Commands
{
    /// <summary>
    ///     <para>
    ///         The default line command parser. This command parser reads lines like this:
    ///     </para>
    ///     <code>
    /// @moveCamera target:Elizabeth smooth:true
    /// </code>
    ///     <para>
    ///         The prefix "@" is configurable. All parameters are named, in the form <em>name</em>:<em>value</em>, and
    ///         separated by spaces.
    ///         If a parameter value needs to contain spaces, it can be wrapped between quotes (<c>"</c>); e.g.:
    ///     </para>
    ///     <code>
    /// @hiddenText over:Mirror text:"Looking in the mirror, you notice its surface swirls like a whirlpool"
    /// </code>
    /// </summary>
    public class DefaultLineCommandParser : ICommandParser
    {
        /// <summary>
        ///     The string describing the regular expression used to match command lines. This regex is missing the prefix.
        /// </summary>
        private const string CommandLineParserBaseRegex =
            @"(?<name>[^\s]+)(?<param>\s+(?<paramName>[a-zA-Z][a-zA-Z0-9]*):(""(?<paramValue>[^""]*)""|(?<paramValue>[^\s]*)))*";

        /// <summary>
        ///     The prefix string to use.
        /// </summary>
        private readonly string _prefix;

        /// <summary>
        ///     The actual regular expression used to match command lines; it's a cache, meaning it's <c>null</c>
        ///     until actually needed.
        /// </summary>
        private Regex? _commandLineParserRegexCache;

        public DefaultLineCommandParser(string prefix = "@")
        {
            _prefix = prefix;
        }

        public IEnumerable<CommandInfo<string>> ParseCommand(CommandParserContext context)
        {
            // uses the regex to parse the command name and parameters
            var matchCollection = GetCommandLineParserRegex().Matches(
                context.StoryStep.Text);
            if (matchCollection.Count == 0) return Enumerable.Empty<CommandInfo<string>>();

            string? commandName = null;
            string[]? paramNames = null;
            string[]? paramValues = null;
            // moves through the various matches and groups to save the relevant data
            foreach (var g in from Match match in matchCollection
                     from Group g in match.Groups
                     select g)
                switch (g.Name)
                {
                    case "name":
                        commandName = g.Value;
                        break;
                    case "paramName":
                        paramNames = g.Captures
                            .Select(capture => capture.Value)
                            .ToArray();
                        break;
                    case "paramValue":
                        paramValues = g.Captures
                            .Select(capture => capture.Value)
                            .ToArray();
                        break;
                }

            // tries to extract the correct command line parser
            commandName ??= ""; // the "@" line returns a null command

            // pairs the parameter names and parameter values
            Debug.Assert(paramNames != null, nameof(paramNames) + " != null");
            Debug.Assert(paramValues != null, nameof(paramValues) + " != null");
            Assert.AreEqual(paramNames.Length, paramValues.Length);
            var parameters = paramNames
                .Zip(paramValues, (paramName, value) => (paramName, value))
                .ToDictionary(p => p.paramName, p => p.value);

            return new[]
                { new CommandInfo<string>(commandName, new ReadOnlyDictionary<string, string>(parameters), null) };
        }

        /// <summary>
        ///     Get the regular expression to match command lines.
        /// </summary>
        /// <returns>The regular expression to match command lines.</returns>
        private Regex GetCommandLineParserRegex()
        {
            _commandLineParserRegexCache ??= new Regex(_prefix + CommandLineParserBaseRegex);
            return _commandLineParserRegexCache;
        }
    }
}