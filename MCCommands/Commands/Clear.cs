using MCCommands.Tokens;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Inventories;
using System.Text;

namespace MCCommands.Commands
{
    internal class Clear : ICommand
    {
        public Clear(IModHelper helper) : base(helper, "clear", "clear [<targets>] [<item>] [<maxCount>]", 2,
            new StringToken(StringToken.Player_Target, "target", "Player not found") 
            {
                IsOptional = true,
                Next = new StringToken(StringToken.Item_Target, "item", "Item not Found")
                {
                    IsOptional = true,
                    StrictValue = false,
                    Next = new NumberToken("maxCount") { IsOptional = true },
                }
            }) { }

        public static bool DoClear(Farmer farmer, Item? item, int limit, ref StringBuilder message)
        {
            int removed = 0;
            Inventory inv = farmer.Items;
            for (int i = 0; i < inv.Count; i++)
            {
                if (inv[i] == null) continue;
                if (item != null && inv[i].QualifiedItemId != item.QualifiedItemId) continue;

                if (limit < inv[i].Stack)
                {
                    removed += limit;
                    inv[i].Stack -= limit;
                    break;
                }
                else
                {
                    removed += inv[i].Stack;
                    limit -= inv[i].Stack;
                    inv[i] = null;
                }
            }
            if (removed > 0)
            {
                message.Append("Removed ").Append(removed).Append(" item(s) from player ").AppendLine(farmer.displayName);//message = $"Removed {itemsRemoved} item(s) from player {context.Player.displayName}";
                return true;
            }
            else
            {
                message.Append("No items were found on player ").AppendLine(farmer.displayName);
                     return false;
            }
        }

        public override bool Execute(List<object> matchedToken, CommandContext context, out string? message)
        {
            StringBuilder sb = new();
            Item? target;
            int limit;
            IEnumerable<Farmer> farmers;
            bool atLeasedClearedSomething = false;

            if (matchedToken.Count > 0)
            {
                farmers = StringToken.DecodeFarmer(StringToken.DecodePlayerTarget(matchedToken[0] as string ?? ""), out message) ?? Array.Empty<Farmer>();
                if (!farmers.Any()) return false;
                if (matchedToken.Count > 1)
                {

                    string id = matchedToken[1] as string ?? "";
                    target = ItemRegistry.Create(id, allowNull: true);
                    target ??= Utility.fuzzyItemSearch(id);
                    target ??= Utility.fuzzyItemSearch(id, useLocalizedNames: true);
                    if (target is null)
                    {
                        message = $"Unable to find item \"{id}\"";
                        return false;
                    }

                    if (matchedToken.Count > 2) limit = (int)matchedToken[2];
                    else limit = 1145141919;
                }
                else
                {
                    target = null;
                    limit = 1145141919;
                }
            }
            else
            {
                farmers = new Farmer[] { context.Player };
                target = null;
                limit = 1145141919;
            }

            foreach (Farmer farmer in farmers) atLeasedClearedSomething |= DoClear(farmer, target, limit, ref sb);
            message = sb.ToString();

            if (!atLeasedClearedSomething) message = "No Item Found";
            return atLeasedClearedSomething;
        }
    }
}
