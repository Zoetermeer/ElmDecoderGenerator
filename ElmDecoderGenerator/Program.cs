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
                    AppDomain.CurrentDomain.AssemblyResolve += SameDirectoryLoader(Path.GetDirectoryName(opts.AssemblyPath));
                    var asm = Assembly.LoadFile(opts.AssemblyPath);
                    
                    var gen = new ElmCodeGenerator(asm, Path.GetFileNameWithoutExtension(opts.ElmSourcePath));
                    gen.Generate();
                    using (var fs = File.OpenWrite(opts.ElmSourcePath)) {
                        var code = gen.GetCode();
                        var bytes = Encoding.UTF8.GetBytes(code);
                        fs.Write(bytes, 0, bytes.Length);
                        
                        fs.Close();
                    }
                });
        }

        private static ResolveEventHandler SameDirectoryLoader(string path) {
            //Note that this requires the following in the project file of the target project:
            //<CopyLocalLockFileAssemblies>true</CopyLocalLockFileAssemblies>
            //So that dynamically loaded assemblies (from Nuget, etc.) are placed in the output directory
            //alongside the primary dll.
            return (sender, args) => {
                var assemblyPath = Path.Combine(path, new AssemblyName(args.Name).Name + ".dll");
                if (!File.Exists(assemblyPath)) return null;
                
                return Assembly.LoadFrom(assemblyPath);
            };
        }

        private static void GenerateElmSource(Type type) {
            
        }
    }
}