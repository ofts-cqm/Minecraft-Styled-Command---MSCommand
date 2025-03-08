using Force.DeepCloner;
using MCCommands.Tokens;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using StardewValley.TerrainFeatures;
using xTile.Dimensions;
using Object = StardewValley.Object;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace MCCommands.Commands
{
    internal class Clone : ICommand
    {
        public enum MaskMode
        {
            Replace,
            Mask,
            Filtered
        }

        public enum CloneMode
        {
            Force,
            Move,
            Normal
        }

        public static readonly StringToken CloneModeToken = new StringToken(() => new string[] { "force", "move", "normal" }, "cloneMod", "Incorrect Argument") { IsOptional = true };
        public static readonly StringToken CloneModeToken2 = new StringToken(() => new string[] { "force", "move", "normal" }, "cloneMod", "Incorrect Argument") { Next = new StringToken(StringToken.Item_Target, "block", "Block Not Found") };

        public Clone(IModHelper helper) : base(helper, "clone", "clone <begin: x y> <end: x y> <destination: x y>", new NumberToken("x"))
        {
            ((NumberToken)FirstToken).Allow(() => new string[] { Game1.player.StandingPixel.X.ToString(), (Game1.getMouseX() + Game1.viewport.X).ToString() })
            .NextToken(new NumberToken("y").Allow(() => new string[] { Game1.player.StandingPixel.Y.ToString(), (Game1.getMouseY() + Game1.viewport.Y).ToString() }))
            .NextToken(new NumberToken("x").Allow(() => new string[] { Game1.player.StandingPixel.X.ToString(), (Game1.getMouseX() + Game1.viewport.X).ToString() }))
            .NextToken(new NumberToken("y").Allow(() => new string[] { Game1.player.StandingPixel.Y.ToString(), (Game1.getMouseY() + Game1.viewport.Y).ToString() }))
            .NextToken(new NumberToken("x").Allow(() => new string[] { Game1.player.StandingPixel.X.ToString(), (Game1.getMouseX() + Game1.viewport.X).ToString() }))
            .NextToken(new NumberToken("y").Allow(() => new string[] { Game1.player.StandingPixel.Y.ToString(), (Game1.getMouseY() + Game1.viewport.Y).ToString() }))
            .NextToken(new SubCommandToken(new Dictionary<string, IToken?>()
            {
                { "filter",  CloneModeToken2 },
                { "replace", CloneModeToken },
                { "mask",    CloneModeToken }
            })
            { IsOptional = true});
        }
        
        public static string[] GetNameFromObject(Object? obj) => obj is null ? Array.Empty<string>() : new string[] { obj.Name, obj.DisplayName, obj.QualifiedItemId, obj.ItemId };
        public static string[] GetNameFromTerrainFeature(TerrainFeature? feature) => feature is null ? Array.Empty<string>() : new string[] { feature.GetType().Name };

        public static bool DoClone<T>(Func<T?> get, Action<T?, bool> set, Func<T?, string[]> name, MaskMode maskMode, CloneMode cloneMode, string filter) where T : class
        {
            T? source = get();
            if (maskMode == MaskMode.Filtered && !name(source).Contains(filter)) return false;
            if (maskMode == MaskMode.Mask && source is null) return false;

            set(source, true);
            if (cloneMode == CloneMode.Move) set(null, false);

            return true;
        }

        public override bool Execute(List<object> matchedToken, CommandContext context, out string? message)
        {
            Rectangle source = new(Math.Min((int)matchedToken[0], (int)matchedToken[2]), Math.Min((int)matchedToken[1], (int)matchedToken[3]), Math.Abs((int)matchedToken[0] - (int)matchedToken[2]), Math.Abs((int)matchedToken[1] - (int)matchedToken[3]));
            Rectangle dest = new((int)matchedToken[4], (int)matchedToken[5], source.Width, source.Height);

            Size mapSize = context.Dim.Map.Layers[0].LayerSize;
            if (source.Right >= mapSize.Width || source.Bottom >= mapSize.Height)
            {
                message = "Source out of bounds";
                return false;
            }

            if (dest.Right >= mapSize.Width || dest.Bottom >= mapSize.Height)
            {
                message = "Destination out of bounds";
                return false;
            }
            int blocksMoved = 0;
            MaskMode maskMode = MaskMode.Replace;
            CloneMode cloneMode = CloneMode.Normal;
            string target = "";

            if (matchedToken.Count > 6) switch (matchedToken[6] as string)
            {
                case "filter":
                    maskMode = MaskMode.Filtered;
                    break;
                case "replace":
                    maskMode = MaskMode.Replace;
                    break;
                case "mask":
                    maskMode = MaskMode.Mask;
                    break;
                default:
                    message = "Incorrect Mask Mode";
                    return false;
            }

            if (matchedToken.Count > 7) switch (matchedToken[7] as string)
            {
                case "force":
                    cloneMode = CloneMode.Force;
                    break;
                case "move":
                    cloneMode = CloneMode.Move;
                    break;
                case "normal":
                    cloneMode = CloneMode.Normal;
                    break;
                default:
                    message = "Incorrect Clone Mode";
                    return false;
            }

            if (maskMode == MaskMode.Filtered)
            {
                target = matchedToken[8] as string ?? "";
            }

            if (source.Intersects(dest) && cloneMode != CloneMode.Force)
            {
                message = "Source and Destination overlap";
                return false;
            }

            Dictionary<Vector2, Object?> objectChangeRequest = new();
            Dictionary<Vector2, TerrainFeature?> terrianChangeRequest = new();

            for (int i = 0; i < source.Height; i++)
            {
                for (int j = 0; j < source.Width; j++)
                {
                    Vector2 sourceVec = new(source.X + j, source.Y + i);
                    Vector2 destVec = new(dest.X + j, dest.Y + i);
                    if (DoClone(() => context.Dim.Objects.TryGetValue(sourceVec, out Object a) ? a : null, (a, dest) => objectChangeRequest.TryAdd(dest ? destVec : sourceVec, a), GetNameFromObject, maskMode, cloneMode, target)) blocksMoved++;
                    if (DoClone(() => context.Dim.terrainFeatures.TryGetValue(sourceVec, out TerrainFeature a) ? a : null, (a, dest) => terrianChangeRequest.TryAdd(dest ? destVec : sourceVec, a), GetNameFromTerrainFeature, maskMode, cloneMode, target)) blocksMoved++;
                }
            }

            foreach (KeyValuePair<Vector2, Object?> pair in objectChangeRequest)
            {
                if (pair.Value is null) context.Dim.Objects.Remove(pair.Key);
                else
                {
                    Object obj = (Object)pair.Value.getOne();
                    obj.CopyFieldsFrom(pair.Value);
                    context.Dim.Objects[pair.Key] = obj;
                }
            }

            foreach (KeyValuePair<Vector2, TerrainFeature?> pair in terrianChangeRequest)
            {
                if (pair.Value is null) context.Dim.terrainFeatures.Remove(pair.Key);
                else context.Dim.terrainFeatures[pair.Key] = pair.Value.DeepClone();
            }

            if (blocksMoved == 0)
            {
                message = "No blocks are cloned";
                return false;
            }

            message = $"Successfully cloned {blocksMoved} blocks";
            return true;
        }
    }
}
