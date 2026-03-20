using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;

/// <summary>
/// UIManager handles all UI elements including HUD, menus, and settings.
/// Manages game flow between different screens.
/// </summary>
public class UIManager : MonoBehaviour
{
    [Header("HUD Elements")]
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI comboText;
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private TextMeshProUGUI matchesText;

    [Header("Panels")]
    [SerializeField] private GameObject mainMenuPanel   ;
    [SerializeField] private GameObject gameplayHUD;
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private GameObject settingsPanel;

    [Header("Game Over UI")]
    [SerializeField] private TextMeshProUGUI finalScoreText;
    [SerializeField] private TextMeshProUGUI matchesCountText;
    [SerializeField] private TextMeshProUGUI accuracyText;
    [SerializeField] private TextMeshProUGUI playTimeText;
    [SerializeField] private Button restartGameButton;
    [SerializeField] private Button mainMenuButton;

    [Header("Pause UI")]
    [SerializeField] private Button resumeButton;
    [SerializeField] private Button pauseMenuButton;
    [SerializeField] private Button pauseMainMenuButton;

    [Header("Settings UI")]
    [SerializeField] private Slider masterVolumeSlider;
    [SerializeField] private Slider sfxVolumeSlider;
    [SerializeField] private Slider bgmVolumeSlider;
    [SerializeField] private Toggle sfxToggle;
    [SerializeField] private Toggle bgmToggle;
    [SerializeField] private Button settingsCloseButton;

    [Header("Managers")]
    [SerializeField] private GameManager gameManager;
    [SerializeField] private GameSessionManager gameSessionManager;
    [SerializeField] private AudioManager audioManager;

    [Header("Animation")]
    [SerializeField] private float panelTransitionDuration = 0.5f;

    private CanvasGroup gameOverCanvasGroup;
    private CanvasGroup pauseCanvasGroup;
    private bool isGamePaused = false;

    private void Awake()
    {
        // UIManager is scene-specific, not persisted across scenes
        // Each scene (Main Menu, Gameplay) has its own instance
    }

    private void Start()
    {
        InitializeUI();
        SubscribeToEvents();
    }

    /// <summary>
    /// Initializes all UI elements.
    /// </summary>
    private void InitializeUI()
    {
        // Get canvas groups for animations
        if (gameOverPanel != null)
            gameOverCanvasGroup = gameOverPanel.GetComponent<CanvasGroup>();
        if (pausePanel != null)
            pauseCanvasGroup = pausePanel.GetComponent<CanvasGroup>();

        // Setup buttons
        if (restartGameButton != null)
            restartGameButton.onClick.AddListener(OnRestartGameClicked);
        if (mainMenuButton != null)
            mainMenuButton.onClick.AddListener(OnMainMenuClicked);
        if (resumeButton != null)
            resumeButton.onClick.AddListener(OnResumeClicked);
        if (pauseMenuButton != null)
            pauseMenuButton.onClick.AddListener(OnPauseMenuClicked);
        if (pauseMainMenuButton != null)
            pauseMainMenuButton.onClick.AddListener(OnPauseMainMenuClicked);
        if (settingsCloseButton != null)
            settingsCloseButton.onClick.AddListener(OnSettingsCloseClicked);

        // Setup volume sliders
        if (masterVolumeSlider != null)
            masterVolumeSlider.onValueChanged.AddListener(OnMasterVolumeChanged);
        if (sfxVolumeSlider != null)
            sfxVolumeSlider.onValueChanged.AddListener(OnSFXVolumeChanged);
        if (bgmVolumeSlider != null)
            bgmVolumeSlider.onValueChanged.AddListener(OnBGMVolumeChanged);

        // Setup toggles
        if (sfxToggle != null)
            sfxToggle.onValueChanged.AddListener(OnSFXToggleChanged);
        if (bgmToggle != null)
            bgmToggle.onValueChanged.AddListener(OnBGMToggleChanged);
    }

