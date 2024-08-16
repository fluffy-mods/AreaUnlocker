// HarmonyPatch_AreaManager.cs
// Copyright Karel Kroeze, 2018-2018

using HarmonyLib;
using Verse;

namespace AreaUnlocker {
    [HarmonyPatch(typeof(AreaManager), nameof(AreaManager.CanMakeNewAllowed))]
    public static class AreaManager_CanMakeNewAllowed {
        private static bool Prefix(ref bool __result) {
            __result = true;
            return false;
        }
    }

    [HarmonyPatch(typeof(AreaManager), "SortAreas")]
    public static class AreaManager_Disable_SortAreas {
        private static bool Prefix() {
            return false;
        }
    }
}
