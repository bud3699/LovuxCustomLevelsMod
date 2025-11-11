using HarmonyLib;
using Steamworks;
using System.Collections.Generic;
using System.Reflection.Emit;
using UnityEngine;
using UnityEngine.InputSystem;

namespace LovuxPatcher
{
    [HarmonyPatch]
    public static class LayerMenuPatch
    {

        /*
        [HarmonyPatch(typeof(LayerMenu), "CalculateMenu")]
        [HarmonyPrefix]
        public static bool Prefix_LayerMenu_CalculateMenu(object __instance)
        {
            __instance.CalculateMenuExtended();
            return false;
        }
        */

        [HarmonyPatch(typeof(LayerMenu), "OpenMenu")]
        [HarmonyPostfix]
        public static void Postfix_LayerMenu_OpenMenu(LayerMenu __instance, InputAction.CallbackContext ctx)
        {
            Debug.Log("Opening Menu");
            var elements = new List<Transform>
                {
                    LayerMenuExtensions.inputField?.transform,
                    LayerMenuExtensions.customLevelUIRoot?.transform,
                    LayerMenuExtensions.versionTextObj?.transform
                };
            if (Director.gameMode == (GameMode)3) elements.Add(LayerMenuExtensions.customLevelCodeRoot?.transform);
            UIController.DelayedTweenMenuElements(Director.instance, true, 0.8f, 0.3f, elements.ToArray());
        }


        [HarmonyPatch(typeof(LayerMenu), "CloseMenu")]
        [HarmonyPostfix]
        public static void Postfix_LayerMenu_CloseMenu(LayerMenu __instance)
        {
            UIController.DelayedTweenMenuElements(Director.instance, false, 0.2f, 0.3f,
                LayerMenuExtensions.inputField?.transform,
                LayerMenuExtensions.customLevelUIRoot?.transform,
                LayerMenuExtensions.versionTextObj?.transform,
                LayerMenuExtensions.customLevelCodeRoot?.transform);
        }

        [HarmonyPatch(typeof(LayerMenu), "CalculateMenu")]
        [HarmonyPostfix]
        public static void Postfix_LayerMenu_CalculateMenu(LayerMenu __instance)
        {
            if (SteamApps.BIsDlcInstalled((AppId_t)3895810u)) { LayerMenuExtensions.CreateCustomLevelLoadButtonUI(); LayerMenuExtensions.CreateLevelCodeUI(); }
            
        }


        
        public static void ApplyPatch(HarmonyLib.Harmony harmony)
        {
            var originalStart = AccessTools.Method(typeof(LayerMenu), "Start");
            var transpilerStart = AccessTools.Method(typeof(LayerMenuPatch), nameof(StartTranspiler));
            harmony.Patch(originalStart, transpiler: new HarmonyMethod(transpilerStart));
        }

        public static IEnumerable<CodeInstruction> StartTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            var codes = new List<CodeInstruction>(instructions);
            var gameModeField = AccessTools.Field(typeof(Director), "gameMode");

            yield return new CodeInstruction(OpCodes.Ldc_I4_3);
            yield return new CodeInstruction(OpCodes.Stsfld, gameModeField);

            foreach (var code in codes)
            {
                yield return code;
            }
        }
    }
}
