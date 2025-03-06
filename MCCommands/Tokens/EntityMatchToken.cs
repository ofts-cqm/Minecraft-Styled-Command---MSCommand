using StardewValley;
using StardewValley.Extensions;

namespace MCCommands.Tokens
{
    internal class EntityMatchToken : IToken
    {
        public IToken? Next { get; set; }

        public EntityMatchToken(string tokenName, string errorMessage = "Entity Not Found") : base(tokenName, errorMessage)
        {
        }

        public override IEnumerable<string>? GetAllValues()
        {
            List<string> players = new() { "@a", "@e", "@n", "@p", "@r", "@s" };
            foreach (Farmer farmer in Game1.getOnlineFarmers())
            {
                players.Add(farmer.displayName);
            }
            return players.ToArray();
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

        public override IToken? GetNextToken(object? readValue) => Next;

        public override bool IsAllowedValue(string value)
        {
            return GetAllValues()?.Contains(value) ?? true;
        }

        public static IEnumerable<Character>? GetEntities(string pattern, GameLocation location, Farmer player)
        {
            if (pattern.StartsWith('@') && pattern.Length == 2)
            {
                switch (pattern[1])
                {
                    case 'a':
                        return Game1.getOnlineFarmers();
                    case 'e':
                        {
                            HashSet<Character> result = new(Game1.getOnlineFarmers());
                            result.AddRange(location.characters);
                            return result;
                        }
                    case 'n':
                        {
                            HashSet<Character> entities = new(Game1.getOnlineFarmers());
                            entities.AddRange(location.characters);
                            float curPos = 1145141919810;
                            Character? current = null;
                            foreach (Character c in entities)
                            {
                                if (c != player && c.currentLocation == player.currentLocation && (c.Position - player.Position).Length() < curPos)
                                {
                                    current = c;
                                    curPos = (c.Position - player.Position).Length();
                                }
                            }
                            return current == null ? null : new Character[] { current };
                        }
                    case 'p':
                        {
                            float curPos = 1145141919810;
                            Character? current = null;
                            foreach (Character c in location.characters)//getOnlineFarmers())
                            {
                                if (c != player && c.currentLocation == player.currentLocation && (c.Position - player.Position).Length() < curPos)
                                {
                                    current = c;
                                    curPos = (c.Position - player.Position).Length();
                                }
                            }
                            return current == null ? null : new Character[] { current };
                        }
                    case 'r': return new Farmer[] { new Random().ChooseFrom(new List<Farmer>(Game1.getOnlineFarmers())) };
                    case 's': return new Farmer[] { player };
                    default: return null;
                }
            }

            foreach (Farmer f in Game1.getOnlineFarmers()) if (f.displayName == pattern) return new Farmer[] { f };
            return null;
        }

        public static IEnumerable<Farmer>? GetPlayers(string pattern, GameLocation location, Farmer player)
        {
            IEnumerable<Character>? temp = GetEntities(pattern, location, player);
            if (temp is null) return null;
            List<Farmer> farmers = new List<Farmer>();
            foreach (Character c in temp) if (c is Farmer f) farmers.Add(f);
            if (!farmers.Any()) return null;
            return farmers;
        }

        public IToken next(IToken next)
        {
            Next = next;
            return next;
        }
    }
}
