using MCCommands.Tokens;
using StardewModdingAPI;

namespace MCCommands.Commands
{
    internal class Effect : ICommand
    {
        public static EntityMatchToken GiveToken = new("target");
        public static EntityMatchToken ClearToken = new("target");

        public Effect(IModHelper helper) : base(helper, "effect", "effect <give|clear>", 2, new SubCommandToken(new Dictionary<string, IToken?>() 
        {
            { "give", GiveToken },
            { "clear", ClearToken }
        }))
        {
            GiveToken.NextToken(new EntityMatchToken()).NextToken(new StringToken(StringToken.Buff_Target, "effect", "effect not found")).NextToken(new NumberToken("seconds", allowInf: true) { IsOptional = true });
            ClearToken.NextToken(new EntityMatchToken() { IsOptional = true }).NextToken(new StringToken(StringToken.Buff_Target, "effect", "effect not found") { IsOptional = true });
        }

        public override bool Execute(List<object> matchedToken, CommandContext context, out string? message)
        {
            throw new NotImplementedException();
        }
    }
}
