using MCCommands.Commands;
using Microsoft.Xna.Framework;
using StardewValley;

namespace MCCommands.Tokens
{
    internal class CoordinateTokenAllowEntity : LinearToken
    {
        public string keyword;
        public static EntityMatchToken EntityMatcher = new();
        public bool allowDecimal;
        public bool onlyOne;

        public CoordinateTokenAllowEntity(string keyword, string tokenName = "pos", bool allowDecimal = true, bool onlyOne = false, string errorMessage = "Unknown Token") : base(tokenName, errorMessage)
        {
            this.keyword = keyword;
            this.allowDecimal = allowDecimal;
            this.onlyOne = onlyOne;
        }

        public override IEnumerable<string>? GetAllValues()
        {
            if (onlyOne)
            {
                return EntityMatcher.GetAllValues()?.Select(s => keyword + " " + s);
            }
            List<string> list = new() { "~ ~", (Game1.getMouseX() + Game1.viewport.X).ToString() + " " + (Game1.getMouseY() + Game1.viewport.Y).ToString()};
            list.AddRange(EntityMatcher.GetAllValues()?.Select(s => keyword + " " + s) ?? Array.Empty<string>());
            return list;
        }

        public override bool MatchToken(List<string> args, out object? readValue, out string? error)
        {
            error = "";
            readValue = null;
            if (args.Count < (onlyOne ? 1 : 2))
            {
                if (IsOptional) return true;
                error = "Incomplete Command";
                return false;
            }

            if (args[0] == keyword)
            {
                args.RemoveAt(0);
                return EntityMatcher.MatchToken(args, out readValue, out error);
            }

            Vector2 pos = new();
            if (!float.TryParse(args[0], out pos.X))
            {
                if (args[0][0] == '~' && float.TryParse(args[0].AsSpan(1), out float offSet))
                {
                    pos.X = CommandContext.CurrentCommandContext.Pos.X / Game1.tileSize + offSet;
                    goto Next;
                }
                else if (args[0] == "~")
                {
                    pos.X = CommandContext.CurrentCommandContext.Pos.X / Game1.tileSize;
                    goto Next;
                }

                error = "Expected Integer";
                return false;
            }

        Next:
            if (onlyOne) goto Finish;

            if (!float.TryParse(args[1], out pos.Y))
            {
                if (args[1][0] == '~' && float.TryParse(args[1].AsSpan(1), out float offSet))
                {
                    pos.Y = CommandContext.CurrentCommandContext.Pos.Y / Game1.tileSize + offSet;
                    goto Finish;
                }
                else if (args[1] == "~")
                {
                    pos.Y = CommandContext.CurrentCommandContext.Pos.Y / Game1.tileSize;
                    goto Finish;
                }

                error = "Expected Integer";
                return false;
            }
        Finish:

            if (pos.X < 0 || pos.Y < 0)
            {
                error = $"Coordinate cannot be negative, found {pos}";
                return false;
            }

            if (allowDecimal) readValue = pos;
            else readValue = new Vector2((int)pos.X, (int)pos.Y);
            args.RemoveAt(0);
            if (onlyOne) readValue = allowDecimal ? pos.X : (int)pos.X;
            else args.RemoveAt(0);
            return true;
        }

        public override bool IsAllowedValue(string value) => true;
    }
}
