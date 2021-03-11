namespace App.Inspection.Metrics
{
    public sealed class UsageMetricResult : IMetricResult
    {
        public float Percentage { get; }

        internal UsageMetricResult(float percentage)
        {
            Percentage = percentage;
        }
    }
}
