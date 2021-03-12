namespace App.Inspection.Metrics
{
    /// <summary>
    /// The result of the scattering metric. <see cref="ScatteringMetric"/>.
    /// </summary>
    public sealed class ScatteringMetricResult : IMetricResult
    {
        /// <summary>
        /// The percentage of source-files that reference a public member of
        /// a package.
        /// </summary>
        public readonly float Percentage;

        internal ScatteringMetricResult(float percentage)
        {
            Percentage = percentage;
        }
    }
}
