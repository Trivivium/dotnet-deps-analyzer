namespace App.Inspection.Metrics
{
    public sealed class ScatteringMetricResult : IMetricResult
    {
        internal ScatteringMetricResult()
        { }

        public string GetDisplayName()
        {
            return "Scattering";
        }
    }
}
