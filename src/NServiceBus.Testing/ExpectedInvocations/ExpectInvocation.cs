namespace NServiceBus.Testing.ExpectedInvocations
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    abstract class ExpectInvocation
    {
        internal abstract void Validate(TestableMessageHandlerContext context);

        internal void Fail<TMessage>(IEnumerable<TMessage> invokedMessages)
        {
            throw new Exception($"{Filter(GetType())} not fulfilled.\nCalls made:\n{string.Join("\n", invokedMessages.Select(i => Filter(i.GetType())))}");
        }

        string Filter(Type type)
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