namespace gui.Antlr
{
    using System.Collections.Generic;
    using Antlr4.Runtime;

    public class AntlrErrorStrategy : DefaultErrorStrategy
    {
        public AntlrErrorStrategy()
        {
            OffendingTokens = new List<IToken>();
        }

        public IList<IToken> OffendingTokens { get; }

        public bool HasErrors => OffendingTokens.Count > 0;

        public override void ReportError(Parser recognizer, RecognitionException e)
        {
            OffendingTokens.Add(e.OffendingToken);
        }

        protected override void ReportInputMismatch(Parser recognizer, InputMismatchException e)
        {
            OffendingTokens.Add(e.OffendingToken);
        }

        protected override void ReportMissingToken(Parser recognizer)
        {
            OffendingTokens.Add(recognizer.CurrentToken);
        }

        protected override void ReportFailedPredicate(Parser recognizer, FailedPredicateException e)
        {
            OffendingTokens.Add(e.OffendingToken);
        }

        protected override void ReportUnwantedToken(Parser recognizer)
        {
            OffendingTokens.Add(recognizer.CurrentToken);
        }
    }
}
