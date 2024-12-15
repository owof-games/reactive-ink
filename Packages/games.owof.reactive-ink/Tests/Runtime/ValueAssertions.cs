using FluentAssertions;
using FluentAssertions.Execution;
using FluentAssertions.Primitives;
using Ink.Runtime;

namespace ReactiveInk.Tests.Tests.Runtime
{
    /// <summary>
    ///     Fluent assertions extension to text Ink runtime values.
    /// </summary>
    public class ValueAssertions : ReferenceTypeAssertions<Value, ValueAssertions>
    {
        public ValueAssertions(Value subject) : base(subject)
        {
        }

        protected override string Identifier => "inkValue";

        /// <summary>
        ///     Asserts that the value contained in an Ink runtime value matches the supplied value.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="because">
        ///     A formatted phrase as is supported by <see cref="string.Format(string, object[])" /> explaining
        ///     why the assertion is needed. If the phrase does not start with the word because, it is prepended automatically.
        /// </param>
        /// <param name="becauseArgs">Zero or more objects to format using the placeholders in because.</param>
        /// <typeparam name="T">
        ///     Type of the value to assert. Only int, float, string, <see cref="Path" /> and
        ///     <see cref="InkList" /> are supported.
        /// </typeparam>
        [CustomAssertion]
        public AndConstraint<ValueAssertions> HaveValue<T>(T value, string because = "", params object[] becauseArgs)
        {
            Execute.Assertion
                .BecauseOf(because, becauseArgs)
                .ForCondition(value is bool || value is int || value is float || value is string || value is Path ||
                              value is InkList)
                .FailWith("Values can only be int, float, string, Path or InkList, not a {0}",
                    () => value.GetType().FullName)
                .Then
                .ForCondition(Subject is Value<T>)
                .FailWith("Expected {context:inkValue} to be a Value<{0}>{reason}, but instead is a {1}",
                    () => typeof(T), () => Subject.valueType)
                .Then
                .Given(() => ((Value<T>)Subject).value)
                .ForCondition(v => v.Equals(value))
                .FailWith("Expected {context:inkValue} to contain value {0}{reason}, but instead contains {1}",
                    _ => value, x => x);

            return new AndConstraint<ValueAssertions>(this);
        }
    }

    public static class ValueExtensions
    {
        public static ValueAssertions Should(this Value value)
        {
            return new ValueAssertions(value);
        }
    }
}