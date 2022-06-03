using System.Reflection;
using HarmonyLib;

namespace NativeLoader
{
    internal class ModHook
    {
        private readonly MethodInfo _onAppStartMeth;

        internal ModHook(IReflect mod) => _onAppStartMeth = mod.GetMethod("OnApplicationStart", AccessTools.all);

        internal void OnAppStart()
        {
            if (_onAppStartMeth == null) return;
            _onAppStartMeth.Invoke(null, null);
        }
    }
}