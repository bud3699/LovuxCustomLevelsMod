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

        }
    }
}
