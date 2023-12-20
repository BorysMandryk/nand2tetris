using System.Text;
using System.Xml;
using System.Xml.Linq;

namespace JackAnalizer
{
    internal class Program
    {
        static void Main(string[] args)
        {
            const string baseDir = @"D:\nand2tetris\projects\10\Tests";
            string filename = "Square";
            string path = Path.Combine(baseDir, filename);

            //Tokenize(path);
            Compile(path);
        }

        static void Compile(string path)
        {
            if (File.GetAttributes(path) == FileAttributes.Directory)
            {
                string[] files = Directory.GetFiles(path, "*.jack", SearchOption.AllDirectories);
                if (files.Length == 0)
                {
                    throw new ArgumentException("The directory must contain files with extension .jack");
                }

                foreach (string f in files)
                {
                    CompileFile(f);
                }
            }
            else if (Path.GetExtension(path) == ".jack")
            {
                CompileFile(path);
            }
            else
            {
                throw new ArgumentException("The path must be to a directory or a file with extension .jack");
            }
        }

        static void CompileFile(string path)
        {
            string dir = Path.GetDirectoryName(path);
            string filename = Path.GetFileNameWithoutExtension(path);
            string outputPath = Path.Combine(dir, $"My{filename}.xml");
            using CompilationEngine engine = new CompilationEngine(path, outputPath);
        }

        static void Tokenize(string path)
        {
            if (File.GetAttributes(path) == FileAttributes.Directory)
            {
                string[] files = Directory.GetFiles(path, "*.jack", SearchOption.AllDirectories);
                if (files.Length == 0)
                {
                    throw new ArgumentException("The directory must contain files with extension .jack");
                }

                foreach (string f in files)
                {
                    TokenizeFile(f);
                }
            }
            else if (Path.GetExtension(path) == ".jack")
            {
                TokenizeFile(path);
            }
            else
            {
                throw new ArgumentException("The path must be to a directory or a file with extension .jack");
            }
        }

        static void TokenizeFile(string path)
        {
            string dir = Path.GetDirectoryName(path);
            string filename = Path.GetFileNameWithoutExtension(path);
            string outputPath = Path.Combine(dir, $"My{filename}T.xml");

            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Indent = true;
            settings.IndentChars = "";
            settings.OmitXmlDeclaration = true;
            settings.Encoding = new UTF8Encoding(false);
            using (XmlWriter xmlWriter = XmlWriter.Create(outputPath, settings))
            using (JackTokenizer tokenizer = new JackTokenizer(path))
            {
                xmlWriter.WriteStartElement("tokens");

                while (tokenizer.Advance())
                {
                    TokenType tokenType = tokenizer.GetTokenType();
                    XElement element;

                    switch (tokenType)
                    {
                        case TokenType.KEYWORD:
                            Keyword keyword = tokenizer.GetKeyword();
                            element = new XElement("keyword", " " + tokenizer.GetKeywordString(keyword) + " ");
                            element.WriteTo(xmlWriter);
                            Console.WriteLine(element);
                            break;
                        case TokenType.SYMBOL:
                            char symbol = tokenizer.GetSymbol();
                            element = new XElement("symbol", " " + symbol + " ");
                            element.WriteTo(xmlWriter);
                            Console.WriteLine(element);
                            break;
                        case TokenType.IDENTIFIER:
                            string identifier = tokenizer.GetIdentifier();
                            element = new XElement("identifier", " " + identifier + " ");
                            element.WriteTo(xmlWriter);
                            Console.WriteLine(element);
                            break;
                        case TokenType.INT_CONST:
                            int intConst = tokenizer.GetIntValue();
                            element = new XElement("integerConstant", " " + intConst + " ");
                            element.WriteTo(xmlWriter);
                            Console.WriteLine(element);
                            break;
                        case TokenType.STRING_CONST:
                            string stringConst = tokenizer.GetStringValue();
                            element = new XElement("stringConstant", " " + stringConst + " ");
                            element.WriteTo(xmlWriter);
                            Console.WriteLine(element);
                            break;
                        default:
                            break;
                    }
                }
                xmlWriter.WriteFullEndElement();
            }
        }
    }
}