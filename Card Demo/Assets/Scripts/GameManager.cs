using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.UI;

/// <summary>
/// GameManager orchestrates the entire Memory Card Game.
/// Handles card creation, game flow, matching logic, and state management.
/// Features: Object pooling, event-driven architecture, clean separation of concerns.
/// </summary>
public class GameManager : MonoBehaviour
{
    [Header("Configuration")]
    [SerializeField] private GameConfiguration gameConfig;
    [SerializeField] private GameObject cardPrefab;
    [SerializeField] private Transform cardContainer;

    [Header("Managers")]
    [SerializeField] private GameSessionManager gameSessionManager;
    [SerializeField] private AudioManager audioManager;
    [SerializeField] private UIManager uiManager;

    // Game state
    private List<CardController> allCards = new List<CardController>();
    private List<CardController> flippedCards = new List<CardController>();
    private List<CardController> cardsBeingEvaluated = new List<CardController>();
    private bool isEvaluating = false;

    // Object pooling
    private Queue<CardController> cardPool = new Queue<CardController>();
    private const int INITIAL_POOL_SIZE = 20;

    // Difficulty
    private int currentDifficulty = 0;
    
    // Save/Load data
    private GameState cachedSaveData = null;

    // Static temporary storage for save data across scene transitions
    private static GameState pendingSaveData = null;
    
    // Static temporary storage for difficulty selection across scene transitions
    private static int pendingDifficulty = 0;
    private static bool hasPendingDifficulty = false;

    // Events
    public delegate void GameInitializedHandler();
    public delegate void CardFlippedHandler(int flippedCount);

    public event GameInitializedHandler OnGameInitialized;
    public event CardFlippedHandler OnCardFlipped;

    private static GameManager instance;

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

    private void Start()
    {
        // Find UIManager in current scene (Gameplay scene)
        if (uiManager == null)
        {
            uiManager = FindFirstObjectByType<UIManager>();
            if (uiManager == null)
            {
                Debug.LogError("UIManager not found in Gameplay scene!");
            }
        }

        // Find AudioManager from persisted instance (Main Menu)
        if (audioManager == null)
        {
            audioManager = AudioManager.Instance;
            if (audioManager != null)
            {
                Debug.Log("[GameManager.Start] AudioManager singleton cached");
            }
            else
            {
                Debug.LogError("[GameManager.Start] AudioManager singleton not found! Make sure it exists in Main Menu scene");
            }
        }

        // Check if there's pending save data from MenuManager (continues a game)
        if (pendingSaveData != null)
        {
            cachedSaveData = pendingSaveData;
            currentDifficulty = pendingSaveData.difficulty;
            pendingSaveData = null; // Clear the static reference
        }
        // Check if there's pending difficulty selection from main menu (new game with selected difficulty)
        else if (hasPendingDifficulty)
        {
            currentDifficulty = pendingDifficulty;
            hasPendingDifficulty = false; // Clear the flag
            pendingDifficulty = 0; // Reset to default
        }

        InitializeGame(currentDifficulty);
    }

    /// <summary>
    /// Initializes the entire game.
    /// </summary>
    public void InitializeGame(int difficultyLevel = 0)
    {
        if (gameConfig == null)
        {
            return;
        }

        currentDifficulty = difficultyLevel;

        // Subscribe to game session events and pass cached save data if loading from continue
        if (gameSessionManager != null)
        {
            gameSessionManager.OnGameOver += HandleGameOver;
            
            // If we have cached save data, pass it to GameSessionManager BEFORE initialization
            if (cachedSaveData != null)
            {
                gameSessionManager.SetCachedSaveData(cachedSaveData);
            }
            
            gameSessionManager.InitializeGameSession(difficultyLevel);
        }

        // Clear and reset
        ClearAllCards();
        allCards.Clear();
        flippedCards.Clear();
        cardsBeingEvaluated.Clear();
        isEvaluating = false;

        // Create game board (which handles card pooling internally)
        CreateGameBoard();

        // Restore card states if loading from save
        if (cachedSaveData != null && cachedSaveData.cards != null && cachedSaveData.cards.Count > 0)
        {
            RestoreCardStates();
            
            // Delete the save file after successful restoration (single-use save)
            if (SaveLoadManager.Instance != null)
            {
                SaveLoadManager.Instance.DeleteSavedGame();
            }
        }

        // Play background music - ensure old BGM is stopped first
        // Use singleton directly for reliability
        AudioManager audioMgr = AudioManager.Instance;
        if (audioMgr != null)
        {
            // Stop any previous BGM to ensure clean slate
            audioMgr.StopBGM();
            
            // Use a small delay to ensure BGM transition is clean
            StartCoroutine(PlayGameplayBGMWithDelay());
        }
        else
        {
            Debug.LogError("[GameManager.InitializeGame] AudioManager singleton is NULL! Cannot play gameplay BGM.");
        }

        OnGameInitialized?.Invoke();
    }
    
