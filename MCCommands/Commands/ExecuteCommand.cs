using MCCommands.Tokens;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Monsters;
using StardewValley.Objects;
using System.Text;
using xTile.Dimensions;
using Object = StardewValley.Object;

namespace MCCommands.Commands
{
    internal class ExecuteCommand : ICommand
    {
        public ExecuteCommand(IModHelper helper) : base(helper, "execute", "execute", 2, new SubCommandToken(new()))
        {
            SubCommandToken commandToken = FirstToken as SubCommandToken ?? new(new());
            commandToken.Values.Add("align", new StringToken(StringToken.Swiss_Target, "axes", "incorrect axes") { ShowAll = false, Next = commandToken });
            commandToken.Values.Add("anchored", new StringToken(() => new string[] {"eyes", "feet"}, "anchors", "incorrect anchors") { Next = commandToken });
            commandToken.Values.Add("as", new EntityMatchToken { Next = commandToken });
            commandToken.Values.Add("at", new EntityMatchToken { Next = commandToken });
            commandToken.Values.Add("facing", new CoordinateTokenAllowEntity("entity") { Next = commandToken });
            commandToken.Values.Add("in", new StringToken(StringToken.Dimention_Target, "dimensions", "incorrect dimensions") { Next = commandToken });
            commandToken.Values.Add("positioned", new CoordinateTokenAllowEntity("as") { Next = commandToken });
            commandToken.Values.Add("rotated", new CoordinateTokenAllowEntity("as", onlyOne: true) { Next = commandToken });
            commandToken.Values.Add("summon", new StringToken(StringToken.Character_Target, "entity", "incorrect monster type") { Next = commandToken });
            SubCommandToken ifUnlessToken = new(new()
            {
                { "block", new CoordinateToken("block_pos", false).SetNext(new StringToken(StringToken.Item_Target, "block", "block not found") { Next = commandToken } ) },
                { "dimension", new StringToken(StringToken.Dimention_Target, "dimensions", "incorrect dimensions") { Next = commandToken } },
                { "entity", new EntityMatchToken { Next = commandToken, StrictValue = false } },
                { "items", new SubCommandToken(new()
                {
                    { "block", new CoordinateToken("sourcePos", false).SetNext(new NumberToken("slot") { Next = commandToken }) },
                    { "entity", new EntityMatchToken("source") {Next = new NumberToken("slot") { Next = commandToken } } }
                }) },
                { "loaded", new CoordinateToken("pos", false).SetNext(commandToken)}
            });
            commandToken.Values.Add("if", ifUnlessToken);
            commandToken.Values.Add("unless", ifUnlessToken);
            commandToken.Values.Add("run", new CommandToken());
            commandToken.Values.Add("debug", null);
        }

