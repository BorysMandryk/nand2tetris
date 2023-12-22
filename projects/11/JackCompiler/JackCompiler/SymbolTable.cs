using System.Collections.Generic;
using System.Linq;

namespace JackCompiler
{
    internal class SymbolTable
    {
        private Dictionary<string, SymbolTableItem> _table;

        public SymbolTable()
        {
            _table = new Dictionary<string, SymbolTableItem>();
        }

        public void Define(string name, VariableKind kind, string type, int index)
        {
            _table[name] = new SymbolTableItem(kind, type, index);
        }

        public int VarCount(VariableKind kind)
        {
            return _table.Count(i => i.Value.Kind == kind);
        }

        public VariableKind KindOf(string name)
        {
            if(_table.TryGetValue(name, out SymbolTableItem item))
            {
                return item.Kind;
            }
            return VariableKind.NONE;
        }

        public string TypeOf(string name)
        {
            return _table[name].Type;
        }

        public int IndexOf(string name)
        {
            return _table[name].Index;
        }

        public bool TryTypeOf(string name, out string type)
        {
            if (_table.TryGetValue(name, out SymbolTableItem item))
            {
                type =  item.Type;
                return true;
            }
            type = default;
            return false;
        }

        public bool TryIndexOf(string name, out int index)
        {
            if (_table.TryGetValue(name, out SymbolTableItem item))
            {
                index = item.Index;
                return true;
            }
            index = default;
            return false;
        }
    }
}
