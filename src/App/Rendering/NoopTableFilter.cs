using App.Inspection.Packages;

namespace App.Rendering
{
    public class NoopTableFilter : ITableFilter
    {
        public bool IsLineExcluded(IPackage package)
        {
            return false;
        }
    }
}
