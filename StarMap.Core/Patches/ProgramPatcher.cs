using HarmonyLib;
using KSA;
using StarMap.API;

namespace StarMap.Core.Patches
{
    [HarmonyPatch(typeof(Program))]
    internal static class ProgramPatcher
    {
        private const string OnDrawUiMethodName = "OnDrawUi";

        [HarmonyPatch(OnDrawUiMethodName)]
        [HarmonyPrefix]
        public static void BeforeOnDrawUi(double dt)
        {
            var mods = StarMapCore.Instance?.LoadedMods.Mods.Get<IStarMapOnUi>() ?? [];

            foreach (var mod in mods)
            {
                mod.OnBeforeUi(dt);
            }
        }

        [HarmonyPatch(OnDrawUiMethodName)]
        [HarmonyPostfix]
        public static void AfterOnDrawUi(double dt)
        {
            var mods = StarMapCore.Instance?.LoadedMods.Mods.Get<IStarMapOnUi>() ?? [];

            foreach (var mod in mods)
            {
                mod.OnAfterUi(dt);
            }
        }
    }
}
