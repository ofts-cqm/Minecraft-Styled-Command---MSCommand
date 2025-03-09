using Microsoft.Xna.Framework;
using StardewValley;

namespace MCCommands.Commands
{
    internal class CommandContext
    {
        public static CommandContext? CurrentCommandContext = null;

        private readonly Action<string, bool> Printer;

        public readonly Farmer Player;
        public readonly Vector2 Pos;
        public readonly GameLocation Dim;

        public CommandContext(Action<string, bool> printer, Farmer player, Vector2 pos, GameLocation dim)
        {
            Printer = printer;
            Player = player;
            Pos = pos;
            Dim = dim;
        }

        public void LogError(string message) => Printer.Invoke(message, true);

        public void LogInfo(string message) => Printer.Invoke(message, false);
    }
}