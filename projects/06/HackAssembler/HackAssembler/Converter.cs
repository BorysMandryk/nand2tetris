namespace HackAssembler
{
    internal class Converter
    {
        private Dictionary<string, string> _compTable = new Dictionary<string, string>
        {
            { "0", "0101010"},
            { "1", "0111111"},
            { "-1", "0111010"},
            { "D", "0001100"},
            { "A", "0110000"},
            { "M", "1110000"},
            { "!D", "0001101"},
            { "!A", "0110001"},
            { "!M", "1110001"},
            { "-D", "0001111"},
            { "-A", "0110011"},
            { "-M", "1110011"},
            { "D+1", "0011111"},
            { "A+1", "0110111"},
            { "M+1", "1110111"},
            { "D-1", "0001110"},
            { "A-1", "0110010"},
            { "M-1", "1110010"},
            { "D+A", "0000010"},
            { "D+M", "1000010"},
            { "D-A", "0010011"},
            { "D-M", "1010011"},
            { "A-D", "0000111"},
            { "M-D", "1000111"},
            { "D&A", "0000000"},
            { "D&M", "1000000"},
            { "D|A", "0010101"},
            { "D|M", "1010101"}
        };

        private Dictionary<string, string> _jumpTable = new Dictionary<string, string>
        {
            { "", "000"},
            { "JGT", "001"},
            { "JEQ", "010"},
            { "JGE", "011"},
            { "JLT", "100"},
            { "JNE", "101"},
            { "JLE", "110"},
            { "JMP", "111"}
        };


        public Converter()
        {
        }

        public string Value(string command, SymbolTable symbolTable)
        {
            if (int.TryParse(command, out int v))
            {
                return Convert.ToString(v, 2).PadLeft(15, '0');
            }

            if (symbolTable.TryGetValue(command, out int sv))
            {
                return Convert.ToString(sv, 2).PadLeft(15, '0');
            }

            int value = symbolTable.AddVariable(command);
            return Convert.ToString(value, 2).PadLeft(15, '0');
        }

        public string Dest(string command)
        {
            char[] instruction = new char[] { '0', '0', '0' };

            if (command.Contains('A'))
            {
                instruction[0] = '1';
            }
            if (command.Contains('D'))
            {
                instruction[1] = '1';
            }
            if (command.Contains('M'))
            {
                instruction[2] = '1';
            }
            return new string(instruction);
        }

        public string Comp(string command)
        {
            return _compTable[command];
        }

        public string Jump(string command)
        {
            return _jumpTable[command];
        }

        public string Label(string label, SymbolTable symbolTable)
        {
            return Convert.ToString(symbolTable[label], 2);
        }
    }
}
