namespace MCCommands.Tokens
{
    internal abstract class LinearToken : IToken
    {
        public IToken? Next;

        protected LinearToken(string tokenName, string errorMessage = "Unknown Token") : base(tokenName, errorMessage)
        {
        }

        public LinearToken NextToken(IToken next)
        {
            Next = next;
            return Next as LinearToken;
        }

        public override IToken? GetNextToken(object? readValue) => readValue == null ? null : Next;
    }
}
