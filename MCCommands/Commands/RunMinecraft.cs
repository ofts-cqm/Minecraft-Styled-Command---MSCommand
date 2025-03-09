using StardewModdingAPI;
using System.Diagnostics;

namespace MCCommands.Commands
{
    internal class RunMinecraft : ICommand
    {
        public IModHelper Helper;

        public RunMinecraft(IModHelper helper) : base(helper, "runRealMinecraft", "Run real minecraft!", 0, null)
        {
            Helper = helper;
        }

        public override bool Execute(List<object> matchedToken, CommandContext context, out string? message)
        {
            if (!Helper.ModContent.DoesAssetExist<object>(".minecraft\\versions\\1.20.6\\1.20.6.jar"))
            {
                message = "Minecraft is missing, downloading... Please wait a few seconds and run the command again";
                Task.Run(async () => await DownloadFileAsync());
                return false;
            }

            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = Helper.DirectoryPath + "/minecraft.bat",//@"C:\Minecraft\SDV\windows.bat",
                Arguments = $"\"{context.Player.displayName}\" \"{Helper.DirectoryPath}\\.minecraft\" \"{Helper.DirectoryPath}\\.minecraft\\userdata\" \"{Helper.DirectoryPath}\\.minecraft\\assetsa\" \"{Helper.DirectoryPath}\\jdk-21.0.6.7-hotspot\\bin\\java.exe\"",
                RedirectStandardOutput = false, // Optional: Redirect output
                UseShellExecute = false,
                CreateNoWindow = false
            };

            Process.Start(startInfo);
            message = "Minecraft Started!";
            return true;
        }

        private async Task DownloadFileAsync()
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    byte[] fileData = await client.GetByteArrayAsync("https://piston-data.mojang.com/v1/objects/05b6f1c6b46a29d6ea82b4e0d42190e42402030f/client.jar");
                    File.WriteAllBytes(Helper.DirectoryPath + "\\.minecraft\\versions\\1.20.6\\1.20.6.jar", fileData);
                    Monitor.Log("Downloaded minecraft 1.20.6 successfully! Try run the command again!", LogLevel.Info);
                }
            }
            catch (Exception ex)
            {
                Monitor.Log($"Failed to download minecraft 1.20.6: {ex.Message}", LogLevel.Error);
            }
        }
    }
}
