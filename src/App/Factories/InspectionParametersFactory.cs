using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

using App.Commands;
using App.Inspection;
using App.Inspection.Metrics;

namespace App.Factories
{
    internal static class InspectionParametersFactory
    {
        public static bool TryCreate(InspectCommandArgs args, [NotNullWhen(true)] out InspectionParameters? parameters, [NotNullWhen(false)] out string? message)
        {
            message = null;
            parameters = null;

            if (!TryParseProjects(args.ExcludedProjects, out var projects, out message))
            {
                return false;
            }

            if (!TryParseNamespaces(args.ExcludedNamespaces, out var namespaces, out message))
            {
                return false;
            }
            
            if (!TryParseMetrics(args.Metrics, out var metrics, out message))
            {
                return false;
            }

            if (!TryParseMaxConcurrency(args.MaxConcurrency, out var maxConcurrency, out message))
            {
                return false;
            }

            parameters = InspectionParameters.CreateWithDefaults(projects, namespaces, metrics, maxConcurrency);

            return true;
        }

        private static bool TryParseProjects(string? value, [NotNullWhen(true)] out IEnumerable<string>? projects, [NotNullWhen(false)] out string? message)
        {
            const StringSplitOptions options = StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries;

            message = null;
            projects = value?.Split(',', options) ?? Enumerable.Empty<string>();

            return true;
        }

        private static bool TryParseNamespaces(string? value, [NotNullWhen(true)] out IEnumerable<string>? namespaces, [NotNullWhen(false)] out string? message)
        {
            const StringSplitOptions options = StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries;

            message = null;
            namespaces = value?.Split(',', options) ?? Enumerable.Empty<string>();

            return true;
        }
        
        private static bool TryParseMetrics(string? value, [NotNullWhen(true)] out ICollection<MetricName>? metrics, [NotNullWhen(false)] out string? message)
        {
            const StringSplitOptions options = StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries;
            
            if (string.IsNullOrWhiteSpace(value))
            {
                value = null;
            }

            message = null;
            metrics = new List<MetricName>();

            if (value != null)
            {
                foreach (var metric in value.Split(',', options))
                {
                    if (!Enum.TryParse<MetricName>(metric, ignoreCase: true, out var parsed))
                    {
                        message = $"Unknown metric: '{metric}'. Available options: {string.Join(", ", Enum.GetNames<MetricName>())}";
                        
                        return false;
                    }
                    
                    metrics.Add(parsed);
                }
            }

            return true;
        }

        private static bool TryParseMaxConcurrency(int value, out int maxConcurrency, [NotNullWhen(false)] out string? message)
        {
            message = null;
            maxConcurrency = 1;
            
            if (value < 1)
            {
                message = "The provided max concurrency cannot be less than 1";

                return false;
            }

            if (!Debugger.IsAttached)
            {
                maxConcurrency = value;
            }
            
            return true;
        }
    }
}
