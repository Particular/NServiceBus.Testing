namespace NServiceBus.Testing;

using System;
using Logging;

class NamedLogger : ILog
{
    public NamedLogger(string name) => this.name = name;

    public bool IsDebugEnabled => DefaultTestingLoggerFactory.IsDebugEnabled;

    public bool IsInfoEnabled => DefaultTestingLoggerFactory.IsInfoEnabled;

    public bool IsWarnEnabled => DefaultTestingLoggerFactory.IsWarnEnabled;

    public bool IsErrorEnabled => DefaultTestingLoggerFactory.IsErrorEnabled;

    public bool IsFatalEnabled => DefaultTestingLoggerFactory.IsFatalEnabled;

    public void Debug(string message) => DefaultTestingLoggerFactory.Write(name, LogLevel.Debug, message);

    public void Debug(string message, Exception exception) => DefaultTestingLoggerFactory.Write(name, LogLevel.Debug, message + Environment.NewLine + exception);

    public void DebugFormat(string format, params object[] args) => DefaultTestingLoggerFactory.Write(name, LogLevel.Debug, string.Format(format, args));

    public void Info(string message) => DefaultTestingLoggerFactory.Write(name, LogLevel.Info, message);

    public void Info(string message, Exception exception) => DefaultTestingLoggerFactory.Write(name, LogLevel.Info, message + Environment.NewLine + exception);

    public void InfoFormat(string format, params object[] args) => DefaultTestingLoggerFactory.Write(name, LogLevel.Info, string.Format(format, args));

    public void Warn(string message) => DefaultTestingLoggerFactory.Write(name, LogLevel.Warn, message);

    public void Warn(string message, Exception exception) => DefaultTestingLoggerFactory.Write(name, LogLevel.Warn, message + Environment.NewLine + exception);

    public void WarnFormat(string format, params object[] args) => DefaultTestingLoggerFactory.Write(name, LogLevel.Warn, string.Format(format, args));

    public void Error(string message) => DefaultTestingLoggerFactory.Write(name, LogLevel.Error, message);

    public void Error(string message, Exception exception) => DefaultTestingLoggerFactory.Write(name, LogLevel.Error, message + Environment.NewLine + exception);

    public void ErrorFormat(string format, params object[] args) => DefaultTestingLoggerFactory.Write(name, LogLevel.Error, string.Format(format, args));

    public void Fatal(string message) => DefaultTestingLoggerFactory.Write(name, LogLevel.Fatal, message);

    public void Fatal(string message, Exception exception) => DefaultTestingLoggerFactory.Write(name, LogLevel.Error, message + Environment.NewLine + exception);

    public void FatalFormat(string format, params object[] args) => DefaultTestingLoggerFactory.Write(name, LogLevel.Fatal, string.Format(format, args));

    string name;
}