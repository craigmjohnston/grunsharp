namespace GrunCS
{
    using System;
    using System.CodeDom.Compiler;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using Microsoft.CodeDom.Providers.DotNetCompilerPlatform;
    using Newtonsoft.Json;

    public static class Loader
    {
        public static Config LoadConfigFromDirectory(string workingDirectory)
        {
            return Loader.LoadConfigFromPath(Path.Combine(workingDirectory, "gruncs.json"));
        }


        public static Config LoadConfigFromPath(string path)
        {
            if (!File.Exists(path))
            {
                // return a default config if there's no config file
                return new Config();
            }

            return JsonConvert.DeserializeObject<Config>(File.ReadAllText(path));
        }


        public static string[] FindFiles(string directoryPath, string[] includes)
        {
            return Directory.GetFiles(directoryPath, "*.cs").Concat(includes).Distinct().ToArray();
        }
        
        
        public static Assembly Compile(string[] filePaths, string[] references)
        {
            // set the current directory to the EXE directory, to fix an issue with roslyn not being found sometimes
            var actualWorkingDirectory = Environment.CurrentDirectory;
            Environment.CurrentDirectory = Main.ExeDirectory;
            
            var codeProvider = new CSharpCodeProvider();

            List<string> assemblyNames = new List<string>
            {
                typeof(Antlr4.Runtime.AntlrFileStream).Assembly.Location,
                typeof(System.CodeDom.CodeArgumentReferenceExpression).Assembly.Location
            };
            
            foreach (var reference in references)
            {
                AssemblyName assemblyName = null;

                try
                {
                    // attempt to load with path to assembly
                    assemblyName = AssemblyName.GetAssemblyName(reference);
                }
                catch (Exception ex) when (ex is FileNotFoundException || ex is BadImageFormatException)
                {
                    // attempt to load with fullname
                    assemblyName = new AssemblyName(reference);

                    if (assemblyName.Version == null)
                    {
                        // as a last resort, load with partial name
                        var assembly = Assembly.LoadWithPartialName(reference);
                        assemblyName = new AssemblyName(assembly.FullName);
                    }
                }

                assemblyNames.Add(Assembly.Load(assemblyName).Location);
            }
            
            var compilerParams = new CompilerParameters(assemblyNames.ToArray());

            CompilerResults result = codeProvider.CompileAssemblyFromFile(compilerParams, filePaths);
            string errorText = String.Empty;
            foreach (CompilerError compilerError in result.Errors)
            {
                errorText += $"({compilerError.Line}:{compilerError.Column}) {compilerError.ErrorText}\n";
            }

            if (errorText.Length > 0)
            {
                throw new Exception(errorText);
            }
            
            Environment.CurrentDirectory = actualWorkingDirectory;
            return result.CompiledAssembly;
        }
    }
}