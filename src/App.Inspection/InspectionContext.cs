using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;

using Microsoft.CodeAnalysis.MSBuild;

using App.Inspection.Metrics;
using App.Inspection.Extensions;

namespace App.Inspection
{
    internal delegate IMetric MetricFactory(ILogger logger, InspectionParameters parameters);
    
    internal class InspectionContext
    {
        private readonly IDictionary<string, MetricFactory> _factories = new Dictionary<string, MetricFactory>
        {
            { "USAGE",      (_, parameters) => new UsageMetric(parameters) },
            { "SCATTERING", (_, _)          => new ScatteringMetric() }
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
        
        public IReadOnlyCollection<IMetric> CreateMetricInstances(ILogger logger, InspectionParameters parameters)
        {
            var instances = new List<IMetric>(_factories.Count);
            
            if (parameters.Metrics.IsEmpty())
            {
                foreach (var factory in _factories.Values)
                {
                    instances.Add(factory(logger, parameters));
                }
            }
            else
            {
                var selected = _factories
                    .Where(kvp => parameters.Metrics.Contains(kvp.Key, StringComparer.OrdinalIgnoreCase))
                    .Select(kvp => kvp.Value);
                
                foreach (var factory in selected)
                {
                    instances.Add(factory(logger, parameters));
                }
            }

            return instances;
        }
    }
}
