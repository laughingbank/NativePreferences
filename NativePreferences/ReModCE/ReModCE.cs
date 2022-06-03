using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using ReMod.Core.Managers;

namespace NativePreferences.ReModCE
{
    internal class ReModCE
    {
        internal static void LoadResources()
        {
            var ourAssembly = Assembly.GetExecutingAssembly();
            var resources = ourAssembly.GetManifestResourceNames();
            foreach (var resource in resources)
            {
                if (!resource.EndsWith(".png")) continue;

                var stream = ourAssembly.GetManifestResourceStream(resource);

                using var ms = new MemoryStream();
                stream.CopyTo(ms);
                var resourceName = Regex.Match(resource, @"([a-zA-Z\d\-_]+)\.png").Groups[1].ToString();
                ResourceManager.LoadSprite("nativepreferences", resourceName, ms.ToArray());
            }
        }
    }
}