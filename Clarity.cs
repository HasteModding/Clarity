using Landfall.Haste;
using Landfall.Modding;
using UnityEngine;
using UnityEngine.Localization;
using Zorro.Settings;
using System.Text.RegularExpressions;

namespace Clarity;

[LandfallPlugin]
public class Clarity
{
    static Clarity()
    {
        Debug.Log("[Clarity] Loaded!");
    }
}

// --- New Resolution Setting ---
[HasteSetting]
public class ResolutionSetting : StringSetting, IExposedSetting
{
    // Regex to validate and parse "WIDTHxHEIGHT" format
    private static readonly Regex ResolutionRegex = new Regex(@"^(\d+)[xX](\d+)$");

    // Store the last successfully applied resolution
    private static int currentWidth = Screen.width;
    private static int currentHeight = Screen.height;
    private static bool currentFullscreen = Screen.fullScreen;

    public override void ApplyValue()
    {
        Match match = ResolutionRegex.Match(Value);

        if (match.Success &&
            int.TryParse(match.Groups[1].Value, out int width) &&
            int.TryParse(match.Groups[2].Value, out int height))
        {
            if (width > 0 && height > 0)
            {
                Screen.SetResolution(width, height, Screen.fullScreen);
                currentWidth = width;
                currentHeight = height;
                currentFullscreen = Screen.fullScreen; // Update just in case
                Debug.Log($"[Clarity] Screen resolution set to {width}x{height}, Fullscreen: {Screen.fullScreen}");
            }
            else
            {
                Debug.LogWarning($"[Clarity] Invalid resolution dimensions: {width}x{height}. Must be positive.");
            }
        }
        else
        {
            Debug.LogWarning($"[Clarity] Invalid resolution format: '{Value}'. Expected 'WIDTHxHEIGHT'.");
        }
    }

    protected override string GetDefaultValue()
    {
        return $"{Screen.width}x{Screen.height}";
    }

    public LocalizedString GetDisplayName() => new UnlocalizedString("Resolution");

    public string GetCategory() => "Graphics";
}

