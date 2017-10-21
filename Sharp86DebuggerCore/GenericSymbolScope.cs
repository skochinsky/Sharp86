using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sharp86
{
    public class GenericSymbolScope : ISymbolScope
    {
        public Symbol ResolveSymbol(string name)
        {
            Symbol sym;
            if (_symbols.TryGetValue(name, out sym))
                return sym;
            return null;
        }

        public void RegisterSymbol(string name, Func<object> callback)
        {
            _symbols.Add(name, new CallbackSymbol(callback));
        }

        Dictionary<string, Symbol> _symbols = new Dictionary<string, Symbol>(StringComparer.InvariantCultureIgnoreCase);
    }
}
