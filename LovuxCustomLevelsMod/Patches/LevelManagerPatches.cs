using HarmonyLib;
using Mindlabor.Utils;
using UnityEngine;

namespace LovuxPatcher
{
    [HarmonyPatch(typeof(LevelManager))]
    public static class LevelManagerPatches
    {
        [HarmonyPatch("CompleteLevelCoroutine")]
        [HarmonyPrefix]
        public static bool Prefix_LevelManager_CompleteLevelCoroutine()
        {
            Debug.LogWarning("[LovuxPatcher] Prefix hit: Finished Level CoRoutine");
            CoroutineUtils.RunCoroutine(LevelManagerExtension.HandleGameModeLevelLoad());
            return false;
        }
    }
}
