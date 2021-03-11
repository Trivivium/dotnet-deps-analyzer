namespace App.Inspection.Metrics
{
    public sealed class ScatteringMetricResult : IMetricResult
    {
        public int UniqueTypeCount;
        public int UsedTypeCount;
        public float Percentage;

        internal ScatteringMetricResult(int uniqueTypeCount, int usedTypeCount, float percentage)
        {
            UniqueTypeCount = uniqueTypeCount;
            UsedTypeCount = usedTypeCount;
            Percentage = percentage;
        }

        public string GetDisplayName()
        {
            return "Scattering";
        }
    }
}
