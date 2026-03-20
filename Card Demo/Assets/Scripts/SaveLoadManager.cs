using UnityEngine;
using System;
using System.IO;
using System.Text;

/// <summary>
/// SaveLoadManager handles game data persistence with encryption.
/// Senior optimization: Uses Base64 encoding to prevent casual cheating via file editing.
/// Also uses const strings instead of magic strings to prevent bugs.
/// </summary>
public class SaveLoadManager : MonoBehaviour
{
    // File name constants (prevents magic strings which lead to bugs)
    private const string SAVE_FILE = "gamesave.dat";
    private const string STATS_FILE = "gamestats.dat";
    private const string SAVE_DIRECTORY = "Saves";

    // Encryption constants
    private const string ENCRYPTION_PREFIX = "MEM_GAME_"; // Simple anti-tampering marker

    [System.Serializable]
    private class GameStats
    {
        public int totalGamesPlayed;
        public float totalPlayTime;
        public int bestScore;
    }

    private static SaveLoadManager instance;
    private string savePath;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeSavePath();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Initializes the save directory path.
    /// </summary>
    private void InitializeSavePath()
    {
        savePath = Path.Combine(Application.persistentDataPath, SAVE_DIRECTORY);
        if (!Directory.Exists(savePath))
        {
            Directory.CreateDirectory(savePath);
            Debug.Log($"Created save directory: {savePath}");
        }
    }

    /// <summary>
    /// Encrypts JSON string using Base64 encoding with anti-tampering marker.
    /// Senior optimization: Prevents casual cheating via file editing.
    /// </summary>
    private string EncryptSaveData(string jsonData)
    {
        try
        {
            // Convert JSON to bytes
            byte[] jsonBytes = Encoding.UTF8.GetBytes(jsonData);

            // Base64 encode
            string encodedData = Convert.ToBase64String(jsonBytes);

            // Add anti-tampering prefix
            return ENCRYPTION_PREFIX + encodedData;
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to encrypt save data: {e.Message}");
            return null;
        }
    }

    /// <summary>
    /// Decrypts Base64 encoded save data.
    /// Verifies anti-tampering marker.
    /// </summary>
    private string DecryptSaveData(string encryptedData)
    {
        try
        {
            // Verify anti-tampering prefix
            if (!encryptedData.StartsWith(ENCRYPTION_PREFIX))
            {
                Debug.LogError("Save file appears to be tampered with or corrupted!");
                return null;
            }

            // Remove prefix (IDE0057 fix: use Range operator)
            string encodedData = encryptedData[ENCRYPTION_PREFIX.Length..];

            // Base64 decode
            byte[] jsonBytes = Convert.FromBase64String(encodedData);

            // Convert bytes to string
            return Encoding.UTF8.GetString(jsonBytes);
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to decrypt save data: {e.Message}");
            return null;
        }
    }

    /// <summary>
    /// Saves the current game state with encryption.
    /// </summary>
    public void SaveGame(GameState data)
    {
        try
        {
            if (string.IsNullOrEmpty(savePath))
            {
                InitializeSavePath();
            }

            if (string.IsNullOrEmpty(savePath))
            {
                Debug.LogError("[SaveLoadManager.SaveGame] Save path is not initialized!");
                return;
            }

            string filePath = Path.Combine(savePath, SAVE_FILE);

            // Serialize to JSON
            string json = JsonUtility.ToJson(data, true);

            // Encrypt before saving
            string encryptedData = EncryptSaveData(json);

            if (encryptedData != null)
            {
                File.WriteAllText(filePath, encryptedData);
                Debug.Log($"[SaveLoadManager.SaveGame] ✓ Game saved securely to {filePath}");
                Debug.Log($"[SaveLoadManager.SaveGame] Save timestamp: {data.saveTimestamp}");
            }
            else
            {
                Debug.LogError("[SaveLoadManager.SaveGame] Failed to encrypt save data");
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"[SaveLoadManager.SaveGame] Failed to save game: {e.Message}");
        }
    }

