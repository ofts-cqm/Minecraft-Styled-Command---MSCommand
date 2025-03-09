using MCCommands.Tokens;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.BellsAndWhistles;
using StardewValley.Network;
using System.Runtime.InteropServices;
using System.Text;
using System.Xml.Serialization;

namespace MCCommands.Commands
{
    public class BossBarData : INetObject<NetFields>
    {
        [XmlIgnore]
        public NetFields NetFields { get; }

        [XmlElement("color")]
        public NetInt color = new();
        [XmlElement("style")]
        public NetInt style = new();
        [XmlElement("max")]
        public NetInt max = new();
        [XmlElement("name")]
        public NetString name = new();
        [XmlElement("visibility")]
        public NetBool visibility = new();
        [XmlElement("value")]
        public NetInt value = new();
        [XmlElement("plauers")]
        public NetString players = new();

        [XmlIgnore]
        public int Color { get => color.Value; set => color.Value = value; }
        [XmlIgnore]
        public int Style { get => style.Value; set => style.Value = value; }
        [XmlIgnore]
        public int Max { get => max.Value; set => max.Value = value; }
        [XmlIgnore]
        public string Name { get => name.Value; set => name.Value = value; }
        [XmlIgnore]
        public bool Visibility { get => visibility.Value; set => visibility.Value = value; }
        [XmlIgnore]
        public int Value { get => value.Value; set => this.value.Value = value; }
        [XmlIgnore]
        public string Players { get => players.Value; set => players.Value = value; }

        public BossBarData()
        {
            Color = 0;
            Style = 4;
            Max = 100;
            Name = "Entity";
            Visibility = true;
            Value = 0;
            Players = "";
            NetFields = new NetFields(NetFields.GetNameForInstance(this)).SetOwner(this)
                .AddField(color, "color")
                .AddField(style, "style")
                .AddField(max, "max")
                .AddField(name, "name")
                .AddField(visibility, "visibility")
                .AddField(value, "value")
                .AddField(players, "players");
        }

        public bool IsVisibleToPlayer(Farmer player) => EntityMatchToken.GetPlayers(Players, player.currentLocation, player)?.Contains(player) ?? false;
    }

    internal class BossBar : ICommand
    {
        public static readonly NetStringDictionary<BossBarData, NetRef<BossBarData>> BossBars = new();
        public static readonly NetStringList RegisteredIDs = new();
        public static PerScreen<List<BossBarData>> VisibleBossBars = new(() => new List<BossBarData>());

        public static IModHelper Helper;
        public static Texture2D BarTexture;

        public BossBar(IModHelper helper) : base(helper, "bossbar", "bossbar <add|get|list|remove|set>", 2, new SubCommandToken(new()
        {
            {
                "add", 
                new StringToken(() => null, "id", "")
                {
                    StrictValue = false,
                    Next = new StringToken(() => null, "name", "") { StrictValue = false }
                }
            },

            {
                "get",
                new StringToken(() => RegisteredIDs.ToArray(), "id", "")
                {
                    StrictValue = true,
                    Next = new SubCommandToken(new()
                    {
                        { "max", null },
                        { "players", null},
                        { "value", null },
                        { "visible", null }
                    })
                }
            },

            { "list", null },

            {
                "remove",
                new StringToken(() => RegisteredIDs.ToArray(), "id", "") { StrictValue = false }
            },

            {
                "set",
                new StringToken(() => RegisteredIDs.ToArray(), "id", "")
                {
                    StrictValue = true,
                    Next = new SubCommandToken(new()
                    {
                        { 
                            "color", 
                            new SubCommandToken(new()
                            {
                                { "blue", null },
                                { "green", null },
                                { "pink", null },
                                { "purple", null },
                                { "red", null },
                                { "white", null },
                                { "yellow", null }
                            }) 
                        },
                        { "max", new NumberToken("max") },
                        { "name", new StringToken(() => null, "name", "") },
                        { "players", new EntityMatchToken("targets", "Player Not Found") { IsOptional = true } },
                        { 
                            "style",
                            new SubCommandToken(new()
                            {
                                { "notched_6", null },
                                { "notched_10", null },
                                { "notched_12", null },
                                { "notched_20", null },
                                { "progress", null },
                            })
                        },
                        { "value", new NumberToken("value") },
                        { "visible", new BoolToken("visible") }
                    })
                }
            },
        }))
        {
            helper.Events.GameLoop.SaveLoaded += ReadBossBar;
            helper.Events.GameLoop.SaveCreated += (_, _) => { BossBars.Clear(); RegisteredIDs.Clear(); };
            helper.Events.GameLoop.Saving += SaveBossBar;
            helper.Events.GameLoop.OneSecondUpdateTicked += UpdateBossBars;
            helper.Events.Display.RenderingHud += DrawBossBar;
            Helper = helper;
            BarTexture = helper.ModContent.Load<Texture2D>("assets/bars");
        }

