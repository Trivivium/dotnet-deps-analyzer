using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;

using Microsoft.CodeAnalysis.MSBuild;

using App.Inspection.Metrics;

namespace App.Inspection
{
    internal class InspectionContext
    {
        private readonly IDictionary<MetricName, Func<IMetric>> _factories = new Dictionary<MetricName, Func<IMetric>>
        {
            { MetricName.Usage, () => new UsageMetric() },
            { MetricName.Scattering, () => new ScatteringMetric() },
            { MetricName.TransitiveCount, () => new TransitiveCountMetric() }
        };
        
        public readonly FileSystemInfo File;
        public readonly MSBuildWorkspace Workspace;
        public readonly InspectionParameters Parameters;

        public InspectionContext(FileSystemInfo file, MSBuildWorkspace workspace, InspectionParameters parameters)
        {
            File = file;
            Workspace = workspace;
            Parameters = parameters;
        }
        
        /// <summary>
        /// Instantiates all metrics specified by the <paramref name="parameters"/>.
        /// </summary>
        /// <param name="parameters">The inspection parameters provided by the user.</param>
        public IEnumerable<IMetric> CreateMetricInstances(InspectionParameters parameters)
        {
            if (parameters.Metrics.Count < 1)
            {
                foreach (var factory in _factories.Values)
                {
                    yield return factory();
                }
            }
            else
            {
                var selected = _factories
                    .Where(kvp => parameters.Metrics.Contains(kvp.Key))
                    .Select(kvp => kvp.Value);
                
                foreach (var factory in selected)
                {
                    yield return factory();
                }
            }
        }
    }
}
