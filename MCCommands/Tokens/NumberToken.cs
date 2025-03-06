namespace MCCommands.Tokens
{
    internal class NumberToken : IToken
    {
        public int Max;
        public int Min;
        public bool AllowInf;
        public IToken? Next;

        public NumberToken(string tokenName, int max = 2147483647, int min = 0, bool allowInf = false) : base(tokenName, "Expected Integer")
        {
            Max = max;
            Min = min;
            AllowInf = allowInf;
            StrictValue = false;
        }

        public override IEnumerable<string>? GetAllValues()
        {
            return AllowInf ? new string[] { "Infinity" } : null;
        }

        public override IToken? GetNextToken(object? readValue)
        {
            return readValue == null ? null : Next;
        }

        public override bool IsAllowedValue(string value)
        {
            return int.TryParse(value, out int i) && i > Min && i < Max;
        }

        public override bool MatchToken(List<string> args, out object? readValue, out string? error)
        {
            if (!base.MatchToken(args, out readValue, out error)) return false;
            if (args.Count == 0) return true;
            
            if (!int.TryParse(args[0], out int num))
            {
                error = "Expected Integer";
                return false;
            }
            
            if (num > Max)
            {
                error = $"Integer must not be more than {Max}, found {num}";
                return false;
            }
            
            if (num < Min)
            {
                error = $"Integer must not be more than {Max}, found {num}";
                return false;
            }

            readValue = num;
            args.RemoveAt(0);
            return true;
        }
    }
}
