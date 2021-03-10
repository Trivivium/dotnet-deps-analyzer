using System;
using System.CommandLine.IO;
using System.CommandLine.Rendering;

using App.Inspection;

namespace App.Logging
{
    /// <summary>
    /// Logs informational and error message from the commands to the console instance
    /// of the command line abstractions.
    /// </summary>
    internal class SystemConsoleLoggerAdapter : ILogger
    {
        private readonly ITerminal _terminal;
        private readonly bool _verbose;

        public SystemConsoleLoggerAdapter(ITerminal terminal, bool verbose)
        {
            _terminal = terminal;
            _verbose = verbose;
        }

        /// <inheritdoc cref="ILogger.LogInformation(string)"/>
        public void LogInformation(string message)
        {
            _terminal.Out.WriteLine($"[INFO] {message}");
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
            _terminal.Error.WriteLine($"[ERR] {message}");
        }

        /// <inheritdoc cref="ILogger.LogError(Exception, string)"/>
        public void LogError(Exception exception, string message)
        {
            _terminal.Error.WriteLine($"[ERR] {message}");
            _terminal.Error.WriteLine($"[Exception] {exception.Message}");
            
            var stacktrace = exception.StackTrace;

            if (stacktrace is not null)
            {
                _terminal.Error.WriteLine(stacktrace);
            }
        }
    }
}
