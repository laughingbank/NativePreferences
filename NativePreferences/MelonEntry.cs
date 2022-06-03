using MelonLoader;
using NativeLoader;

namespace NativePreferences
{
    public class MelonEntry
    {

        private static readonly NativePreferences Instance = new NativePreferences();

        public static void OnApplicationStart()
        {
            NativeLogger.Msg("Loaded NativePreferences v6.1.1.5 by Dotlezz & Null");
            ReModCE.ReModCE.LoadResources();
            MelonCoroutines.Start(Instance.OnUIManager());
        }
    }
}
