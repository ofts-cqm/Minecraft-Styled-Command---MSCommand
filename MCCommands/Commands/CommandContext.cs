using Microsoft.Xna.Framework;
using StardewValley;

namespace MCCommands.Commands
{
    internal class CommandContext
    {
        public static CommandContext? CurrentCommandContext = null;

        private readonly Action<string, bool> Printer;

        public Character Player;
        public Vector2 Pos;
        public Character PositionedEntity;
        public GameLocation Dim;
        public int Facing;

        public CommandContext(Action<string, bool> printer, Farmer player, Vector2 pos, GameLocation dim)
        {
            Printer = printer;
            Player = player;
            Pos = pos;
            Dim = dim;
            PositionedEntity = player;
            Facing = player.FacingDirection;
        }

        public CommandContext(CommandContext source)
        {
            Printer = source.Printer;
            Player = source.Player;
            Pos = source.Pos;
            Dim = source.Dim;
            PositionedEntity = source.PositionedEntity;
            Facing = source.Facing;
        }

        public void LogError(string message) => Printer.Invoke(message, true);

        public void LogInfo(string message) => Printer.Invoke(message, false);
    }
}