    private System.Collections.IEnumerator PlayGameplayBGMWithDelay()
    {
        yield return new WaitForEndOfFrame();
        
        AudioManager audioMgr = AudioManager.Instance;
        if (audioMgr != null)
        {
            audioMgr.PlayGameplayBGM();
        }
        else
        {
            Debug.LogError("[GameManager.PlayGameplayBGMWithDelay] AudioManager singleton is NULL!");
        }
    }

    /// <summary>
    /// Pre-creates card objects for efficient object pooling.
    /// </summary>
    private void PrePoolCards(int count)
    {
        for (int i = 0; i < count; i++)
        {
            CreatePooledCard();
        }
    }

    private CardController CreatePooledCard()
    {
        GameObject cardGO = Instantiate(cardPrefab, cardContainer);
        CardController controller = cardGO.GetComponent<CardController>();
        if (controller != null)
        {
            cardGO.SetActive(false);
            cardPool.Enqueue(controller);
        }
        else
        {
            Destroy(cardGO);
        }
        return controller;
    }

    /// <summary>
    /// Gets a card from the pool or creates a new one.
    /// </summary>
    private CardController GetCardFromPool()
    {
        CardController card = null;

        if (cardPool.Count > 0)
        {
            card = cardPool.Dequeue();
        }
        else
        {
            // Create new card if pool is exhausted
            CreatePooledCard();
            if (cardPool.Count > 0)
            {
                card = cardPool.Dequeue();
            }
        }

        if (card != null)
        {
            card.gameObject.SetActive(true);
        }
        else
        {
            Debug.LogError("Failed to get or create card from pool!");
        }

        return card;
    }

    /// <summary>
    /// Returns a card to the pool.
    /// </summary>
    private void ReturnCardToPool(CardController card)
    {
        card.gameObject.SetActive(false);
        card.ResetCard();
        cardPool.Enqueue(card);
    }

