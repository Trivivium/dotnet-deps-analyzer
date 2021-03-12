using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;

using Microsoft.CodeAnalysis.MSBuild;

using App.Inspection.Metrics;
using App.Inspection.Extensions;

namespace App.Inspection
{
    internal class InspectionContext
    {
        private readonly IDictionary<string, Func<IMetric>> _factories = new Dictionary<string, Func<IMetric>>
        {
            { "USAGE",      () => new UsageMetric() },
            { "SCATTERING", () => new ScatteringMetric() }
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
                    .Where(kvp => parameters.Metrics.Contains(kvp.Key, StringComparer.OrdinalIgnoreCase))
                    .Select(kvp => kvp.Value);
                
                foreach (var factory in selected)
                {
                    yield return factory();
                }
            }
        }
    }
}