        public struct TemperoryRecord
        {
            public string Name;
            public int Value;
            public int Max;
            public int Color;
            public int Style;
            public bool Visibility;
            public string Players;

            public TemperoryRecord(BossBarData data)
            {
                Name = data.Name;
                Value = data.Value;
                Max = data.Max;
                Color = data.Color;
                Style = data.Style;
                Visibility = data.Visibility;
                Players = data.Players;
            }
        }

        public static void SaveBossBar(object? sender, SavingEventArgs e)
        {
            Dictionary<string, TemperoryRecord> BossBars2 = new();
            foreach (string id in RegisteredIDs)
            {
                BossBarData data = BossBars[id];
                BossBars2.Add(id, new TemperoryRecord(data));
            }
            Helper.Data.WriteSaveData("BossBars", BossBars2);
        }

        public static void ReadBossBar(object? sender, SaveLoadedEventArgs e)
        {
            BossBars.Clear();
            foreach (KeyValuePair<string, TemperoryRecord> p in Helper.Data.ReadSaveData<Dictionary<string, TemperoryRecord>>("BossBars") ?? new())
            {
                BossBars.Add(p.Key, new BossBarData
                {
                    Name = p.Value.Name,
                    Value = p.Value.Value,
                    Max = p.Value.Max,
                    Color = p.Value.Color,
                    Style = p.Value.Style,
                    Visibility = p.Value.Visibility,
                    Players = p.Value.Players
                });
            }
            RegisteredIDs.Clear();
            foreach (string id in BossBars.Keys) RegisteredIDs.Add(id.ToString());
            UpdateBossBars(null, null);
        }

        public static void UpdateBossBars(object? sender, OneSecondUpdateTickedEventArgs? _)
        {
            VisibleBossBars.Value.Clear();
            if (!Context.IsWorldReady) return;
            foreach (BossBarData data in BossBars.Values)
            {
                if (VisibleBossBars.Value.Count > 3) return;
                if (data.Visibility && data.IsVisibleToPlayer(Game1.player)) VisibleBossBars.Value.Add(data);
            }
        }

        public static void DrawBossBar(object? sender, RenderingHudEventArgs e)
        {
            int position = GetStartingPixel();
            SpriteBatch sb = e.SpriteBatch;
            for (int i = 0; i < VisibleBossBars.Value.Count; i++)
            {
                BossBarData data = VisibleBossBars.Value[i];
                sb.DrawString(Game1.smallFont, data.Name, new Vector2((Game1.viewport.Width - Game1.smallFont.MeasureString(data.Name).X * 1.5f) / 2, position + i * 60), Color.White, 0, Vector2.Zero, 1.5f, SpriteEffects.None, 1);
                //SpriteText.drawStringHorizontallyCenteredAt(sb, data.Name, Game1.viewport.Width / 2, position + i * 50);
                sb.Draw(BarTexture, new Rectangle((Game1.viewport.Width - 728) / 2, position + i * 60 + 40, (int)(728 * (data.Value / (float)data.Max)), 20), new Rectangle(0, data.Color * 10 + 5, (int)(182 * (data.Value / (float)data.Max)), 5), Color.White);
                sb.Draw(BarTexture, new Rectangle((Game1.viewport.Width - 728) / 2 + (int)(728 * (data.Value / (float)data.Max)), position + i * 60 + 40, (int)(728 * (1 - data.Value / (float)data.Max)), 20), new Rectangle((int)(182 * (data.Value / (float)data.Max)), data.Color * 10, (int)(182 * (1 - data.Value / (float)data.Max)), 5), Color.White);
                sb.Draw(BarTexture, new Rectangle((Game1.viewport.Width - 728) / 2, position + i * 60 + 40, 728, 20), new Rectangle(192, data.Style * 5, 182, 5), Color.White);
            }
        }

        public static int GetStartingPixel()
        {
            Point standingPixel = Game1.player.StandingPixel;
            Vector2 vector = Game1.GlobalToLocal(globalPosition: new Vector2(standingPixel.X, standingPixel.Y), viewport: Game1.viewport);
            return Game1.options.pinToolbarToggle || (vector.Y <= Game1.viewport.Height / 2 + 64) ? 10 : 110;
        }

