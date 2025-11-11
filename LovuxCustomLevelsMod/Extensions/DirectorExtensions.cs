using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HarmonyLib;
using DG.Tweening;
using System.Linq;

namespace LovuxPatcher
{
    public static class DirectorExtensions
    {
        public static void LoadCustomLevelFromCode(this object directorInstance, string LevelCode = "A6XEAX")
        {
            if (UIManager.instance == null)
            {
                Debug.LogWarning("<color=yellow>UIManager not ready — deferring custom level load...</color>");
                if (directorInstance is MonoBehaviour mono)
                    mono.StartCoroutine(DeferredLoadCustomLevel(directorInstance, LevelCode));
                else
                    Debug.LogError("Director instance is not a MonoBehaviour — cannot start coroutine!");
                return;
            }

            ExecuteLoadCustomLevel(directorInstance, LevelCode);
        }

        private static IEnumerator DeferredLoadCustomLevel(object directorInstance, string LevelCode)
        {
            float waitTime = 0f;
            while (UIManager.instance == null)
            {
                waitTime += Time.deltaTime;
                yield return null;
            }
            Debug.Log($"<color=lime>UIManager detected after {waitTime:F2}s — continuing custom level load...</color>");
            ExecuteLoadCustomLevel(directorInstance, LevelCode);
        }

        private static void ExecuteLoadCustomLevel(object directorInstance, string LevelCode)
        {
            Debug.Log("<color=red>Game Mode: Loading Custom Level</color>");
            Traverse traverse = Traverse.Create(directorInstance);

            object levelBuilder = traverse.Field("levelBuilder").GetValue();
            if (levelBuilder == null)
            {
                Debug.LogError("LevelGameBuilder reference is null!");
                return;
            }

            object gameLevel;
            levelBuilder.InitCustomLevelFromCode(LevelCode, out gameLevel);
            //CustomLevelCompleteUI.gameLevelUpload = JsonUtility.ToJson(gameLevel);

            if (gameLevel == null)
            {
                Debug.LogError("Failed to load custom level — aborting LoadCustomLevel!");
                return;
            }
            DiscordManagerPatches.LevelCodeDiscord = LevelCode;

            AccessTools.Method(directorInstance.GetType(), "InitCommonSystems")?.Invoke(directorInstance, null);

            var openingField = traverse.Field("opening").GetValue();
            AccessTools.Method(typeof(LevelAnimation), "OpenLevel")?.Invoke(null, new object[] { openingField });

            traverse.Field("editingGameLevel").SetValue(null);

            AccessTools.Method(directorInstance.GetType(), "InitEntitiesAndCropGrid")?.Invoke(directorInstance, null);

            object replayManager = traverse.Field("replayManager").GetValue();
            if (replayManager != null)
                AccessTools.Method(replayManager.GetType(), "Init")?.Invoke(replayManager, null);
            else
                Debug.LogWarning("ReplayManager is null!");
        }
    }
}
