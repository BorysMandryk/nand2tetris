using System.IO;
using System;
using JackCompiler.SyntaxAnalyzer;

namespace JackCompiler
{
    internal class JackCompiler
    {
        static void Main(string[] args)
        {
            //const string baseDir = @"D:\nand2tetris\projects\11\Tests";
            //string[] subDirs = Directory.GetDirectories(baseDir);
            //foreach(string d in subDirs)
            //{
            //    string path = Path.Combine(baseDir, d);
            //    Compile(path);
            //}

            Compile(args[0]);
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
            string outputPath = Path.Combine(dir, $"{filename}.vm");
            using CompilationEngine engine = new CompilationEngine(path, outputPath);
        }
    }
}
