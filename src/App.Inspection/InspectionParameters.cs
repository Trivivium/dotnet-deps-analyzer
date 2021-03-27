using System;
using System.Linq;
using System.Collections.Generic;

using Microsoft.CodeAnalysis;

using App.Inspection.Metrics;

namespace App.Inspection
{
    /// <summary>
    /// Declares the configuration parameters of the analysis.
    /// </summary>
    public sealed class InspectionParameters
    {
        private static readonly string[] _defaultNamespaceExclusions = {
            "System.", 
            "Microsoft.",
            "Microsoft.CSharp",
            "NETStandard.Library"
        };

        /// <summary>
        /// Creates a parameter instance with default the configuration. The defaults excludes namespaces
        /// that starts with a 'System.' and 'Microsoft.' prefix.
        /// </summary>
        /// <param name="excludedProjects">A collection of projects to exclude from the analysis.</param>
        /// <param name="excludedNamespaces">A collection of namespaces to exclude in addition to the defaults.</param>
        /// <param name="metrics">An optional collection of metrics to compute. If <see langword="null"/> is provided all metrics are computed.</param>
        /// <param name="maxConcurrency">Declares the maximum number of projects being inspected in parallel.</param>
        public static InspectionParameters CreateWithDefaults(
            IEnumerable<string> excludedProjects,
            IEnumerable<string> excludedNamespaces,
            IEnumerable<MetricName>? metrics,
            int maxConcurrency)
        {
            excludedNamespaces = excludedNamespaces
                .Union(_defaultNamespaceExclusions)
                .Distinct();

            return new InspectionParameters(excludedProjects, excludedNamespaces, metrics, maxConcurrency);
        }

        internal readonly ICollection<string> ExcludedProjects;
        internal readonly ICollection<string> ExcludedNamespaces;
        internal readonly ICollection<MetricName> Metrics;
        internal readonly int MaxConcurrency;

        private InspectionParameters(
            IEnumerable<string> excludedProjects,
            IEnumerable<string> ignoredNamespaces,
            IEnumerable<MetricName>? metrics,
            int maxConcurrency)
        {
            MaxConcurrency = maxConcurrency;
            ExcludedProjects = excludedProjects
                .Select(p => p.ToUpperInvariant())
                .ToList();

            ExcludedNamespaces = ignoredNamespaces
                .Select(p => p.ToUpperInvariant())
                .ToList();
            
            Metrics = (metrics ?? new List<MetricName>()).ToList();
        }

        internal bool IsProjectExcluded(Project project)
        {
            return ExcludedProjects.Contains(project.Name, StringComparer.OrdinalIgnoreCase);
        }
    }
}
