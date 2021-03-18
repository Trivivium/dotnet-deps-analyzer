using System.Collections.Generic;
using System.Diagnostics;

using App.Inspection.Metrics;
using App.Inspection.Packages;

namespace App.Inspection
{
    [DebuggerDisplay("{GetDebugView(),nq}")]
    public sealed class PackageInspectionResult
    {
        /// <summary>
        /// The package the metrics are calculated for.
        /// </summary>
        public IPackage Package { get; }
        
        /// <summary>
        /// A collection of the results of the metrics that was computed on the package.
        /// </summary>
        public IReadOnlyCollection<IMetricResult?> Metrics { get; }
        
        internal PackageInspectionResult(IPackage package, IReadOnlyCollection<IMetricResult?> metrics)
        {
            Package = package;
            Metrics = metrics;
        }

        public void Deconstruct(out IPackage package, out IReadOnlyCollection<IMetricResult?> metrics)
        {
            package = Package;
            metrics = Metrics;
        }

        private string GetDebugView()
        {
            return $"Name: {Package.Name} - ID: {Package.ID}";
        }
    }
}
