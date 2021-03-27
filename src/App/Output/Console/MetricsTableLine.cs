using System;

using NuGet.Versioning;

using App.Inspection.Packages;

namespace App.Output.Console
{
    internal sealed class MetricsTableLine
    {
        public Guid ID { get; }
        public int Depth { get; }
        public string Package { get; }
        public PackageReferenceType PackageType { get; }
        public SemanticVersion PackageVersion { get; }
        public float? Usage { get; set; }
        public float? Scatter { get; set; }
        public int? TransientCount { get; set; }

        public MetricsTableLine(Guid id, int depth, string name, PackageReferenceType type, SemanticVersion version)
        {
            ID = id;
            Depth = depth;
            Package = name;
            PackageType = type;
            PackageVersion = version;
        }
    }
}
