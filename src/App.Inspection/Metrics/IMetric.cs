using Microsoft.CodeAnalysis;

using App.Inspection.Packages;

namespace App.Inspection.Metrics
{
    /// <summary>
    /// Declares a metric to compute on a package of a project.
    /// </summary>
    internal interface IMetric
    {
        /// <summary>
        /// Computes the metric result.
        /// </summary>
        /// <param name="project">The project being inspected.</param>
        /// <param name="compilation">The Roslyn compilation of the project.</param>
        /// <param name="package">The package to generate metrics for.</param>
        /// <param name="registry">The registry of information collected about the project.</param>
        public IMetricResult Compute(Project project, Compilation compilation, PackageExecutableLoaded package, Registry registry);
    }
}
