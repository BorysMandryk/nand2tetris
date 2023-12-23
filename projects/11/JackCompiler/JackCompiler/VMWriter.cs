using System;
using System.Collections.Generic;
using System.IO;

namespace JackCompiler
{
    internal class VMWriter : IDisposable
    {
        private static readonly Dictionary<MemorySegment, string> _memorySegmentToString = new Dictionary<MemorySegment, string>()
        {
            { MemorySegment.CONST, "constant" },
            { MemorySegment.STATIC, "static" },
            { MemorySegment.LOCAL, "local" },
            { MemorySegment.ARG, "argument" },
            { MemorySegment.THIS, "this" },
            { MemorySegment.THAT, "that" },
            { MemorySegment.TEMP, "temp" },
            { MemorySegment.POINTER, "pointer" }
        };

        private static readonly Dictionary<ArithmeticCommand, string> _arithmeticToString = new Dictionary<ArithmeticCommand, string>()
        { 
            {ArithmeticCommand.ADD, "add" },
            {ArithmeticCommand.SUB, "sub" },
            {ArithmeticCommand.NEG, "neg" },
            {ArithmeticCommand.EQ, "eq" },
            {ArithmeticCommand.GT, "gt" },
            {ArithmeticCommand.LT, "lt" },
            {ArithmeticCommand.AND, "and" },
            {ArithmeticCommand.OR, "or" },
            {ArithmeticCommand.NOT, "not" }
        };

        private StreamWriter _streamWriter;

        private bool _disposed = false;

        public VMWriter(string output)
        {
            _streamWriter = new StreamWriter(output);
        }

        public virtual void Dispose()
        {
            if(_disposed)
            {
                return;
            }

            _streamWriter.Dispose();
            _disposed = true;
        }

        public void WritePush(MemorySegment segment, int index)
        {
            _streamWriter.WriteLine($"push {_memorySegmentToString[segment]} {index}");
        }

        public void WritePop(MemorySegment segment, int index)
        {
            _streamWriter.WriteLine($"pop {_memorySegmentToString[segment]} {index}");
        }

        public void WriteArithmetic(ArithmeticCommand command)
        {
            _streamWriter.WriteLine(_arithmeticToString[command]);
        }

        public void WriteLabel(string label)
        {
            _streamWriter.WriteLine($"label {label}");
        }

        public void WriteGoto(string label)
        {
            _streamWriter.WriteLine($"goto {label}");
        }

        public void WriteIf(string label)
        {
            _streamWriter.WriteLine($"if-goto {label}");
        }

        public void WriteCall(string name, int nArgs)
        {
            _streamWriter.WriteLine($"call {name} {nArgs}");
        }

        public void WriteFunction(string name, int nLocals)
        {
            _streamWriter.WriteLine($"function {name} {nLocals}");
        }

        public void WriteReturn()
        {
            _streamWriter.WriteLine($"return");
        }

        public void Close()
        {
            Dispose();
        }
    }
}
