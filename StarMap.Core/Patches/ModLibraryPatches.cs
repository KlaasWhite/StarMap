using HarmonyLib;
using KSA;

namespace StarMap.Core.Patches
{
    [HarmonyPatch(typeof(ModLibrary))]
    internal class ModLibraryPatches
    {
        [HarmonyPatch(nameof(ModLibrary.LoadAll))]
        [HarmonyPostfix]
        public static void AfterLoad()
        {
            StarMapCore.Instance?.LoadedMods.OnAllModsLoaded();
        }
    }
}
