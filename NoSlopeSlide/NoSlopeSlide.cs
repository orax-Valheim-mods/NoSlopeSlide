using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;

namespace NoSlopeSlide;

[BepInPlugin(PluginGUID, PluginName, PluginVersion)]
public class NoSlopeSlide : BaseUnityPlugin
{
    public const string PluginGUID = "orax.NoSlopeSlide";
    public const string PluginName = "NoSlopeSlide";
    public const string PluginVersion = "0.1.0";

    public static ConfigEntry<bool> Enabled;

    private Harmony _harmony;

    private void Awake()
    {
        Enabled = Config.Bind("General", "Enabled", true, "Enable this mod.");

        _harmony = new Harmony(PluginGUID);
        _harmony.PatchAll();

        Logger.LogInfo($"{PluginName} v{PluginVersion} loaded.");
    }

    private void OnDestroy()
    {
        _harmony?.UnpatchSelf();
    }
}
