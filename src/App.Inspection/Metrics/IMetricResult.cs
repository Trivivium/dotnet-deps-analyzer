namespace App.Inspection.Metrics
{
    public interface IMetricResult
    {
        /// <summary>
        /// Gets the name of the metric for display purposes.
        /// </summary>
        public string GetDisplayName();
    }
}
