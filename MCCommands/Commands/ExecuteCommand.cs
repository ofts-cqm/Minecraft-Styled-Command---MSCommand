using MCCommands.Tokens;
using StardewModdingAPI;

namespace MCCommands.Commands
{
    internal class ExecuteCommand : ICommand
    {
        public ExecuteCommand(IModHelper helper) : base(helper, "execute", "execute", 2, new SubCommandToken(new()))
        {
            SubCommandToken commandToken = FirstToken as SubCommandToken ?? new(new());
            commandToken.Values.Add("align", new StringToken(StringToken.Swiss_Target, "axes", "incorrect axes") { ShowAll = false, Next = commandToken });
            commandToken.Values.Add("anchored", new StringToken(() => new string[] {"eyes", "feet"}, "anchors", "incorrect anchors") { Next = commandToken });
            commandToken.Values.Add( "as", new EntityMatchToken { Next = commandToken });
            commandToken.Values.Add("at", new EntityMatchToken { Next = commandToken });
            commandToken.Values.Add("facing", new CoordinateTokenAllowEntity("entity") { Next = new StringToken(() => new string[] {"eyes", "feet"}, "anchors", "incorrect anchors") { Next = commandToken } });
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
        }

        public override bool Execute(List<object> matchedToken, CommandContext context, out string? message)
        {
            throw new NotImplementedException();
        }
    }
}
