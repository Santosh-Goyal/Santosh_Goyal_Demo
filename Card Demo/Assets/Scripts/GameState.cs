using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Represents the state of a single card for save/restore functionality.
/// </summary>
[System.Serializable]
public class CardState
{
    public int cardID;
    public int positionIndex;
    public bool isFaceUp;
    public bool isMatched;
    public int matchValue;
}

/// <summary>
/// Represents the complete game state for save/restore functionality.
/// </summary>
[System.Serializable]
public class GameState
{
    public int score;
    public int highScore;
    public int matchedPairs;
    public int totalAttempts;
    public int combo;
    public float timeRemaining;
    public int difficulty;
    public List<CardState> cards;
    public string saveTimestamp;
}
