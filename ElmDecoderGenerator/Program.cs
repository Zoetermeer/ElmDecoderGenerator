using System;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using CommandLine;
using Newtonsoft.Json;

namespace ElmDecoderGenerator
{
    class Program {
        public class Options {
            [Option("assembly-path", Required = true, HelpText = "The full path to the .NET assembly containing JSON types")]
            public string AssemblyPath { get; set; }
            
            [Option("elm-source-path", Required = true, HelpText = "The full path of the target Elm source file (will be created if not present)")]
            public string ElmSourcePath { get; set; }
        }
        
        static void Main(string[] args) {
            Parser.Default.ParseArguments<Options>(args)
                .WithParsed(opts => {
                    var asm = Assembly.LoadFile(opts.AssemblyPath);
                    var gen = new ElmCodeGenerator(asm);
                    gen.Generate();
                    using (var fs = File.OpenWrite(opts.ElmSourcePath)) {
                        var code = gen.GetCode();
                        var bytes = Encoding.UTF8.GetBytes(code);
                        fs.Write(bytes, 0, bytes.Length);
                        
                        fs.Close();
                    }
                });
            
            Console.WriteLine("Hello World!");
        }

        private static void GenerateElmSource(Type type) {
            
        }
    }
}