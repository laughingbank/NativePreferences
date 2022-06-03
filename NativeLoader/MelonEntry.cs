using System;
using System.IO;
using System.Linq;
using System.Net;
using MelonLoader;
using System.Reflection;
using System.Security.Cryptography;
using NativeLoader;

[assembly: MelonInfo(typeof(MelonEntry), "NativeLoader", "1.0.4.3")]
[assembly: MelonGame("VRChat", "VRChat")]
[assembly: MelonColor(ConsoleColor.DarkMagenta)]

namespace NativeLoader
{
    public class MelonEntry : MelonMod
    {
        private static readonly MelonPreferences_Category NativePreferencesCategory =
            MelonPreferences.CreateCategory("NativePreferences", "Native Preferences");

        public static MelonPreferences_Entry<string> FileLocation =
            NativePreferencesCategory.CreateEntry("FileLocation",
                $"{MelonUtils.GameDirectory}\\UserData\\NativePreferences.json", "File Location",
                "Requires restart to load from location!");
        
        private static ModHook _nativePreferences;

        public override void OnApplicationStart()
        {
            using var client = new WebClient();
            byte[] localCore = null;
            byte[] githubCore = null;
            byte[] mod;

            if (File.Exists("ReMod.Core.dll")) localCore = File.ReadAllBytes("ReMod.Core.dll");
              try
            {
                githubCore =
                    client.DownloadData(
                        "https://github.com/RequiDev/ReMod.Core/releases/latest/download/ReMod.Core.dll");
            }
            catch (Exception)
            {
                NativeLogger.Warn("Could not get latest version of ReMod.Core from GitHub.");
            }

            if (localCore == null)
            {
                if (githubCore == null)
                {
                    NativeLogger.Warn("ReMod.Core was not found, and could not be downloaded from GitHub.");
                    NativeLogger.Warn("NativePreferences cannot load without ReMod.Core.");
                    return;
                }

                NativeLogger.Msg("Downloading ReMod.Core to disk.");

                localCore = githubCore;
                File.WriteAllBytes("ReMod.Core.dll", githubCore);
            }

            if (Hash(githubCore) != Hash(localCore))
            {
                NativeLogger.Msg("Updating ReMod.Core to latest version.");
                File.WriteAllBytes("ReMod.Core.dll", githubCore!);
            }

            try
            {
                mod = client.DownloadData(
                    "https://github.com/laughingbank/NativePreferences/releases/latest/download/NativePreferences.dll");
            }
            catch (Exception)
            {
                NativeLogger.Warn("Could not get latest version of NativePreferences from GitHub.");
                return;
            }

         
            var assembly = Assembly.Load(mod);
            _nativePreferences = new ModHook(assembly.GetType("NativePreferences.MelonEntry"));
            _nativePreferences.OnAppStart();
        }

        private static string Hash(byte[] data)
        {
            using var sha256 = new SHA256Managed();
            return string.Concat(sha256.ComputeHash(data).Select(b => b.ToString("X2")));
        }
    }
}