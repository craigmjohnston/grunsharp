namespace GrunCS
{
    using System;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using System.Windows.Controls;
    using Antlr4.Runtime;
    using Antlr4.Runtime.Tree;
    using GrunCS.Annotations;
    using GrunCS.Graphs;

    public class MainWindowViewModel : INotifyPropertyChanged
    {
        public MainWindowViewModel()
        {
            this.Graph = new TokenGraph();
            this.DrawTraverse(this.Graph, MainWindow.Tree, MainWindow.Parser);
        }
        
        public event PropertyChangedEventHandler PropertyChanged;
        
        public TokenGraph Graph { get; private set; }


        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        
        private PayloadVertex DrawTraverse(TokenGraph graph, ITree tree, Parser parser)
        {
            PayloadVertex vertex;

            if (tree is IErrorNode)
            {
                vertex = new ErrorVertex((IErrorNode)tree);
            }
            else if (tree is IRuleNode)
            {
                vertex = new RuleVertex((IRuleNode)tree.Payload);
            }
            else if (tree.Payload is IToken)
            {
                vertex = new TokenVertex((IToken)tree.Payload);
            }
            else
            {
                throw new ArgumentException();
            }
            
            graph.AddVertex(vertex);

            for (int i = 0; i < tree.ChildCount; i++)
            {
                var childVertex = this.DrawTraverse(graph, tree.GetChild(i), parser);
                var edge = new TokenEdge(vertex, childVertex);
                graph.AddEdge(edge);
            }

            return vertex;
        }
    }
}