    /// <summary>
    /// Creates the game board with properly shuffled cards.
    /// When loading a saved game, uses the exact saved board layout (no shuffle).
    /// </summary>
    private void CreateGameBoard()
    {
        DifficultyLevel diffSettings = gameConfig.GetDifficultyLevel(currentDifficulty);
        int gridRows = diffSettings.gridRows;
        int gridCols = diffSettings.gridColumns;
        int totalCards = gridRows * gridCols;

        if (totalCards % 2 != 0)
        {
            Debug.LogError("Total cards must be even for pairs!");
            return;
        }

        // Pre-pool exactly the cards we need
        PrePoolCards(totalCards);

        // Create card data list - either from saved game or new shuffled
        List<CardDefinition> cardDataList = new List<CardDefinition>();
        int pairCount = totalCards / 2;
        
        bool isLoadingFromSave = cachedSaveData != null && cachedSaveData.cards != null && cachedSaveData.cards.Count == totalCards;
        
        if (isLoadingFromSave)
        {   
            foreach (CardState savedCard in cachedSaveData.cards)
            {
                // Find the CardDefinition with this matchValue
                CardDefinition cardDef = null;
                foreach (CardDefinition def in gameConfig.CardDefinitions)
                {
                    if (def.matchValue == savedCard.matchValue)
                    {
                        cardDef = def;
                        break;
                    }
                }
                
                if (cardDef != null)
                {
                    cardDataList.Add(cardDef);
                }
                else
                {
                    Debug.LogWarning($"[GameManager.CreateGameBoard] Could not find CardDefinition with matchValue {savedCard.matchValue}");
                }
            }
        }
        else
        {
            // New game - create and shuffle card data
            for (int i = 0; i < pairCount; i++)
            {
                CardDefinition cardDef = gameConfig.CardDefinitions[i % gameConfig.CardDefinitions.Length];
                cardDataList.Add(cardDef);
                cardDataList.Add(cardDef);
            }

            ShuffleList(cardDataList);
        }

        // Calculate layout with auto-calculated spacing based on difficulty
        float cardWidth = 100f;
        float cardHeight = 150f;
        float spacingX = gameConfig.GetCardSpacingX(currentDifficulty);  // Auto-calculated based on grid size
        float spacingY = gameConfig.GetCardSpacingY(currentDifficulty);  // Auto-calculated based on grid size

        // Setup card container
        if (cardContainer != null)
        {
            RectTransform containerRect = cardContainer.GetComponent<RectTransform>();
            if (containerRect != null)
            {
                // Calculate container size based on grid
                float totalWidth = gridCols * cardWidth + (gridCols - 1) * spacingX;
                float totalHeight = gridRows * cardHeight + (gridRows - 1) * spacingY;

                // Center the container with proper pivot
                containerRect.anchorMin = new Vector2(0.5f, 0.5f);
                containerRect.anchorMax = new Vector2(0.5f, 0.5f);
                containerRect.pivot = new Vector2(0.5f, 0.5f);
                containerRect.anchoredPosition = Vector2.zero;
                containerRect.sizeDelta = new Vector2(totalWidth, totalHeight);
            }

            // Disable any layout components that might interfere with manual positioning
            GridLayoutGroup gridLayout = cardContainer.GetComponent<GridLayoutGroup>();
            if (gridLayout != null)
            {
                gridLayout.enabled = false;
            }
        }

        // Create cards
        for (int i = 0; i < totalCards; i++)
        {
            int row = i / gridCols;
            int col = i % gridCols;

            // Calculate position using local coordinates (relative to container)
            float posX = col * (cardWidth + spacingX) + cardWidth / 2f;
            float posY = -row * (cardHeight + spacingY) - cardHeight / 2f;

            CardController card = GetCardFromPool();

            if (card == null)
            {
                Debug.LogError($"Failed to get card {i} from pool!");
                continue;
            }

            // Set local position relative to card container
            card.transform.SetParent(cardContainer, false);
            RectTransform cardRect = card.GetComponent<RectTransform>();
            if (cardRect != null)
            {
                cardRect.anchorMin = new Vector2(0.5f, 0.5f);
                cardRect.anchorMax = new Vector2(0.5f, 0.5f);
                cardRect.pivot = new Vector2(0.5f, 0.5f);
                cardRect.anchoredPosition = new Vector2(posX - cardWidth / 2f, posY + cardHeight / 2f);
                cardRect.sizeDelta = new Vector2(cardWidth, cardHeight);
            }
            else
            {
                card.transform.localPosition = new Vector3(posX, posY, 0f);
            }

            card.transform.rotation = Quaternion.identity;
            card.transform.localScale = Vector3.one;

            // Initialize card
            CardDefinition cardDef = cardDataList[i];
            card.Initialize(
                cardDef.cardID,
                cardDef.matchValue,
                cardDef.cardName,
                cardDef.cardFront,
                gameConfig.CardBackSprite,
                this,
                audioManager,
                gameConfig
            );

            allCards.Add(card);
        }
    }

    /// <summary>
    /// Called when a card is clicked.
    /// Senior optimization: Allows flipping additional cards while first two are being evaluated.
    /// This improves UX by not blocking player input during match evaluation.
    /// </summary>
    public void CardClicked(CardController card)
    {
        if (card.IsFaceUp() || card.IsMatched() || flippedCards.Contains(card))
            return;

        // Allow flipping cards even while evaluation is in progress
        card.Flip();
        flippedCards.Add(card);
        OnCardFlipped?.Invoke(flippedCards.Count);

        // Start evaluation when we have the first pair
        if (flippedCards.Count == 2 && !isEvaluating)
        {
            StartCoroutine(CheckForMatch());
        }
        // Additional cards can be flipped while evaluation is happening
    }

