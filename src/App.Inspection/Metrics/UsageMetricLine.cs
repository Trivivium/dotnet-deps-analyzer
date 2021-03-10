using System.Collections.Generic;

namespace App.Inspection.Metrics
{
    public sealed class UsageMetricLine
    {
        public float Percentage { get; }
        
        public ICollection<SourceFileLocation> Locations { get; }
        
        internal UsageMetricLine(float percentage, ICollection<SourceFileLocation> locations)
        {
            Percentage = percentage;
            Locations = locations;
        }
    }
}
