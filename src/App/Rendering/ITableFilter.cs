using App.Inspection.Packages;

namespace App.Rendering
{
    public interface ITableFilter
    {
        public bool IsLineExcluded(IPackage package);
    }
}
