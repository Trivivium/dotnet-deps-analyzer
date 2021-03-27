using System;
using System.IO;

#nullable disable warnings

namespace App.Commands
{
    public class InspectCommandArgs
    {
        /// <summary>
        /// Declares the path to the .csproj or .sln file of the project or solution to inspect.
        /// </summary>
        public FileInfo Path { get; set; }
        
        /// <summary>
        /// Indicates if the inspect command was invoked with verbose logging enabled.
        /// </summary>
        public bool Verbose { get; set; }
        
        /// <summary>
        /// Indicates if the inspect command was invoked in headless mode. This disables formatting of
        /// output (e.g., the progress indicator).
        /// </summary>
        public bool Headless { get; set; }

        /// <summary>
        /// Declares the maximum number of threads inspecting projects in parallel.
        /// </summary>
        public int MaxConcurrency { get; set; } = Environment.ProcessorCount;
    }
}
