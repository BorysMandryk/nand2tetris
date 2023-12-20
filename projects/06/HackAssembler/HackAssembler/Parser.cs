using System.Text.RegularExpressions;

namespace HackAssembler
{
    internal class Parser
    {
        private List<string> _parsedCommands = new List<string>();
        private string _filepath;
        private int _pointer;
        private string _currentCommand;

        private static readonly Regex _whitespaceRegex = new Regex(@"\s+");

        public Parser(string filepath)
        {
            _filepath = filepath;
        }

        public void ParseFileFirstTime(SymbolTable symbolTable)
        {
            using (var streamReader = new StreamReader(_filepath))
            {
                while (!streamReader.EndOfStream)
                {
                    string? line = streamReader.ReadLine();
                    if (IsWhiteSpaceOrComment(line))
                    {
                        continue;
                    }

                    _currentCommand = line;
                    if (GetCommandType() == CommandType.Label)
                    {
                        symbolTable[Label()] = _pointer;
                        continue;
                    }
                    line = RemoveWhiteSpaces(line);
                    line = TrimComment(line);
                    _parsedCommands.Add(line);

                    _pointer++;
                }
            }
            _pointer = 0;
        }

        public bool HasMoreCommands()
        {
            return _pointer < _parsedCommands.Count;
        }

        public void Advance()
        {
            _currentCommand = _parsedCommands[_pointer++];
        }

        private bool IsWhiteSpaceOrComment(string str)
        {
            return string.IsNullOrWhiteSpace(str) || str.StartsWith("//");
        }

        private string RemoveWhiteSpaces(string str)
        {
            return _whitespaceRegex.Replace(str, string.Empty);
        }

        private string TrimComment(string str)
        {
            var index = str.IndexOf("//");
            if (index == -1)
            {
                return str;
            }
            return str.Substring(0, index);
        }

        public CommandType GetCommandType()
        {
            if (_currentCommand.StartsWith("@"))
            {
                return CommandType.A;
            }
            else if (_currentCommand.StartsWith("("))
            {
                return CommandType.Label;
            }
            else
            {
                return CommandType.C;
            }
        }


        public string Value()
        {
            return _currentCommand.Substring(1);
        }

        public string Dest()
        {
            var index = _currentCommand.IndexOf("=");
            if (index == -1)
            {
                return string.Empty;
            }
            return _currentCommand.Substring(0, index);
        }

        public string Comp()
        {
            var startIndex = _currentCommand.IndexOf("=");
            if (startIndex == -1)
            {
                startIndex = 0;
            }
            else
            {
                startIndex++;
            }

            var endIndex = _currentCommand.IndexOf(";");
            if (endIndex == -1)
            {
                endIndex = _currentCommand.Length;
            }
            var length = endIndex - startIndex;
            return _currentCommand.Substring(startIndex, length);
        }

        public string Jump()
        {
            var index = _currentCommand.IndexOf(";");
            if (index == -1)
            {
                return string.Empty;
            }
            return _currentCommand.Substring(index + 1);
        }

        public string Label()
        {
            return _currentCommand.Trim('(', ')');
        }
    }
}
