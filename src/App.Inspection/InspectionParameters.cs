using System.Linq;
using System.Collections.Generic;

using Microsoft.CodeAnalysis;

namespace App.Inspection
{
    /// <summary>
    /// Declares the configuration parameters of the analysis.
    /// </summary>
    public sealed class InspectionParameters
    {
        private static readonly string[] _defaultIgnoredNamespaces = {
            "System.",
            "Microsoft."
        };

        /// <summary>
        /// Creates a parameter instance with default the configuration. The defaults ignores namespaces
        /// that starts with a 'System.' and 'Microsoft.' prefix.
        /// </summary>
        /// <param name="excludedProjects">A collection of projects to exclude from the analysis.</param>
        /// <param name="ignoredNamespaces">A collection of namespaces to ignore in addition to the defaults.</param>
        /// <param name="metrics">An optional collection of metrics to compute. If <see langword="null"/> is provided all metrics are computed.</param>
        public static InspectionParameters CreateWithDefaults(
            IEnumerable<string> excludedProjects,
            IEnumerable<string> ignoredNamespaces,
            IEnumerable<string>? metrics)
        {
            ignoredNamespaces = ignoredNamespaces
                .Union(_defaultIgnoredNamespaces)
                .Distinct();

            return new InspectionParameters(excludedProjects, ignoredNamespaces, metrics);
        }

        private readonly ICollection<string> _excludedProjects;
        private readonly ICollection<string> _ignoredNamespaces;
        
        internal List<string> Metrics { get; }

        private InspectionParameters(
            IEnumerable<string> excludedProjects,
            IEnumerable<string> ignoredNamespaces,
            IEnumerable<string>? metrics)
        {
            _excludedProjects = excludedProjects
                .Select(p => p.ToUpperInvariant())
                .ToList();

            _ignoredNamespaces = ignoredNamespaces
                .Select(p => p.ToUpperInvariant())
                .ToList();
            
            Metrics = (metrics ?? new List<string>())
                .Select(p => p.ToUpperInvariant())
                .ToList();
        }

        public bool IsProjectExcluded(Project project)
        {
            var name = project.Name.ToUpperInvariant();

            return _excludedProjects.Contains(name);
        }

        public bool IsNamespaceIgnored(in string ns)
        {
            var normalized = ns.ToUpperInvariant();

            return normalized == "SYSTEM" || _ignoredNamespaces.Any(n => normalized.StartsWith(n));
        }
    }
}
