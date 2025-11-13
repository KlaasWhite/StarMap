using KSA;
using HarmonyLib;

namespace StarMap.Core
{
    [HarmonyPatch]
    internal static class ModLoaderPatcher
    {
        private static ModManager? _modManager;
        private readonly static Harmony? _harmony = new Harmony("StarMap.Core.ModLoaderPatcher");

        public static void Patch(ModManager modManager)
        {
            _modManager = modManager;
            _harmony?.PatchAll();
        }

        public static void Unload()
        {
            _modManager = null;
            //MainScreen.Screens = [];
            _harmony?.UnpatchAll();
        }

        [HarmonyPatch(typeof(Mod), nameof(Mod.PrepareSystems))]
        [HarmonyPrefix]
        public static void OnLoadMod(this Mod __instance)
        {
            _modManager?.LoadMod(__instance);
        }

        [HarmonyPatch(typeof(ModLibrary), nameof(ModLibrary.LoadAll))]
        [HarmonyPostfix]
        public static void AfterLoad()
        {
            _modManager?.OnAllModsLoaded();
            /*var screens = MainScreen.Screens.ToList();
            screens.Insert(MainScreen.Screens.Length - 1, new ModManagerScreen(_modManager));

            MainScreen.Screens = screens.ToArray();*/
            _harmony?.UnpatchAll(_harmony.Id);
        }
    }
}
