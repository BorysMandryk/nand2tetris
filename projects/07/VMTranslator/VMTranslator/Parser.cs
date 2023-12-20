using System;
using System.IO;
using System.Linq;

namespace VMTranslator
{
    internal class Parser : IDisposable
    {
        private StreamReader _streamReader;
        private bool _disposed = false;
        private string _currentCommand;
        private string? _nextCommand;

        private static readonly string[] _arithmeticCommands = new string[]
                {
            "add", "sub", "neg", "eq", "gt", "lt", "and", "or", "not"
                };

        public Parser(string path)
        {
            _streamReader = new StreamReader(path);
            _nextCommand = GetNextCommand();
        }

        public bool HasMoreCommands()
        {
            return _nextCommand != null;
        }

        public void Advance()
        {
            _currentCommand = _nextCommand;
            _nextCommand = GetNextCommand();
        }

        private string? GetNextCommand()
        {
            string? line;
            do
            {
                line = _streamReader.ReadLine();
                if (line == null)
                {
                    return null;
                }
            } while (string.IsNullOrWhiteSpace(line) || IsComment(line));

            line = RemoveComment(line);
            line = line.TrimEnd();
            return line;
        }

        private bool IsComment(string str)
        {
            return str.TrimStart().StartsWith("//");
        }

        private string RemoveComment(string str)
        {
            int index = str.IndexOf("//");
            if (index == -1)
            {
                return str;
            }
            return str[..index];
        }

        public CommandType GetCommandType()
        {
            int index = _currentCommand.IndexOf(' ');
            string command = _currentCommand;
            if (index != -1)
            {
                command = _currentCommand[..index];
            }

            if (_arithmeticCommands.Contains(command))
            {
                return CommandType.C_ARITHMETIC;
            }
            else if (command == "pop")
            {
                return CommandType.C_POP;
            }
            else if (command == "push")
            {
                return CommandType.C_PUSH;
            }
            else if (command == "label")
            {
                return CommandType.C_LABEL;
            }
            else if (command == "goto")
            {
                return CommandType.C_GOTO;
            }
            else if (command == "if-goto")
            {
                return CommandType.C_IF;
            }
            else if (command == "function")
            {
                return CommandType.C_FUNCTION;
            }
            else if (command == "call")
            {
                return CommandType.C_CALL;
            }
            else if (command == "return")
            {
                return CommandType.C_RETURN;
            }
            else
            {
                throw new Exception("Line does not contain a command");
            }
        }

        public string Arg1()
        {
            string[] args = _currentCommand.Split(' ');
            if (args.Length == 1)
            {
                if (GetCommandType() == CommandType.C_RETURN)
                {
                    throw new Exception("Unexpected CommandType.C_RETURN");
                }
                return args[0];
            }
            else if (args.Length > 1)
            {
                return args[1];
            }
            else
            {
                throw new Exception("Line is empty");
            }
        }

        public int Arg2()
        {
            string[] args = _currentCommand.Split(' ');

            if (args.Length == 3)
            {
                if (int.TryParse(args[2], out int res))
                {
                    return res;
                }
                throw new ArgumentException("Invalid argument type");
            }
            else
            {
                throw new Exception("Unsupported CommandType");
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                _streamReader.Close();
            }
            _disposed = true;
        }

        ~Parser()
        {
            Dispose(false);
        }
    }
}
