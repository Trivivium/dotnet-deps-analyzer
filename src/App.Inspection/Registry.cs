using System.Linq;
using System.Collections.Generic;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.FindSymbols;

using App.Inspection.Packages;

namespace App.Inspection
{
    internal class Registry
    {
        private readonly Dictionary<Package, Dictionary<ISymbol, HashSet<ReferenceLocation>>> _items;
        private readonly IEqualityComparer<ISymbol?> _comparer;

        public Registry()
        {
            _items = new Dictionary<Package, Dictionary<ISymbol, HashSet<ReferenceLocation>>>();
            _comparer = SymbolEqualityComparer.Default;
        }

        /// <summary>
        /// Adds a <paramref name="package"/> to the registry.
        /// </summary>
        /// <param name="package">The package to group usage of members by.</param>
        public void AddPackage(PackageExecutableLoaded package)
        {
            _items.Add(package, new Dictionary<ISymbol, HashSet<ReferenceLocation>>(_comparer));
        }

        /// <summary>
        /// Adds a collection of <paramref name="symbols"/> used by the specified <paramref name="package"/> to
        /// the registry. This operations filters out all symbols with no locations, and de-duplicates the
        /// symbols and locations. 
        /// </summary>
        /// <param name="package">The package to assign the symbols to.</param>
        /// <param name="symbols">The collection of symbols to add.</param>
        public void AddPackageSymbols(PackageExecutableLoaded package, IEnumerable<ReferencedSymbol> symbols)
        {
            if (!_items.ContainsKey(package))
            {
                AddPackage(package);
            }
            
            if (!_items.TryGetValue(package, out var locations))
            {
                locations = new Dictionary<ISymbol, HashSet<ReferenceLocation>>(_comparer);

                _items[package] = locations;
            }

            foreach (var symbol in symbols)
            {
                if (!symbol.Locations.Any())
                {
                    continue;
                }
                
                if (!locations.ContainsKey(symbol.Definition))
                {
                    locations.Add(symbol.Definition, new HashSet<ReferenceLocation>());
                }

                foreach (var location in symbol.Locations)
                {
                    locations[symbol.Definition].Add(location);
                }
            }
        }

        /// <summary>
        /// Gets a collection of locations in the project's source-files, where a
        /// member of the specified <paramref name="package"/> is used.
        /// </summary>
        /// <param name="package">The package to filter by.</param>
        public IEnumerable<ReferenceLocation> GetReferenceLocationsAcrossSymbols(Package package)
        {
            return _items[package].SelectMany(symbol => symbol.Value);
        }
        
        /// <summary>
        /// Gets the number of members used from the specified <paramref name="package"/>.
        /// </summary>
        /// <param name="package">The package to filter by.</param>
        public int GetUsedTypeCount(Package package)
        {
            return _items[package].Count;
        }
    }
}
