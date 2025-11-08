using System;
using System.IO;
using System.Net;
using UnityEngine;
using HarmonyLib;

namespace LovuxPatcher
{
    public static class LevelGameBuilderExtensions
    {
        public static object CurrentGameLevelObject {get; private set;}
        public static void InitCustomLevelFromCode(this object instance, string levelCode, out object currentGameLevel)
        {
            currentGameLevel = null;

            try
            {
                string url = "http://127.0.0.1:7000/lovux/api/levels/code/" + levelCode;
                Debug.Log($"<color=yellow>[InitCustomLevelFromCode] Fetching level from server: {url}</color>");

                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                request.Method = "GET";

                string json;
                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                {
                    json = reader.ReadToEnd();
                }

                Debug.Log($"<color=cyan>[InitCustomLevelFromCode] Received JSON: {json}</color>");

                if (string.IsNullOrEmpty(json))
                {
                    Debug.LogError("<color=red>[InitCustomLevelFromCode] Empty level JSON received!</color>");
                    return;
                }

                GameLevelData levelData = JsonUtility.FromJson<GameLevelData>(json);
                if (levelData == null)
                {
                    Debug.LogError("<color=red>[InitCustomLevelFromCode] Failed to parse level JSON into GameLevelData!</color>");
                    return;
                }

                Debug.Log($"<color=green>[InitCustomLevelFromCode] Parsed GameLevelData successfully.</color>");

                Type gameLevelType = AccessTools.TypeByName("GameLevel");
                if (gameLevelType == null)
                {
                    Debug.LogError("<color=red>[InitCustomLevelFromCode] Could not find type GameLevel!</color>");
                    return;
                }

                object gameLevel = ScriptableObject.CreateInstance(gameLevelType);
                Debug.Log($"<color=green>[InitCustomLevelFromCode] Created GameLevel instance.</color>");

                Traverse gameLevelTraverse = Traverse.Create(gameLevel);
                gameLevelTraverse.Field("entityData").SetValue(levelData.entityData.Replace("\r", ""));
                Debug.Log($"<color=green>[InitCustomLevelFromCode] Set entityData field.</color>");

                Traverse traverse = Traverse.Create(instance);
                traverse.Field("customLevel").SetValue(true);
                traverse.Field("customGameLevel").SetValue(gameLevel);
                traverse.Field("currentScene").SetValue(false);
                Debug.Log($"<color=green>[InitCustomLevelFromCode] Set customLevel, customGameLevel, and currentScene fields.</color>");

                traverse.Method("ResetScene").GetValue();
                Debug.Log($"<color=green>[InitCustomLevelFromCode] Called ResetScene method.</color>");

                traverse.Method("LoadLevel", new object[] { gameLevel }).GetValue();
                Debug.Log($"<color=green>[InitCustomLevelFromCode] Called LoadLevel method.</color>");

                currentGameLevel = CurrentGameLevelObject = gameLevel;

                Debug.Log($"<color=green>[InitCustomLevelFromCode] Custom level loaded from code '{levelCode}'!</color>");
            }
            catch (Exception ex)
            {
                Debug.LogError($"<color=red>[InitCustomLevelFromCode] Failed to fetch or load level: {ex.Message}</color>");
                Debug.LogException(ex);
            }
        }
    }
}
