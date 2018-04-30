namespace GrunCS
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.IO;
    using System.Reflection;
    using System.Windows.Controls;
    using System.Windows.Input;
    using System.Windows.Media;
    using Antlr4.Runtime;
    using Antlr4.Runtime.Tree;
    using GrunCS.Graphs;
    using QuickGraph.Data;

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        private MainWindowViewModel viewModel;
        
        public static ITree Tree { get; set; }
        
        public static Parser Parser { get; set; }
        
        public static Lexer Lexer { get; set; }
        
        public static string[] LexerSymbolicNames { get; set; }
        
        
        public MainWindow()
        {
            this.viewModel = new MainWindowViewModel();
            this.DataContext = this.viewModel;
            this.InitializeComponent();
            
            this.TvMain.Items.Add(this.Traverse(MainWindow.Tree, MainWindow.Parser));
        }
        
        private TreeViewItem Traverse(ITree tree, Parser parser)
        {
            string text = null;
            
            if (tree is IRuleNode)
            {
                text = MainWindow.Parser.RuleNames[((IRuleNode)tree).RuleContext.RuleIndex];
            }
            else if (tree.Payload is IToken)
            {
                text = ((IToken) tree.Payload).Text.Replace("\n", "\\n");
            }
            
            var result = new TreeViewItem
            {
                Header = text
            };

            if (tree is IErrorNode)
            {
                result.Background = Brushes.IndianRed;
            }

            for (int i = 0; i < tree.ChildCount; i++)
            {
                var item = this.Traverse(tree.GetChild(i), parser);
                result.Items.Add(item);
            }

            return result;
        }
    }
}