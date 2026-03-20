using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// GameConfiguration holds all configurable game settings.
/// This ScriptableObject allows designers to tune game balance without code changes.
/// Grid settings are now ONLY in DifficultyLevel - no global grid settings.
/// Card spacing is auto-calculated based on grid size.
/// </summary>
[CreateAssetMenu(fileName = "GameConfiguration", menuName = "Memory Game/Game Configuration")]
public class GameConfiguration : ScriptableObject
{
    [Header("Animation Settings")]
    [SerializeField] private float cardFlipDuration = 0.25f;
    [SerializeField] private float mismatchResetDelay = 1.5f;
    [SerializeField] private AnimationCurve flipEase = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("Game Rules")]
    [SerializeField] private bool enableTimeLimit = true;

    [Header("Scoring System")]
    [SerializeField] private int pointsPerMatch = 100;
    [SerializeField] private int pointsPerMismatch = -10;
    [SerializeField] private int comboMultiplier = 1;
    [SerializeField] private int maxCombo = 10;

    [Header("Card Definitions")]
    [SerializeField] private CardDefinition[] cardDefinitions;
    [SerializeField] private Sprite cardBackSprite;

    [Header("Difficulty Settings")]
    [SerializeField] private DifficultyLevel[] difficultyLevels;

    // Card positioning constants for auto-spacing calculation
    private const float CARD_WIDTH = 100f;
    private const float CARD_HEIGHT = 150f;
    private const float BASE_SPACING = 15f;  // Base spacing for smallest grid

    // Properties
    public float CardFlipDuration => cardFlipDuration;
    public float MismatchResetDelay => mismatchResetDelay;
    public AnimationCurve FlipEase => flipEase;
    public bool EnableTimeLimit => enableTimeLimit;
    public CardDefinition[] CardDefinitions => cardDefinitions;
    public Sprite CardBackSprite => cardBackSprite;
    public int PointsPerMatch => pointsPerMatch;
    public int PointsPerMismatch => pointsPerMismatch;
    public int ComboMultiplier => comboMultiplier;
    public int MaxCombo => maxCombo;
    public DifficultyLevel[] DifficultyLevels => difficultyLevels;

    /// <summary>
    /// Gets difficulty settings by level.
    /// </summary>
    public DifficultyLevel GetDifficultyLevel(int level)
    {
        if (level < 0 || level >= difficultyLevels.Length)
            return difficultyLevels[0];
        return difficultyLevels[level];
    }

    /// <summary>
    /// Gets total cards for a specific difficulty level.
    /// </summary>
    public int GetTotalCards(int difficultyLevel)
    {
        DifficultyLevel diff = GetDifficultyLevel(difficultyLevel);
        return diff.gridRows * diff.gridColumns;
    }

    /// <summary>
    /// Gets total cards (uses level 0 by default - for backward compatibility).
    /// </summary>
    public int TotalCards => GetTotalCards(0);

    /// <summary>
    /// Auto-calculates card spacing based on grid size.
    /// Larger grids get smaller spacing to fit on canvas.
    /// </summary>
    public float GetCardSpacingX(int difficultyLevel)
    {
        DifficultyLevel diff = GetDifficultyLevel(difficultyLevel);
        float totalWidth = (diff.gridColumns * CARD_WIDTH) + ((diff.gridColumns - 1) * BASE_SPACING);
        
        // Reduce spacing if total width exceeds reasonable limits (~ 1000 pixels)
        if (totalWidth > 1000f)
        {
            return BASE_SPACING * 0.5f;  // Halve spacing for large grids
        }
        return BASE_SPACING;
    }

    /// <summary>
    /// Auto-calculates card spacing Y based on grid size.
    /// </summary>
    public float GetCardSpacingY(int difficultyLevel)
    {
        DifficultyLevel diff = GetDifficultyLevel(difficultyLevel);
        float totalHeight = (diff.gridRows * CARD_HEIGHT) + ((diff.gridRows - 1) * BASE_SPACING);
        
        // Reduce spacing if total height exceeds reasonable limits (~ 800 pixels)
        if (totalHeight > 800f)
        {
            return BASE_SPACING * 0.5f;  // Halve spacing for large grids
        }
        return BASE_SPACING;
    }
}

/// <summary>
/// Card definition with visual and gameplay properties.
/// </summary>
[System.Serializable]
public class CardDefinition
{
    public int cardID;
    public string cardName;
    public Sprite cardFront;
    public int matchValue;
    [TextArea(2, 4)]
    public string description;

    public CardDefinition(int id, string name, Sprite front, int value)
    {
        cardID = id;
        cardName = name;
        cardFront = front;
        matchValue = value;
    }
}

/// <summary>
/// Difficulty level configuration.
/// </summary>
[System.Serializable]
public class DifficultyLevel
{
    public string levelName;
    public int gridRows;
    public int gridColumns;
    public int timeLimitSeconds;
    public float cardFlipDurationModifier = 1f;
}
