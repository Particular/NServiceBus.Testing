namespace NServiceBus.Testing
{
    using System;

    /// <summary>
    /// 
    /// </summary>
    public class Test
    {
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="THandler"></typeparam>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public static Handler<THandler> Handler<THandler>()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="handler"></param>
        /// <typeparam name="THandler"></typeparam>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public static Handler<THandler> Handler<THandler>(THandler handler)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key1"></param>
        /// <param name="header1"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public Test SetIncomingHeader(string key1, string header1)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public static Test Initialize()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TCommand"></typeparam>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public Test OnMessage<TCommand>()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="command"></param>
        /// <typeparam name="TCommand"></typeparam>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public Test OnMessage<TCommand>(TCommand command)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public Test ExpectDoNotContinueDispatchingCurrentMessageToHandlers()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public Test ExpectHandleCurrentMessageLater()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="func"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public Test ExpectSendToSites<T>(Func<object, object, bool> func)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="func"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public Test ExpectNotSendToSites<T>(Func<object, object, bool> func)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="action"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public Test WithExternalDependencies<T>(Action<T> action)
        {
            throw new NotImplementedException();
        }
    }
}