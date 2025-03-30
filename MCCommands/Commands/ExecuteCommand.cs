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
            commandToken.Values.Add("facing", new CoordinateTokenAllowEntity("entity", allowDecimal: false, onlyOne: true) { Next = new StringToken(() => new string[] {"eyes", "feet"}, "anchors", "incorrect anchors") { Next = commandToken } });
            commandToken.Values.Add("in", new StringToken(StringToken.Dimention_Target, "dimensions", "incorrect dimensions") { Next = commandToken });
            commandToken.Values.Add("positioned", new CoordinateTokenAllowEntity("as") { Next = commandToken });
            commandToken.Values.Add("rotated", new CoordinateTokenAllowEntity("as", onlyOne: true) { Next = commandToken });
            commandToken.Values.Add("summon", new StringToken(StringToken.Character_Target, "entity", "incorrect monster type") { Next = commandToken });
            SubCommandToken ifUnlessToken = new(new()
            {
                { "block", new CoordinateToken("block_pos", false) { Next = new StringToken(StringToken.Item_Target, "block", "block not found") { Next = commandToken } } },
                { "dimension", new StringToken(StringToken.Dimention_Target, "dimensions", "incorrect dimensions") { Next = commandToken } },
                { "entity", new EntityMatchToken { Next = commandToken } },
                { "items", new SubCommandToken(new()
                {
                    { "block", new CoordinateToken("sourcePos", false) {Next = new NumberToken("slot") { Next = commandToken } } },
                    { "entity", new CoordinateToken("source", false) {Next = new NumberToken("slot") { Next = commandToken } } }
                }) },
                { "loaded", new CoordinateToken("pos", false) { Next = commandToken } }
            });
            commandToken.Values.Add("if", ifUnlessToken);
            commandToken.Values.Add("unless", ifUnlessToken);
            commandToken.Values.Add("run", new CommandToken());
        }

        public override bool Execute(List<object> matchedToken, CommandContext context, out string? message)
        {
            message = null;
            switch (matchedToken[0])
            {
                case "align":
                    {
                        string op = (string)matchedToken[1];
                        if (op.Contains('x')) context.Pos.X = (int)context.Pos.X;
                        if (op.Contains('y')) context.Pos.Y = (int)context.Pos.Y;
                        return Execute(matchedToken.GetRange(2, matchedToken.Count - 2), context, out message);
                    }
                case "anchored":
                    {
                        string op = (string)matchedToken[1];
                        if (op == "eyes") context.Pos.Y = context.PositionedEntity.Position.Y - 1;
                        else context.Pos.Y = context.PositionedEntity.Position.Y;
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
                            Vector2 dif2 = block - context.Pos;
                            double angle2 = Math.Atan2(dif2.X, dif2.Y);
                            if (angle2 < 0) angle2 += Math.PI * 2;

                            if (angle2 < Math.PI / 4 || angle2 > Math.PI * 7 / 4) context.Facing = 1;
                            else if (angle2 < Math.PI * 3 / 4) context.Facing = 0;
                            else if (angle2 < Math.PI * 5 / 4) context.Facing = 3;
                            else context.Facing = 2;
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
                                double angle1 = Math.Atan2(dif1.X, dif1.Y);
                                if (angle1 < 0) angle1 += Math.PI * 2;

                                if (angle1 < Math.PI / 4 || angle1 > Math.PI * 7 / 4) context.Facing = 1;
                                else if (angle1 < Math.PI * 3 / 4) context.Facing = 0;
                                else if (angle1 < Math.PI * 5 / 4) context.Facing = 3;
                                else context.Facing = 2;
                                succeeded |= Execute(matchedToken.GetRange(2, matchedToken.Count - 2), new CommandContext(context), out message);
                                sb.AppendLine(message);
                            }
                            message = sb.ToString();
                            return succeeded;
                        }

                        Vector2 dif = targets.First().Position - context.Pos;
                        double angle = Math.Atan2(dif.X, dif.Y);
                        if (angle < 0) angle += Math.PI * 2;

                        if (angle < Math.PI / 4 || angle > Math.PI * 7 / 4) context.Facing = 1;
                        else if (angle < Math.PI * 3 / 4) context.Facing = 0;
                        else if (angle < Math.PI * 5 / 4) context.Facing = 3;
                        else context.Facing = 2;
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
                            context.Pos = pos;
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
                        if (matchedToken[1] is int facing)
                        {
                            if (facing > 4)
                            {
                                message = "Invalid facing direction";
                                return false;
                            }
                            context.Facing = facing;
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
                        string fullTypeName = "StardewValley.Monsters." + matchedToken[1];
                        Type? monsterType = Type.GetType(fullTypeName);
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
                                    Vector2 block = (Vector2)matchedToken[2];
                                    string name = (string)matchedToken[3];
                                    if ((context.Dim.Objects.TryGetValue(block, out Object obj) && (obj.Name == name || obj.DisplayName == name || obj.QualifiedItemId == name || obj.ItemId == name)) ^ ((string)matchedToken[1] == "unless"))
                                    {
                                        return Execute(matchedToken.GetRange(3, matchedToken.Count - 3), context, out message);
                                    }
                                    return true;
                                }
                            case "dimension":
                                {
                                    string dim = (string)matchedToken[2];
                                    if ((context.Dim.Name == dim || context.Dim.DisplayName == dim) ^ ((string)matchedToken[1] == "unless"))
                                    {
                                        return Execute(matchedToken.GetRange(3, matchedToken.Count - 3), context, out message);
                                    }
                                    return true;
                                }
                            case "entity":
                                {
                                    if ((EntityMatchToken.GetEntities((string)matchedToken[2], context) is not null) ^ ((string)matchedToken[1] == "unless"))
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
                                                Vector2 sourcePos = (Vector2)matchedToken[3];
                                                int slot = (int)matchedToken[4];
                                                string name = (string)matchedToken[5];
                                                if (context.Dim.Objects.TryGetValue(sourcePos, out Object obj) && obj is Chest chest)
                                                {
                                                    if (chest.Items.Count < slot)
                                                    {
                                                        message = "Invalid Slot";
                                                        return false;
                                                    }
                                                    Item? item = chest.Items[slot];
                                                    if ((item is not null && (item.DisplayName == name || item.Name == name || item.ItemId == name || item.QualifiedItemId == name)) ^ ((string)matchedToken[1] == "unless"))
                                                    {
                                                        return Execute(matchedToken.GetRange(4, matchedToken.Count - 4), context, out message);
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
                                                    if ((item is not null && (item.DisplayName == name || item.Name == name || item.ItemId == name || item.QualifiedItemId == name)) ^ ((string)matchedToken[1] == "unless"))
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
                                    Vector2 pos = (Vector2)matchedToken[2];
                                    Size mapSize = context.Dim.Map.Layers[0].LayerSize;
                                    if ((pos.X >= mapSize.Width || pos.Y >= mapSize.Height) ^ ((string)matchedToken[1] == "unless"))
                                    {
                                        return Execute(matchedToken.GetRange(3, matchedToken.Count - 3), context, out message);
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
                            return command.Execute(matchedToken, context, out message);
                        }
                        return false;
                    }
            }
            return false;
        }
    }
}
