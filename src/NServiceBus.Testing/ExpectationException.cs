namespace NServiceBus.Testing
{
    using System;

    /// <summary>
    /// Exception representing a failed expectation.
    /// </summary>
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