        public bool Subcommand_Add(List<object> matchedToken, CommandContext context, out string? message)
        {
            if (matchedToken.Count < 3)
            {
                message = "Incomplete arguments";
                return false;
            }

            if (RegisteredIDs.Contains(matchedToken[1] as string))
            {
                message = "ID already exists";
                return false;
            }

            RegisteredIDs.Add(matchedToken[1] as string);
            BossBars.Add(matchedToken[1] as string, new BossBarData { Name = matchedToken[2] as string ?? "Entity" });
            message = $"Create Custom Boss Bar [{matchedToken[2]}]";
            return true;
        }

        public bool Subcommand_Get(List<object> matchedToken, CommandContext context, out string? message)
        {
            if (matchedToken.Count < 3)
            {
                message = "Incomplete arguments";
                return false;
            }

            if (!BossBars.TryGetValue(matchedToken[1] as string, out BossBarData bar))
            {
                message = "ID does not exists";
                return false;
            }

            switch (matchedToken[2])
            {
                case "max":
                    {
                        message = $"Custom bossbar [{bar.Name}] has a maximum of {bar.Max}";
                        return true;
                    }
                case "players":
                    {
                        IEnumerable<Farmer>? allPlayers = EntityMatchToken.GetPlayers(bar.Players, context.Dim, context.Player);

                        if (allPlayers is null)
                        {
                            message = $"Custom bossbar [{bar.Name}] has no players currently online";
                            return true;
                        }

                        StringBuilder sb = new("Custom bossbar [");
                        sb.Append(bar.Name).Append("] has ").Append(allPlayers.Count()).Append(" player(s) currently Online: ");
                        foreach (Farmer player in allPlayers) sb.Append(player.displayName).Append(", ");
                        message = sb.ToString();
                        message = message.Remove(message.Length - 2);
                        return true;
                    }
                case "value":
                    {
                        message = $"Custom bossbar [{bar.Name}] has a value of {bar.Value}";
                        return true;
                    }
                case "visible":
                    {
                        message = $"Custom bossbar [{bar.Name}] is currently {(bar.Visibility ? "shown" : "hidden")}";
                        return true;
                    }

                default:
                    message = "Invalid Subcommand";
                    return false;
            }
        }

        public bool Subcommand_List(List<object> matchedToken, CommandContext context, out string? message)
        {
            if (!BossBars.Any())
            {
                message = "There Are No Custom Boss Bar(s) active";
                return true;
            }
            StringBuilder sb = new("There are ");
            sb.Append(RegisteredIDs.Count).Append(" custom bossbar(s) active: ");
            foreach (BossBarData data in BossBars.Values)
            {
                sb.Append(data.Name).Append(", ");
            }
            message = sb.ToString();
            message = message.Remove(message.Length - 2);
            return true;
        }

        public bool Subcommand_Remove(List<object> matchedToken, CommandContext context, out string? message)
        {
            if (matchedToken.Count < 2)
            {
                message = "Incomplete arguments";
                return false;
            }

            if (!RegisteredIDs.Contains(matchedToken[1] as string))
            {
                message = "ID does not exists";
                return false;
            }

            RegisteredIDs.Remove(matchedToken[1] as string);
            BossBars.Remove(matchedToken[1] as string);
            message = $"Removed Custom Boss Bar [{matchedToken[2]}]";
            return true;
        }

