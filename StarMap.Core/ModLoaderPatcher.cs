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

            // Currently needed to force patch in release mode
            Harmony.GetAllPatchedMethods();
        }

        public static void Unload()
        {
            _modManager = null;
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
            _harmony?.UnpatchAll(_harmony.Id);
        }
    }
}
