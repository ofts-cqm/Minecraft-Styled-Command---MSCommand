using StardewModdingAPI;
using StardewValley;
using System.Text;

namespace MCCommands.Commands
{
    internal class BanList : ICommand
    {
        public BanList(IModHelper helper) : base(helper, "banlist", "banlist", 3, null)
        {
        }

        public override bool Execute(List<object> matchedToken, CommandContext context, out string? message)
        {
            StringBuilder sb = new();
            foreach (string ban in Game1.bannedUsers.Values)
            {
                sb.AppendLine(ban ?? "Unnamed");
            }
            message = sb.ToString();
            return true;
        }
    }
}
