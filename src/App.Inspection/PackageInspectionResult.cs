using System.Collections.Generic;

using App.Inspection.Metrics;

namespace App.Inspection
{
    public sealed class PackageInspectionResult
    {
        private readonly Package _package;
        
        public IReadOnlyCollection<IMetricResult> Metrics { get; }

        public string Name => _package.DisplayName;

        internal PackageInspectionResult(Package package, IReadOnlyCollection<IMetricResult> metrics)
        {
            _package = package;
            Metrics = metrics;
        }
    }
}
