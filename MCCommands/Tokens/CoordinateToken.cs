using MCCommands.Commands;
using StardewValley;

namespace MCCommands.Tokens
{
    internal class CoordinateToken : LinearToken
    {
        public bool isX;
        public bool allowDecimal;

        public CoordinateToken(string tokenName, bool allowDecimal, bool isX = true) : base(tokenName, "Expected Integer")
        {
            StrictValue = false;
            this.isX = isX;
            this.allowDecimal = allowDecimal;
            if (isX) Next = new CoordinateToken(tokenName, false, false);
        }

        public override IEnumerable<string>? GetAllValues()
        {
            if (isX) return new string[] { "~", (Game1.getMouseX() + Game1.viewport.X).ToString() };
            else return new string[] { "~", (Game1.getMouseY() + Game1.viewport.Y).ToString() };
        }

        public override bool IsAllowedValue(string value) => int.TryParse(value, out int i) && i >= 0;

        public override LinearToken NextToken(IToken next)
        {
            if (isX)
            {
                return ((LinearToken)Next).NextToken(next);
            }
            else
            {
                return base.NextToken(next);
            }
        }

        public override bool MatchToken(List<string> args, out object? readValue, out string? error)
        {
            if (!base.MatchToken(args, out readValue, out error)) return false;
            if (args.Count == 0) return true;

            if (!float.TryParse(args[0], out float num))
            {
                if (args[0][0] == '~' && float.TryParse(args[0].AsSpan(1), out float offSet))
                {
                    if (isX) num = CommandContext.CurrentCommandContext.Pos.X + offSet;
                    else num = CommandContext.CurrentCommandContext.Pos.Y + offSet;
                    goto Next;
                }

                error = "Expected Integer";
                return false;
            }

            Next:
            if (num < 0)
            {
                error = $"Coordinate cannot be negative, found {num}";
                return false;
            }

            if (allowDecimal) readValue = num;
            else readValue = (int)num;
            args.RemoveAt(0);
            return true;
        }
    }
}
