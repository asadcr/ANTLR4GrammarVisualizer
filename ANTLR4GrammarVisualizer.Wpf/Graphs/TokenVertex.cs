namespace AntlrGrammarVisualizer.Wpf.Graphs
{
    using Antlr4.Runtime;
    using Antlr4.Runtime.Tree;

    public class PayloadVertex
    {
        public virtual string Text { get; }
        
        public virtual bool IsError { get; }
        
        public virtual string Type { get; }
    }

    public class ErrorVertex : PayloadVertex
    {
        public ErrorVertex(IErrorNode errorNode)
        {
            ErrorNode = errorNode;
        }
        
        public IErrorNode ErrorNode { get; }

        public override string Text => ErrorNode.Symbol.Text;

        public override bool IsError => true;

        public override string Type => "Error";
    }

    public class TokenVertex : PayloadVertex
    {
        public TokenVertex(IToken token)
        {
            Token = token;
        }
        
        public IToken Token { get; }

        public override string Text
        {
            get
            {
                if (string.IsNullOrWhiteSpace(Token.Text) && (Token.Text == null || !Token.Text.Contains("\n")))
                {
                    return $"<{MainWindow.LexerSymbolicNames[Token.Type]}>";
                }
                
                return Token.Text.Replace("\n", "\\n");
            }
        }

        public override string Type => "Token";
    }

    public class RuleVertex : PayloadVertex
    {
        public RuleVertex(IRuleNode ruleNode)
        {
            RuleNode = ruleNode;
        }
        
        public IRuleNode RuleNode { get; }

        public override string Text => MainWindow.Parser.RuleNames[RuleNode.RuleContext.RuleIndex];

        public override string Type => "Rule";
    }
}