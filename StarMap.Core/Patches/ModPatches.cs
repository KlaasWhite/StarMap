using HarmonyLib;
using KSA;

namespace StarMap.Core.Patches
{
    [HarmonyPatch(typeof(Mod))]
    internal static class ModPatches
    {
        [HarmonyPatch(nameof(Mod.PrepareSystems))]
        [HarmonyPrefix]
        public static void OnLoadMod(this Mod __instance)
        {
            StarMapCore.Instance?.LoadedMods.LoadMod(__instance);
        }
    }
}
