using System.IO;

namespace VMTranslator
{
    internal class VMTranslator
    {
        static void Main(string[] args)
        {
            Translate(args[0]);
        }

        public static void Translate(string path, bool isSingleFile = false)
        {
            FileAttributes attr = File.GetAttributes(path);
            if (attr.HasFlag(FileAttributes.Directory))
            {
                string outputPath = Path.Combine(path, Path.GetFileNameWithoutExtension(path) + ".asm");
                using (CodeWriter codeWriter = new CodeWriter(outputPath))
                {
                    codeWriter.WriteInit();
                    string[] files = Directory.GetFiles(path, "*.vm", SearchOption.AllDirectories);
                    foreach (var f in files)
                    {
                        TranslateFile(f, codeWriter);
                    }
                }
            }
            else
            {
                string outputPath = ChangeFileExtension(path, "asm");
                using (CodeWriter codeWriter = new CodeWriter(outputPath))
                {
                    TranslateFile(path, codeWriter);
                }
            }
        }

        private static void TranslateFile(string path, CodeWriter codeWriter)
        {
            using (Parser parser = new Parser(path))
            {
                codeWriter.SetFileName(Path.GetFileNameWithoutExtension(path));
                while (parser.HasMoreCommands())
                {
                    parser.Advance();
                    CommandType commandType = parser.GetCommandType();
                    string arg1;
                    int arg2;
                    switch (commandType)
                    {
                        case CommandType.C_ARITHMETIC:
                            arg1 = parser.Arg1();
                            codeWriter.WriteArithmetic(arg1);
                            break;
                        case CommandType.C_PUSH:
                            arg1 = parser.Arg1();
                            arg2 = parser.Arg2();
                            codeWriter.WritePush(arg1, arg2);
                            break;
                        case CommandType.C_POP:
                            arg1 = parser.Arg1();
                            arg2 = parser.Arg2();
                            codeWriter.WritePop(arg1, arg2);
                            break;
                        case CommandType.C_LABEL:
                            arg1 = parser.Arg1();
                            codeWriter.WriteLabel(arg1);
                            break;
                        case CommandType.C_GOTO:
                            arg1 = parser.Arg1();
                            codeWriter.WriteGoto(arg1);
                            break;
                        case CommandType.C_IF:
                            arg1 = parser.Arg1();
                            codeWriter.WriteIf(arg1);
                            break;
                        case CommandType.C_FUNCTION:
                            arg1 = parser.Arg1();
                            arg2 = parser.Arg2();
                            codeWriter.WriteFunction(arg1, arg2);
                            break;
                        case CommandType.C_RETURN:
                            codeWriter.WriteReturn();
                            break;
                        case CommandType.C_CALL:
                            arg1 = parser.Arg1();
                            arg2 = parser.Arg2();
                            codeWriter.WriteCall(arg1, arg2);
                            break;
                        default:
                            break;
                    }
                }
            }
        }

        static string ChangeFileExtension(string filename, string newExtension)
        {
            FileAttributes attr = File.GetAttributes(filename);
            if (attr.HasFlag(FileAttributes.Directory))
            {
                return filename + "." + newExtension;
            }

            int index = filename.LastIndexOf('.');
            return filename.Substring(0, index + 1) + newExtension;
        }
    }
}