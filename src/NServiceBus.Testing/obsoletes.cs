namespace NServiceBus.Testing
{
    using System;

    public partial class Saga<T> where T : Saga
    {
        /// <summary>
        /// Asserts that the saga is either complete or not.
        /// </summary>
        [ObsoleteEx(
            Message = "Use 'ExpectSagaCompleted' or 'ExpectSagaNotCompleted' instead",
            RemoveInVersion = "8",
            TreatAsErrorFromVersion = "7")]
        public Saga<T> AssertSagaCompletionIs(bool complete)
        {
            throw new NotImplementedException();
        }
    }
}