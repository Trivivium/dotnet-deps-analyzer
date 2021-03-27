using App.Inspection.Packages;

namespace App.Output.Filters
{
    internal sealed class NoopResultsFilter : IResultsFilter
    {
        public bool IsLineExcluded(IPackage package)
        {
            return false;
        }
    }
}
