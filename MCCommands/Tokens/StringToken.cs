using StardewValley;
using StardewValley.Extensions;
using StardewValley.ItemTypeDefinitions;

namespace MCCommands.Tokens
{
    internal class StringToken : LinearToken
    {
        public Func<string[]?> AllValues;

        public StringToken(Func<string[]?> allValues, string tokenName, string errorMessage) : base(tokenName, errorMessage)
        {
            AllValues = allValues;
        }

        public override IEnumerable<string>? GetAllValues() => AllValues.Invoke();

        public override bool IsAllowedValue(string value) 
        {
            var a = AllValues.Invoke();
            return AllValues.Invoke()?.Contains(value) ?? true;
        }

        public override bool MatchToken(List<string> args, out object? readValue, out string? error)
        {
            if (!base.MatchToken(args, out readValue, out error)) return false;
            if (args.Count > 0)
            {
                readValue = args[0];
                args.RemoveAt(0);
            }
            return true;
        }

        [Obsolete]
        public static string[] Player_Target()
        {
            List<string> players = new() { "@a", "@e", "@n", "@p", "@r", "@s" };
            foreach (Farmer farmer in Game1.getOnlineFarmers())
            {
                players.Add(farmer.displayName);
            }
            return players.ToArray();
        }

        [Obsolete]
        public static IEnumerable<Farmer>? DecodeFarmer(IEnumerable<Character>? temp, out string message)
        {
            if (temp is null)
            {
                message = "Player not found";
                return null;
            }
            List<Farmer> farmers = new List<Farmer>();
            foreach (Character c in temp) if (c is Farmer f) farmers.Add(f);
            if (!farmers.Any())
            {
                message = "Player not found";
                return null;
            }
            message = "";
            return farmers;
        }

        [Obsolete]
        public static IEnumerable<Character>? DecodePlayerTarget(string player)
        {
            if (player.StartsWith('@') && player.Length == 2)
            {
                switch (player[1])
                {
                    case 'a':
                        return Game1.getOnlineFarmers();
                    case 'e':
                        {
                            HashSet<Character> result = new(Game1.getOnlineFarmers());
                            result.AddRange(Game1.player.currentLocation.characters);
                            return result;
                        }
                    case 'n':
                        {
                            HashSet<Character> entities = new(Game1.getOnlineFarmers());
                            entities.AddRange(Game1.player.currentLocation.characters);
                            float curPos = 1145141919810;
                            Character? current = null;
                            foreach (Character c in entities)
                            {
                                if (c != Game1.player && (c.Position - Game1.player.Position).Length() < curPos)
                                {
                                    current = c;
                                    curPos = (c.Position - Game1.player.Position).Length();
                                }
                            }
                            return current == null ? null : new Character[] { current };
                        }
                    case 'p':
                        {
                            float curPos = 1145141919810;
                            Character? current = null;
                            foreach (Character c in Game1.currentLocation.characters)//getOnlineFarmers())
                            {
                                if (c != Game1.player && (c.Position - Game1.player.Position).Length() < curPos)
                                {
                                    current = c;
                                    curPos = (c.Position - Game1.player.Position).Length();
                                }
                            }
                            return current == null ? null : new Character[] { current };
                        }
                    case 'r': return new Farmer[] {new Random().ChooseFrom(new List<Farmer>(Game1.getOnlineFarmers()))};
                    case 's': return new Farmer[] { Game1.player };
                    default: return null;
                }
            }

            foreach (Farmer f in Game1.getOnlineFarmers()) if (f.displayName == player) return new Farmer[] { f };
            return null;
        }

        public static string[] Item_Target()
        {
            List<string> ids = new();
            foreach (IItemDataDefinition def in ItemRegistry.ItemTypes)
            {
                ids.AddRange(def.GetAllIds());
            }
            return ids.ToArray();
        }
    }
}
