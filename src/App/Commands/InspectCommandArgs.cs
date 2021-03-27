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
        /// Declares a comma-separated list of metrics to compute for all inspected projects.
        /// </summary>
        public string? Metrics { get; set; }
        
        /// <summary>
        /// Declares a comma-separated list of namespaces to exclude from the inspection results.
        /// </summary>
        public string? ExcludedNamespaces { get; set; }
        
        /// <summary>
        /// Declares a comma-separated list of projects to exclude from the inspection results.
        /// </summary>
        public string? ExcludedProjects { get; set; }
        
        /// <summary>
        /// Indicates if the results should show all packages regardless of whether it has metrics
        /// relevant for the project.
        /// </summary>
        public bool ShowAll { get; set; }
        
        /// <summary>
        /// Declares the maximum number of threads inspecting projects in parallel.
        /// </summary>
        public int MaxConcurrency { get; set; } = Environment.ProcessorCount;
    }
}
