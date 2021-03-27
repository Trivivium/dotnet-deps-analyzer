namespace App.Inspection.Metrics
{
    public class TransitiveCountMetricResult : IMetricResult
    {
        public readonly int Count;

        public TransitiveCountMetricResult(int count)
        {
            Count = count;
        }
    }
}
