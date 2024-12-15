#nullable enable
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace ReactiveInk.Commands
{
    /// <summary>
    ///     Information about a command to execute.
    /// </summary>
    /// <param name="CommandName">Name of the command.</param>
    /// <param name="NamedParameters">A map containing all the named parameters (if any).</param>
    /// <param name="PositionalParameters">The list of all positional parameters (if any).</param>
    /// <typeparam name="TValue">The type of parameter values.</typeparam>
    public record CommandInfo<TValue>(
        string CommandName,
        IReadOnlyDictionary<string, TValue>? NamedParameters,
        TValue[]? PositionalParameters)
    {
        /// <summary>
        ///     Number of positional parameters.
        /// </summary>
        public int PositionalParametersCount => PositionalParameters?.Length ?? 0;

        /// <summary>
        ///     Get the positional parameter at given index.
        /// </summary>
        /// <param name="index">Index of the parameter.</param>
        /// <exception cref="InvalidOperationException">If the index was negative or greater than the number of parameters.</exception>
        public TValue this[int index]
        {
            get
            {
                if (PositionalParameters == null || index < 0 || index >= PositionalParameters.Length)
                    throw new InvalidOperationException($"Cannot find positional parameter: {index}");

                return PositionalParameters[index];
            }
        }

        /// <summary>
        ///     The names of all named parameters.
        /// </summary>
        public IEnumerable<string> NamedParametersNames => NamedParameters?.Keys ?? Array.Empty<string>();

        /// <summary>
        ///     Get the (named) parameter with given name.
        /// </summary>
        /// <param name="parameterName">Name of the parameter.</param>
        /// <exception cref="InvalidOperationException">If no parameter has the given name.</exception>
        public TValue this[string parameterName]
        {
            get
            {
                if (NamedParameters == null || !NamedParameters.TryGetValue(parameterName, out var parameterValue))
                    throw new InvalidOperationException($"Cannot find parameter: {parameterName}");

                return parameterValue;
            }
        }

        /// <summary>
        ///     Get the (positional) parameter value at given index.
        /// </summary>
        /// <param name="index">The index of the parameter to retrieve.</param>
        /// <param name="value">
        ///     When this method returns, the parameter value at given index, if the index is found;
        ///     otherwise, the default value for the type of the <c>value</c> parameter. this parameter is passed uninitialized.
        /// </param>
        /// <returns><c>true</c> if the parameter at given index was found; otherwise, <c>false</c>.</returns>
        /// <exception cref="ArgumentOutOfRangeException"><c>index</c> is negative.</exception>
        public bool TryGetParameter(int index, [NotNullWhen(true)] out TValue? value)
        {
            if (index < 0) throw new ArgumentOutOfRangeException(nameof(index), "index cannot be negative");

            if (PositionalParameters == null || PositionalParameters.Length <= index)
            {
                value = default;
                return false;
            }

            value = PositionalParameters[index];
            Debug.Assert(value != null, "Parameters should never be null");
            return true;
        }

        /// <summary>
        ///     Get the (named) parameter value at given index.
        /// </summary>
        /// <param name="name">The name of the parameter to retrieve.</param>
        /// <param name="value">
        ///     When this method returns, the of the parameter with given name, if the name is found;
        ///     otherwise, the default value for the type of the <c>value</c> parameter. This parameter is passed uninitialized.
        /// </param>
        /// <returns><c>true</c> if the parameter with given name was found; otherwise, <c>false</c>.</returns>
        /// <exception cref="ArgumentNullException"><c>name</c> is null.</exception>
        public bool TryGetParameter(string name, [NotNullWhen(true)] out TValue? value)
        {
            if (name == null) throw new ArgumentNullException(nameof(name));

            if (NamedParameters == null || !NamedParameters.TryGetValue(name, out value))
            {
                value = default;
                return false;
            }

            Debug.Assert(value != null, "Parameters should never be null");
            return true;
        }
    }
}