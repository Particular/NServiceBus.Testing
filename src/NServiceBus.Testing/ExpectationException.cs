namespace NServiceBus.Testing
{
    using System;

    /// <summary>
    /// Exception representing a failed expectation.
    /// </summary>
    [ObsoleteEx(
     Message = "Use the arrange act assert (AAA) syntax instead. Please see the upgrade guide for more details.",
     RemoveInVersion = "9",
     TreatAsErrorFromVersion = "8")]
    public class ExpectationException : Exception
    {
        /// <summary>
        /// Creates a new instance of a <see cref="ExpectationException"/>.
        /// </summary>
        public ExpectationException()
        {
        }

        /// <summary>
        /// Creates a new instance of a <see cref="ExpectationException"/>.
        /// </summary>
        public ExpectationException(string message) : base(message)
        {
        }
    }
}