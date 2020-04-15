namespace GrunCS
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Reflection;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Media;
    using Antlr4.Runtime;
    using Antlr4.Runtime.Tree;
    using gui;
    using gui.Antlr;

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        private MainWindowViewModel _viewModel;
        
        public static ITree Tree { get; set; }
        
        public static Parser Parser { get; set; }
                
        public static string[] LexerSymbolicNames { get; set; }

        public static IList<string> LexerTokens { get; set; }

        public MainWindow()
        {
            InitializeComponent();

            InputBox.TextChanged += InputBox_TextChanged;
        }

        private void InputBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                Process(InputBox.Text);

                if (Tree != null)
                {
                    _viewModel = new MainWindowViewModel();
                    DataContext = _viewModel;
                }

                TokensListView.Items.Clear();
                foreach (var lexerToken in LexerTokens)
                {
                    TokensListView.Items.Add(lexerToken);
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
                throw;
            }
        }

        private static TreeViewItem Traverse(ITree tree)
        {
            string text = null;
            
            if (tree is IRuleNode node)
            {
                text = Parser.RuleNames[node.RuleContext.RuleIndex];
            }
            else if (tree.Payload is IToken token)
            {
                if (string.IsNullOrWhiteSpace(token.Text) && (token.Text == null || !token.Text.Contains("\n")))
                {
                    text = $"<{LexerSymbolicNames[token.Type]}>";
                }
                else
                {
                    text = token.Text.Replace("\n", "\\n");
                }
            }
            
            var result = new TreeViewItem
            {
                Header = text,
                FontWeight = tree is IRuleNode ? FontWeights.Bold : FontWeights.Normal
            };

            if (tree is IErrorNode)
            {
                result.Background = Brushes.IndianRed;
            }

            for (var i = 0; i < tree.ChildCount; i++)
            {
                var item = Traverse(tree.GetChild(i));
                result.Items.Add(item);
            }

            return result;
        }

        private void Process(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                return;
            }

            var sw = Stopwatch.StartNew();

            var lexer = new CppLexer(new AntlrFileStream(input));
            var tokenStream = new CommonTokenStream(lexer);
            tokenStream.Fill();

            var errorHandler = new AntlrErrorStrategy();
            var parser = new CppParser(tokenStream)
            {
                BuildParseTree = true,
                ErrorHandler = errorHandler
            };

            ParserRuleContext tree = parser.translationunit();

            var tokenSymbolicNames = (string[])lexer.GetType().GetField("_SymbolicNames", BindingFlags.Static | BindingFlags.NonPublic).GetValue(lexer);
            var tokens = GetTokens(tokenStream, tokenSymbolicNames);

            var errorStatus = errorHandler.HasErrors ? 
                $"Parser Error at {string.Join(Environment.NewLine, errorHandler.OffendingTokens.Select(t => TokenToString(t, tokenSymbolicNames)))}" : 
                "No Errors";

            ErrorStatus.Text = $"Elapsed: {sw.Elapsed.TotalSeconds}s" + Environment.NewLine + errorStatus;

            var count = GetTreeChildCount(tree);
            var criteria = count <= 500;
            Tree = criteria ? tree : ParserRuleContext.EmptyContext;

            Parser = parser;
            LexerSymbolicNames = tokenSymbolicNames;
            LexerTokens =  tokens;
        }

        private static IList<string> GetTokens(BufferedTokenStream tokenStream, string[] tokenSymbolicNames)
        {
            return tokenStream.GetTokens().Select(token => token is CommonToken ? TokenToString(token, tokenSymbolicNames) : token.ToString()).ToArray();
        }

        private static string TokenToString(IToken token, IReadOnlyList<string> tokenSymbolicNames)
        {
            if (token == null)
            {
                return "<no text>";
            }
            var text = (token.Text ?? "<no text>").Replace("\n", "\\n").Replace("\r", "\\r").Replace("\t", "\\t");
            var typeName = token.Type > 0 && token.Type < tokenSymbolicNames.Count ? tokenSymbolicNames[token.Type] : token.Type.ToString();

            return $"{text}:<{typeName}>:{token.Line}:{token.Column}";
        }

        private static int GetTreeChildCount(ParserRuleContext tree)
        {
            return GetTreeChildCount((IParseTree) tree).Sum();
        }

        private static IEnumerable<int> GetTreeChildCount(ITree tree)
        {
            if (tree.ChildCount == 0)
            {
                yield return 1;
            }

            for (var i = 0; i < tree.ChildCount; i++)
            {
                foreach (var i1 in GetTreeChildCount(tree.GetChild(i)))
                {
                    yield return i1;
                }
            }
        }
    }
}