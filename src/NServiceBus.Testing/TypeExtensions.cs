namespace NServiceBus.Testing
 {
     using System;
     using System.Collections.Generic;
     using System.Linq;
     using System.Linq.Expressions;
     using System.Threading.Tasks;

     static class TypeExtensions
     {
         public static IEnumerable<Func<object, object, IMessageHandlerContext, Task>> CreateInvokers(this Type targetType, Type messageType, Type interfaceGenericType)
         {
             var interfaceTypes = targetType.GetInterfaces()
                 .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == interfaceGenericType)
                 .Where(i => i.GenericTypeArguments.First().IsAssignableFrom(messageType));

             foreach (var interfaceType in interfaceTypes)
             {
                var methodInfo = targetType.GetInterfaceMap(interfaceType).TargetMethods.FirstOrDefault();
                if (methodInfo == null)
                {
                    yield break;
                }

                var target = Expression.Parameter(typeof(object));
                var messageParam = Expression.Parameter(typeof(object));
                var contextParam = Expression.Parameter(typeof(IMessageHandlerContext));

                var castTarget = Expression.Convert(target, targetType);

                var methodParameters = methodInfo.GetParameters();
                var messageCastParam = Expression.Convert(messageParam, methodParameters.ElementAt(0).ParameterType);

                Expression body = Expression.Call(castTarget, methodInfo, messageCastParam, contextParam);

                yield return Expression.Lambda<Func<object, object, IMessageHandlerContext, Task>>(body, target, messageParam, contextParam).Compile();
            }
         }

         public static async Task InvokeSerially(this IEnumerable<Func<object, object, IMessageHandlerContext, Task>> invokers, object instance, object message, IMessageHandlerContext context)
         {
            foreach (var invocation in invokers)
            {
                await invocation(instance, message, context).ConfigureAwait(false);
            }
        }
     }
 }
