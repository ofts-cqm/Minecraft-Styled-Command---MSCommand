using MCCommands.Tokens;
using StardewModdingAPI;
using StardewValley;

namespace MCCommands.Commands
{
    internal class OP : ICommand
    {
        public IModHelper Helper;

        public OP(IModHelper helper) : base(helper, "op", "op <targets>", 3, new StringToken(() => null, "target", "Target Not Found") { StrictValue = false })
        {
            Helper = helper;
            helper.Events.GameLoop.SaveLoaded += ReadOPs;
            helper.Events.GameLoop.Saving += SaveOPs;
            helper.Events.GameLoop.SaveCreated += (_, _) => ModEntry.OPs.Clear();
        }

        public void ReadOPs(object? sender, EventArgs _)
        {
            ModEntry.OPs.CopyFrom(Helper.Data.ReadSaveData<Dictionary<long, int>>("ops") ?? new() { { Game1.player.UniqueMultiplayerID, 4 } });
        }

        public void SaveOPs(object? sender, EventArgs _)
        {
            Helper.Data.WriteSaveData("ops", ModEntry.OPs);
        }

        public override bool Execute(List<object> matchedToken, CommandContext context, out string? message)
        {
            IEnumerable<Farmer>? targets = EntityMatchToken.GetPlayers(matchedToken[0] as string, context);
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
                if (ModEntry.OPs.ContainsKey(target.UniqueMultiplayerID)) {
                    ModEntry.OPs.Remove(target.UniqueMultiplayerID);
                }
                ModEntry.OPs.Add(target.UniqueMultiplayerID, ModEntry.serverProperty.DefaultOpLevel);
                message += $"Player {target.displayName} is now an Op!\n";
            }

            return true;
        }
    }
}
