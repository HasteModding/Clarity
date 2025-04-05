using Landfall.Haste;
using Landfall.Modding;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.UI;
using Zorro.Settings;
using System.Text.RegularExpressions;

namespace Clarity
{
    [LandfallPlugin]
    public class Clarity
    {
        // Flag to determine if the experimental ultrawide fix is enabled.
        public static bool UltrawideFixEnabled = false;

        // Monitor instance to process later-spawned canvases.
        private static UltrawideFixMonitor? _monitor;

        static Clarity()
        {
            Debug.Log("[Clarity] Loaded!");
            // By default, the experimental fix is off.
        }

        /// <summary>
        /// Ensures that a monitor object exists to check for new canvases.
        /// </summary>
        public static void EnsureMonitor()
        {
            if (_monitor == null)
            {
                GameObject go = new GameObject("UltrawideFixMonitor");
                _monitor = go.AddComponent<UltrawideFixMonitor>();
                UnityEngine.Object.DontDestroyOnLoad(go);
                Debug.Log("[Clarity] UltrawideFixMonitor created.");
            }
        }

        /// <summary>
        /// Applies the ultrawide fix to every canvas found in the scene.
        /// </summary>
        public static void ApplyUltrawideFix()
        {
            if (!UltrawideFixEnabled)
            {
                Debug.Log("[Clarity] Experimental ultrawide fix is disabled, skipping fix.");
                return;
            }

            Canvas[] canvases = GameObject.FindObjectsOfType<Canvas>();
            if (canvases != null && canvases.Length > 0)
            {
                foreach (Canvas canvas in canvases)
                {
                    ApplyFixToCanvas(canvas);
                }
            }
            else
            {
                Debug.LogWarning("[Clarity] No canvases found in scene.");
            }
        }

        /// <summary>
        /// Applies the CanvasScaler modifications to the given canvas.
        /// </summary>
        public static void ApplyFixToCanvas(Canvas canvas)
        {
            if (canvas == null)
            {
                return;
            }

            CanvasScaler scaler = canvas.GetComponent<CanvasScaler>();
            if (scaler != null)
            {
                // Set the reference resolution for which the UI is designed.
                scaler.referenceResolution = new Vector2(1920, 1080);
                // Set the scaling mode.
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                // Blend between width and height matching (0 = match width, 1 = match height).
                scaler.matchWidthOrHeight = 0.5f;
                Debug.Log($"[Clarity] Ultrawide fix applied to canvas: {canvas.name}");
            }
            else
            {
                Debug.LogWarning($"[Clarity] CanvasScaler not found on canvas: {canvas.name}");
            }
        }
    }

    /// <summary>
    /// A monitor that periodically checks for new canvases and applies the ultrawide fix.
    /// </summary>
    public class UltrawideFixMonitor : MonoBehaviour
    {
        // Keeps track of canvases that have already been processed.
        private readonly HashSet<Canvas> processedCanvases = new HashSet<Canvas>();

        void Update()
        {
            if (!Clarity.UltrawideFixEnabled)
            {
                return;
            }

            Canvas[] canvases = GameObject.FindObjectsOfType<Canvas>();
            foreach (Canvas canvas in canvases)
            {
                if (!processedCanvases.Contains(canvas))
                {
                    Clarity.ApplyFixToCanvas(canvas);
                    processedCanvases.Add(canvas);
                }
            }
        }
    }

    // --- Experimental Ultrawide Fix Setting (Off by Default) ---
    [HasteSetting]
    public class EnableUltrawideFixSetting : BoolSetting, IExposedSetting
    {
        public override void ApplyValue()
        {
            Clarity.UltrawideFixEnabled = Value;
            if (Value)
            {
                // Enable the monitor and update all existing canvases.
                Clarity.EnsureMonitor();
                Clarity.ApplyUltrawideFix();
                Debug.Log("[Clarity] Experimental ultrawide fix has been enabled.");
            }
            else
            {
                Debug.Log("[Clarity] Experimental ultrawide fix has been disabled.");
            }
        }

        protected override bool GetDefaultValue()
        {
            return false; // Off by default.
        }

        // Implement abstract members using LocalizedString.
        public override LocalizedString OnString => new UnlocalizedString("Enabled");
        public override LocalizedString OffString => new UnlocalizedString("Disabled");

        public LocalizedString GetDisplayName() =>
            new UnlocalizedString("Enable Ultrawide Fix");

        public string GetCategory() => "Graphics";
    }

    // --- New Resolution Setting ---
    [HasteSetting]
    public class ResolutionSetting : StringSetting, IExposedSetting
    {
        // Regex to validate and parse "WIDTHxHEIGHT" formats.
        private static readonly Regex ResolutionRegex = new Regex(@"^(\d+)[xX](\d+)$");

        // Store the last successfully applied resolution.
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
                    currentFullscreen = Screen.fullScreen;
                    Debug.Log($"[Clarity] Screen resolution set to {width}x{height}, " +
                              $"Fullscreen: {Screen.fullScreen}");
                    // Reapply the ultrawide fix; the monitor will catch any changes.
                    Clarity.ApplyUltrawideFix();
                }
                else
                {
                    Debug.LogWarning($"[Clarity] Invalid resolution dimensions: " +
                                     $"{width}x{height}. Must be positive.");
                }
            }
            else
            {
                Debug.LogWarning($"[Clarity] Invalid resolution format: '{Value}'. " +
                                 "Expected 'WIDTHxHEIGHT'.");
            }
        }

        protected override string GetDefaultValue()
        {
            return $"{Screen.width}x{Screen.height}";
        }

        public LocalizedString GetDisplayName() =>
            new UnlocalizedString("Resolution");

        public string GetCategory() => "Graphics";
    }
}
