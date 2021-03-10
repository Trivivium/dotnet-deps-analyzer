using System;
using System.CommandLine;
using System.CommandLine.IO;

using App.Inspection;

namespace App
{
    /// <summary>
    /// Logs informational and error message from the commands to the console instance
    /// of the command line abstractions.
    /// </summary>
    public class CommandLineConsoleLogger : ILogger
    {
        private readonly IConsole _console;
        private readonly bool _verbose;

        public CommandLineConsoleLogger(IConsole console, bool verbose)
        {
            _console = console;
            _verbose = verbose;
        }

        /// <inheritdoc cref="ILogger.LogInformation(string)"/>
        public void LogInformation(string message)
        {
            _console.Out.WriteLine(message);
        }
        
        /// <inheritdoc cref="ILogger.LogVerbose(string)"/>
        public void LogVerbose(string message)
        {
            if (_verbose)
            {
                LogInformation(message);
            }
        }

        /// <inheritdoc cref="ILogger.LogError(string)"/>
        public void LogError(string message)
        {
            _console.Error.WriteLine($"[ERR] {message}");
        }

        /// <inheritdoc cref="ILogger.LogError(Exception, string)"/>
        public void LogError(Exception exception, string message)
        {
            _console.Error.WriteLine($"[ERR] {message}");
            _console.Error.WriteLine($"[Exception] {exception.Message}");
            
            var stacktrace = exception.StackTrace;

            if (stacktrace is not null)
            {
                _console.Error.WriteLine(stacktrace);
            }
        }
    }
}
