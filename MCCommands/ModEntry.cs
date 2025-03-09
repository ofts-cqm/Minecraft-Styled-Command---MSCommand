using MCCommands.Commands;
using MCCommands.Tokens;
using StardewModdingAPI;

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
            helper.Events.GameLoop.SaveLoaded += StringToken.Internal_Item_Target;
            helper.Events.GameLoop.SaveCreated += StringToken.Internal_Item_Target;
        }
    }
}