    /// <summary>
    /// Resets all volume sliders to maximum (1.0).
    /// Called when main menu starts to ensure full volume.
    /// </summary>
    public void ResetAllVolumeSlidersToMax()
    {
        Debug.Log("[UIManager.ResetAllVolumeSlidersToMax] Resetting all volume sliders to 1.0");
        
        if (masterVolumeSlider != null)
        {
            masterVolumeSlider.value = 1f;
            Debug.Log("[UIManager.ResetAllVolumeSlidersToMax] Master Volume set to 1.0");
        }
        else
        {
            Debug.LogWarning("[UIManager.ResetAllVolumeSlidersToMax] Master Volume Slider is NULL!");
        }

        if (sfxVolumeSlider != null)
        {
            sfxVolumeSlider.value = 1f;
            Debug.Log("[UIManager.ResetAllVolumeSlidersToMax] SFX Volume set to 1.0");
        }
        else
        {
            Debug.LogWarning("[UIManager.ResetAllVolumeSlidersToMax] SFX Volume Slider is NULL!");
        }

        if (bgmVolumeSlider != null)
        {
            bgmVolumeSlider.value = 1f;
            Debug.Log("[UIManager.ResetAllVolumeSlidersToMax] BGM Volume set to 1.0");
        }
        else
        {
            Debug.LogWarning("[UIManager.ResetAllVolumeSlidersToMax] BGM Volume Slider is NULL!");
        }
    }

    /// <summary>
    /// Subscribes to game events.
    /// </summary>
    private void SubscribeToEvents()
    {
        if (gameSessionManager != null)
        {
            gameSessionManager.OnScoreChanged += UpdateScoreDisplay;
            gameSessionManager.OnComboChanged += UpdateComboDisplay;
            gameSessionManager.OnTimeChanged += UpdateTimerDisplay;
            gameSessionManager.OnMatchOccurred += UpdateMatchesDisplay;
            gameSessionManager.OnGameOver += HandleGameOver;
            
            // Force update UI with current values in case events already fired before subscription
            // This is critical when loading a saved game, as stats are restored before UIManager subscribes
            Debug.Log("[UIManager.SubscribeToEvents] Syncing UI with current game state");
            UpdateScoreDisplay(gameSessionManager.Score);
            UpdateComboDisplay(gameSessionManager.Combo);
            UpdateTimerDisplay(gameSessionManager.TimeRemaining);
            UpdateMatchesDisplay(gameSessionManager.MatchedPairs, gameSessionManager.TotalMatches);
        }

        if (gameManager != null)
        {
            gameManager.OnGameInitialized += OnGameInitialized;
        }
    }

    /// <summary>
    /// Shows the main menu.
    /// </summary>
    public void ShowMainMenu()
    {
        Time.timeScale = 1f;
        SetPanelActive(mainMenuPanel, true);
        SetPanelActive(gameplayHUD, false);
        SetPanelActive(gameOverPanel, false);
        SetPanelActive(pausePanel, false);

        AudioManager audioMgr = AudioManager.Instance;
        if (audioMgr != null)
        {
            audioMgr.PlayMenuBGM();
            Debug.Log("[UIManager.ShowMainMenu] Menu BGM playing via singleton");
        }
        else
        {
            Debug.LogWarning("[UIManager.ShowMainMenu] AudioManager singleton is NULL!");
        }
    }

    /// <summary>
    /// Shows game play HUD.
    /// </summary>
    private void OnGameInitialized()
    {
        SetPanelActive(mainMenuPanel, false);
        SetPanelActive(gameplayHUD, true);
        SetPanelActive(gameOverPanel, false);
        SetPanelActive(pausePanel, false);
        isGamePaused = false;

        // Reset mismatch display
        if (matchesText != null)
        {
            matchesText.text = "Matches: 0/0";
        }

        Debug.Log("Game initialized - all panels reset");
    }

