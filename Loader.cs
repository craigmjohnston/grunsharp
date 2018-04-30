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


        public static string[] FindFiles(string grammarName, string startRule, string directoryPath, string[] includes)
        {
            string[] files = Directory.GetFiles(directoryPath, "*.cs");
            string[] filenamesWithoutExtensions = files.Select(Path.GetFileNameWithoutExtension).ToArray();

            HashSet<string> output = new HashSet<string>();

            string lexerName = grammarName + "Lexer";
            if (filenamesWithoutExtensions.Contains(lexerName))
            {
                output.Add(files[Array.IndexOf(filenamesWithoutExtensions, lexerName)]);
            }
            else
            {
                lexerName = grammarName;
                if (filenamesWithoutExtensions.Contains(lexerName))
                {
                    output.Add(files[Array.IndexOf(filenamesWithoutExtensions, lexerName)]);
                }
                else
                {
                    throw new FileNotFoundException($"Can't load {lexerName} as lexer or parser");
                }
            }

            if (startRule != "tokens")
            {
                string parserName = grammarName + "Parser";
                if (filenamesWithoutExtensions.Contains(parserName))
                {
                    output.Add(files[Array.IndexOf(filenamesWithoutExtensions, parserName)]);

                    if (filenamesWithoutExtensions.Contains(parserName + "Listener"))
                    {
                        output.Add(files[Array.IndexOf(filenamesWithoutExtensions, parserName + "Listener")]);
                    }
                    else
                    {
                        throw new FileNotFoundException($"Can't find {parserName}Listener parser listener file");
                    }
                }
                else
                {
                    throw new FileNotFoundException($"Can't find {parserName} parser file");
                }
            }

            foreach (var include in includes)
            {
                output.Add(include);
            }

            return output.ToArray();
        }
        
        
        public static Assembly Compile(string[] filePaths, string[] references)
        {
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
            
            return result.CompiledAssembly;
        }
    }
}