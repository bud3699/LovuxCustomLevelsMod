using MelonLoader;
using HarmonyLib;
using System.Reflection;

[assembly: MelonInfo(typeof(LovuxPatcher.Main), "Custom Levels", "0.1.0", "Bud3699")]
[assembly: MelonGame("Mindlabor", "Lovux")]

namespace LovuxPatcher
{
    public class Main : MelonMod
    {
        public static string ModVersion;
        public override void OnInitializeMelon()
        {
            MelonLogger.Msg("Mod Starting!");
            ModVersion = Info.Version;
            MelonLogger.Msg("Version: " + ModVersion);

            SaveManagerCustom.InitializeSave();
            MelonLogger.Msg("Initialized CustomSaveManager");

            SteamworksManager.Initialize();

            var harmony = new HarmonyLib.Harmony("com.bud3699.lovux.patch");
            //harmony.PatchAll(Assembly.GetExecutingAssembly());
            MelonLogger.Msg("Harmony initialized!");

            DirectorPatches.ApplyPatch(harmony);
            MelonLogger.Msg("Manual patch for Director applied!");

            MenuButtonSandboxPatches.ApplyPatch(harmony);
            MelonLogger.Msg("Patched MenuButtonSandbox to allow icon changes");

            AchievementPatcher.ApplyPatch(harmony);
            MelonLogger.Msg("Patched Achievements to prevent achievements when loading custom levels");

            DiscordManagerPatches.ApplyPatch(harmony);
            MelonLogger.Msg("Patched DiscordManager to show custom level presence");
        }
    }
}
