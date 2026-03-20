using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

/// <summary>
/// AudioManager handles all audio playback for the Memory Card Game.
/// Features: SFX/BGM separation, volume control, sound pooling for optimization.
/// </summary>
public class AudioManager : MonoBehaviour
{
    [Header("Audio Sources")]
    [SerializeField] private AudioSource sfxAudioSource;
    [SerializeField] private AudioSource bgmAudioSource;

    [Header("Sound Effects")]
    [SerializeField] private AudioClip flipSound;
    [SerializeField] private AudioClip matchSound;
    [SerializeField] private AudioClip mismatchSound;
    [SerializeField] private AudioClip winSound;
    [SerializeField] private AudioClip gameOverSound;
    [SerializeField] private AudioClip buttonClickSound;
    [SerializeField] private AudioClip comboSound;

    [Header("Background Music")]
    [SerializeField] private AudioClip bgmGameplay;
    [SerializeField] private AudioClip bgmMenu;

    [Header("Volume Settings")]
    [SerializeField] private float masterVolume = 1f;
    [SerializeField] private float sfxVolume = 1f;
    [SerializeField] private float bgmVolume = 0.7f;

    private static AudioManager instance;
    private Dictionary<string, AudioClip> soundCache = new Dictionary<string, AudioClip>();
    private Coroutine bgmCoroutine;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            
            Debug.Log("[AudioManager.Awake] AudioManager initialized as singleton (DontDestroyOnLoad)");
            Debug.Log("[AudioManager.Awake] SFX AudioSource: " + (sfxAudioSource != null ? "✓ Assigned" : "✗ NULL"));
            Debug.Log("[AudioManager.Awake] BGM AudioSource: " + (bgmAudioSource != null ? "✓ Assigned" : "✗ NULL"));
            
            InitializeSoundCache();
            EnsureAudioSourcesEnabled();

