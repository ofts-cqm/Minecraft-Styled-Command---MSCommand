using MCCommands.Tokens;
using StardewModdingAPI;
using StardewValley;

namespace MCCommands.Commands
{
    internal class Ban : ICommand
    {
        public Ban(IModHelper helper) : base(helper, "ban", "ban <targets> [<reason>]", new StringToken(StringToken.Player_Target, "target", "Player not found")
        {
            Next = new StringToken(() => null, "reason", "Invalid Reason")
            {
                IsOptional = true
            }
        })
        {
        }

        public override bool Execute(List<object> matchedToken, CommandContext context, out string? message)
        {
            IEnumerable<Farmer> farmers = StringToken.DecodeFarmer(StringToken.DecodePlayerTarget(matchedToken[0] as string ?? ""), out message) ?? Array.Empty<Farmer>();
            if (!farmers.Any()) return false;
            foreach (Farmer farmer in farmers)
            {
                Game1.server.ban(farmer.UniqueMultiplayerID);
            }
            message = (farmers.Count() == 1 ? $"Banned player {farmers.ElementAt(0).displayName}, reason: " : $"Banned {farmers.Count()} players, reason: ") + (matchedToken.Count > 1 ? matchedToken[1] : "Banned by an operator");
            return true;
        }
    }
}
