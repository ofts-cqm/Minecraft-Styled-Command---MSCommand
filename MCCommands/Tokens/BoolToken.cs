namespace MCCommands.Tokens
{
    internal class BoolToken : LinearToken
    {
        public BoolToken(string tokenName, string errorMessage = "Unknown Token") : base(tokenName, errorMessage)
        {
            ShowAll = true;
        }

        public override bool MatchToken(List<string> args, out object? readValue, out string? error)
        {
            if (!base.MatchToken(args, out readValue, out error)) return false;
            if (args.Count == 0) return true;

            if (args[0] == "true" || args[0] == "false")
            {
                readValue = args[0];
                args.RemoveAt(0);
            }
            error = ErrorMessage;
            return false;
        }

        public override IEnumerable<string>? GetAllValues() => new string[] { "true", "false" };

        public override bool IsAllowedValue(string value) => value == "true" || value == "false";
    }
}
