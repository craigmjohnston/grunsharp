namespace GrunCS
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text.RegularExpressions;
    using System.Windows;
    using Antlr4.Runtime;
    using NDesk.Options;

    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            bool showHelp = false;
            bool showGui = false;
            bool showTokens = false;
            bool showTree = false;
            string configFileLocation = null;

            // parse command line args
            var options = new OptionSet
            {
                { "to|tokens", "Print the tokens", v => showTokens = true},
                { "tr|tree", "Print the tree", v => showTree = true },
                { "g|gui", "Show the GUI", v => showGui = true },
                { "c|config", "Specify an explicit config file (gruncs.json) location", v => configFileLocation = v },
                { "h|help",  "show this message and exit", v => showHelp = true },
            };

            List<string> extraArgs = null;
            try
            {
                extraArgs = options.Parse(e.Args);
            }
            catch (OptionException exception)
            {
                this.PrintInputException(exception);
                this.Shutdown();
                return;
            }

            // if help flag present, quit out
            if (showHelp)
            {
                options.WriteOptionDescriptions(Console.Out);
                this.Shutdown();
                return;
            }
            
            if (extraArgs == null || extraArgs.Count < 3)
            {
                this.PrintInputException(new ArgumentException("Necessary arguments missing."));
                this.Shutdown();
                return;
            }

            // load and parse
            try
            {
                string testFilePath = Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, extraArgs[2]));
                
                Main main = new Main
                {
                    ShowTokens = showTokens,
                    ShowTree = showTree,
                    ConfigFileLocation = configFileLocation
                };
                
                main.Process(Environment.CurrentDirectory, extraArgs[0], extraArgs[1], testFilePath);
            }
            catch (Exception exception)
            {
                Console.WriteLine("There was an error when trying to parse.");
                Console.WriteLine(exception.Message);
                this.Shutdown();
                throw;
            }

            // don't allow the window to show if the gui flag isn't set
            if (!showGui)
            {
                this.Shutdown();
            }
        }


        private void PrintInputException(Exception exception)
        {
            Console.Write("gruncs: ");
            Console.WriteLine(exception.Message);
            Console.WriteLine("Try `gruncs --help for more information.");
        }


        private void Shutdown()
        {
            System.Windows.Application.Current.Shutdown();
        }
    }
}