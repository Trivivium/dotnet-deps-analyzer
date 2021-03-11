namespace App.Inspection.Metrics
{
    public sealed class ScatteringMetricResult : IMetricResult
    {
        public float Percentage;

        internal ScatteringMetricResult(float percentage)
        {
            Percentage = percentage;
        }
    }
}
