using System;
using System.IO;

namespace VMTranslator
{
    internal class CodeWriter : IDisposable
    {
        private StreamWriter _streamWriter;
        private string _filename;
        private string _functionName;
        private int _retCnt;
        private int _eqCnt;
        private int _gtCnt;
        private int _ltCnt;

        private bool _disposed = false;

        public CodeWriter(string path)
        {
            _streamWriter = new StreamWriter(path);
        }

        public void SetFileName(string filename)
        {
            _filename = filename;
            _functionName = _filename;
        }

        public void WriteArithmetic(string command)
        {
            switch (command)
            {
                case "add":
                    WriteAdd();
                    break;
                case "sub":
                    WriteSub();
                    break;
                case "neg":
                    WriteNeg();
                    break;
                case "eq":
                    WriteEq();
                    break;
                case "gt":
                    WriteGt();
                    break;
                case "lt":
                    WriteLt();
                    break;
                case "and":
                    WriteAnd();
                    break;
                case "or":
                    WriteOr();
                    break;
                case "not":
                    WriteNot();
                    break;
                default:
                    throw new ArgumentException("Unknown command");
            }
            _streamWriter.WriteLine(_streamWriter.NewLine);
        }

        public void WritePush(string segment, int index)
        {
            string str = $@"// push {segment} {index}
";
            switch (segment)
            {
                case "local":
                    str += $@"@LCL
D=M
@{index}
A=D+A
D=M";
                    break;
                case "argument":
                    str += $@"@ARG
D=M
@{index}
A=D+A
D=M";
                    break;
                case "this":
                    str += $@"@THIS
D=M
@{index}
A=D+A
D=M";
                    break;
                case "that":
                    str += $@"@THAT
D=M
@{index}
A=D+A
D=M";
                    break;
                case "constant":
                    str += $@"@{index}
D=A";
                    break;
                case "static":
                    str += $@"@{_filename}.{index}
D=M";
                    break;
                case "temp":
                    str += $@"@5
D=A
@{index}
A=D+A
D=M";
                    break;
                case "pointer":
                    if (index == 0)
                    {
                        str += $@"@THIS
D=M";
                    }
                    else if (index == 1)
                    {
                        str += $@"@THAT
D=M";
                    }
                    else
                    {
                        throw new ArgumentException("Index for pointer must be 0 or 1");
                    }
                    break;
                default:
                    throw new ArgumentException("Unknown command");
            }
            str += $@"
@SP
A=M
M=D
@SP
M=M+1";
            _streamWriter.Write(str);
            _streamWriter.WriteLine(_streamWriter.NewLine);
        }

        public void WritePop(string segment, int index)
        {
            string str = $@"// pop {segment} {index}
@SP
AM=M-1
D=M
";
            switch (segment)
            {
                case "local":
                    str += PopValue("LCL", index);
                    break;
                case "argument":
                    str += PopValue("ARG", index);
                    break;
                case "this":
                    str += PopValue("THIS", index);
                    break;
                case "that":
                    str += PopValue("THAT", index);
                    break;
                case "constant":
                    throw new InvalidOperationException("Unsupported command for constant");
                case "static":
                    str += $@"@{_filename}.{index}
M=D";
                    break;
                case "temp":
                    str += PopValue("5", index, true);
                    break;
                case "pointer":
                    if (index == 0)
                    {
                        str += $@"@THIS
M=D";
                    }
                    else if (index == 1)
                    {
                        str += $@"@THAT
M=D";
                    }
                    else
                    {
                        throw new ArgumentException("Index for pointer must be 0 or 1");
                    }
                    break;
                default:
                    throw new ArgumentException("Unknown command");
            }

            _streamWriter.Write(str);
            _streamWriter.WriteLine(_streamWriter.NewLine);
        }

        public void WriteInit()
        {
            string str = @"@256
D=A
@SP
M=D";
            _streamWriter.WriteLine(str);
            WriteCall("Sys.init", 0);
        }

        public void WriteLabel(string label)
        {
            string str = @$"// label {label}
({_functionName}${label})";
            _streamWriter.WriteLine(str + _streamWriter.NewLine);
        }

        public void WriteGoto(string label)
        {
            string str = $@"// goto {label}
@{_functionName}${label}
0;JMP";
            _streamWriter.WriteLine(str + _streamWriter.NewLine);
        }

        public void WriteIf(string label)
        {
            string str = @$"// if-goto
@SP
AM=M-1
D=M
@{_functionName}${label}
D;JNE";
            _streamWriter.WriteLine(str + _streamWriter.NewLine);
        }

