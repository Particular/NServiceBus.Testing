namespace NServiceBus.Testing
 {
     using System;
     using System.Linq;
     using System.Linq.Expressions;
     using System.Threading.Tasks;
 
     static class TypeExtensions
     {
         public static Func<object, object, IMessageHandlerContext, Task> CreateInvoker(this Type targetType, Type messageType, Type interfaceGenericType)
         {
             var interfaceType = interfaceGenericType.MakeGenericType(messageType);
 
             if (!interfaceType.IsAssignableFrom(targetType))
             {
                 return null;
             }
 
             var methodInfo = targetType.GetInterfaceMap(interfaceType).TargetMethods.FirstOrDefault();
             if (methodInfo == null)
             {
                 return null;
             }
 
             var target = Expression.Parameter(typeof(object));
             var messageParam = Expression.Parameter(typeof(object));
             var contextParam = Expression.Parameter(typeof(IMessageHandlerContext));
 
             var castTarget = Expression.Convert(target, targetType);
 
             var methodParameters = methodInfo.GetParameters();
             var messageCastParam = Expression.Convert(messageParam, methodParameters.ElementAt(0).ParameterType);
 
             Expression body = Expression.Call(castTarget, methodInfo, messageCastParam, contextParam);
 
             return Expression.Lambda<Func<object, object, IMessageHandlerContext, Task>>(body, target, messageParam, contextParam).Compile();
         }
     }
 } 
