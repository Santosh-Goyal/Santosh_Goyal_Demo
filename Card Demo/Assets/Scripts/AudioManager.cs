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
            
            InitializeSoundCache();
            EnsureAudioSourcesEnabled();

            // Subscribe to scene load events
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
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
            bgmAudioSource.Stop();
            bgmAudioSource.clip = null;
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
            }
            if (!sfxAudioSource.enabled)
            {
                sfxAudioSource.enabled = true;
            }
        }

        if (bgmAudioSource != null)
        {
            if (!bgmAudioSource.gameObject.activeSelf)
            {
                bgmAudioSource.gameObject.SetActive(true);
            }
            if (!bgmAudioSource.enabled)
            {
                bgmAudioSource.enabled = true;
            }
        }
    }

    private void InitializeSoundCache()
    {   
        soundCache["flip"] = flipSound;
        
        soundCache["match"] = matchSound;
        
        soundCache["mismatch"] = mismatchSound;
        
        soundCache["win"] = winSound;
        
        soundCache["gameOver"] = gameOverSound;
        
        soundCache["click"] = buttonClickSound;
        
        soundCache["combo"] = comboSound;
    }

    /// <summary>
    /// Plays a sound effect.
    /// </summary>
    public void PlaySFX(AudioClip clip, float volumeScale = 1f)
    {
        if (sfxAudioSource == null)
        {
            return;
        }

        if (!sfxAudioSource.gameObject.activeSelf)
        {
            sfxAudioSource.gameObject.SetActive(true);
        }

        if (!sfxAudioSource.enabled)
        {
            sfxAudioSource.enabled = true;
        }

        if (clip != null)
        {
            float finalVolume = sfxVolume * masterVolume * volumeScale;
            sfxAudioSource.PlayOneShot(clip, finalVolume);
        }
    }

    /// <summary>
    /// Plays a named sound effect from cache.
    /// </summary>
    public void PlaySFX(string soundName, float volumeScale = 1f)
    {
        if (soundCache.ContainsKey(soundName))
        {
            AudioClip clip = soundCache[soundName];
            if (clip != null)
            {
                PlaySFX(clip, volumeScale);
            }
        }
    }

    public void PlayFlipSound()
    {
        PlaySFX("flip");
    }
    
    public void PlayMatchSound()
    {
        PlaySFX("match", 1.2f);
    }
    
    public void PlayMismatchSound()
    {
        PlaySFX("mismatch");
    }
    
    public void PlayWinSound()
    {
        PlaySFX("win", 1.5f);
    }
    
    public void PlayGameOverSound()
    {
        PlaySFX("gameOver", 1.3f);
    }
    
    public void PlayButtonClickSound()
    {
        PlaySFX("click", 0.8f);
    }
    
    public void PlayComboSound()
    {
        PlaySFX("combo", 1f);
    }

    /// <summary>
    /// Plays background music.
    /// </summary>
    public void PlayBGM(AudioClip bgmClip)
    {        
        if (bgmAudioSource == null)
        {
            return;
        }

        if (bgmClip == null)
        {
            return;
        }

        // Cancel any existing BGM coroutine
        if (bgmCoroutine != null)
        {
            StopCoroutine(bgmCoroutine);
        }

        // Ensure component is enabled
        if (!bgmAudioSource.gameObject.activeSelf)
        {
            bgmAudioSource.gameObject.SetActive(true);
        }

        if (!bgmAudioSource.enabled)
        {
            bgmAudioSource.enabled = true;
        }

        // Stop any currently playing BGM and clear clip
        if (bgmAudioSource.isPlaying)
        {
            bgmAudioSource.Stop();
        }

        // Clear the current clip to ensure clean slate
        bgmAudioSource.clip = null;

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
            }
            
            if (!bgmAudioSource.gameObject.activeSelf)
            {
                bgmAudioSource.gameObject.SetActive(true);
            }
            
            // Assign the BGM clip to audio source
            bgmAudioSource.clip = bgmClip;
            
            // Now that clip is assigned, reset playback position
            bgmAudioSource.time = 0f;
            
            // Configure and play
            bgmAudioSource.volume = bgmVolume * masterVolume;
            bgmAudioSource.loop = true;
            bgmAudioSource.Play();
        }
    }

    public void PlayGameplayBGM()
    {
        PlayBGM(bgmGameplay);
    }
    
    public void PlayMenuBGM()
    {
        PlayBGM(bgmMenu);
    }

    public void StopBGM()
    {        
        // Cancel any existing BGM coroutine
        if (bgmCoroutine != null)
        {
            StopCoroutine(bgmCoroutine);
            bgmCoroutine = null;
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
                bgmAudioSource.Stop();
                bgmAudioSource.clip = null;
            }
            else
            {
                bgmAudioSource.clip = null;
            }
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
    public static AudioManager Instance => instance;
}
