using MCCommands.Tokens;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Monsters;
using System.Text;

namespace MCCommands.Commands
{
    internal class Effect : ICommand
    {
        public static EntityMatchToken GiveToken = new("target");
        public static EntityMatchToken ClearToken = new("target");

        public Effect(IModHelper helper) : base(helper, "effect", "effect <give|clear>", 2, new SubCommandToken(new Dictionary<string, IToken?>() 
        {
            { "give", GiveToken },
            { "clear", ClearToken }
        }))
        {
            GiveToken.NextToken(new EntityMatchToken()).NextToken(new StringToken(StringToken.Buff_Target, "effect", "effect not found")).NextToken(new NumberToken("seconds", allowInf: true) { IsOptional = true });
            ClearToken.NextToken(new EntityMatchToken() { IsOptional = true }).NextToken(new StringToken(StringToken.Buff_Target, "effect", "effect not found") { IsOptional = true });
        }

        public void clearEffect(Farmer target, string? effect, StringBuilder sb)
        {
            if (effect == null)
            {
                foreach (string b in target.buffs.AppliedBuffIds.ToArray())
                {
                    target.buffs.Remove(b);
                }
                return;
            }
            target.buffs.Remove(effect);
        }

        public override bool Execute(List<object> matchedToken, CommandContext context, out string? message)
        {
            if ((string)matchedToken[0] == "clear")
            {
                IEnumerable<Farmer>? temp;
                string? buff = null;
                if (matchedToken.Count > 1)
                {
                    temp = EntityMatchToken.GetPlayers((string)matchedToken[1], context.Player.currentLocation, context.Player);
                    if (temp == null)
                    {
                        message = "target not found";
                        return false;
                    }

                    if (matchedToken.Count > 2)
                    {
                        buff = (string)matchedToken[2];
                    }
                }
                else temp = new Farmer[] { Game1.player };

                StringBuilder sb = new();
                foreach (Farmer f in temp)
                {
                    if (!f.buffs.AppliedBuffIds.Any()) continue;
                    clearEffect(f, buff, sb); 
                }
                if (sb.Length == 0)
                {
                    message = "No effect to clear";
                    return false;
                }

                message = "cleared effects for " + sb.ToString();
                return true;
            }

            IEnumerable<Farmer>? fmrs = EntityMatchToken.GetPlayers((string)matchedToken[1], context.Player.currentLocation, context.Player);
            if (fmrs == null)
            {
                message = "target not found";
                return false;
            }
            int duration = (int)matchedToken[2];
            foreach (Farmer f in fmrs)
            {
                if (!f.buffs.AppliedBuffIds.Contains((string)matchedToken[3]))
                {
                    f.applyBuff(new Buff((string)matchedToken[3], source: "MS-Command", displaySource: "MS-Command", duration: duration));
                }
            }
            message = $"Applied buff to {fmrs.Count()} players";
            return true;
        }
    }
}