    /// <summary>
    /// Shows the game over panel.
    /// </summary>
    public void ShowGameOverPanel(int score, int matches, int accuracy, float playTime)
    {
        StartCoroutine(ShowGameOverPanelCoroutine(score, matches, accuracy, playTime));
    }

    private IEnumerator ShowGameOverPanelCoroutine(int score, int matches, int accuracy, float playTime)
    {
        yield return new WaitForSeconds(0.5f);

        if (finalScoreText != null)
            finalScoreText.text = score.ToString();
        if (matchesCountText != null)
            matchesCountText.text = matches.ToString();
        if (accuracyText != null)
            accuracyText.text = accuracy + "%";
        if (playTimeText != null)
            playTimeText.text = FormatTime(playTime);

        SetPanelActive(gameplayHUD, false);
        SetPanelActive(gameOverPanel, true);

        if (gameOverCanvasGroup != null)
        {
            gameOverCanvasGroup.alpha = 0f;
            yield return StartCoroutine(FadePanel(gameOverCanvasGroup, 0f, 1f, panelTransitionDuration));
        }
    }

    /// <summary>
    /// Pauses the game.
    /// </summary>
    public void PauseGame()
    {
        if (isGamePaused) return;
        isGamePaused = true;
        gameSessionManager.PauseGame();
        SetPanelActive(pausePanel, true);
    }

    /// <summary>
    /// Resumes the game.
    /// </summary>
    private void OnResumeClicked()
    {
        if (!isGamePaused) return;
        isGamePaused = false;
        gameSessionManager.ResumeGame();
        SetPanelActive(pausePanel, false);
        AudioManager audioMgr = AudioManager.Instance;
        if (audioMgr != null)
            audioMgr.PlayButtonClickSound();
    }

    /// <summary>
    /// Opens settings panel.
    /// </summary>
    public void OpenSettings()
    {
        SetPanelActive(settingsPanel, true);
    }

    private void OnSettingsCloseClicked()
    {
        SetPanelActive(settingsPanel, false);
    }

    /// <summary>
    /// Restarts the game.
    /// </summary>
    private void OnRestartGameClicked()
    {
        AudioManager audioMgr = AudioManager.Instance;
        if (audioMgr != null)
            audioMgr.PlayButtonClickSound();
        gameManager.RestartGame();
    }

    /// <summary>
    /// Returns to main menu.
    /// </summary>
    private void OnMainMenuClicked()
    {
        AudioManager audioMgr = AudioManager.Instance;
        if (audioMgr != null)
            audioMgr.PlayButtonClickSound();

        // Call with false to NOT save (game over should not save)
        ReturnToMainMenu(false);
    }

    /// <summary>
    /// Loads the Main Menu scene.
    /// </summary>
    private void ReturnToMainMenu(bool shouldSaveGame = false)
    {
        Time.timeScale = 1f;

        // Disable game over panel first
        SetPanelActive(gameOverPanel, false);
        SetPanelActive(pausePanel, false);
        
        // ONLY SAVE if called from pause menu, NOT from game over panel
        if (shouldSaveGame && gameManager != null)
        {
            Debug.Log("[UIManager.ReturnToMainMenu] Saving current game state before returning to menu...");
            gameManager.SaveGame(true);
            Debug.Log("[UIManager.ReturnToMainMenu] ✓ Game state saved successfully!");
        }
        else if (!shouldSaveGame)
        {
            Debug.Log("[UIManager.ReturnToMainMenu] Skipping save - called from Game Over panel");
        }

        // Stop gameplay BGM before loading menu scene using singleton
        AudioManager audioMgr = AudioManager.Instance;
        if (audioMgr != null)
        {
            // Aggressively stop all BGM to ensure clean transition
            audioMgr.StopBGM();
            Debug.Log("[UIManager.ReturnToMainMenu] Gameplay BGM stopped before returning to menu");
        }
        else
        {
            Debug.LogWarning("[UIManager.ReturnToMainMenu] AudioManager singleton is NULL!");
        }

        // Load scene immediately - AudioManager.OnSceneLoaded will handle BGM cleanup
        // and MenuManager will handle starting the new BGM
        SceneManager.LoadScene(0); // Load Main Menu scene (index 0)
    }

