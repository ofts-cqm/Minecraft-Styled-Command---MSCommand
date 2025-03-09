
using MCCommands.Tokens;
using StardewModdingAPI;
using StardewValley;

namespace MCCommands.Commands
{
    internal class Deop : ICommand
    {
        public Deop(IModHelper helper) : base(helper, "deop", "deop <targets>", 3, new StringToken(() => null, "target", "Target Not Found") { StrictValue = false })
        {
        }

        public override bool Execute(List<object> matchedToken, CommandContext context, out string? message)
        {
            IEnumerable<Farmer>? targets = EntityMatchToken.GetPlayers(matchedToken[0] as string, context.Dim, context.Player);
            if (targets is null)
            {
                foreach (Farmer target in Game1.getOnlineFarmers())
                {
                    string hex = target.UniqueMultiplayerID.ToString("x16");
                    if ($"{hex[..4]}-{hex[4..8]}-{hex[8..12]}-{hex[12..]}" == matchedToken[0] as string)
                    {
                        targets = new Farmer[] { target };
                        break;
                    }
                }
            }

            if (targets is null)
            {
                message = "Player not found";
                return false;
            }

            message = "";
            foreach (Farmer target in targets)
            {
                ModEntry.OPs.Remove(target.UniqueMultiplayerID);
                message += $"Player {target.displayName} is no longer an Op!\n";
            }

            return true;
        }
    }
}
