namespace MCCommands.Tokens
{
    internal abstract class IToken
    {
        public string TokenName;
        public string ErrorMessage;
        public bool IsOptional = false;
        public bool ShowAll = false;
        public bool StrictValue = true;
        public bool IsPlainText = false;

        public IToken(string tokenName, string errorMessage = "Unknown Token")
        {
            TokenName = tokenName;
            ErrorMessage = errorMessage;
        }

        public abstract bool IsAllowedValue(string value);

        public virtual bool MatchToken(List<string> args, out object? readValue, out string? error)
        {
            error = null;
            readValue = null;
            if (args.Count == 0)
            {
                if (IsOptional) return true;
                error = "Incomplete Command";
                return false;
            }
            if (StrictValue && !IsAllowedValue(args[0]))
            {
                error = ErrorMessage;
                return false;
            }
            return true;
        }

        public abstract IEnumerable<string>? GetAllValues();

        public abstract IToken? GetNextToken(object? readValue);
    }
}
