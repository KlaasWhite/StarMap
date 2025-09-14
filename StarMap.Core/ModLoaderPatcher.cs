using DummyProgram;
using DummyProgram.Screens;
using HarmonyLib;
using StarMap.Core.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarMap.Core
{
    [HarmonyPatch]
    internal static class ModLoaderPatcher
    {
        private static ModManager? _modManager;
        private static IModRepository? _modRespository;
        private readonly static Harmony? _harmony = new Harmony("StarMap.Core.ModLoaderPatcher");

        public static void Patch(ModManager modManager, IModRepository modRepository)
        {
            _modManager = modManager;
            _modRespository = modRepository;
            _harmony?.PatchAll();
        }

        public static void Unload()
        {
            _modManager = null;
            _modRespository = null;
            MainScreen.Screens = [];
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
            var screens = MainScreen.Screens.ToList();
            screens.Insert(MainScreen.Screens.Length - 1, new ModManagerScreen(_modManager, _modRespository));

            MainScreen.Screens = screens.ToArray();
            _harmony?.UnpatchAll(_harmony.Id);
        }
    }
}
