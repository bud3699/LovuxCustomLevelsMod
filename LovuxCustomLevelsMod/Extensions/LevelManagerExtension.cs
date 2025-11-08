using HarmonyLib;
using Mindlabor.Utils;
using System.Collections;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace LovuxPatcher
{
    public static class LevelManagerExtension
    {
        public static IEnumerator HandleGameModeLevelLoad() {
            Debug.Log("IN IEnum");
            var levelManagerType = typeof(LevelManager);
            var traverse = Traverse.Create(levelManagerType);

            if (Director.gameMode == GameMode.SandboxPlay)
            {
                Debug.Log("Finished Custom Level"); //Here we need to add support for uploading level
            }
            if (Director.gameMode == GameMode.Game || Director.gameMode.ToString() == "3")
            {
                int levelIndex = traverse.Field("levelIndex").GetValue<int>();
                AudioManager.instance?.PlaySFX(Director.instance.success);
                LevelAnimation.isLevelLoading = true;

                var method = AccessTools.Method(typeof(LevelManager), "LoadLevel");
                if (method != null)
                {
                    if (Director.gameMode == GameMode.Game)
                    {
                        FBPP.SetInt(SaveManager.currentLevel, levelIndex + 1);
                        var enumerator = (IEnumerator)method.Invoke(null, new object[] { levelIndex + 1 });
                        CoroutineUtils.RunCoroutine(enumerator);

                    }
                    else if (Director.gameMode.ToString() == "3")
                    {
                        Director.instance.CloseLevel();
                        Frame.instance?.PositiveFeedback();
                        yield return new WaitWhile(() => Frame.mexicanWave);
                        Director.instance.CloseLevel();
                        yield return new WaitWhile(() => LevelAnimation.isLevelLoading);
                        yield return new WaitForSecondsRealtime(0.1f);
                        Director.instance.Init();
                        Debug.Log("Load Finished screen here ??? Load something here to say you've completed it.. for now just reload level..");
                    }
                }
            }

        }

    }
}
