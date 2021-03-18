namespace App.Inspection.Metrics
{
    public class TransientCountMetricResult : IMetricResult
    {
        public readonly int Count;

        public TransientCountMetricResult(int count)
        {
            Count = count;
        }
    }
}
