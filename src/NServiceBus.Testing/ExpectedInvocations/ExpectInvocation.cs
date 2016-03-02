namespace NServiceBus.Testing.ExpectedInvocations
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    abstract class ExpectInvocation
    {
        public abstract void Validate(TestableMessageHandlerContext context);

        protected void Fail<TMessage>(IEnumerable<TMessage> invokedMessages)
        {
            throw new Exception($"{GetExpecationName(GetType())} not fulfilled.\nCalls made:\n{string.Join("\n", invokedMessages.Select(i => GetExpecationName(i.GetType())))}");
        }

        protected void Fail(string message)
        {
            throw new Exception(message);
        }

        string GetExpecationName(Type type)
        {
            var formattedString = type.ToString()
                .Replace("NServiceBus.Testing.", "")
                .Replace("`1[", "<")
                .Replace("`2[", "<");

            if (formattedString.EndsWith("]"))
            {
                formattedString = formattedString.Substring(0, formattedString.Length - 1) + ">";
            }

            return formattedString;
        }
    }
}