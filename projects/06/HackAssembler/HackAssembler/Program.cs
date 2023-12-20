namespace HackAssembler
{
    internal class Program
    {
        static void Main(string[] args)
        {
            const string baseDir = @"D:\nand2tetris\projects\06";
            string[] files = Directory.GetFiles(baseDir, "*.asm", SearchOption.AllDirectories);

            foreach (var f in files)
            {
                string outputFile = ReplaceFileExtension(f, "hack");
                Assemble(f, outputFile);
            }
        }

        private static void Assemble(string inputPath, string outputPath)
        {
            var parser = new Parser(inputPath);
            var symbolTable = new SymbolTable
            {
                { "R0", 0},
                { "R1", 1},
                { "R2", 2},
                { "R3", 3},
                { "R4", 4},
                { "R5", 5},
                { "R6", 6},
                { "R7", 7},
                { "R8", 8},
                { "R9", 9},
                { "R10", 10},
                { "R11", 11},
                { "R12", 12},
                { "R13", 13},
                { "R14", 14},
                { "R15", 15},
                { "SCREEN", 16384},
                { "KBD", 24576},
                { "SP", 0},
                { "LCL", 1},
                { "ARG", 2},
                { "THIS", 3},
                { "THAT", 4},
            };

            var converter = new Converter();

            parser.ParseFileFirstTime(symbolTable);
            using (StreamWriter outputFile = new StreamWriter(outputPath))
            {
                while (parser.HasMoreCommands())
                {
                    parser.Advance();
                    var commandType = parser.GetCommandType();

                    var instruction = "";
                    switch (commandType)
                    {
                        case CommandType.A:
                            var v = parser.Value();
                            var vv = converter.Value(v, symbolTable);
                            instruction = "0" + vv;
                            break;
                        case CommandType.C:
                            var d = parser.Dest();
                            var c = parser.Comp();
                            var j = parser.Jump();

                            var dd = converter.Dest(d);
                            var cc = converter.Comp(c);
                            var jj = converter.Jump(j);

                            instruction = "111" + cc + dd + jj;
                            break;
                        case CommandType.Label:
                            break;
                        default:
                            break;
                    }
                    outputFile.WriteLine(instruction);
                }
            }
        }

        private static string ReplaceFileExtension(string filename, string newExtension)
        {
            var index = filename.LastIndexOf(".");
            return filename.Substring(0, index + 1) + newExtension;
        }
    }
}