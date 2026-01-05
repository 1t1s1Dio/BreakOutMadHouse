using UnityEngine;
using System;

public class CoinManager : MonoBehaviour
{
    public static CoinManager Instance;

    public int Coins { get; private set; }

    public event Action<int> OnCoinsChanged;

    private const string COINS_KEY = "PLAYER_COINS";

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        LoadCoins();
        OnCoinsChanged?.Invoke(Coins);
    }

    private void LoadCoins()
    {
        Coins = PlayerPrefs.GetInt(COINS_KEY, 0);
    }

    private void SaveCoins()
    {
        PlayerPrefs.SetInt(COINS_KEY, Coins);
    }


    public void AddCoins(int amount)
    {
        Coins += amount;
        SaveCoins();
        OnCoinsChanged?.Invoke(Coins);
    }


    public bool TrySpendCoins(int amount)
    {
        if (Coins < amount)
            return false;

        Coins -= amount;
        SaveCoins();
        OnCoinsChanged?.Invoke(Coins);
        return true;
    }

    public void SetCoins(int amount)
    {
        Coins = Mathf.Max(0, amount);
        SaveCoins();
        OnCoinsChanged?.Invoke(Coins);
    }

    // שימוש רק אם אתה רוצה איפוס מוחלט (New Game)
    public void ResetCoins()
    {
        Coins = 0;
        SaveCoins();
        OnCoinsChanged?.Invoke(Coins);
    }
}
