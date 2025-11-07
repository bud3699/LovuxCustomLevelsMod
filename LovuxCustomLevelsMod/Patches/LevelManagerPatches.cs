using HarmonyLib;
using Mindlabor.Utils;


namespace LovuxPatcher
{
    public static class LevelManagerPatches
    {
        [HarmonyPatch(typeof(LevelManager), "CompleteLevelCoroutine")]
        [HarmonyPostfix]
        public static void Postfix_LevelManager_CompleteLevelCoroutine()
        {
            if (Director.gameMode == GameMode.Game)
            {
                AudioManager.instance?.PlaySFX(Director.instance.success);
                LevelAnimation.isLevelLoading = true;
                

                //load level co-routine here to reload finished level

            }
        }
    }
}
