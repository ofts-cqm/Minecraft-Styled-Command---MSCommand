using MCCommands.Commands;
using StardewModdingAPI;
using StardewValley;
using StardewValley.TerrainFeatures;

namespace MCCommands
{
    internal class ModEntry : Mod
    {
        public override void Entry(IModHelper helper)
        {
            ICommand.Monitor = Monitor;
            _ = new Clear(helper);
            _ = new Advancement(helper);
            _ = new Ban(helper);
            _ = new BanList(helper);
            _ = new BossBar(helper);
            _ = new Clone(helper);
            Monitor.Log(typeof(TerrainFeature).Name, LogLevel.Info);
        }
    }
}
