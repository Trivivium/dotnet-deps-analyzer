namespace App.Inspection
{
    public interface ILogger
    {
        /// <summary>
        /// Logs an informational message to output.
        /// </summary>
        /// <param name="message">The message to display.</param>
        public void LogInformation(string message);

        /// <summary>
        /// Logs an error message to output.
        /// </summary>
        /// <param name="message">The message to display.</param>
        public void LogWarning(string message);

        /// <summary>
        /// Logs an error message to output.
        /// </summary>
        /// <param name="message">The message to display.</param>
        public void LogError(string message);
        
        /// <summary>
        /// Logs verbose informational message to output.
        /// This is intended for users to gain insight into the decisions made during the analysis.
        /// </summary>
        /// <param name="message">The message to display.</param>
        public void LogVerbose(string message);
    }
}
