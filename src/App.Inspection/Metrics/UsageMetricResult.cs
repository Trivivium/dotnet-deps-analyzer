using System.Collections.Generic;

namespace App.Inspection.Metrics
{
    public sealed class UsageMetricResult : IMetricResult
    {
        public float Percentage { get; }
        
        public IReadOnlyCollection<SourceFileLocation> Locations { get; }

        internal UsageMetricResult(float percentage, IReadOnlyCollection<SourceFileLocation> locations)
        {
            Percentage = percentage;
            Locations = locations;
        }
        
        public string GetDisplayName()
        {
            return "Usage";
        }
    }
}
