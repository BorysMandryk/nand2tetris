using System;
using System.IO;

namespace JackCompiler.Logger
{
    internal class Logger : IDisposable
    {
        protected TextWriter _writer;
        protected bool _disposed = false;
        public bool Show { get; set; } = true;

        public Logger(TextWriter output)
        {
            _writer = output;
        }

        public virtual void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _writer.Dispose();
            _disposed = true;
        }

        public void Log(string str)
        {
            if (Show)
            {
                _writer.WriteLine(str);
            }
        }
    }
}