        public bool Subcommand_Set(List<object> matchedToken, CommandContext context, out string? message)
        {
            if (matchedToken.Count < 4)
            {
                message = "Incomplete arguments";
                return false;
            }

            if (!BossBars.TryGetValue(matchedToken[1] as string, out BossBarData bar))
            {
                message = "ID does not exists";
                return false;
            }

            switch (matchedToken[2])
            {
                case "color":
                    {
                        if (matchedToken[3] is string color)
                        {
                            int newColor;
                            switch (color)
                            {
                                case "pink":
                                    newColor = 0;
                                    break;
                                case "blue":
                                    newColor = 1;
                                    break;
                                case "red":
                                    newColor = 2;
                                    break;
                                case "green":
                                    newColor = 3;
                                    break;
                                case "yellow":
                                    newColor = 4;
                                    break;
                                case "purple":
                                    newColor = 5;
                                    break;
                                case "white":
                                    newColor = 6;
                                    break;
                                default:
                                    message = "Invalid Color";
                                    return false;
                            }

                            if (bar.Color == newColor)
                            {
                                message = $"Nothing Changed. That is already the color of this bossbar";
                                return false;
                            }

                            bar.Color = newColor;
                            message = $"Custom bossbar [{bar.Name}] has changed color";
                            return true;
                        }
                        message = "Invalid Color";
                        return false;
                    }
                case "max":
                    {
                        int max = (int)matchedToken[3];
                        if (bar.Max == max)
                        {
                            message = $"Custom bossbar already has maximum value {max}";
                            return false;
                        }
                        bar.Max = max;
                        message = $"Custom bossbar [{bar.Name}] has a maximum of {max}";
                        return true;
                    }
                case "name":
                    {
                        if (bar.Name == matchedToken[3] as string)
                        {
                            message = $"Nothing Changed. That is already the name of this bossbar";
                            return false;
                        }
                        message = $"Custom bossbar [{bar.Name}] has been renamed";
                        bar.Name = matchedToken[3] as string ?? "Entity";
                        return true;
                    }
                case "players":
                    {
                        if (bar.Players == matchedToken[3] as string)
                        {
                            message = $"Nothing Changed. Those players are already on the bossbar with nobody to add or remove";
                            return false;
                        }

                        string players = matchedToken[3] as string ?? "";
                        if (players == "@e")
                        {
                            foreach (Character c in EntityMatchToken.GetEntities(players, context.Dim, context.Player) ?? Array.Empty<Character>())
                            {
                                if (c is not Farmer)
                                {
                                    message = "Only players can be affected by this command, but the provided selecter includes entities";
                                    return false;
                                }
                            }
                        }
                        bar.Players = players;
                        IEnumerable<Farmer>? allPlayers = EntityMatchToken.GetPlayers(players, context.Dim, context.Player);

                        if (allPlayers is null)
                        {
                            message = $"Custom bossbar [{bar.Name}] no longer has any players";
                            return true;
                        }

                        StringBuilder sb = new("Custom bossbar [");
                        sb.Append(bar.Name).Append("] now has ").Append(allPlayers.Count()).Append(" player(s): ");
                        foreach (Farmer player in allPlayers) sb.Append(player.displayName).Append(", ");
                        message = sb.ToString();
                        message = message.Remove(message.Length - 2);
                        UpdateBossBars(null, null);
                        return true;
                    }
                case "style":
                    {
                        if (matchedToken[3] is string color)
                        {
                            int newColor;
                            switch (color)
                            {
                                case "notched_6":
                                    newColor = 0;
                                    break;
                                case "notched_10":
                                    newColor = 1;
                                    break;
                                case "notched_12":
                                    newColor = 2;
                                    break;
                                case "notched_20":
                                    newColor = 3;
                                    break;
                                case "progress":
                                    newColor = 4;
                                    break;
                                default:
                                    message = "Invalid Style";
                                    return false;
                            }

                            if (bar.Style == newColor)
                            {
                                message = $"Nothing Changed. That is already the style of this bossbar";
                                return false;
                            }

                            bar.Style = newColor;
                            message = $"Custom bossbar [{bar.Name}] has changed style";
                            return true;
                        }
                        message = "Invalid Style";
                        return false;
                    }

                case "value":
                    {
                        int max = (int)matchedToken[3];
                        if (bar.Value == max)
                        {
                            message = $"Custom bossbar already has value {max}";
                            return false;
                        }
                        bar.Value = max;
                        message = $"Custom bossbar [{bar.Name}] has a value of {max}";
                        return true;
                    }
                case "visible":
                    {

                        bool visible = (bool)matchedToken[3];
                        if (bar.Visibility == visible)
                        {
                            message = $"Nothing Changed. Custom bossbar [{bar.Name}] is already {(visible ? "visible" : "hidden")}";
                            return false;
                        }
                        bar.Visibility = visible;
                        message = $"Custom bossbar [{bar.Name}] is now {(visible ? "visible" : "hidden")}";
                        return true;
                    }
                default:
                    message = "Invalid Subcommand";
                    return false;
            }
        }

        public override bool Execute(List<object> matchedToken, CommandContext context, out string? message)
        {
            if (matchedToken.Count == 0)
            {
                message = "Incomplete arguments";
                return false;
            }

            switch (matchedToken[0])
            {
                case "add": return Subcommand_Add(matchedToken, context, out message);
                case "get": return Subcommand_Get(matchedToken, context, out message);
                case "remove": return Subcommand_Remove(matchedToken, context, out message);
                case "list": return Subcommand_List(matchedToken, context, out message);
                case "set": return Subcommand_Set(matchedToken, context, out message);

                default:
                    message = "Invalid Subcommand";
                    return false;
            }
        }
    }
}