            // Subscribe to scene load events
            SceneManager.sceneLoaded += OnSceneLoaded;
            Debug.Log("[AudioManager.Awake] Scene loaded event subscribed");
        }
        else
        {
            Debug.LogWarning("[AudioManager.Awake] AudioManager instance already exists! Destroying duplicate.");
            Destroy(gameObject);
        }
    }

    private void OnDestroy()
    {
        if (instance == this)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
    }

    /// <summary>
    /// Called when a scene is loaded to re-enable audio sources if needed.
    /// Stops any existing BGM to ensure clean transition between scenes.
    /// </summary>
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log($"[OnSceneLoaded] Scene: {scene.name}, Mode: {mode}");
        
        EnsureAudioSourcesEnabled();
        
        // Stop any existing BGM and coroutines when transitioning scenes
        // This prevents old BGM from continuing to play in new scene
        if (bgmCoroutine != null)
        {
            StopCoroutine(bgmCoroutine);
            bgmCoroutine = null;
            Debug.Log("[OnSceneLoaded] Previous BGM coroutine stopped");
        }
        
        if (bgmAudioSource != null && bgmAudioSource.isPlaying)
        {
            Debug.Log($"[OnSceneLoaded] Stopping currently playing BGM: {bgmAudioSource.clip?.name}");
            bgmAudioSource.Stop();
            bgmAudioSource.clip = null;
            Debug.Log($"[OnSceneLoaded] BGM stopped and clip cleared. Scene loaded: {scene.name}");
        }
        else
        {
            Debug.Log($"[OnSceneLoaded] No BGM currently playing. Scene loaded: {scene.name}");
        }
    }

    /// <summary>
    /// Ensures audio sources are enabled and properly configured.
    /// </summary>
    private void EnsureAudioSourcesEnabled()
    {
        if (sfxAudioSource != null)
        {
            if (!sfxAudioSource.gameObject.activeSelf)
            {
                sfxAudioSource.gameObject.SetActive(true);
                Debug.LogWarning("SFX AudioSource GameObject was inactive, enabling it now.");
            }
            if (!sfxAudioSource.enabled)
            {
                sfxAudioSource.enabled = true;
                Debug.LogWarning("SFX AudioSource component was disabled, enabling it now.");
            }
        }

        if (bgmAudioSource != null)
        {
            if (!bgmAudioSource.gameObject.activeSelf)
            {
                bgmAudioSource.gameObject.SetActive(true);
                Debug.LogWarning("BGM AudioSource GameObject was inactive, enabling it now.");
            }
            if (!bgmAudioSource.enabled)
            {
                bgmAudioSource.enabled = true;
                Debug.LogWarning("BGM AudioSource component was disabled, enabling it now.");
            }
        }
    }

    private void InitializeSoundCache()
    {
        Debug.Log("[InitializeSoundCache] Building sound cache...");
        
        soundCache["flip"] = flipSound;
        if (flipSound != null) Debug.Log("[InitializeSoundCache] ✓ flip: " + flipSound.name);
        else Debug.LogWarning("[InitializeSoundCache] ✗ flip is NULL!");
        
        soundCache["match"] = matchSound;
        if (matchSound != null) Debug.Log("[InitializeSoundCache] ✓ match: " + matchSound.name);
        else Debug.LogWarning("[InitializeSoundCache] ✗ match is NULL!");
        
        soundCache["mismatch"] = mismatchSound;
        if (mismatchSound != null) Debug.Log("[InitializeSoundCache] ✓ mismatch: " + mismatchSound.name);
        else Debug.LogWarning("[InitializeSoundCache] ✗ mismatch is NULL!");
        
        soundCache["win"] = winSound;
        if (winSound != null) Debug.Log("[InitializeSoundCache] ✓ win: " + winSound.name);
        else Debug.LogWarning("[InitializeSoundCache] ✗ win is NULL!");
        
        soundCache["gameOver"] = gameOverSound;
        if (gameOverSound != null) Debug.Log("[InitializeSoundCache] ✓ gameOver: " + gameOverSound.name);
        else Debug.LogWarning("[InitializeSoundCache] ✗ gameOver is NULL!");
        
        soundCache["click"] = buttonClickSound;
        if (buttonClickSound != null) Debug.Log("[InitializeSoundCache] ✓ click: " + buttonClickSound.name);
        else Debug.LogWarning("[InitializeSoundCache] ✗ click is NULL!");
        
        soundCache["combo"] = comboSound;
        if (comboSound != null) Debug.Log("[InitializeSoundCache] ✓ combo: " + comboSound.name);
        else Debug.LogWarning("[InitializeSoundCache] ✗ combo is NULL!");
        
        Debug.Log($"[InitializeSoundCache] Cache initialized with {soundCache.Count} sounds");
    }

    /// <summary>
    /// Plays a sound effect.
    /// </summary>
    public void PlaySFX(AudioClip clip, float volumeScale = 1f)
    {
        if (sfxAudioSource == null)
        {
            Debug.LogWarning("[PlaySFX] SFX AudioSource not assigned!");
            return;
        }

        if (!sfxAudioSource.gameObject.activeSelf)
        {
            sfxAudioSource.gameObject.SetActive(true);
            Debug.Log("[PlaySFX] SFX GameObject activated");
        }

        if (!sfxAudioSource.enabled)
        {
            sfxAudioSource.enabled = true;
            Debug.Log("[PlaySFX] SFX AudioSource component enabled");
        }

        if (clip != null)
        {
            float finalVolume = sfxVolume * masterVolume * volumeScale;
            sfxAudioSource.PlayOneShot(clip, finalVolume);
            Debug.Log($"[PlaySFX] Playing: {clip.name} (Volume: {finalVolume})");
        }
        else
        {
            Debug.LogWarning("[PlaySFX] Clip is NULL! Cannot play sound effect.");
        }
    }

    /// <summary>
    /// Plays a named sound effect from cache.
    /// </summary>
    public void PlaySFX(string soundName, float volumeScale = 1f)
    {
        Debug.Log($"[PlaySFX] Attempting to play sound: {soundName}");
        
        if (soundCache.ContainsKey(soundName))
        {
            AudioClip clip = soundCache[soundName];
            if (clip != null)
            {
                Debug.Log($"[PlaySFX] Found in cache: {soundName}, clip: {clip.name}");
                PlaySFX(clip, volumeScale);
            }
            else
            {
                Debug.LogWarning($"[PlaySFX] Sound '{soundName}' is in cache but clip is NULL!");
            }
        }
        else
        {
            Debug.LogError($"[PlaySFX] Sound '{soundName}' NOT found in cache! Available sounds: {string.Join(", ", soundCache.Keys)}");
        }
    }

    public void PlayFlipSound()
    {
        Debug.Log("[PlayFlipSound] Called");
        PlaySFX("flip");
    }
    
    public void PlayMatchSound()
    {
        Debug.Log("[PlayMatchSound] Called");
        PlaySFX("match", 1.2f);
    }
    
    public void PlayMismatchSound()
    {
        Debug.Log("[PlayMismatchSound] Called");
        PlaySFX("mismatch");
    }
    
    public void PlayWinSound()
    {
        Debug.Log("[PlayWinSound] Called");
        PlaySFX("win", 1.5f);
    }
    
    public void PlayGameOverSound()
    {
        Debug.Log("[PlayGameOverSound] Called");
        PlaySFX("gameOver", 1.3f);
    }
    
    public void PlayButtonClickSound()
    {
        Debug.Log("[PlayButtonClickSound] Called");
        PlaySFX("click", 0.8f);
    }
    
    public void PlayComboSound()
    {
        Debug.Log("[PlayComboSound] Called");
        PlaySFX("combo", 1f);
    }

    /// <summary>
    /// Plays background music.
    /// </summary>
    public void PlayBGM(AudioClip bgmClip)
    {
        Debug.Log($"[PlayBGM] Called with clip: {bgmClip?.name}");
        
        if (bgmAudioSource == null)
        {
            Debug.LogWarning("BGM AudioSource not assigned!");
            return;
        }

        if (bgmClip == null)
        {
            Debug.LogWarning("[PlayBGM] BGM Clip is null! Cannot play.");
            return;
        }

        // Cancel any existing BGM coroutine
        if (bgmCoroutine != null)
        {
            StopCoroutine(bgmCoroutine);
            Debug.Log("[PlayBGM] Previous BGM coroutine cancelled");
        }

        // Ensure component is enabled
        if (!bgmAudioSource.gameObject.activeSelf)
        {
            bgmAudioSource.gameObject.SetActive(true);
            Debug.Log("[PlayBGM] BGM GameObject activated");
        }

        if (!bgmAudioSource.enabled)
        {
            bgmAudioSource.enabled = true;
            Debug.Log("[PlayBGM] BGM AudioSource component enabled");
        }

        // Stop any currently playing BGM and clear clip
        if (bgmAudioSource.isPlaying)
        {
            bgmAudioSource.Stop();
            Debug.Log($"[PlayBGM] Previous BGM stopped. Was playing: {bgmAudioSource.clip?.name}");
        }

        // Clear the current clip to ensure clean slate
        bgmAudioSource.clip = null;
        Debug.Log("[PlayBGM] Clip cleared, starting coroutine to assign new clip");

        // Start new BGM coroutine
        bgmCoroutine = StartCoroutine(PlayBGMWithDelay(bgmClip));
    }

    private IEnumerator PlayBGMWithDelay(AudioClip bgmClip)
    {
        // Wait one frame to ensure clean transition
        yield return null;

        if (bgmAudioSource != null && bgmClip != null)
        {
            // Double-check that audio source is ready
            if (!bgmAudioSource.enabled)
            {
                bgmAudioSource.enabled = true;
                Debug.Log("BGM AudioSource re-enabled in PlayBGMWithDelay");
            }
            
            if (!bgmAudioSource.gameObject.activeSelf)
            {
                bgmAudioSource.gameObject.SetActive(true);
                Debug.Log("BGM GameObject re-activated in PlayBGMWithDelay");
            }
            
            // Assign the BGM clip to audio source
            bgmAudioSource.clip = bgmClip;
            Debug.Log($"BGM clip assigned: {bgmClip.name}");
            
            // Now that clip is assigned, reset playback position
            bgmAudioSource.time = 0f;
            
            // Configure and play
            bgmAudioSource.volume = bgmVolume * masterVolume;
            bgmAudioSource.loop = true;
            bgmAudioSource.Play();
            Debug.Log($"BGM now playing: {bgmClip.name} (Volume: {bgmAudioSource.volume})");
        }
        else
        {
            Debug.LogWarning($"Failed to play BGM: AudioSource={bgmAudioSource}, Clip={bgmClip}");
        }
    }

    public void PlayGameplayBGM()
    {
        Debug.Log("[PlayGameplayBGM] Called. bgmGameplay clip: " + (bgmGameplay?.name ?? "NULL"));
        PlayBGM(bgmGameplay);
    }
    
    public void PlayMenuBGM()
    {
        Debug.Log("[PlayMenuBGM] Called. bgmMenu clip: " + (bgmMenu?.name ?? "NULL"));
        PlayBGM(bgmMenu);
    }

    public void StopBGM()
    {
        Debug.Log("[StopBGM] Stopping BGM...");
        
        // Cancel any existing BGM coroutine
        if (bgmCoroutine != null)
        {
            StopCoroutine(bgmCoroutine);
            bgmCoroutine = null;
            Debug.Log("[StopBGM] BGM coroutine cancelled");
        }

        if (bgmAudioSource != null)
        {
            // Ensure it's enabled before trying to stop
            if (!bgmAudioSource.gameObject.activeSelf)
            {
                bgmAudioSource.gameObject.SetActive(true);
            }
            if (!bgmAudioSource.enabled)
            {
                bgmAudioSource.enabled = true;
            }

            if (bgmAudioSource.isPlaying)
            {
                Debug.Log($"[StopBGM] Stopping currently playing BGM: {bgmAudioSource.clip?.name}");
                bgmAudioSource.Stop();
                bgmAudioSource.clip = null;
                Debug.Log("[StopBGM] BGM stopped and clip cleared");
            }
            else
            {
                Debug.Log("[StopBGM] No BGM currently playing, but clearing clip anyway");
                bgmAudioSource.clip = null;
            }
        }
        else
        {
            Debug.LogWarning("[StopBGM] bgmAudioSource is NULL!");
        }
    }

    public void PauseBGM()
    {
        if (bgmAudioSource != null)
        {
            // Ensure it's enabled before trying to pause
            if (!bgmAudioSource.gameObject.activeSelf)
            {
                bgmAudioSource.gameObject.SetActive(true);
            }
            if (!bgmAudioSource.enabled)
            {
                bgmAudioSource.enabled = true;
            }

            if (bgmAudioSource.isPlaying)
            {
                bgmAudioSource.Pause();
            }
        }
    }

    public void ResumeBGM()
    {
        if (bgmAudioSource != null)
        {
            // Ensure it's enabled before trying to resume
            if (!bgmAudioSource.gameObject.activeSelf)
            {
                bgmAudioSource.gameObject.SetActive(true);
            }
            if (!bgmAudioSource.enabled)
            {
                bgmAudioSource.enabled = true;
            }

            if (!bgmAudioSource.isPlaying)
            {
                bgmAudioSource.Play();
            }
        }
    }

    /// <summary>
    /// Sets master volume (0-1).
    /// </summary>
    public void SetMasterVolume(float volume)
    {
        masterVolume = Mathf.Clamp01(volume);
        AudioListener.volume = masterVolume;
        UpdateAudioSourceVolumes();
    }

    public void SetSFXVolume(float volume)
    {
        sfxVolume = Mathf.Clamp01(volume);
        if (sfxAudioSource != null)
            sfxAudioSource.volume = sfxVolume * masterVolume;
    }

    public void SetBGMVolume(float volume)
    {
        bgmVolume = Mathf.Clamp01(volume);
        UpdateAudioSourceVolumes();
    }

    private void UpdateAudioSourceVolumes()
    {
        if (bgmAudioSource != null)
            bgmAudioSource.volume = bgmVolume * masterVolume;
    }

    // Getters
    public float GetMasterVolume() => masterVolume;
    public float GetSFXVolume() => sfxVolume;
    public float GetBGMVolume() => bgmVolume;

    /// <summary>
    /// Diagnostic method: Reports the status of all audio clips and components.
    /// Call this from the console to debug audio issues.
    /// </summary>
    public void DiagnoseAudio()
    {
        Debug.Log("========== AUDIO DIAGNOSTIC REPORT ==========");
        
        // Check audio sources
        Debug.Log($"[Diagnosis] SFX AudioSource: {(sfxAudioSource != null ? "✓ Assigned" : "✗ NULL")}");
        if (sfxAudioSource != null)
        {
            Debug.Log($"  - GameObject Active: {sfxAudioSource.gameObject.activeSelf}");
            Debug.Log($"  - Component Enabled: {sfxAudioSource.enabled}");
            Debug.Log($"  - Volume: {sfxAudioSource.volume}");
        }
        
        Debug.Log($"[Diagnosis] BGM AudioSource: {(bgmAudioSource != null ? "✓ Assigned" : "✗ NULL")}");
        if (bgmAudioSource != null)
        {
            Debug.Log($"  - GameObject Active: {bgmAudioSource.gameObject.activeSelf}");
            Debug.Log($"  - Component Enabled: {bgmAudioSource.enabled}");
            Debug.Log($"  - Volume: {bgmAudioSource.volume}");
        }
        
        // Check volume settings
        Debug.Log($"[Diagnosis] Master Volume: {masterVolume}");
        Debug.Log($"[Diagnosis] SFX Volume: {sfxVolume}");
        Debug.Log($"[Diagnosis] BGM Volume: {bgmVolume}");
        
        // Check all SFX clips
        Debug.Log("[Diagnosis] ========== SOUND EFFECTS CLIPS ==========");
        Debug.Log($"  flip: {(flipSound != null ? "✓ " + flipSound.name : "✗ NULL")}");
        Debug.Log($"  match: {(matchSound != null ? "✓ " + matchSound.name : "✗ NULL")}");
        Debug.Log($"  mismatch: {(mismatchSound != null ? "✓ " + mismatchSound.name : "✗ NULL")}");
        Debug.Log($"  win: {(winSound != null ? "✓ " + winSound.name : "✗ NULL")}");
        Debug.Log($"  gameOver: {(gameOverSound != null ? "✓ " + gameOverSound.name : "✗ NULL")}");
        Debug.Log($"  click: {(buttonClickSound != null ? "✓ " + buttonClickSound.name : "✗ NULL")}");
        Debug.Log($"  combo: {(comboSound != null ? "✓ " + comboSound.name : "✗ NULL")}");
        
        // Check BGM clips
        Debug.Log("[Diagnosis] ========== BACKGROUND MUSIC CLIPS ==========");
        Debug.Log($"  bgmGameplay: {(bgmGameplay != null ? "✓ " + bgmGameplay.name : "✗ NULL")}");
        Debug.Log($"  bgmMenu: {(bgmMenu != null ? "✓ " + bgmMenu.name : "✗ NULL")}");
        
        // Check cache
        Debug.Log("[Diagnosis] ========== SOUND CACHE ==========");
        Debug.Log($"  Cache Size: {soundCache.Count}");
        foreach (var kvp in soundCache)
        {
            Debug.Log($"  {kvp.Key}: {(kvp.Value != null ? "✓ " + kvp.Value.name : "✗ NULL")}");
        }
        
        Debug.Log("========== END DIAGNOSTIC REPORT ==========");
    }

    public static AudioManager Instance => instance;
}
