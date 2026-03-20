using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// MenuManager handles main menu interactions and scene transitions.
/// </summary>
public class MenuManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Button newGameButton;
    [SerializeField] private Button continueGameButton;
    [SerializeField] private Button settingsButton;
    [SerializeField] private Button quitButton;
    [SerializeField] private TextMeshProUGUI bestScoreText;
    [SerializeField] private TextMeshProUGUI gamesPlayedText;
    
    [Header("Difficulty Selection")]
    [SerializeField] private Button[] difficultyButtons = new Button[4];  // Easy, Medium, Hard, Expert
    private int selectedDifficulty = 0;  // 0 = Easy (default)
    private Color selectedButtonColor = new Color(0.2f, 0.8f, 0.2f, 1f);  // Green for selected
    private Color deselectedButtonColor = new Color(0.8f, 0.8f, 0.8f, 1f);  // Gray for deselected

    [Header("Managers")]
    [SerializeField] private AudioManager audioManager;
    [SerializeField] private SaveLoadManager saveLoadManager;
    [SerializeField] private UIManager uiManager;

    private int bestScore = 0;
    private int gamesPlayed = 0;

    private void Awake()
    {
        // Find AudioManager if not assigned (for inspector reference)
        if (audioManager == null)
        {
            audioManager = AudioManager.Instance;
            if (audioManager != null)
            {
                Debug.Log("[MenuManager.Awake] AudioManager singleton found and cached");
            }
            else
            {
                Debug.LogWarning("[MenuManager.Awake] AudioManager singleton not found! Make sure it exists in Main Menu scene");
            }
        }
    }

    private void Start()
    {
        // Find UIManager in current scene (Main Menu scene)
        if (uiManager == null)
        {
            uiManager = FindFirstObjectByType<UIManager>();
            if (uiManager == null)
            {
                Debug.LogWarning("UIManager not found in Main Menu scene!");
            }
        }

        // Get SaveLoadManager singleton if not assigned
        if (saveLoadManager == null)
        {
            saveLoadManager = SaveLoadManager.Instance;
            if (saveLoadManager != null)
            {
                Debug.Log("[MenuManager.Start] SaveLoadManager singleton found and cached");
            }
            else
            {
                Debug.LogWarning("[MenuManager.Start] SaveLoadManager singleton not found!");
            }
        }

        InitializeMenu();
        SubscribeToButtons();
        LoadAndDisplayStats();
    }

    /// <summary>
    /// Initializes the main menu.
    /// </summary>
    private void InitializeMenu()
    {
        Time.timeScale = 1f;

        // Reset volume sliders to maximum (1.0) when menu starts
        if (uiManager != null)
        {
            uiManager.ResetAllVolumeSlidersToMax();
            Debug.Log("[InitializeMenu] Volume sliders reset to maximum");
        }

        // Use AudioManager singleton directly for reliability
        AudioManager audioMgr = AudioManager.Instance;
        
        if (audioMgr != null)
        {
            // Ensure any previous BGM is completely stopped before playing menu BGM
            // This prevents gameplay BGM from continuing to play
            audioMgr.StopBGM();
            Debug.Log("[InitializeMenu] AudioManager singleton found, stopped previous BGM. Starting delayed menu BGM...");
            
            // Brief delay to ensure complete stop before starting menu BGM
            StartCoroutine(StartMenuBGMWithDelay());
        }
        else
        {
            Debug.LogError("[InitializeMenu] AudioManager singleton is NULL! Cannot initialize menu BGM.");
        }
    }
    
    private System.Collections.IEnumerator StartMenuBGMWithDelay()
    {
        yield return new WaitForEndOfFrame();
        
        Debug.Log("[StartMenuBGMWithDelay] Coroutine executing...");
        
        // Get AudioManager singleton directly
        AudioManager audioMgr = AudioManager.Instance;
        
        if (audioMgr != null)
        {
            Debug.Log("[StartMenuBGMWithDelay] AudioManager singleton found, calling PlayMenuBGM");
            audioMgr.PlayMenuBGM();
            Debug.Log("[StartMenuBGMWithDelay] Menu BGM call completed");
        }
        else
        {
            Debug.LogError("[StartMenuBGMWithDelay] AudioManager singleton is NULL! Cannot play menu BGM.");
        }
    }

    /// <summary>
    /// Subscribes to button click events.
    /// </summary>
    private void SubscribeToButtons()
    {
        if (newGameButton != null)
            newGameButton.onClick.AddListener(OnNewGameClicked);

        if (continueGameButton != null)
        {
            continueGameButton.onClick.AddListener(OnContinueGameClicked);
            
            // Check if a save exists and set continue button interactability accordingly
            SaveLoadManager saveMgr = saveLoadManager != null ? saveLoadManager : SaveLoadManager.Instance;
            
            if (saveMgr != null)
            {
                bool hasSave = saveMgr.HasSavedGame();
                continueGameButton.interactable = hasSave;
                
                if (hasSave)
                {
                    Debug.Log("[MenuManager.SubscribeToButtons] ✓ Continue button is ENABLED - save file exists");
                }
                else
                {
                    Debug.Log("[MenuManager.SubscribeToButtons] Continue button is DISABLED - no save file found");
                }
            }
            else
            {
                Debug.LogWarning("[MenuManager.SubscribeToButtons] SaveLoadManager not found! Cannot check for save file.");
                continueGameButton.interactable = false;
            }
        }

        if (settingsButton != null)
            settingsButton.onClick.AddListener(OnSettingsClicked);

        if (quitButton != null)
            quitButton.onClick.AddListener(OnQuitClicked);
        
        // Subscribe to difficulty buttons (MAIN MENU ONLY - not gameplay scene)
        Debug.Log("[MenuManager.SubscribeToButtons] Setting up main menu difficulty buttons...");
        
        int connectedButtons = 0;
        for (int i = 0; i < difficultyButtons.Length; i++)
        {
            int difficultyIndex = i;  // Capture for closure
            if (difficultyButtons[i] != null)
            {
                difficultyButtons[i].onClick.AddListener(() => OnDifficultySelected(difficultyIndex));
                connectedButtons++;
                Debug.Log($"[MenuManager.SubscribeToButtons] Connected difficulty button {i}");
            }
            else
            {
                Debug.LogWarning($"[MenuManager.SubscribeToButtons] ⚠️ Difficulty button {i} is NOT assigned in inspector! Please assign it in the MenuManager component.");
            }
        }
        
        if (connectedButtons == 0)
        {
            Debug.LogError("[MenuManager.SubscribeToButtons] ❌ NO difficulty buttons assigned! Main menu difficulty selection won't work. Please assign buttons in MenuManager inspector.");
        }
        else
        {
            Debug.Log($"[MenuManager.SubscribeToButtons] ✓ Successfully connected {connectedButtons}/4 difficulty buttons");
        }
        
        // Set initial difficulty button visual state (Easy selected by default)
        UpdateDifficultyButtonVisuals();
        
        Debug.Log("[MenuManager.SubscribeToButtons] All buttons subscribed, difficulty set to Easy (0)");
    }
    
    /// <summary>
    /// Updates the visual state of difficulty buttons to show which is selected.
    /// </summary>
    private void UpdateDifficultyButtonVisuals()
    {
        for (int i = 0; i < difficultyButtons.Length; i++)
        {
            if (difficultyButtons[i] != null)
            {
                ColorBlock colors = difficultyButtons[i].colors;
                colors.normalColor = (i == selectedDifficulty) ? selectedButtonColor : deselectedButtonColor;
                difficultyButtons[i].colors = colors;
                
                // Also update text to show selection (make it bold if selected)
                TextMeshProUGUI buttonText = difficultyButtons[i].GetComponentInChildren<TextMeshProUGUI>();
                if (buttonText != null)
                {
                    buttonText.fontStyle = (i == selectedDifficulty) ? FontStyles.Bold : FontStyles.Normal;
                }
            }
        }
    }

    /// <summary>
    /// Loads and displays game statistics.
    /// </summary>
    private void LoadAndDisplayStats()
    {
        if (saveLoadManager != null)
        {
            var (gamesCount, totalTime, bestScoreValue) = saveLoadManager.LoadStatistics();
            bestScore = bestScoreValue;
            gamesPlayed = gamesCount;

            if (bestScoreText != null)
                bestScoreText.text = "Best Score: " + bestScore.ToString();

            if (gamesPlayedText != null)
                gamesPlayedText.text = "Games Played: " + gamesPlayed.ToString();
        }
    }

    /// <summary>
    /// New game button clicked.
    /// </summary>
    private void OnNewGameClicked()
    {
        AudioManager audioMgr = AudioManager.Instance;
        if (audioMgr != null)
            audioMgr.PlayButtonClickSound();

        // Start new game with selected difficulty
        Debug.Log($"[MenuManager.OnNewGameClicked] Starting new game with selected difficulty: {selectedDifficulty}");
        
        GameManager.SetDifficultyForNewGame(selectedDifficulty);
        SceneManager.LoadScene(1);
    }

    /// <summary>
    /// Difficulty button clicked.
    /// </summary>
    private void OnDifficultySelected(int difficulty)
    {
        AudioManager audioMgr = AudioManager.Instance;
        if (audioMgr != null)
            audioMgr.PlayButtonClickSound();
        
        selectedDifficulty = difficulty;
        UpdateDifficultyButtonVisuals();
        
        string[] difficultyNames = { "Easy", "Medium", "Hard", "Expert" };
        string diffName = (difficulty >= 0 && difficulty < difficultyNames.Length) ? difficultyNames[difficulty] : "Unknown";
        Debug.Log($"[MenuManager.OnDifficultySelected] Difficulty selected: {diffName} (Index: {difficulty})");
    }

    /// <summary>
    /// Continue game button clicked.
    /// </summary>
    private void OnContinueGameClicked()
    {
        AudioManager audioMgr = AudioManager.Instance;
        if (audioMgr != null)
            audioMgr.PlayButtonClickSound();

        if (SaveLoadManager.Instance != null)
        {
            GameState data = SaveLoadManager.Instance.LoadGame();
            if (data != null)
            {
                Debug.Log("[MenuManager.OnContinueGameClicked] Continuing previous game...");
                Debug.Log($"[MenuManager.OnContinueGameClicked] Loaded Score: {data.score}, Pairs: {data.matchedPairs}, Difficulty: {data.difficulty}");
                Debug.Log($"[MenuManager.OnContinueGameClicked] Card States: {(data.cards != null ? data.cards.Count : 0)} cards");
                
                // Cache the save data BEFORE loading the scene
                GameManager.PrepareSaveDataForLoading(data);
                Debug.Log("[MenuManager.OnContinueGameClicked] Save data prepared, loading gameplay scene...");
                
                // Load the gameplay scene - GameManager will detect cached data in Start()
                // and will restore all game state (score, pairs, card states, etc.)
                SceneManager.LoadScene(1);
            }
            else
            {
                Debug.LogWarning("[MenuManager.OnContinueGameClicked] No saved game data found!");
            }
        }
    }

    /// <summary>
    /// Settings button clicked.
    /// </summary>
    private void OnSettingsClicked()
    {
        AudioManager audioMgr = AudioManager.Instance;
        if (audioMgr != null)
            audioMgr.PlayButtonClickSound();

        if (uiManager != null)
        {
            uiManager.OpenSettings();
        }
    }

    /// <summary>
    /// Quit button clicked.
    /// </summary>
    private void OnQuitClicked()
    {
        AudioManager audioMgr = AudioManager.Instance;
        if (audioMgr != null)
            audioMgr.PlayButtonClickSound();

        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }
}
