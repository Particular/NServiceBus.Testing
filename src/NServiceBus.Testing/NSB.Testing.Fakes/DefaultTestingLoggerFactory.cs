namespace NServiceBus.Testing;

using System;
using System.IO;
using Logging;
using Particular.Obsoletes;

[ObsoleteMetadata(
    Message = "Implement a custom logger using Microsoft.Extensions.Logging.ILoggerProvider instead",
    TreatAsErrorFromVersion = "11",
    RemoveInVersion = "12")]
[Obsolete("Implement a custom logger using Microsoft.Extensions.Logging.ILoggerProvider instead. Will be treated as an error from version 11.0.0. Will be removed in version 12.0.0.", false)]
class DefaultTestingLoggerFactory : ILoggerFactory
{
    public static bool IsDebugEnabled => FilterLevel <= LogLevel.Debug;

    public static bool IsInfoEnabled => FilterLevel <= LogLevel.Info;

    public static bool IsWarnEnabled => FilterLevel <= LogLevel.Warn;
    public static bool IsErrorEnabled => FilterLevel <= LogLevel.Error;
    public static bool IsFatalEnabled => FilterLevel <= LogLevel.Fatal;

    static LogLevel FilterLevel => TestingLoggerFactory.currentScope.Value?.Item2 ?? TestingLoggerFactory.lazyLevel.Value;

    static TextWriter TextWriter => TestingLoggerFactory.currentScope.Value?.Item1 ?? TestingLoggerFactory.lazyWriter.Value;

    public ILog GetLogger(Type type) => GetLogger(type.FullName);

    public ILog GetLogger(string name) => new NamedLogger(name);

    public static void Write(string name, LogLevel messageLevel, string message)
    {
        if (messageLevel < FilterLevel)
        {
            return;
        }

#pragma warning disable PS0023 // DateTime.UtcNow or DateTimeOffset.UtcNow should be used instead of DateTime.Now and DateTimeOffset.Now, unless the value is being used for displaying the current date-time in a user's local time zone
        var datePart = DateTimeOffset.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
#pragma warning restore PS0023 // DateTime.UtcNow or DateTimeOffset.UtcNow should be used instead of DateTime.Now and DateTimeOffset.Now, unless the value is being used for displaying the current date-time in a user's local time zone
        var paddedLevel = messageLevel.ToString().ToUpper().PadRight(5);
        var fullMessage = $"{datePart} {paddedLevel} {name} {message}";
        var writer = TextWriter;
        lock (writer)
        {
            writer.Write(fullMessage);
        }
    }
}