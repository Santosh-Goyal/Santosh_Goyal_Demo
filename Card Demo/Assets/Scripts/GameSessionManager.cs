using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// GameSessionManager manages the current game session state.
/// Tracks score, combo, matches, and game progression.
/// </summary>
public class GameSessionManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameConfiguration gameConfig;
    [SerializeField] private AudioManager audioManager;

    // Game state
    private int score = 0;
    private int matchedPairs = 0;
    private int totalAttempts = 0;
    private int combo = 0;
    private float timeRemaining = 0;
    private float sessionStartTime = 0;
    private bool isGameActive = false;
    private bool isGameOver = false;
    private int difficulty = 0;  // Current difficulty level

    // History
    private List<MatchResult> matchHistory = new List<MatchResult>();
    
    // Cached save data for restoration
    private GameState cachedSaveData = null;

    // Events
    public delegate void ScoreChangedHandler(int newScore);
    public delegate void ComboChangedHandler(int comboCount);
    public delegate void MatchOccurredHandler(int matchedPairsCount, int totalMatches);
    public delegate void TimeChangedHandler(float timeRemaining);
    public delegate void GameStateChangedHandler(bool isActive);
    public delegate void GameOverHandler(int finalScore, int matchedPairs, float playTime);

    public event ScoreChangedHandler OnScoreChanged;
    public event ComboChangedHandler OnComboChanged;
    public event MatchOccurredHandler OnMatchOccurred;
    public event TimeChangedHandler OnTimeChanged;
    public event GameStateChangedHandler OnGameStateChanged;
    public event GameOverHandler OnGameOver;

    private static GameSessionManager instance;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Update()
    {
        if (isGameActive && gameConfig.EnableTimeLimit && timeRemaining > 0)
        {
            timeRemaining -= Time.deltaTime;
            OnTimeChanged?.Invoke(Mathf.Max(0, timeRemaining));

            if (timeRemaining <= 0)
            {
                EndGame();
            }
        }
    }

    /// <summary>
    /// Initializes a new game session.
    /// </summary>
    public void InitializeGameSession(int difficultyLevel = 0)
    {
        if (gameConfig == null)
        {
            Debug.LogError("GameConfiguration not assigned!");
            return;
        }

        difficulty = difficultyLevel;
        DifficultyLevel diffSettings = gameConfig.GetDifficultyLevel(difficultyLevel);

        // Check if we have cached save data to restore
        if (cachedSaveData != null)
        {
            difficulty = cachedSaveData.difficulty;
            int totalPairs = gameConfig.GetTotalCards(difficulty) / 2;
            
            score = cachedSaveData.score;
            matchedPairs = cachedSaveData.matchedPairs;
            totalAttempts = cachedSaveData.totalAttempts;
            combo = cachedSaveData.combo;
            timeRemaining = cachedSaveData.timeRemaining;
            
            sessionStartTime = Time.time;
            isGameActive = true;
            isGameOver = false;
            matchHistory.Clear();

            // Fire all events to update UI with restored values
            OnGameStateChanged?.Invoke(isGameActive);
            OnScoreChanged?.Invoke(score);
            OnComboChanged?.Invoke(combo);
            OnTimeChanged?.Invoke(Mathf.Max(0, timeRemaining));
            OnMatchOccurred?.Invoke(matchedPairs, totalPairs);
            
            // Clear cached data after restoration
            cachedSaveData = null;
        }
        else
        {
            // Normal initialization for new game
            score = 0;
            matchedPairs = 0;
            totalAttempts = 0;
            combo = 0;
            difficulty = difficultyLevel;  // Store current difficulty level
            timeRemaining = diffSettings.timeLimitSeconds;
            sessionStartTime = Time.time;
            isGameActive = true;
            isGameOver = false;
            matchHistory.Clear();
            
            int totalPairs = gameConfig.GetTotalCards(difficulty) / 2;

            OnGameStateChanged?.Invoke(isGameActive);
            OnScoreChanged?.Invoke(score);
            OnComboChanged?.Invoke(combo);
            OnTimeChanged?.Invoke(Mathf.Max(0, timeRemaining));
            OnMatchOccurred?.Invoke(matchedPairs, totalPairs);
        }
    }

    /// <summary>
    /// Sets cached save data for restoration when the game session initializes.
    /// Call this before loading the game scene.
    /// </summary>
    public void SetCachedSaveData(GameState saveData)
    {
        if (saveData != null)
        {
            cachedSaveData = saveData;
        }
        else
        {
            Debug.LogWarning("[GameSessionManager.SetCachedSaveData] Attempted to set null GameState!");
        }
    }

    /// <summary>
    /// Records a successful match.
    /// </summary>
    public void RecordMatch(int matchValue)
    {
        if (!isGameActive) return;

        matchedPairs++;
        combo++;
        totalAttempts++;

        // Calculate points with combo multiplier
        int basePoints = gameConfig.PointsPerMatch;
        int comboBonus = (combo - 1) * gameConfig.ComboMultiplier;
        int totalPoints = basePoints + comboBonus;

        score += totalPoints;

        matchHistory.Add(new MatchResult
        {
            matchValue = matchValue,
            pointsEarned = totalPoints,
            comboAtTime = combo,
            wasSuccess = true
        });

        OnScoreChanged?.Invoke(score);
        OnComboChanged?.Invoke(combo);
        int totalPairs = gameConfig.GetTotalCards(difficulty) / 2;
        OnMatchOccurred?.Invoke(matchedPairs, totalPairs);

        // Play audio feedback using singleton for reliability
        AudioManager audioMgr = AudioManager.Instance;
        if (audioMgr != null)
        {
            audioMgr.PlayMatchSound();
            if (combo > 1 && combo % 3 == 0)
            {
                audioMgr.PlayComboSound();
            }
        }
        else
        {
            Debug.LogError("[GameSessionManager.RecordMatch] AudioManager singleton is NULL!");
        }

        // Check win condition
        if (matchedPairs >= totalPairs)
        {
            EndGame(true);
        }
    }

    /// <summary>
    /// Records a mismatch attempt.
    /// </summary>
    public void RecordMismatch()
    {
        if (!isGameActive) return;

        combo = 0;
        totalAttempts++;

        int penalty = gameConfig.PointsPerMismatch;
        score = Mathf.Max(0, score + penalty);

        matchHistory.Add(new MatchResult
        {
            pointsEarned = penalty,
            comboAtTime = 0,
            wasSuccess = false
        });

        OnScoreChanged?.Invoke(score);
        OnComboChanged?.Invoke(combo);

        // Play audio feedback using singleton for reliability
        AudioManager audioMgr = AudioManager.Instance;
        if (audioMgr != null)
        {
            audioMgr.PlayMismatchSound();
        }
        else
        {
            Debug.LogError("[GameSessionManager.RecordMismatch] AudioManager singleton is NULL!");
        }
    }

    /// <summary>
    /// Ends the game session.
    /// </summary>
    public void EndGame(bool isWin = false)
    {
        isGameActive = false;
        isGameOver = true;
        OnGameStateChanged?.Invoke(isGameActive);

        float playTime = Time.time - sessionStartTime;

        // Play audio feedback using singleton for reliability
        AudioManager audioMgr = AudioManager.Instance;
        if (audioMgr != null)
        {
            if (isWin)
            {
                audioMgr.PlayWinSound();
            }
            else
            {
                audioMgr.PlayGameOverSound();
            }
        }
        else
        {
            Debug.LogError("[GameSessionManager.EndGame] AudioManager singleton is NULL!");
        }

        OnGameOver?.Invoke(score, matchedPairs, playTime);

        // Save statistics
        if (SaveLoadManager.Instance != null)
        {
            var (totalGames, totalTime, bestScore) = SaveLoadManager.Instance.LoadStatistics();
            totalGames++;
            totalTime += playTime;
            bestScore = Mathf.Max(bestScore, score);
            SaveLoadManager.Instance.SaveStatistics(totalGames, totalTime, bestScore);
        }
    }

    /// <summary>
    /// Pauses the game session.
    /// </summary>
    public void PauseGame()
    {
        isGameActive = false;
        Time.timeScale = 0f;
        if (audioManager != null)
        {
            audioManager.PauseBGM();
        }
    }

    /// <summary>
    /// Resumes the game session.
    /// </summary>
    public void ResumeGame()
    {
        if (!isGameOver)
        {
            isGameActive = true;
            Time.timeScale = 1f;
            if (audioManager != null)
            {
                audioManager.ResumeBGM();
            }
        }
    }

    // Getters
    public int GetScore() => score;
    public int GetMatchedPairs() => matchedPairs;
    public int GetCombo() => combo;
    public float GetTimeRemaining() => Mathf.Max(0, timeRemaining);
    public bool IsGameActive() => isGameActive;
    public bool IsGameOver() => isGameOver;
    public int GetTotalAttempts() => totalAttempts;
    public float GetAccuracy() => totalAttempts > 0 ? (matchedPairs / (float)totalAttempts) * 100f : 0f;
    public int GetDifficulty() => difficulty;
    
    // Public getters for current game state (used for UI sync after load)
    public int Score => score;
    public int Combo => combo;
    public float TimeRemaining => timeRemaining;
    public int MatchedPairs => matchedPairs;
    public int TotalMatches => gameConfig != null ? gameConfig.GetTotalCards(difficulty) / 2 : 0;

    public static GameSessionManager Instance => instance;

    /// <summary>
    /// Records match history for analytics.
    /// </summary>
    [System.Serializable]
    public class MatchResult
    {
        public int matchValue;
        public int pointsEarned;
        public int comboAtTime;
        public bool wasSuccess;
    }
}
