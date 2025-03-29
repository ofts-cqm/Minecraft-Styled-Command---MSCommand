namespace MCCommands.Tokens
{
    internal class NumberToken : LinearToken
    {
        public int Max;
        public int Min;
        public bool AllowInf;
        public Func<IEnumerable<string>?>? AllValues;

        public NumberToken(string tokenName, int max = 2147483647, int min = 0, bool allowInf = false) : base(tokenName, "Expected Integer")
        {
            Max = max;
            Min = min;
            AllowInf = allowInf;
            StrictValue = false;
        }

        public NumberToken Allow(Func<IEnumerable<string>?> values)
        {
            AllValues = values;
            return this;
        }

        public override IEnumerable<string>? GetAllValues()
        {
            return AllValues == null ? (AllowInf ? new string[] { "infinity" } : null) : AllValues();
        }

        public override bool IsAllowedValue(string value)
        {
            return int.TryParse(value, out int i) && i >= Min && i < Max;
        }

        public override bool MatchToken(List<string> args, out object? readValue, out string? error)
        {
            if (!base.MatchToken(args, out readValue, out error)) return false;
            if (args.Count == 0) return true;

            if (args[0] == "infinite")
            {
                readValue = int.MaxValue;
                return true;
            }
            
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
                error = $"Integer must not be less than {Max}, found {num}";
                return false;
            }

            readValue = num;
            args.RemoveAt(0);
            return true;
        }
    }
}