        public void WriteFunction(string functionName, int numVars)
        {
            _functionName = functionName;
            _retCnt = 0;

            string str = $@"// function {functionName} {numVars}
({functionName})
@SP
A=M";
            for (int i = 0; i < numVars; i++)
            {
                str += @"
M=0
@SP
AM=M+1";
            }
            _streamWriter.WriteLine(str + _streamWriter.NewLine);

        }

        public void WriteCall(string functionName, int numArgs)
        {
            string str = $@"// call {functionName}
{PushValue(GetReturnAddress(), true)}
{PushValue("LCL")}
{PushValue("ARG")}
{PushValue("THIS")}
{PushValue("THAT")}
@SP
D=M
@LCL
M=D
@{numArgs + 5}
D=D-A
@ARG
M=D
@{functionName}
0;JMP
({GetReturnAddress()})";
            _streamWriter.WriteLine(str);
            _retCnt++;
        }

        public void WriteReturn()
        {
            _streamWriter.WriteLine("// return");

            string RestoreVariable(string variable)
            {
                return $@"@LCL
AM=M-1
D=M
@{variable}
M=D";
            }

            _streamWriter.WriteLine(@"@LCL
D=M
@5
A=D-A
D=M
@R13
M=D");
            WritePop("argument", 0);
            string str = $@"@ARG
D=M
@SP
M=D+1
{RestoreVariable("THAT")}
{RestoreVariable("THIS")}
{RestoreVariable("ARG")}
{RestoreVariable("LCL")}
@R13
A=M
0;JMP";
            _streamWriter.WriteLine(str + _streamWriter.NewLine);
        }

        private string PushValue(string value, bool isConstant = false)
        {
            return $@"@{value}
D={(isConstant ? 'A' : 'M')}
@SP
A=M
M=D
@SP
M=M+1";
        }

        private string PopValue(string address, int index, bool isConstant = false)
        {
            return $@"@{address}
D=D+{(isConstant ? 'A' : 'M')}
@{index}
D=D+A
@SP
A=M
A=M
A=D-A
M=D-A";
        }

        private string GetReturnAddress()
        {
            return $"{_functionName}$ret.{_retCnt}";
        }

        #region WriteArithmeticCommands
        private void WriteAdd()
        {
            string str = @"// add
@SP
AM=M-1
D=M
A=A-1
M=M+D";
            _streamWriter.Write(str);
        }

        private void WriteSub()
        {
            string str = @"// sub
@SP
AM=M-1
D=M
A=A-1
M=M-D";
            _streamWriter.Write(str);
        }

        private void WriteNeg()
        {
            string str = @"// neg
@SP
A=M-1
M=-M";
            _streamWriter.Write(str);
        }

        private void WriteEq()
        {
            string str = $@"// eq
@SP
AM=M-1
D=M
A=A-1
D=M-D
@EQUAL_{_eqCnt}
D;JEQ
@SP
A=M-1
M=0
@EQ_END_{_eqCnt}
0;JMP
(EQUAL_{_eqCnt})
@SP
A=M-1
M=-1
(EQ_END_{_eqCnt})";
            _eqCnt++;
            _streamWriter.Write(str);
        }

        private void WriteGt()
        {
            string str = $@"// gt
@SP
AM=M-1
D=M
A=A-1
D=M-D
@GREATER_{_gtCnt}
D;JGT
@SP
A=M-1
M=0
@GT_END_{_gtCnt}
0;JMP
(GREATER_{_gtCnt})
@SP
A=M-1
M=-1
(GT_END_{_gtCnt})";
            _gtCnt++;
            _streamWriter.Write(str);
        }

        private void WriteLt()
        {
            string str = $@"// lt
@SP
AM=M-1
D=M
A=A-1
D=M-D
@LESS_{_ltCnt}
D;JLT
@SP
A=M-1
M=0
@LT_END_{_ltCnt}
0;JMP
(LESS_{_ltCnt})
@SP
A=M-1
M=-1
(LT_END_{_ltCnt})";
            _ltCnt++;
            _streamWriter.Write(str);
        }

        private void WriteAnd()
        {
            string str = @"// and
@SP
AM=M-1
D=M
A=A-1
M=M&D";
            _streamWriter.Write(str);
        }

        private void WriteOr()
        {
            string str = @"// or
@SP
AM=M-1
D=M
A=A-1
M=M|D";
            _streamWriter.Write(str);
        }

        private void WriteNot()
        {
            string str = @"// not
@SP
A=M-1
M=!M";
            _streamWriter.Write(str);
        }
        #endregion

        #region Dispose
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                _streamWriter.Close();
            }

            _disposed = true;
        }

        ~CodeWriter()
        {
            Dispose(false);
        }
        #endregion
    }
}
