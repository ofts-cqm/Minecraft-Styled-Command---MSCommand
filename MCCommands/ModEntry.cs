using MCCommands.Commands;
using StardewModdingAPI;
using StardewValley;

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
        }
    }
}
