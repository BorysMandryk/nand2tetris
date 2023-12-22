namespace JackCompiler
{
    internal class SymbolTableItem
    {
        public VariableKind Kind { get; set; }
        public string Type { get; set; }
        public int Index { get; set; }

        public SymbolTableItem(VariableKind kind, string type, int index)
        {
            Kind = kind;
            Type = type;
            Index = index;
        }
    }
}
