using System;
using MelonLoader;

namespace NativeLoader
{
    public static class NativeLogger
    {
        private static readonly MelonLogger.Instance Instance = new("NativePreferences", ConsoleColor.DarkMagenta);

        public static void Msg(object obj) => Instance.Msg(obj);

        public static void Error(object obj) => Instance.Error(obj);

        public static void Warn(object obj) => Instance.Warning(obj);
    }
}