using System;

namespace JackCompiler
{
    internal class VMWriter
    {
        public VMWriter(string output)
        {
            throw new NotImplementedException();
        }

        public void WritePush(MemorySegment segment, int index)
        {
            throw new NotImplementedException();
        }

        public void WritePop(MemorySegment segment, int index)
        {
            throw new NotImplementedException();
        }

        public void WriteArithmetic(ArithmeticCommand command)
        {
            throw new NotImplementedException();
        }

        public void WriteLabel(string label)
        {
            throw new NotImplementedException();
        }

        public void WriteGoto(string label)
        {
            throw new NotImplementedException();
        }

        public void WriteIf(string label)
        {
            throw new NotImplementedException();
        }

        public void WriteCall(string name, int nArgs)
        {
            throw new NotImplementedException();
        }

        public void WriteFunction(string name, int nLocals)
        {
            throw new NotImplementedException();
        }

        public void WriteReturn()
        {
            throw new NotImplementedException();
        }

        public void Close()
        {
            throw new NotImplementedException();
        }
    }
}
