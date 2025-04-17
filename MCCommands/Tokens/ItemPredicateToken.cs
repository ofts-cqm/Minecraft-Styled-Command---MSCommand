using StardewValley;
using StardewValley.ItemTypeDefinitions;

namespace MCCommands.Tokens
{
    internal struct ParsedPredicate
    {
        string? item;
        int[]? min;
        int[]? max;
        bool[]? seperator;
    }

    internal class ItemPredicateToken : LinearToken
    {
        public ItemPredicateToken(string tokenName = "predicate", string errorMessage = "invalide predicate") : base(tokenName, errorMessage)
        {
            StrictValue = false;
        }

        public override bool MatchToken(List<string> args, out object? readValue, out string? error)
        {
            error = "";
            readValue = null;
            if (args.Count == 0)
            {
                error = "Expected predicate";
                return false;
            }

            string type = "", test = "", temp = args[0];
            if (temp.Contains('['))
            {
                if (temp.Contains(']'))
                {
                    int index = temp.IndexOf('[');
                    type = temp.Substring(0, index);
                    test = temp.Substring(index + 1);
                }
                else
                {
                    error = "Expected ']'";
                    return false;
                }
            }
            else
            {
                type = temp;
            }

            List<int> min = new();
            List<int> max = new();
            List<bool> seperator = new();

            while (temp[0] != ']')
            {
                if (temp.StartsWith("count") && (temp[5] == '~' || temp[5] == '='))
                {
                    temp = temp.Substring(6);
                    throw new NotImplementedException();
                }
                else
                {
                    error = "Unsupported Test";
                    return false;
                }
            }
        }

        public bool ItemMatched(Item item, ParsedPredicate predicate)
        {
            throw new NotImplementedException();
        }

        public override IEnumerable<string>? GetAllValues() => null;

        public override bool IsAllowedValue(string value) => true;
    }
}
