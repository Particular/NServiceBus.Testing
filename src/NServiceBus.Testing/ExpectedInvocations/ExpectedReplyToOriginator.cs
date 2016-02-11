namespace NServiceBus.Testing.ExpectedInvocations
{
    using System;

    class ExpectedReplyToOriginator<TMessage> : ExpectedInvocation
    {
        public ExpectedReplyToOriginator(Func<TMessage, bool> check = null)
        {
            this.check = check;
        }

        internal override void Validate(TestableMessageHandlerContext context)
        {
            throw new NotImplementedException();
        }

        readonly Func<TMessage, bool> check;
    }
}