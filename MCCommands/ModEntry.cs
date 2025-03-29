using MCCommands.Commands;
using MCCommands.Tokens;
using Netcode;
using StardewModdingAPI;
using StardewValley.Network;

namespace MCCommands
{
    public class ServerProperty
    {
        public int DefaultOpLevel { get; set; } = 4;
    }

    internal class ModEntry : Mod
    {
        public static ServerProperty serverProperty = new();
        public static readonly NetLongDictionary<int, NetInt> OPs = new();
        public static IModHelper ModHelper { get; private set; } = null!;

        public override void Entry(IModHelper helper)
        {
            ICommand.Monitor = Monitor;
            ModHelper = helper;

            _ = new Clear(helper);
            _ = new Advancement(helper);
            _ = new Ban(helper);
            _ = new BanList(helper);
            _ = new BossBar(helper);
            _ = new Clone(helper);
            _ = new OP(helper);
            _ = new Deop(helper);
            _ = new RunMinecraft(helper);
            _ = new Effect(helper);
            _ = new Enchant(helper);

            helper.Events.GameLoop.SaveLoaded += StringToken.Internal_Item_Target;
            helper.Events.GameLoop.SaveCreated += StringToken.Internal_Item_Target;
            helper.Events.GameLoop.GameLaunched += StringToken.Internal_Enchantment_Target;

            ServerProperty? property = helper.Data.ReadJsonFile<ServerProperty>("server.property");
            if (property is null)
            {
                helper.Data.WriteJsonFile("server.property", serverProperty);
            }
            else
            {
                serverProperty = property;
            }
            Monitor.Log(helper.DirectoryPath, LogLevel.Info);
        }
    }
}