    /// <summary>
    /// Pause menu button clicked.
    /// </summary>
    private void OnPauseMenuClicked()
    {
        if (audioManager != null)
            audioManager.PlayButtonClickSound();
        PauseGame();
    }

    /// <summary>
    /// Main menu button in pause menu clicked.
    /// Should SAVE the game since this is pause menu.
    /// </summary>
    private void OnPauseMainMenuClicked()
    {
        AudioManager audioMgr = AudioManager.Instance;
        if (audioMgr != null)
            audioMgr.PlayButtonClickSound();

        // Call with true to SAVE (pause menu should save before returning to menu)
        ReturnToMainMenu(true);
    }

    /// <summary>
    /// Updates score display.
    /// </summary>
    private void UpdateScoreDisplay(int score)
    {
        if (scoreText != null)
            scoreText.text = "Score: " + score.ToString();
    }

    /// <summary>
    /// Updates combo display.
    /// </summary>
    private void UpdateComboDisplay(int combo)
    {
        if (comboText != null)
        {
            if (combo > 0)
            {
                comboText.text = "Combo: " + combo.ToString() + "x";
                comboText.gameObject.SetActive(true);
            }
            else
            {
                comboText.gameObject.SetActive(false);
            }
        }
    }

    /// <summary>
    /// Updates timer display.
    /// </summary>
    private void UpdateTimerDisplay(float timeRemaining)
    {
        if (timerText != null)
            timerText.text = "Time: " + FormatTime(timeRemaining);
    }

    /// <summary>
    /// Updates matches display.
    /// </summary>
    private void UpdateMatchesDisplay(int matchedPairs, int totalMatches)
    {
        if (matchesText != null)
            matchesText.text = "Matches: " + matchedPairs + "/" + totalMatches;
    }

    /// <summary>
    /// Handles game over.
    /// </summary>
    private void HandleGameOver(int score, int matches, float playTime)
    {
        // Game over handled by ShowGameOverPanel
    }

    /// <summary>
    /// Volume slider changed.
    /// </summary>
    private void OnMasterVolumeChanged(float value)
    {
        if (audioManager != null)
            audioManager.SetMasterVolume(value);
    }

    private void OnSFXVolumeChanged(float value)
    {
        if (audioManager != null)
            audioManager.SetSFXVolume(value);
    }

    private void OnBGMVolumeChanged(float value)
    {
        if (audioManager != null)
            audioManager.SetBGMVolume(value);
    }

    private void OnSFXToggleChanged(bool isOn)
    {
        if (audioManager != null)
        {
            audioManager.SetSFXVolume(isOn ? 1f : 0f);
        }
    }

    private void OnBGMToggleChanged(bool isOn)
    {
        if (audioManager != null)
        {
            audioManager.SetBGMVolume(isOn ? 0.7f : 0f);
        }
    }

    /// <summary>
    /// Utility functions.
    /// </summary>
    private void SetPanelActive(GameObject panel, bool active)
    {
        if (panel != null)
            panel.SetActive(active);
    }

    private IEnumerator FadePanel(CanvasGroup canvasGroup, float startAlpha, float endAlpha, float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(startAlpha, endAlpha, elapsed / duration);
            yield return null;
        }
        canvasGroup.alpha = endAlpha;
    }

    private string FormatTime(float seconds)
    {
        int mins = Mathf.FloorToInt(seconds / 60f);
        int secs = Mathf.FloorToInt(seconds % 60f);
        return string.Format("{0:00}:{1:00}", mins, secs);
    }
}
