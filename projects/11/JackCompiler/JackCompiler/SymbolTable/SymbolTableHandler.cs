using System;
using System.Collections.Generic;

namespace JackCompiler.SymbolTable
{
    internal class SymbolTableHandler
    {
        private SymbolTable _classTable;
        private SymbolTable _subroutineTable;

        public SymbolTableHandler()
        {
            _classTable = new SymbolTable();
        }

        public void StartSubroutine()
        {
            _subroutineTable = new SymbolTable();
        }

        public void Define(string name, VariableKind kind, string type)
        {
            if (kind == VariableKind.STATIC || kind == VariableKind.FIELD)
            {
                _classTable.Define(name, kind, type, VarCount(kind));
                return;
            }
            if (kind == VariableKind.ARG || kind == VariableKind.VAR)
            {
                _subroutineTable.Define(name, kind, type, VarCount(kind));
                return;
            }

            throw new ArgumentException($"Cannot define variable of kind {kind}");

        }

        public int VarCount(VariableKind kind)
        {
            if (kind == VariableKind.STATIC || kind == VariableKind.FIELD)
            {
                return _classTable.VarCount(kind);
            }

            if (kind == VariableKind.ARG || kind == VariableKind.VAR)
            {
                return _subroutineTable.VarCount(kind);
            }

            throw new ArgumentException($"Unsupported variables of kind {kind}");
        }

        public VariableKind KindOf(string name)
        {
            VariableKind kind = _subroutineTable.KindOf(name);
            if (kind != VariableKind.NONE)
            {
                return kind;
            }

            kind = _classTable.KindOf(name);
            return kind;
        }

        public string TypeOf(string name)
        {
            string type;
            if (_subroutineTable.TryTypeOf(name, out type))
            {
                return type;
            }
            if (_classTable.TryTypeOf(name, out type))
            {
                return type;
            }

            throw new KeyNotFoundException($"Variable with the specified name is not found: {name}");
        }

        public int IndexOf(string name)
        {
            int index;
            if (_subroutineTable.TryIndexOf(name, out index))
            {
                return index;
            }
            if (_classTable.TryIndexOf(name, out index))
            {
                return index;
            }

            throw new KeyNotFoundException($"Variable with the specified name is not found: {name}");
        }
    }
}
