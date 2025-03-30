using MCCommands.Tokens;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Enchantments;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCCommands.Commands
{
    internal class Enchant : ICommand
    {
        public Enchant(IModHelper helper) : base(helper, "enchant", "enchant <targets> <enchantment> [<level>]", 2, new EntityMatchToken())
        {
            ((LinearToken)FirstToken).NextToken(new StringToken(StringToken.Enchantment_Target, "enchantment", "enchantment not found")).NextToken(new NumberToken("level", 32767, 0, true) { IsOptional = true });
        }

        public override bool Execute(List<object> matchedToken, CommandContext context, out string? message)
        {
            IEnumerable<Farmer>? targets = EntityMatchToken.GetPlayers((string)matchedToken[0], context);
            BaseEnchantment? enchantment = StringToken.GetEnchantment((string)matchedToken[1]);
            int level = matchedToken.Count > 2 ? (int)matchedToken[2] : 1;

            if (targets == null)
            {
                message = "player not found";
                return false;
            }

            if (enchantment == null)
            {
                message = "enchantment not found";
                return false;
            }
            enchantment.Level = level;
            StringBuilder sb = new();
            string enchantmentName = (string)matchedToken[1];
            foreach (Farmer target in targets) 
            {
                Tool? item = target.ActiveItem as Tool;
                if (item == null) 
                {
                    sb.Append(target.displayName).AppendLine(" does not hold a tool");
                    continue;
                }
                if (!item.CanAddEnchantment(enchantment) || !enchantment.CanApplyTo(item)) 
                {
                    sb.Append("cannot apply ").Append(enchantmentName).Append(" to ").AppendLine(item.DisplayName);
                    continue;
                }

                item.AddEnchantment(enchantment);
                sb.Append("Applied ").Append(enchantmentName).Append(" to ").AppendLine(item.DisplayName);
            }
            message = sb.ToString();
            return true;
        }
    }
}
