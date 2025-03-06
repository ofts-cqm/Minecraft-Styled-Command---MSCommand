namespace MCCommands.Tokens
{
    internal class SubCommandToken : IToken
    {
        private readonly Dictionary<string, IToken?> Values;

        public SubCommandToken(Dictionary<string, IToken?> values) : base("", "Incorrect Argument for Command")
        {
            Values = values;
        }

        public override IEnumerable<string>? GetAllValues()
        {
            return Values.Keys;
        }

        public override bool MatchToken(List<string> args, out object? readValue, out string? error)
        {
            if (!base.MatchToken(args, out readValue, out error)) return false;
            if (args.Count > 0)
            {
                readValue = args[0];
                args.RemoveAt(0);
            }
            return true;
        }

        public override IToken? GetNextToken(object? readValue)
        {
            return readValue == null ? null : Values.TryGetValue(readValue as string ?? "", out IToken? next) ? next : null;
        }

        public override bool IsAllowedValue(string value)
        {
            return Values.ContainsKey(value);
        }
    }
}
