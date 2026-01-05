using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Run")]
    [SerializeField] private int startingLives = 3;

    [Header("Combo")]
    [SerializeField] private float comboDuration = 4f;

    public int lives { get; private set; }
    public int score { get; private set; }
    public int currentLevelIndex = 1;

    private int comboHits = 0;
    public int comboMultiplier { get; private set; } = 1;
    private float comboTimer;

    private bool isGameOver = false;
    private bool levelCompleted = false;
    public bool IsGameOver => isGameOver;

    public event System.Action<int> OnLivesChanged;
    public event System.Action<int> OnScoreChanged;
    public event System.Action<int> OnComboChanged;
    public event System.Action<float> OnComboTimerChanged;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Update()
    {
        if (comboHits < 4) return;

        comboTimer -= Time.deltaTime;

        if (comboTimer <= 0f)
        {
            ResetCombo();
            return;
        }

        OnComboTimerChanged?.Invoke(Mathf.Clamp01(comboTimer / comboDuration));
    }

    public void StartNewGame()
    {
        levelCompleted = false;
        isGameOver = false;

        lives = startingLives;
        score = 0;
        currentLevelIndex = 1;

        ResetCombo();
        RefreshUI();
    }

    public void AddScore(int baseAmount)
    {
        score += baseAmount * comboMultiplier;
        OnScoreChanged?.Invoke(score);
    }

    public void RegisterHit()
    {
        comboHits++;

        int newMultiplier = CalculateComboMultiplier(comboHits);

        if (comboHits < 4)
        {
            if (comboMultiplier != 1)
            {
                comboMultiplier = 1;
                OnComboChanged?.Invoke(1);
            }

            OnComboTimerChanged?.Invoke(0f);
            return;
        }

        comboMultiplier = newMultiplier;
        OnComboChanged?.Invoke(comboMultiplier);

        comboTimer = comboDuration;
        OnComboTimerChanged?.Invoke(1f);
    }

    private int CalculateComboMultiplier(int hits)
    {
        if (hits < 4) return 1;

        int multiplier = 2;
        int threshold = 4;

        while (hits >= threshold * 2)
        {
            threshold *= 2;
            multiplier *= 2;
        }

        return multiplier;
    }

    public void ResetCombo()
    {
        comboHits = 0;
        comboMultiplier = 1;
        comboTimer = 0f;

        OnComboChanged?.Invoke(1);
        OnComboTimerChanged?.Invoke(0f);
    }

    public void RequestLoseLife() => LoseLife();

    private void LoseLife()
    {
        lives--;
        OnLivesChanged?.Invoke(lives);

        SaveSystem.SaveGame();
        ResetCombo();

        if (lives <= 0)
        {
            isGameOver = true;

            if (UIManager.Instance != null)
                UIManager.Instance.ShowGameOver(score);

            return;
        }

        if (BallSpawner.Instance != null)
            BallSpawner.Instance.SpawnMainBall();

        RefreshUI();
    }

    public void AddLife(int amount)
    {
        lives += amount;
        OnLivesChanged?.Invoke(lives);
    }

    public void LevelComplete()
    {
        if (levelCompleted) return;
        levelCompleted = true;

        if (UIManager.Instance != null)
            UIManager.Instance.ShowLevelComplete(score);
    }

    public void RestartLevel()
    {
        levelCompleted = false;
        isGameOver = false;

        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void NextLevel()
    {
        levelCompleted = false;
        isGameOver = false;

        Time.timeScale = 1f;

        int next = SceneManager.GetActiveScene().buildIndex + 1;
        currentLevelIndex = (next < SceneManager.sceneCountInBuildSettings) ? next : 1;

        ResetCombo();
        RefreshUI();
        SaveSystem.SaveGame();
        SceneManager.LoadScene(currentLevelIndex);
    }

    public void ApplyLoadedData(int loadedLives, int loadedScore, int loadedLevelIndex)
    {
        levelCompleted = false;
        isGameOver = false;

        lives = loadedLives;
        score = loadedScore;
        currentLevelIndex = loadedLevelIndex;

        ResetCombo();
        RefreshUI();
    }


    public void RefreshUI()
    {
        OnLivesChanged?.Invoke(lives);
        OnScoreChanged?.Invoke(score);
        OnComboChanged?.Invoke(comboMultiplier);

        float normalized = (comboMultiplier > 1 && comboTimer > 0f)
            ? Mathf.Clamp01(comboTimer / comboDuration)
            : 0f;

        OnComboTimerChanged?.Invoke(normalized);
    }
}