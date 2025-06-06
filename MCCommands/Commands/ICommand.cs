﻿using MCCommands.Tokens;
using StardewModdingAPI;
using StardewValley;
using System.Text;

namespace MCCommands.Commands
{
    internal abstract class ICommand
    {
        public IToken? FirstToken;
        public int OPLevelRequired;
        public string CommandName;
        public static IMonitor Monitor;
        public static Dictionary<string, ICommand> RegisteredCommands = new();

        public ICommand(IModHelper helper, string name, string doct, int OPLevelRequired, IToken? tokens)
        {
            FirstToken = tokens;
            CommandName = name;
            this.OPLevelRequired = OPLevelRequired;
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
            RegisteredCommands.Add(name, this);
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
            if (context.Player is not Farmer f || (ModEntry.OPs.TryGetValue(f.UniqueMultiplayerID, out int opLevel) ? opLevel : 0) < OPLevelRequired)
            {
                context.LogError("You do not have permission to use this command");
                return;
            }
            CommandContext.CurrentCommandContext = context;
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
                    context.LogError(getParsedCommand(oldArgs, args) + " <-- [Here]");
                    return;
                }

                token = token.GetNextToken(val);
            }

            if (Execute(matchedToken, context, out string? message))
            {
                if (message != null) context.LogInfo(message);
            }
            else if (message != null) context.LogError(message);
            CommandContext.CurrentCommandContext = null;
        }

        public abstract bool Execute(List<object> matchedToken, CommandContext context, out string? message);
    }
}
