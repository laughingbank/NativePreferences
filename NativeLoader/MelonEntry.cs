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
            if (File.Exists("NativePreferences.dll")) mod = File.ReadAllBytes("NativePreferences.dll");
         
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