    /// <summary>
    /// Checks if the two flipped cards match and handles the result.
    /// Senior optimization: Only blocks interaction briefly for the first pair evaluation.
    /// </summary>
    private IEnumerator CheckForMatch()
    {
        isEvaluating = true;

        // Store the two cards being evaluated
        CardController card1 = flippedCards[0];
        CardController card2 = flippedCards[1];

        // Wait for flip animation to complete
        yield return new WaitForSeconds(gameConfig.CardFlipDuration + 0.2f);

        bool isMatch = card1.GetMatchValue() == card2.GetMatchValue();

        if (isMatch)
        {
            // Match successful
            card1.LockCard();
            card2.LockCard();

            // Play match animations
            StartCoroutine(card1.MatchAnimation());
            StartCoroutine(card2.MatchAnimation());

            // Record in game session
            gameSessionManager.RecordMatch(card1.GetMatchValue());

            // Remove matched cards from flipped list
            flippedCards.Remove(card1);
            flippedCards.Remove(card2);

            isEvaluating = false;

            // If more cards are waiting to be evaluated, check them
            if (flippedCards.Count >= 2)
            {
                StartCoroutine(CheckForMatch());
            }
        }
        else
        {
            // Mismatch
            yield return new WaitForSeconds(gameConfig.MismatchResetDelay);

            // Flip cards back
            card1.FlipBack();
            card2.FlipBack();

            yield return new WaitForSeconds(gameConfig.CardFlipDuration + 0.1f);

            // Record mismatch
            gameSessionManager.RecordMismatch();

            // Remove mismatched cards from flipped list
            flippedCards.Remove(card1);
            flippedCards.Remove(card2);

            isEvaluating = false;

            // If more cards are waiting to be evaluated, check them
            if (flippedCards.Count >= 2)
            {
                StartCoroutine(CheckForMatch());
            }
        }
    }

    /// <summary>
    /// Handles the game over event.
    /// </summary>
    private void HandleGameOver(int finalScore, int matchedPairs, float playTime)
    {
        isEvaluating = true; // Prevent further interaction

        // Show game over UI
        if (uiManager != null)
        {
            int accuracy = Mathf.RoundToInt(gameSessionManager.GetAccuracy());
            uiManager.ShowGameOverPanel(finalScore, matchedPairs, accuracy, playTime);
        }
    }

    /// <summary>
    /// Restarts the game with the same difficulty.
    /// </summary>
    public void RestartGame()
    {   
        // Clear any cached save data to ensure fresh new game (not loading from save)
        cachedSaveData = null;
        pendingSaveData = null;
        
        Time.timeScale = 1f;
        flippedCards.Clear();
        cardsBeingEvaluated.Clear();
        isEvaluating = false;

        // Return all cards to pool
        foreach (CardController card in allCards)
        {
            ReturnCardToPool(card);
        }
        allCards.Clear();

        // Reinitialize with fresh game state (no cached data)
        if (gameSessionManager != null)
        {
            gameSessionManager.InitializeGameSession(currentDifficulty);
        }

        InitializeGame(currentDifficulty);
    }

    /// <summary>
    /// Changes difficulty and restarts.
    /// </summary>
    public void ChangeDifficultyAndRestart(int newDifficulty)
    {
        currentDifficulty = newDifficulty;
        RestartGame();
    }

    /// <summary>
    /// Saves the current game state.
    /// </summary>
    public void SaveGame(bool shouldSave = true)
    {
        // Only save if the flag is true (prevents saving from game over panel)
        if (!shouldSave)
        {
            Debug.Log("[GameManager.SaveGame] Save skipped - not called from pause menu");
            return;
        }

        if (SaveLoadManager.Instance == null)
        {
            Debug.LogError("[GameManager.SaveGame] SaveLoadManager singleton is NULL!");
            return;
        }
        
        if (gameSessionManager == null)
        {
            Debug.LogError("[GameManager.SaveGame] GameSessionManager is NULL!");
            return;
        }

        // Collect all card states from the game board
        List<CardState> cardStatesList = new List<CardState>();
        for (int i = 0; i < allCards.Count; i++)
        {
            CardController card = allCards[i];
            cardStatesList.Add(new CardState
            {
                cardID = card.GetCardID(),
                matchValue = card.GetMatchValue(),
                isMatched = card.IsMatched(),
                isFaceUp = card.IsFaceUp(),
                positionIndex = i
            });
        }

        GameState data = new GameState
        {
            score = gameSessionManager.GetScore(),
            highScore = 0, // Loaded from stats
            matchedPairs = gameSessionManager.GetMatchedPairs(),
            totalAttempts = gameSessionManager.GetTotalAttempts(),
            combo = gameSessionManager.GetCombo(),
            timeRemaining = gameSessionManager.GetTimeRemaining(),
            difficulty = currentDifficulty,
            saveTimestamp = System.DateTime.Now.ToString(),
            cards = cardStatesList
        };
        
        SaveLoadManager.Instance.SaveGame(data);
    }

