using System;
using System.Collections.Generic;

using App.Inspection.Metrics;
using App.Inspection.Packages;

namespace App.Inspection
{
    public sealed class PackageInspectionResult
    {
        private readonly Package _package;
        
        /// <summary>
        /// A collection of the results of the metrics that was computed on the package.
        /// </summary>
        public IReadOnlyCollection<IMetricResult?> Metrics { get; }

        /// <summary>
        /// The name of the package.
        /// </summary>
        public string Name => _package.Name;

        public string Version => _package?.Version?.ToString() ?? "";
        
        public string Type => Enum.GetName(typeof(PackageReferenceType), _package.Type) ?? "<unknown>";

        internal PackageInspectionResult(Package package, IReadOnlyCollection<IMetricResult?> metrics)
        {
            _package = package;
            Metrics = metrics;
        }
    }
}
