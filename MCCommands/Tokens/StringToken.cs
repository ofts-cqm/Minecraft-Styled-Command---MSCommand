using StardewModdingAPI;
using StardewValley;
using StardewValley.Enchantments;
using StardewValley.Extensions;
using StardewValley.GameData.Buffs;
using StardewValley.ItemTypeDefinitions;
using StardewValley.Monsters;
using System.Reflection;

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
                    case 'r': return new Farmer[] { new Random().ChooseFrom(new List<Farmer>(Game1.getOnlineFarmers())) };
                    case 's': return new Farmer[] { Game1.player };
                    default: return null;
                }
            }

            foreach (Farmer f in Game1.getOnlineFarmers()) if (f.displayName == player) return new Farmer[] { f };
            return null;
        }

        private static string[] ParsedItemTarget = Array.Empty<string>();

        public static void Internal_Item_Target(object? sender, EventArgs _)
        {
            List<string> ids = new();
            foreach (IItemDataDefinition def in ItemRegistry.ItemTypes)
            {
                ids.AddRange(def.GetAllIds());
                foreach (string id in def.GetAllIds())
                {
                    ParsedItemData item = def.GetData(id);
                    ids.Add(item.InternalName);
                    ids.Add(item.DisplayName);
                }
            }
            ParsedItemTarget = ids.ToArray();
        }

        public static string[] Item_Target() => ParsedItemTarget;

        private static string[] ParsedBuffTarget = Array.Empty<string>();

        public static void Internal_Buff_Target(object? sender, EventArgs _)
        {
            Dictionary<string, BuffData> dict = DataLoader.Buffs(Game1.content);
            List<string> temp = new(dict.Keys);
            foreach (BuffData data in dict.Values) temp.Add(data.DisplayName);
        }

        public static string[] Buff_Target() => ParsedBuffTarget;

        private static Dictionary<string, Type> ParsedEnchantmentTarget = new();
        private static Dictionary<string, Type> ParsedCharacterTarget = new();

        public static void Internal_Enchantment_Target(object? sender, EventArgs _)
        {
            foreach(Type t in Assembly.LoadFrom(Path.Combine(Constants.GamePath, "Stardew Valley.dll")).GetTypes().Where(t => t.IsSubclassOf(typeof(BaseEnchantment)) || t.IsSubclassOf(typeof(Monster))))
            {
                if (t.IsSubclassOf(typeof(BaseEnchantment))) ParsedEnchantmentTarget.Add(t.Name, t);
                else ParsedCharacterTarget.Add(t.Name, t);
            }
        }

        public static string[] Enchantment_Target() => ParsedEnchantmentTarget.Keys.ToArray();
        public static string[] Character_Target() => ParsedCharacterTarget.Keys.ToArray();

        public static BaseEnchantment? GetEnchantment(string name) => ParsedEnchantmentTarget.TryGetValue(name, out Type? t) ? t.GetConstructor(Array.Empty<Type>())?.Invoke(null) as BaseEnchantment ?? null : null;

        public static string[] Swiss_Target() => new string[]{ "x" , "y", "xy", "yx"};

        public static string[] Dimention_Target() => ModEntry.ModHelper.Multiplayer.GetActiveLocations().Select(l => l.Name).ToArray();
    }
}
