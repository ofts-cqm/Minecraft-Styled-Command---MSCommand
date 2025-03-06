
using MCCommands.Tokens;
using StardewModdingAPI;
using StardewValley;

namespace MCCommands.Commands
{
    internal class Advancement : ICommand
    {
        public static Dictionary<string, int> AdvancementData = new();

        private static readonly StringToken AdvToken = new(() => AdvancementData.Keys.ToArray(), "advancement", "Advancement Not Found");

        public Advancement(IModHelper helper) : base(
            helper, "advancement", "advancement (grant|revoke) <targets> (everything|only)",
            new StringToken(() => new string[] {"grant", "revoke"}, "(grant|revoke)", "Incorrect Argument for Command")
            {
                ShowAll = true,
                Next = new EntityMatchToken("target", "Player not found")
                {
                    Next = new SubCommandToken(new Dictionary<string, IToken?>() {
                        {"everything", null },
                        { "only", AdvToken },
                    })
                }
            })
        {
            helper.Events.Content.LocaleChanged += UpdateAchievements;
            helper.Events.GameLoop.GameLaunched += UpdateAchievements;
        }

        public override bool Execute(List<object> matchedToken, CommandContext context, out string? message)
        {
            bool grant = matchedToken[0] as string == "grant";
            IEnumerable<Farmer>? farmers = EntityMatchToken.GetPlayers((string)matchedToken[1], context.Dim, context.Player);
            if (farmers == null || !farmers.Any())
            {
                message = "Player not found";
                return false;
            }
            message = "";

            if (matchedToken[2] as string == "everything")
            {
                foreach (Farmer farmer in farmers)
                {
                    if (!grant) farmer.achievements.Clear();
                    else foreach (KeyValuePair<string, int> pair in AdvancementData) Game1.getAchievement(pair.Value);
                }
                return true;
            }
            
            if (AdvancementData.TryGetValue(matchedToken[3] as string ?? "", out int id))
            {
                foreach (Farmer farmer in farmers)
                {
                    if (grant) farmer.achievements.Add(id);
                    else farmer.achievements.Remove(id);
                }
                return true;
            }
            message = "Advancement not found";
            return false;
        }

        public static void UpdateAchievements(object? sender, EventArgs _)
        {
            AdvancementData.Clear();
            foreach (KeyValuePair<int, string> pair in Game1.achievements) AdvancementData.Add(pair.Value.Split('^')[0].Trim(), pair.Key);
        }
    }
}
