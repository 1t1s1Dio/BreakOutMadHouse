using UnityEngine;
using System.IO;
using UnityEngine.SceneManagement;

public static class SaveSystem
{
    private static string savePath => Application.persistentDataPath + "/save.json";

    public static void SaveGame()
    {
        SaveData data = new SaveData();

        // שומרים את השלב הפעיל
        data.currentLevelIndex = SceneManager.GetActiveScene().buildIndex;

        // Coins
        if (CoinManager.Instance != null)
            data.coins = CoinManager.Instance.Coins;

        // GameManager
        if (GameManager.Instance != null)
        {
            data.lives = GameManager.Instance.lives;
            data.score = GameManager.Instance.score;
            data.currentLevelIndex = GameManager.Instance.currentLevelIndex; // ✅ עדיף מקור אחד
        }

        // Upgrades
        if (UpgradeManager.Instance != null)
        {
            data.paddleSizeLevel = UpgradeManager.Instance.paddleSizeLevel;
            data.paddleSpeedLevel = UpgradeManager.Instance.paddleSpeedLevel;
            data.extraLifeLevel = UpgradeManager.Instance.extraLifeLevel;
            data.extraBallLevel = UpgradeManager.Instance.extraBallLevel;
        }

        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(savePath, json);

        Debug.Log("Game Saved: " + json);
    }

    public static bool LoadGame()
    {
        if (!File.Exists(savePath))
        {
            Debug.Log("No Save Found");
            return false;
        }

        string json = File.ReadAllText(savePath);
        SaveData data = JsonUtility.FromJson<SaveData>(json);

        // Coins
        if (CoinManager.Instance != null)
            CoinManager.Instance.SetCoins(data.coins);

        // GameManager
        // GameManager
        if (GameManager.Instance != null)
        {
            GameManager.Instance.ApplyLoadedData(data.lives, data.score, data.currentLevelIndex);
            GameManager.Instance.RefreshUI(); // ✅ חוקי, כי זה מתוך GameManager
        }



        // Upgrades
        if (UpgradeManager.Instance != null)
        {
            UpgradeManager.Instance.paddleSizeLevel = data.paddleSizeLevel;
            UpgradeManager.Instance.paddleSpeedLevel = data.paddleSpeedLevel;
            UpgradeManager.Instance.extraLifeLevel = data.extraLifeLevel;
            UpgradeManager.Instance.extraBallLevel = data.extraBallLevel;
        }

        Debug.Log("Game Loaded: " + json);
        return true;
    }

    public static void DeleteSave()
    {
        if (File.Exists(savePath))
            File.Delete(savePath);
    }
}
