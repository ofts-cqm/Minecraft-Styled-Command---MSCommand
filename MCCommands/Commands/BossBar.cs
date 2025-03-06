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
using System.Text;

namespace MCCommands.Commands
{
    public class BossBarData : INetObject<NetFields>
    {
        public NetFields NetFields { get; }

        public NetInt color = new();
        public NetInt style = new();
        public NetInt max = new();
        public NetString name = new();
        public NetBool visibility = new();
        public NetInt value = new();
        public NetString players = new();

        public int Color { get => color.Value; set => color.Value = value; }
        public int Style { get => style.Value; set => style.Value = value; }
        public int Max { get => max.Value; set => max.Value = value; }
        public string Name { get => name.Value; set => name.Value = value; }
        public bool Visibility { get => visibility.Value; set => visibility.Value = value; }
        public int Value { get => value.Value; set => this.value.Value = value; }
        public string Players { get => players.Value; set => players.Value = value; }

        public BossBarData()
        {
            NetFields = new NetFields(NetFields.GetNameForInstance(this))
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
        public static PerScreen<List<BossBarData>> VisibleBossBars = new();

        public static IModHelper Helper;
        public static Texture2D BarTexture;

        public BossBar(IModHelper helper) : base(helper, "bossbar", "bossbar <add|get|list|remove|set>", new SubCommandToken(new()
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

        public static void SaveBossBar(object? sender, SavingEventArgs e)
        {
            Helper.Data.WriteSaveData("BossBars", BossBars);
        }

        public static void ReadBossBar(object? sender, SaveLoadedEventArgs e)
        {
            BossBars.CopyFrom(Helper.Data.ReadSaveData<Dictionary<string, BossBarData>>("BossBars") ?? new());
            RegisteredIDs.Clear();
            foreach (string id in BossBars.Keys) RegisteredIDs.Add(id.ToString());
        }

        public static void UpdateBossBars(object? sender, OneSecondUpdateTickedEventArgs _)
        {
            VisibleBossBars.Value.Clear();
            foreach (BossBarData data in BossBars.Values)
            {
                if (VisibleBossBars.Value.Count > 3) return;
                if (data.Visibility && data.IsVisibleToPlayer(Game1.player)) VisibleBossBars.Value.Add(data);
            }
        }

        public static void DrawBossBar(object? sender, RenderingHudEventArgs e)
        {
            int position = 10;
            SpriteBatch sb = e.SpriteBatch;
            for (int i = 0; i < VisibleBossBars.Value.Count; i++)
            {
                BossBarData data = VisibleBossBars.Value[i];
                SpriteText.drawStringHorizontallyCenteredAt(sb, data.Name, Game1.viewport.Width / 2, position + i * 50);
                sb.Draw(BarTexture, new Rectangle((Game1.viewport.Width - 728) / 2, position + i * 50 + 30, (int)(728 * (data.Value / (float)data.Max)), 20), new Rectangle(0, data.Color * 10 + 5, (int)(182 * (data.Value / (float)data.Max)), 5), Color.White);
                sb.Draw(BarTexture, new Rectangle((Game1.viewport.Width - 728) / 2 + (int)(728 * (data.Value / (float)data.Max)), position + i * 50 + 30, (int)(728 * (1 - data.Value / (float)data.Max)), 20), new Rectangle((int)(182 * (data.Value / (float)data.Max)), data.Color * 10, (int)(182 * (1 - data.Value / (float)data.Max)), 5), Color.White);
                sb.Draw(BarTexture, new Rectangle((Game1.viewport.Width - 728) / 2, position + i * 50 + 20, 728, 30), new Rectangle(182, data.Style * 5, 182, 5), Color.White);
            }
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
                                case "blue":
                                    newColor = 0;
                                    break;
                                case "green":
                                    newColor = 1;
                                    break;
                                case "pink":
                                    newColor = 2;
                                    break;
                                case "purple":
                                    newColor = 3;
                                    break;
                                case "red":
                                    newColor = 4;
                                    break;
                                case "white":
                                    newColor = 5;
                                    break;
                                case "yellow":
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
                        if (int.TryParse(matchedToken[3] as string, out int max))
                        {
                            if (bar.Max == max)
                            {
                                message = $"Custom bossbar already has maximum value {max}"; 
                                return false;
                            }
                            bar.Max = max;
                            message = $"Custom bossbar [{bar.Name}] has a maximum of {max}";
                            return true;
                        }
                        message = "Invalid Value";
                        return false;
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
                        IEnumerable<Farmer>? allPlayers = EntityMatchToken.GetPlayers(bar.Players, context.Dim, context.Player);

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
                        if (int.TryParse(matchedToken[3] as string, out int max))
                        {
                            if (bar.Value == max)
                            {
                                message = $"Custom bossbar already has value {max}";
                                return false;
                            }
                            bar.Value = max;
                            message = $"Custom bossbar [{bar.Name}] has a value of {max}";
                            return true;
                        }
                        message = "Invalid Value";
                        return false;
                    }
                case "visible":
                    {
                        if (bool.TryParse(matchedToken[3] as string, out bool visible))
                        {
                            if (bar.Visibility == visible)
                            {
                                message = $"Nothing Changed. Custom bossbar [{bar.Name}] is already {(visible ? "visible" : "hidden")}";
                                return false;
                            }
                            bar.Visibility = visible;
                            message = $"Custom bossbar [{bar.Name}] is now {(visible ? "visible" : "hidden")}";
                            return true;
                        }
                        message = "Invalid Value";
                        return false;
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