        public override bool Execute(List<object> matchedToken, CommandContext context, out string? message)
        {
            message = null;
            switch (matchedToken[0])
            {
                case "align":
                    {
                        string op = (string)matchedToken[1];
                        if (op.Contains('x')) context.Pos.X = (int)context.Pos.X / Game1.tileSize * Game1.tileSize;
                        if (op.Contains('y')) context.Pos.Y = (int)context.Pos.Y / Game1.tileSize * Game1.tileSize;
                        return Execute(matchedToken.GetRange(2, matchedToken.Count - 2), context, out message);
                    }
                case "anchored":
                    {
                        string op = (string)matchedToken[1];
                        if (op == "eyes") context.Pos.Y = context.PositionedEntity.Position.Y - 64;
                        else context.Pos.Y = context.PositionedEntity.StandingPixel.Y;
                        return Execute(matchedToken.GetRange(2, matchedToken.Count - 2), context, out message);
                    }
                case "as":
                    {
                        IEnumerable<Farmer>? targets = EntityMatchToken.GetPlayers((string)matchedToken[1], context);
                        if (targets == null)
                        {
                            message = "No players found";
                            return false;
                        }
                        
                        if (targets.Count() > 1)
                        {
                            StringBuilder sb = new();
                            bool succeeded = false;
                            foreach (Farmer target in targets)
                            {
                                succeeded |= Execute(matchedToken.GetRange(2, matchedToken.Count - 2), new CommandContext(context) { Player = target }, out message);
                                sb.AppendLine(message);
                            }
                            message = sb.ToString();
                            return succeeded;
                        }

                        context.Player = targets.First();
                        return Execute(matchedToken.GetRange(2, matchedToken.Count - 2), context, out message);
                    }

                case "at":
                    {
                        IEnumerable<Character>? targets = EntityMatchToken.GetEntities((string)matchedToken[1], context);
                        if (targets == null)
                        {
                            message = "No entity found";
                            return false;
                        }

                        if (targets.Count() > 1)
                        {
                            StringBuilder sb = new();
                            bool succeeded = false;
                            foreach (Character target in targets)
                            {
                                succeeded |= Execute(matchedToken.GetRange(2, matchedToken.Count - 2), new CommandContext(context) { Pos = target.Position, PositionedEntity = target, Dim = target.currentLocation, Facing = target.FacingDirection}, out message);
                                sb.AppendLine(message);
                            }
                            message = sb.ToString();
                            return succeeded;
                        }

                        context.Pos = targets.First().Position;
                        context.PositionedEntity = targets.First();
                        context.Dim = targets.First().currentLocation;
                        context.Facing = targets.First().FacingDirection;
                        return Execute(matchedToken.GetRange(2, matchedToken.Count - 2), context, out message);
                    }

                case "facing":
                    {
                        if (matchedToken[1] is Vector2 block)
                        {
                            Vector2 dif2 = block * Game1.tileSize - context.Pos;
                            if (Math.Abs(dif2.X) > Math.Abs(dif2.Y)) context.Facing = dif2.X > 0 ? 1 : 3;
                            else context.Facing = dif2.Y > 0 ? 2 : 0;
                            return Execute(matchedToken.GetRange(2, matchedToken.Count - 2), context, out message);
                        }

                        IEnumerable<Character>? targets = EntityMatchToken.GetEntities((string)matchedToken[1], context);
                        if (targets == null)
                        {
                            message = "No entities found";
                            return false;
                        }

                        if (targets.Count() > 1)
                        {
                            StringBuilder sb = new();
                            bool succeeded = false;
                            foreach (Character target in targets)
                            {
                                Vector2 dif1 = targets.First().Position - context.Pos;
                                if (Math.Abs(dif1.X) > Math.Abs(dif1.Y)) context.Facing = dif1.X > 0 ? 1 : 3;
                                else context.Facing = dif1.Y > 0 ? 2 : 0;
                                succeeded |= Execute(matchedToken.GetRange(2, matchedToken.Count - 2), new CommandContext(context), out message);
                                sb.AppendLine(message);
                            }
                            message = sb.ToString();
                            return succeeded;
                        }

                        Vector2 dif = targets.First().Position - context.Pos;
                        if (Math.Abs(dif.X) > Math.Abs(dif.Y)) context.Facing = dif.X > 0 ? 1 : 3;
                        else context.Facing = dif.Y > 0 ? 2 : 0;
                        return Execute(matchedToken.GetRange(2, matchedToken.Count - 2), context, out message);
                    }

                case "in" :
                    {
                        string dim = (string)matchedToken[1];
                        GameLocation? location = Game1.getLocationFromName(dim);
                        if (location == null)
                        {
                            message = "Location not found";
                            return false;
                        }

                        context.Dim = location;
                        return Execute(matchedToken.GetRange(2, matchedToken.Count - 2), context, out message);
                    }

                case "positioned":
                    {
                        if (matchedToken[1] is Vector2 pos)
                        {
                            context.Pos = pos * Game1.tileSize;
                            return Execute(matchedToken.GetRange(2, matchedToken.Count - 2), context, out message);
                        }

                        IEnumerable<Character>? targets = EntityMatchToken.GetEntities((string)matchedToken[1], context);
                        if (targets == null)
                        {
                            message = "No entities found";
                            return false;
                        }

                        if (targets.Count() > 1)
                        {
                            StringBuilder sb = new();
                            bool succeeded = false;
                            foreach (Character target in targets)
                            {
                                succeeded |= Execute(matchedToken.GetRange(2, matchedToken.Count - 2), new CommandContext(context) { Pos = target.Position, PositionedEntity = target }, out message);
                                sb.AppendLine(message);
                            }
                            message = sb.ToString();
                            return succeeded;
                        }

                        context.PositionedEntity = targets.First();
                        context.Pos = targets.First().Position;
                        return Execute(matchedToken.GetRange(2, matchedToken.Count - 2), context, out message);
                    }

                case "rotated":
                    {
                        if (matchedToken[1] is float facing)
                        {
                            if (facing > 4)
                            {
                                message = "Invalid facing direction";
                                return false;
                            }
                            context.Facing = (int)facing;
                            return Execute(matchedToken.GetRange(2, matchedToken.Count - 2), context, out message);
                        }

                        IEnumerable<Character>? targets = EntityMatchToken.GetEntities((string)matchedToken[1], context);
                        if (targets == null)
                        {
                            message = "No entities found";
                            return false;
                        }

                        if (targets.Count() > 1)
                        {
                            StringBuilder sb = new();
                            bool succeeded = false;
                            foreach (Character target in targets)
                            {
                                succeeded |= Execute(matchedToken.GetRange(2, matchedToken.Count - 2), new CommandContext(context) { Facing = target.FacingDirection }, out message);
                                sb.AppendLine(message);
                            }
                            message = sb.ToString();
                            return succeeded;
                        }

                        context.Facing = targets.First().FacingDirection;
                        return Execute(matchedToken.GetRange(2, matchedToken.Count - 2), context, out message);
                    }
                case "summon":
                    {
                        Type? monsterType = StringToken.GetCharacter((string)matchedToken[1]);
                        if (monsterType is null)
                        {
                            message = matchedToken[1] + " does not exist";
                            return false;
                        }

                        Monster mon = Activator.CreateInstance(monsterType, context.Pos) as Monster;
                        context.Dim.characters.Add(mon);
                        context.Player = mon;
                        return Execute(matchedToken.GetRange(2, matchedToken.Count - 2), context, out message);
                    }
                case "if":
                case "unless":
                    {
                        switch (matchedToken[1])
                        {
                            case "block":
                                {
                                    int x = (int)matchedToken[2], y = (int)matchedToken[3];
                                    Vector2 block = new Vector2(x, y);
                                    string name = (string)matchedToken[4];
                                    if ((context.Dim.Objects.TryGetValue(block, out Object obj) && (obj.Name == name || obj.DisplayName == name || obj.QualifiedItemId == name || obj.ItemId == name)) ^ ((string)matchedToken[0] == "unless"))
                                    {
                                        return Execute(matchedToken.GetRange(5, matchedToken.Count - 5), context, out message);
                                    }
                                    return true;
                                }
                            case "dimension":
                                {
                                    string dim = (string)matchedToken[2];
                                    if ((context.Dim.Name == dim || context.Dim.DisplayName == dim) ^ ((string)matchedToken[0] == "unless"))
                                    {
                                        return Execute(matchedToken.GetRange(3, matchedToken.Count - 3), context, out message);
                                    }
                                    return true;
                                }
                            case "entity":
                                {
                                    if ((EntityMatchToken.GetEntities((string)matchedToken[2], context) is not null) ^ ((string)matchedToken[0] == "unless"))
                                    {
                                        return Execute(matchedToken.GetRange(3, matchedToken.Count - 3), context, out message);
                                    }
                                    return true;
                                }
                            case "items":
                                {
                                    switch (matchedToken[2])
                                    {
                                        case "block":
                                            {
                                                int x = (int)matchedToken[2], y = (int)matchedToken[3];
                                                Vector2 sourcePos = new Vector2(x, y);
                                                int slot = (int)matchedToken[5];
                                                string name = (string)matchedToken[6];
                                                if (context.Dim.Objects.TryGetValue(sourcePos, out Object obj) && obj is Chest chest)
                                                {
                                                    if (chest.Items.Count < slot)
                                                    {
                                                        message = "Invalid Slot";
                                                        return false;
                                                    }
                                                    Item? item = chest.Items[slot];
                                                    if ((item is not null && (item.DisplayName == name || item.Name == name || item.ItemId == name || item.QualifiedItemId == name)) ^ ((string)matchedToken[0] == "unless"))
                                                    {
                                                        return Execute(matchedToken.GetRange(7, matchedToken.Count - 7), context, out message);
                                                    }
                                                }
                                                return true;
                                            }
                                        case "entity":
                                            {
                                                IEnumerable<Farmer>? temp = EntityMatchToken.GetPlayers((string)matchedToken[3], context);
                                                if (temp == null)
                                                {
                                                    message = "target not found";
                                                    return false;
                                                }

                                                int slot = (int)matchedToken[4];
                                                string name = (string)matchedToken[5];
                                                bool success = false;
                                                foreach (Farmer c in temp)
                                                {
                                                    Item item = null;
                                                    if (slot >= 0 && slot < 36)
                                                    {
                                                        if (c.Items.Count > slot) item = c.Items[slot];
                                                    }
                                                    else if (slot == 100)
                                                    {
                                                        item = c.boots.Value;
                                                    }
                                                    else if (slot == 101)
                                                    {
                                                        item = c.pantsItem.Value;
                                                    }
                                                    else if (slot == 102)
                                                    {
                                                        item = c.shirtItem.Value;
                                                    }
                                                    else if (slot == 103)
                                                    {
                                                        item = c.hat.Value;
                                                    }
                                                    else if (slot >= 200 && slot <= 209)
                                                    {
                                                        item = c.team.GetOrCreateGlobalInventory("JunimoChests")[slot - 200];
                                                    }
                                                    else
                                                    {
                                                        message = "invalid slot";
                                                        return false;
                                                    }
                                                    if ((item is not null && (item.DisplayName == name || item.Name == name || item.ItemId == name || item.QualifiedItemId == name)) ^ ((string)matchedToken[0] == "unless"))
                                                    {
                                                        return Execute(matchedToken.GetRange(4, matchedToken.Count - 4), context, out message);
                                                    }
                                                }
                                                return true;
                                            }
                                    }
                                    break;
                                }
                            case "loaded":
                                {
                                    int x = (int)matchedToken[2], y = (int)matchedToken[3];
                                    Vector2 pos = new Vector2(x, y);
                                    Size mapSize = context.Dim.Map.Layers[0].LayerSize;
                                    if (!(pos.X >= mapSize.Width || pos.Y >= mapSize.Height) ^ ((string)matchedToken[0] == "unless"))
                                    {
                                        return Execute(matchedToken.GetRange(4, matchedToken.Count - 4), context, out message);
                                    }
                                    return true;
                                }
                        }
                        return false;
                    }
                case "run":
                    {
                        if (matchedToken[1] is ICommand command)
                        {
                            matchedToken.RemoveAt(0);
                            matchedToken.RemoveAt(0);
                            return command.Execute(matchedToken, context, out message);
                        }
                        return false;
                    }
                case "debug":
                    {
                        StringBuilder sb = new StringBuilder("source: ").AppendLine(context.Player.displayName);
                        sb.AppendLine("position: " + context.Pos);
                        sb.AppendLine("facing: " + context.Facing);
                        sb.AppendLine("dimension: " + context.Dim.Name);
                        sb.AppendLine("positioned entity: " + context.PositionedEntity?.displayName);
                        message = sb.ToString();
                        return true;
                    }
            }
            return false;
        }
    }
}
