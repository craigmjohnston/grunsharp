namespace GrunCS
{
    using System;
    using System.CodeDom.Compiler;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using Antlr4.Runtime;
    using Microsoft.CodeDom.Providers.DotNetCompilerPlatform;
    using Newtonsoft.Json;

    public class Main
    {
        private string[] tokenSymbolicNames;
        private Config config;
        
        public bool ShowTokens { get; set; }
        
        public bool ShowTree { get; set; }

        public string ConfigFileLocation { get; set; } = null;
        
        public void Process(string workingDirectory, string grammarName, string startRule, string testFile)
        {
            this.config = this.ConfigFileLocation != null 
                ? Loader.LoadConfigFromPath(this.ConfigFileLocation) 
                : Loader.LoadConfigFromDirectory(workingDirectory);
            
            string[] classFiles = Loader.FindFiles(grammarName, startRule, workingDirectory, this.config.Include);
            Assembly assembly = Loader.Compile(classFiles, this.config.References);
            
            this.Parse(assembly, testFile, startRule);
        }


        private void Parse(Assembly assembly, string filePath, string startRule)
        {
            string fileString = File.ReadAllText(filePath);
            
            var inputStream = new AntlrInputStream(fileString);
            var lexer = this.GetLexer(assembly, inputStream);
            var tokenStream = new CommonTokenStream(lexer);
            tokenStream.Fill();
            
            if (this.ShowTokens)
            {
                foreach (var token in tokenStream.GetTokens())
                {
                    if (token is CommonToken)
                    {
                        Console.WriteLine(this.TokenToString(token, lexer));
                    }
                    else
                    {
                        Console.WriteLine(token.ToString());
                    }
                }
            }
            
            var parser = this.GetParser(assembly, tokenStream);
            parser.BuildParseTree = true;
            
            ParserRuleContext tree = this.RunRule(parser, startRule);
            
            MainWindow.Tree = tree;
            MainWindow.Parser = parser;
            MainWindow.Lexer = lexer;
            MainWindow.LexerSymbolicNames = this.tokenSymbolicNames;

            if (this.ShowTree)
            {
                Console.WriteLine(tree.ToStringTree(parser));
            }
        }
        
        
        private Lexer GetLexer(Assembly assembly, AntlrInputStream inputStream)
        {
            var types = assembly.GetTypes();
            var lexerType = types.FirstOrDefault(t => typeof(Lexer).IsAssignableFrom(t));

            if (lexerType == null)
            {
                throw new ArgumentException();
            }

            var lexer = Activator.CreateInstance(lexerType, inputStream);

            var namesField = lexerType.GetField("_SymbolicNames", BindingFlags.Static | BindingFlags.NonPublic);
            this.tokenSymbolicNames = (string[]) namesField.GetValue(lexer);

            return (Lexer)lexer;
        }


        private Parser GetParser(Assembly assembly, CommonTokenStream tokenStream)
        {
            var types = assembly.GetTypes();
            var parserType = types.FirstOrDefault(t => typeof(Parser).IsAssignableFrom(t));

            if (parserType == null)
            {
                throw new ArgumentException();
            }

            var parser = Activator.CreateInstance(parserType, tokenStream);
            return (Parser)parser;
        }


        private ParserRuleContext RunRule(Parser parser, string startRule)
        {
            MethodInfo ruleMethod = parser.GetType().GetMethod(startRule);
            var methodTypes = ruleMethod
                .GetParameters()
                .Select(p => p.ParameterType)
                .Concat(new[] {ruleMethod.ReturnType})
                .ToArray();

            Type delegateType = Expression.GetDelegateType(methodTypes);
            var methodDelegate = ruleMethod.CreateDelegate(delegateType, parser);
            return (ParserRuleContext) methodDelegate.DynamicInvoke();
        }


        private string TokenToString(IToken token, Lexer lexer)
        {
            string str = string.Empty;
            if (token.Channel > 0)
                str = ",channel=" + (object) token.Channel;
            string text = token.Text;
            string typeName = token.Type > 0 && token.Type < this.tokenSymbolicNames.Length ? this.tokenSymbolicNames[token.Type] : token.Type.ToString();
            
            return "[@" + (object) token.TokenIndex + "," + (object) token.StartIndex + ":" + (object) token.StopIndex +
                   "='" + (text == null
                       ? "<no text>"
                       : text.Replace("\n", "\\n").Replace("\r", "\\r").Replace("\t", "\\t")) + "',<" + typeName + ">" + str + "," + (object) token.Line + ":" + (object) token.Column + "]";
        }
    }
}