    /// <summary>
    /// Loads a saved game and restores the game state.
    /// NOTE: For a complete solution, this would need to:
    /// - Recreate the exact board state
    /// - Set which cards are flipped/matched
    /// - Restore player progress bit-by-bit
    /// For now, it loads data and initializes with loaded difficulty.
    /// </summary>
    public void LoadGame()
    {
        if (SaveLoadManager.Instance == null)
        {
            Debug.LogError("[GameManager.LoadGame] SaveLoadManager singleton is NULL!");
            return;
        }

        GameState data = SaveLoadManager.Instance.LoadGame();
        if (data != null)
        {
            // For now, start with the same difficulty
            // TODO: Full implementation would restore all card states and game variables
            ChangeDifficultyAndRestart(data.difficulty);
        }
        else
        {
            Debug.LogWarning("[GameManager.LoadGame] No saved game found to load.");
        }
    }

    /// <summary>
    /// Restores card states from cached save data.
    /// This includes which cards are matched and their pattern values.
    /// Called after the board is created to apply the saved card state.
    /// </summary>
    private void RestoreCardStates()
    {
        if (cachedSaveData == null || cachedSaveData.cards == null || cachedSaveData.cards.Count == 0)
        {
            Debug.LogWarning("[GameManager.RestoreCardStates] No card states to restore!");
            return;
        }

        if (allCards.Count != cachedSaveData.cards.Count)
        {
            Debug.LogWarning($"[GameManager.RestoreCardStates] Card count mismatch! Board has {allCards.Count} cards but save has {cachedSaveData.cards.Count}");
            return;
        }

        // Apply saved card states in order (position by position)
        for (int i = 0; i < allCards.Count; i++)
        {
            CardController card = allCards[i];
            CardState savedState = cachedSaveData.cards[i];

            // First, ensure card is in correct face-up state (before locking)
            bool cardCurrentlyFaceUp = card.IsFaceUp();
            if (savedState.isFaceUp && !cardCurrentlyFaceUp)
            {
                // Card should be face up but isn't - flip it
                card.Flip();
            }
            else if (!savedState.isFaceUp && cardCurrentlyFaceUp)
            {
                // Card should be face down but is up - flip it back
                card.Flip();
            }

            // If card was matched in saved state, lock it
            if (savedState.isMatched)
            {
                card.LockCard();
            }
        }
    }

    /// <summary>
    /// Clears all cards from the scene.
    /// </summary>
    private void ClearAllCards()
    {
        foreach (Transform child in cardContainer)
        {
            Destroy(child.gameObject);
        }
        cardPool.Clear();
    }

    /// <summary>
    /// Fisher-Yates shuffle algorithm.
    /// </summary>
    private void ShuffleList<T>(List<T> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int randomIndex = Random.Range(0, i + 1);
            T temp = list[i];
            list[i] = list[randomIndex];
            list[randomIndex] = temp;
        }
    }

    // Getters
    public bool CanInteract() => !isEvaluating;
    public GameConfiguration GetGameConfig() => gameConfig;
    public int GetCurrentDifficulty() => currentDifficulty;

    /// <summary>
    /// Static method to prepare game data before loading the Gameplay scene.
    /// Call this from MenuManager before loading Scene 1.
    /// </summary>
    public static void PrepareSaveDataForLoading(GameState saveData)
    {
        if (saveData != null)
        {
            pendingSaveData = saveData;
        }
        else
        {
            Debug.LogWarning("[GameManager.PrepareSaveDataForLoading] Attempted to prepare null GameState!");
        }
    }

    /// <summary>
    /// Sets difficulty selection for a new game.
    /// Should be called from MenuManager before loading gameplay scene.
    /// </summary>
    public static void SetDifficultyForNewGame(int difficulty)
    {
        pendingDifficulty = difficulty;
        hasPendingDifficulty = true;
    }

    public static GameManager Instance => instance;
}
