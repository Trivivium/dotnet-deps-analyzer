namespace App.Inspection.Metrics
{
    /// <summary>
    /// The result of the usage metric. <see cref="UsageMetric"/>.
    /// </summary>
    public sealed class UsageMetricResult : IMetricResult
    {
        /// <summary>
        /// The percentage of public members of a package used
        /// by a project.
        /// </summary>
        public readonly float Percentage;

        internal UsageMetricResult(float percentage)
        {
            Percentage = percentage;
        }
    }
}
