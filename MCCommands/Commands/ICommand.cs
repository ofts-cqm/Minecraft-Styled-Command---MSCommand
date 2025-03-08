using MCCommands.Tokens;
using StardewModdingAPI;
using StardewValley;
using System.Text;

namespace MCCommands.Commands
{
    internal abstract class ICommand
    {
        public IToken? FirstToken;
        public int OPLevelRequired = 2;
        public string CommandName;
        public static IMonitor Monitor;

        public ICommand(IModHelper helper, string name, string doct, IToken? tokens)
        {
            FirstToken = tokens;
            CommandName = name;
            helper.ConsoleCommands.Add(
                name, doct, 
                (a, b) => MatchAndExecute(
                    new List<string>(b), 
                    new CommandContext(
                        (str, err) => Monitor.Log(str, err ? LogLevel.Error : LogLevel.Info), 
                        Game1.player, Game1.player.getStandingPosition(), Game1.player.currentLocation
                    )
                )
            );
        }

        public string getParsedCommand(List<string> parsed, List<string> original)
        {
            StringBuilder sb = new(CommandName);
            for (int i = 0; i < original.Count - parsed.Count; i++) sb.Append(original[i]).Append(' ');
            return sb.ToString();
        }

        public void MatchAndExecute(List<string> args, CommandContext context)
        {
            if (!Context.IsWorldReady) return;
            List<object> matchedToken = new();
            string previousStr = args.Count > 0 ? args[0] : CommandName;
            IToken? token = FirstToken;
            List<string> oldArgs = new(args);
            while (token != null) 
            {
                if (token.MatchToken(args, out object? val, out string? error) && val != null) matchedToken.Add(val);
                else if (error != null)
                {
                    context.LogError(error);
                    context.LogError(oldArgs.Aggregate((a, b) => a + ' ' + b) + " <-- [Here]");
                    return;
                }

                token = token.GetNextToken(val);
            }

            if (Execute(matchedToken, context, out string? message))
            {
                if (message != null) context.LogInfo(message);
            }
            else if (message != null) context.LogError(message);
        }

        public abstract bool Execute(List<object> matchedToken, CommandContext context, out string? message);
    }
}
