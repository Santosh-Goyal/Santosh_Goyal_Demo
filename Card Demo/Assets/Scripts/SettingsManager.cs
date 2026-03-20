using UnityEngine;

/// <summary>
/// SettingsManager handles persistent game settings like audio volumes and preferences.
/// Uses PlayerPrefs for cross-session persistence.
/// </summary>
public class SettingsManager : MonoBehaviour
{
    [Header("Default Settings")]
    [SerializeField] private float defaultMasterVolume = 1f;
    [SerializeField] private float defaultSFXVolume = 1f;
    [SerializeField] private float defaultBGMVolume = 0.7f;

    // Settings keys
    private const string MASTER_VOLUME_KEY = "MasterVolume";
    private const string SFX_VOLUME_KEY = "SFXVolume";
    private const string BGM_VOLUME_KEY = "BGMVolume";
    private const string SFX_ENABLED_KEY = "SFXEnabled";
    private const string BGM_ENABLED_KEY = "BGMEnabled";
    private const string SCREEN_WIDTH_KEY = "ScreenWidth";
    private const string SCREEN_HEIGHT_KEY = "ScreenHeight";
    private const string FULLSCREEN_KEY = "Fullscreen";

    private static SettingsManager instance;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeSettings();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Initializes settings with defaults if not already set.
    /// </summary>
    private void InitializeSettings()
    {
        if (!PlayerPrefs.HasKey(MASTER_VOLUME_KEY))
        {
            SetMasterVolume(defaultMasterVolume);
        }
        if (!PlayerPrefs.HasKey(SFX_VOLUME_KEY))
        {
            SetSFXVolume(defaultSFXVolume);
        }
        if (!PlayerPrefs.HasKey(BGM_VOLUME_KEY))
        {
            SetBGMVolume(defaultBGMVolume);
        }
        if (!PlayerPrefs.HasKey(SFX_ENABLED_KEY))
        {
            SetSFXEnabled(true);
        }
        if (!PlayerPrefs.HasKey(BGM_ENABLED_KEY))
        {
            SetBGMEnabled(true);
        }

        PlayerPrefs.Save();
    }

    /// <summary>
    /// Audio settings
    /// </summary>
    public void SetMasterVolume(float volume)
    {
        float clampedVolume = Mathf.Clamp01(volume);
        PlayerPrefs.SetFloat(MASTER_VOLUME_KEY, clampedVolume);
        PlayerPrefs.Save();

        if (AudioManager.Instance != null)
            AudioManager.Instance.SetMasterVolume(clampedVolume);
    }

    public void SetSFXVolume(float volume)
    {
        float clampedVolume = Mathf.Clamp01(volume);
        PlayerPrefs.SetFloat(SFX_VOLUME_KEY, clampedVolume);
        PlayerPrefs.Save();

        if (AudioManager.Instance != null)
            AudioManager.Instance.SetSFXVolume(clampedVolume);
    }

    public void SetBGMVolume(float volume)
    {
        float clampedVolume = Mathf.Clamp01(volume);
        PlayerPrefs.SetFloat(BGM_VOLUME_KEY, clampedVolume);
        PlayerPrefs.Save();

        if (AudioManager.Instance != null)
            AudioManager.Instance.SetBGMVolume(clampedVolume);
    }

    public void SetSFXEnabled(bool enabled)
    {
        PlayerPrefs.SetInt(SFX_ENABLED_KEY, enabled ? 1 : 0);
        PlayerPrefs.Save();

        if (AudioManager.Instance != null)
            AudioManager.Instance.SetSFXVolume(enabled ? GetSFXVolume() : 0f);
    }

    public void SetBGMEnabled(bool enabled)
    {
        PlayerPrefs.SetInt(BGM_ENABLED_KEY, enabled ? 1 : 0);
        PlayerPrefs.Save();

        if (AudioManager.Instance != null)
            AudioManager.Instance.SetBGMVolume(enabled ? GetBGMVolume() : 0f);
    }

    public float GetMasterVolume() => PlayerPrefs.GetFloat(MASTER_VOLUME_KEY, defaultMasterVolume);
    public float GetSFXVolume() => PlayerPrefs.GetFloat(SFX_VOLUME_KEY, defaultSFXVolume);
    public float GetBGMVolume() => PlayerPrefs.GetFloat(BGM_VOLUME_KEY, defaultBGMVolume);
    public bool IsSFXEnabled() => PlayerPrefs.GetInt(SFX_ENABLED_KEY, 1) == 1;
    public bool IsBGMEnabled() => PlayerPrefs.GetInt(BGM_ENABLED_KEY, 1) == 1;

    /// <summary>
    /// Display settings
    /// </summary>
    public void SetResolution(int width, int height, bool fullscreen)
    {
        Screen.SetResolution(width, height, fullscreen);
        PlayerPrefs.SetInt(SCREEN_WIDTH_KEY, width);
        PlayerPrefs.SetInt(SCREEN_HEIGHT_KEY, height);
        PlayerPrefs.SetInt(FULLSCREEN_KEY, fullscreen ? 1 : 0);
        PlayerPrefs.Save();
    }

    public void GetResolution(out int width, out int height, out bool fullscreen)
    {
        width = PlayerPrefs.GetInt(SCREEN_WIDTH_KEY, Screen.currentResolution.width);
        height = PlayerPrefs.GetInt(SCREEN_HEIGHT_KEY, Screen.currentResolution.height);
        fullscreen = PlayerPrefs.GetInt(FULLSCREEN_KEY, 1) == 1;
    }

    /// <summary>
    /// Resets all settings to defaults.
    /// </summary>
    public void ResetToDefaults()
    {
        PlayerPrefs.DeleteKey(MASTER_VOLUME_KEY);
        PlayerPrefs.DeleteKey(SFX_VOLUME_KEY);
        PlayerPrefs.DeleteKey(BGM_VOLUME_KEY);
        PlayerPrefs.DeleteKey(SFX_ENABLED_KEY);
        PlayerPrefs.DeleteKey(BGM_ENABLED_KEY);
        PlayerPrefs.Save();

        InitializeSettings();
    }

    public static SettingsManager Instance => instance;
}
