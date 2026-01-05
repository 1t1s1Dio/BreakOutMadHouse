using UnityEngine;

public class UpgradeManager : MonoBehaviour
{
    public static UpgradeManager Instance;

    [Header("Upgrade Costs")]
    public int paddleSizeCost = 50;
    public int paddleSpeedCost = 40;
    public int extraLifeCost = 60;
    public int extraBallCost = 100;

    [Header("Upgrade Levels")]
    public int paddleSizeLevel;
    public int paddleSpeedLevel;
    public int extraLifeLevel;
    public int extraBallLevel;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    // =======================
    // הרחבת הפדל
    // =======================
    public void UpgradePaddleSize()
    {
        if (!EnsurePaddle()) return;

        if (!CoinManager.Instance.TrySpendCoins(paddleSizeCost))
            return;

        paddleSizeLevel++;
        PaddleController.Instance.IncreaseSize(0.3f);

        SaveSystem.SaveGame();
    }

    // =======================
    // מהירות הפדל
    // =======================
    public void UpgradePaddleSpeed()
    {
        if (!EnsurePaddle()) return;

        if (!CoinManager.Instance.TrySpendCoins(paddleSpeedCost))
            return;

        paddleSpeedLevel++;
        PaddleController.Instance.IncreaseSpeed(1f);

        SaveSystem.SaveGame();
    }

    // =======================
    // חיים נוספים
    // =======================
    public void UpgradeExtraLife()
    {
        if (!CoinManager.Instance.TrySpendCoins(extraLifeCost))
            return;

        extraLifeLevel++;

        if (GameManager.Instance != null)
            GameManager.Instance.AddLife(1);

        SaveSystem.SaveGame();
    }

    // =======================
    // כדור נוסף
    // =======================
    public void UpgradeExtraBall()
    {
        if (CoinManager.Instance == null) return;
        if (!CoinManager.Instance.TrySpendCoins(extraBallCost)) return;

        extraBallLevel++;

        if (BallSpawner.Instance != null)
            BallSpawner.Instance.SetExtraBallLevel(extraBallLevel);

        SaveSystem.SaveGame();
    }


    public void ResetUpgrades()
    {
        paddleSizeLevel = 0;
        paddleSpeedLevel = 0;
        extraLifeLevel = 0;
        extraBallLevel = 0;
    }


    // =======================
    // בדיקות הגנה
    // =======================
    private bool EnsurePaddle()
    {
        return PaddleController.Instance != null;
    }
}
