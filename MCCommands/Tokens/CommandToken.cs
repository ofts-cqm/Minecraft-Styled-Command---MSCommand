using MCCommands.Commands;
using StardewModdingAPI;

namespace MCCommands.Tokens
{
    internal class CommandToken : IToken
    {
        public CommandToken(string tokenName = "execute", string errorMessage = "Command not found") : base(tokenName, errorMessage)
        {

        }

        public override IEnumerable<string>? GetAllValues() => ICommand.RegisteredCommands.Keys;

        public override bool MatchToken(List<string> args, out object? readValue, out string? error)
        {
            if (!base.MatchToken(args, out readValue, out error)) return false;
            if (args.Count > 0)
            {
                readValue = ICommand.RegisteredCommands[args[0]];
                args.RemoveAt(0);
            }
            return true;
        }

        public override IToken? GetNextToken(object? readValue)
        {
            if (readValue is ICommand command) return command.FirstToken;
            return null;
        }

        public override bool IsAllowedValue(string value) => GetAllValues()?.Contains(value) ?? false;
    }
}
