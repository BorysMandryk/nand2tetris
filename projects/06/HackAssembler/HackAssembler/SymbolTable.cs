namespace HackAssembler
{
    internal class SymbolTable : Dictionary<string, int>
    {
        private int _currentRegister = 16;

        public int AddVariable(string key)
        {
            this[key] = _currentRegister;
            return _currentRegister++;
        }
    }
}
