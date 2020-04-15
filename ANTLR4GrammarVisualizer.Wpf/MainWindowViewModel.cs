namespace ANTLR4GrammarVisualizer.Wpf
{
    using System;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using Antlr4.Runtime;
    using Antlr4.Runtime.Tree;
    using Graphs;

    public class MainWindowViewModel : INotifyPropertyChanged
    {
        public MainWindowViewModel()
        {
            Graph = new TokenGraph();
            DrawTraverse(Graph, MainWindow.Tree, MainWindow.Parser);
        }
        
        public event PropertyChangedEventHandler PropertyChanged;
        
        public TokenGraph Graph { get; }


        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        
        private PayloadVertex DrawTraverse(TokenGraph graph, ITree tree, Parser parser)
        {
            PayloadVertex vertex;

            if (tree is IErrorNode node)
            {
                vertex = new ErrorVertex(node);
            }
            else if (tree is IRuleNode)
            {
                vertex = new RuleVertex((IRuleNode)tree.Payload);
            }
            else if (tree.Payload is IToken token)
            {
                vertex = new TokenVertex(token);
            }
            else
            {
                throw new ArgumentException();
            }
            
            graph.AddVertex(vertex);

            for (var i = 0; i < tree.ChildCount; i++)
            {
                var childVertex = DrawTraverse(graph, tree.GetChild(i), parser);
                var edge = new TokenEdge(vertex, childVertex);
                graph.AddEdge(edge);
            }

            return vertex;
        }
    }
}