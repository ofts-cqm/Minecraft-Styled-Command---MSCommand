using MCCommands.Tokens;
using StardewModdingAPI;
using StardewValley;
using StardewValley.GameData.Buffs;
using StardewValley.Monsters;
using StardewValley.TokenizableStrings;
using System.Text;

namespace MCCommands.Commands
{
    internal class Effect : ICommand
    {
        public static EntityMatchToken GiveToken = new("target");
        public static EntityMatchToken ClearToken = new("target") { IsOptional = true };
        public static Dictionary<string, string> EffectData = new();

        public Effect(IModHelper helper) : base(helper, "effect", "effect <give|clear>", 2, new SubCommandToken(new Dictionary<string, IToken?>() 
        {
            { "give", GiveToken },
            { "clear", ClearToken }
        }))
        {
            GiveToken.NextToken(new StringToken(EffectData.Keys.ToArray, "effect", "effect not found")).NextToken(new NumberToken("seconds", allowInf: true) { IsOptional = true });
            ClearToken.NextToken(new StringToken(EffectData.Keys.ToArray, "effect", "effect not found") { IsOptional = true });
            helper.Events.GameLoop.GameLaunched += UpdateEffects;
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
                        if (!EffectData.TryGetValue((string)matchedToken[2], out buff))
                        {
                            message = "effect not found";
                            return false;
                        }
                    }
                }
                else temp = new Farmer[] { Game1.player };

                StringBuilder sb = new();
                foreach (Farmer f in temp)
                {
                    if (!f.buffs.AppliedBuffIds.Any()) continue;
                    sb.Append(f.displayName).Append(' ');
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
            int duration = (int)matchedToken[3];
            if (!EffectData.TryGetValue((string)matchedToken[2], out string id)){
                message = "effect not found";
                return false;
            }
            foreach (Farmer f in fmrs)
            {
                if (!f.buffs.AppliedBuffIds.Contains(id))
                {
                    f.applyBuff(new Buff(id, source: "MS-Command", displaySource: "MS-Command", duration: duration));
                }
            }
            message = $"Applied buff to {fmrs.Count()} players";
            return true;
        }

        public static void UpdateEffects(object? sender, EventArgs _)
        {
            EffectData.Clear();
            Dictionary<string, int> counter = new();
            foreach (KeyValuePair<string, BuffData> pair in DataLoader.Buffs(Game1.content)) 
            {
                string name = TokenParser.ParseText(pair.Value.DisplayName);
                if (counter.ContainsKey(name))
                {
                    counter[name]++;
                    if (counter[name] == 1) 
                    {
                        EffectData.Add(name.Replace(" ", "") + "_0", EffectData[name.Replace(" ", "")]);
                        EffectData.Remove(name.Replace(" ", ""));
                    }
                    name += "_" + counter[name];
                }
                else counter.Add(name, 0);
                EffectData.Add(name.Replace(" ", ""), pair.Key);
            }
            //foreach(KeyValuePair<string, string> pair in EffectData)
            //    Monitor.Log("Added Effect " + pair. Key, LogLevel.Info);
        }
    }
}
