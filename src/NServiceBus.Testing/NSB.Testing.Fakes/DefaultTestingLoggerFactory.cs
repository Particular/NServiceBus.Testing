namespace NServiceBus.Testing
{
    using System;
    using System.IO;
    using Logging;

    class DefaultTestingLoggerFactory : ILoggerFactory
    {

        public static bool IsDebugEnabled => LogLevel.Debug >= FilterLevel();

        public static bool IsInfoEnabled => LogLevel.Info >= FilterLevel();

        public static bool IsWarnEnabled => LogLevel.Warn >= FilterLevel();
        public static bool IsErrorEnabled => LogLevel.Error >= FilterLevel();
        public static bool IsFatalEnabled => LogLevel.Fatal >= FilterLevel();

        public ILog GetLogger(Type type)
        {
            return GetLogger(type.FullName);
        }

        public ILog GetLogger(string name)
        {
            return new NamedLogger(name);
        }

        public static void Write(string name, LogLevel messageLevel, string message)
        {
            if (messageLevel < FilterLevel())
            {
                return;
            }

            var datePart = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
            var paddedLevel = messageLevel.ToString().ToUpper().PadRight(5);
            var fullMessage = $"{datePart} {paddedLevel} {name} {message}";
            var writer = TextWriter();
            lock (writer)
            {
                writer.Write(fullMessage);
            }
        }

        static LogLevel FilterLevel()
        {
            return TestingLoggerFactory.currentScope.Value?.Item2 ?? TestingLoggerFactory.lazyLevel.Value;
        }

        static TextWriter TextWriter()
        {
            return TestingLoggerFactory.currentScope.Value?.Item1 ?? TestingLoggerFactory.lazyWriter.Value;
        }
    }
}