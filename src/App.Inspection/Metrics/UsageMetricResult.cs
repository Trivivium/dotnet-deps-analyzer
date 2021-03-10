namespace App.Inspection.Metrics
{
    public sealed class UsageMetricResult : IMetricResult
    {
        internal UsageMetricResult()
        { }
        
        public string GetDisplayName()
        {
            return "Usage";
        }
    }
}
