using System.Collections.Generic;
using System.Linq;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.FindSymbols;

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

        public void AddPackage(Package package)
        {
            _items.Add(package, new Dictionary<ISymbol, HashSet<ReferenceLocation>>(_comparer));
        }

        public void AddPackageSymbols(Package package, IEnumerable<ReferencedSymbol> symbols)
        {
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

        public Dictionary<ISymbol, HashSet<ReferenceLocation>> Get(Package package)
        {
            return _items[package];
        }

        public IEnumerable<ReferenceLocation> GetReferenceLocationsAcrossSymbols(Package package)
        {
            return _items[package].SelectMany(symbol => symbol.Value);
        }
        
        public int GetUsedTypeCount(Package package)
        {
            return _items[package].Count;
        }
    }
}