    /// <summary>
    /// Loads the saved game state with decryption.
    /// </summary>
    public GameState LoadGame()
    {
        try
        {
            if (string.IsNullOrEmpty(savePath))
            {
                InitializeSavePath();
            }

            if (string.IsNullOrEmpty(savePath))
            {
                Debug.LogError("[SaveLoadManager.LoadGame] Save path is not initialized!");
                return null;
            }

            string filePath = Path.Combine(savePath, SAVE_FILE);
            if (File.Exists(filePath))
            {
                // Read encrypted data
                string encryptedData = File.ReadAllText(filePath);

                // Decrypt
                string json = DecryptSaveData(encryptedData);

                if (json != null)
                {
                    GameState data = JsonUtility.FromJson<GameState>(json);
                    Debug.Log("[SaveLoadManager.LoadGame] ✓ Game loaded successfully!");
                    Debug.Log($"[SaveLoadManager.LoadGame] Loaded timestamp: {data.saveTimestamp}");
                    return data;
                }
                else
                {
                    Debug.LogError("[SaveLoadManager.LoadGame] Failed to decrypt save file");
                    return null;
                }
            }
            else
            {
                Debug.Log("[SaveLoadManager.LoadGame] No saved game found at: " + filePath);
                return null;
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"[SaveLoadManager.LoadGame] Failed to load game: {e.Message}");
            return null;
        }
    }

    /// <summary>
    /// Checks if a saved game exists.
    /// </summary>
    public bool HasSavedGame()
    {
        if (string.IsNullOrEmpty(savePath))
        {
            InitializeSavePath();
        }

        if (string.IsNullOrEmpty(savePath))
            return false;

        string filePath = Path.Combine(savePath, SAVE_FILE);
        return File.Exists(filePath);
    }

    /// <summary>
    /// Deletes the saved game.
    /// </summary>
    public void DeleteSavedGame()
    {
        try
        {
            if (string.IsNullOrEmpty(savePath))
            {
                InitializeSavePath();
            }

            if (string.IsNullOrEmpty(savePath))
            {
                Debug.LogError("Save path is not initialized!");
                return;
            }

            string filePath = Path.Combine(savePath, SAVE_FILE);
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
                Debug.Log("Saved game deleted");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to delete save: {e.Message}");
        }
    }

    /// <summary>
    /// Saves game statistics with encryption.
    /// </summary>
    public void SaveStatistics(int totalGames, float totalTime, int bestScore)
    {
        try
        {
            if (string.IsNullOrEmpty(savePath))
            {
                InitializeSavePath();
            }

            if (string.IsNullOrEmpty(savePath))
            {
                Debug.LogError("Save path is not initialized!");
                return;
            }

            string filePath = Path.Combine(savePath, STATS_FILE);

            GameStats stats = new()
            {
                totalGamesPlayed = totalGames,
                totalPlayTime = totalTime,
                bestScore = bestScore
            };

            // Serialize to JSON
            string json = JsonUtility.ToJson(stats, true);

            // Encrypt before saving
            string encryptedData = EncryptSaveData(json);

            if (encryptedData != null)
            {
                File.WriteAllText(filePath, encryptedData);
                Debug.Log("Statistics saved securely");
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to save statistics: {e.Message}");
        }
    }

    /// <summary>
    /// Loads game statistics with decryption.
    /// </summary>
    public (int, float, int) LoadStatistics()
    {
        try
        {
            if (string.IsNullOrEmpty(savePath))
            {
                InitializeSavePath();
            }

            if (string.IsNullOrEmpty(savePath))
            {
                Debug.LogError("Save path is not initialized!");
                return (0, 0, 0);
            }

            string filePath = Path.Combine(savePath, STATS_FILE);
            if (File.Exists(filePath))
            {
                // Read encrypted data
                string encryptedData = File.ReadAllText(filePath);

                // Decrypt
                string json = DecryptSaveData(encryptedData);

                if (json != null)
                {
                    GameStats stats = JsonUtility.FromJson<GameStats>(json);
                    return (stats.totalGamesPlayed, stats.totalPlayTime, stats.bestScore);
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to load statistics: {e.Message}");
        }
        return (0, 0, 0);
    }

    public static SaveLoadManager Instance => instance;
}
