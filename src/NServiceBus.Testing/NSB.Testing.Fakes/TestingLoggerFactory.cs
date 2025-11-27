namespace NServiceBus.Testing;

using System;
using System.IO;
using System.Threading;
using Logging;

/// <summary>
/// Logger factory which allows to log to a text writer.
/// </summary>
public class TestingLoggerFactory : LoggingFactoryDefinition
{
    /// <summary>
    /// Creates a new instance of a testing logger factory.
    /// </summary>
    public TestingLoggerFactory()
    {
        lazyLevel = new Lazy<LogLevel>(() => LogLevel.Debug);
        lazyWriter = new Lazy<TextWriter>(() => TextWriter.Null);
    }

    /// <summary>
    /// Controls the <see cref="LogLevel" /> for all default logging.
    /// </summary>
    /// <param name="level">The log level to be used.</param>
    public void Level(LogLevel level) => lazyLevel = new Lazy<LogLevel>(() => level);

    /// <summary>
    /// Instructs the logger to write to the provided text writer for all default logging.
    /// </summary>
    /// <param name="writer">The text writer to be used.</param>
    public void WriteTo(TextWriter writer) => lazyWriter = new Lazy<TextWriter>(() => writer);

    /// <summary>
    /// Instructs the logger to write to the provided text writer for the given scope.
    /// </summary>
    /// <param name="writer">The text writer to be used.</param>
    /// <param name="level">The log level to be used.</param>
    /// <returns>The logging scope. Cannot be nested.</returns>
    public IDisposable BeginScope(TextWriter writer, LogLevel level = LogLevel.Debug) => new Scope(writer, level);

    /// <summary>
    /// Constructs an instance of <see cref="ILoggerFactory" /> for use by <see cref="LogManager.Use{T}" />.
    /// </summary>
    protected override ILoggerFactory GetLoggingFactory()
    {
        if (testingLoggerFactory == null)
        {
            testingLoggerFactory = new DefaultTestingLoggerFactory();
        }

        return testingLoggerFactory;
    }

    internal static AsyncLocal<Tuple<TextWriter, LogLevel>> currentScope = new AsyncLocal<Tuple<TextWriter, LogLevel>>();
    internal static Lazy<LogLevel> lazyLevel;
    internal static Lazy<TextWriter> lazyWriter;
    static DefaultTestingLoggerFactory testingLoggerFactory;

    class Scope : IDisposable
    {
        public Scope(TextWriter writer, LogLevel logLevel)
        {
            if (currentScope.Value != null)
            {
                throw new InvalidOperationException("Nesting of logging scopes is not allowed.");
            }

            currentScope.Value = Tuple.Create(writer, logLevel);
        }

        public void Dispose() => currentScope.Value = null;
    }
}