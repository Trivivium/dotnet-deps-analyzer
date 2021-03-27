using App.Inspection.Packages;

namespace App.Output.Filters
{
    internal interface IResultsFilter
    {
        public bool IsLineExcluded(IPackage package);
    }
}
