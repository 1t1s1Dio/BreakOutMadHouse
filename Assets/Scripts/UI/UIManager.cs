using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    [Header("HUD Root")]
    [SerializeField] private GameObject hudRoot;

    [Header("HUD Text")]
    [SerializeField] private TMP_Text livesText;
    [SerializeField] private TMP_Text scoreText;

    [Header("Combo")]
    [SerializeField] private TMP_Text comboText;
    [SerializeField] private Slider comboBar;

    [Header("Combo Visuals")]
    [SerializeField] private Color normalScoreColor = Color.white;
    [SerializeField] private Color comboScoreColor = Color.yellow;

    [Header("Combo Settings")]
    [SerializeField] private int comboTextThreshold = 2; // תציג טקסט החל מ-x2

    [Header("Panels")]
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private GameObject levelCompletePanel;
    [SerializeField] private GameObject upgradePanel;
    [SerializeField] private GameObject pausePanel;

    private Coroutine comboAnimCoroutine;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        SceneManager.sceneLoaded += OnSceneLoaded;

        HideHUD();
        HideAllPanels();
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        UnhookGameEvents();
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Time.timeScale = 1f;

        RebindScenePanels();
        HideAllPanels();

        if (scene.name == "MainMenu")
            HideHUD();
        else
            ShowHUD();

        HookGameEvents();
    }

    private void HookGameEvents()
    {
        if (GameManager.Instance == null) return;

        UnhookGameEvents();

        GameManager.Instance.OnLivesChanged += UpdateLives;
        GameManager.Instance.OnScoreChanged += UpdateScore;
        GameManager.Instance.OnComboChanged += UpdateComboVisual;
        GameManager.Instance.OnComboTimerChanged += UpdateComboBar;

        // Sync immediately
        UpdateLives(GameManager.Instance.lives);
        UpdateScore(GameManager.Instance.score);
        UpdateComboVisual(GameManager.Instance.comboMultiplier);
        UpdateComboBar(0f);
    }

    private void UnhookGameEvents()
    {
        if (GameManager.Instance == null) return;

        GameManager.Instance.OnLivesChanged -= UpdateLives;
        GameManager.Instance.OnScoreChanged -= UpdateScore;
        GameManager.Instance.OnComboChanged -= UpdateComboVisual;
        GameManager.Instance.OnComboTimerChanged -= UpdateComboBar;
    }

    public void ShowHUD()
    {
        if (hudRoot != null) hudRoot.SetActive(true);

        if (comboBar != null)
        {
            comboBar.minValue = 0f;
            comboBar.maxValue = 1f;
            comboBar.value = 0f;
            comboBar.gameObject.SetActive(false);
        }

        if (comboText != null)
            comboText.gameObject.SetActive(false);
    }

    public void HideHUD()
    {
        if (hudRoot != null) hudRoot.SetActive(false);
    }

    public void HideAllPanels()
    {
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        if (levelCompletePanel != null) levelCompletePanel.SetActive(false);
        if (upgradePanel != null) upgradePanel.SetActive(false);
        if (pausePanel != null) pausePanel.SetActive(false);
    }

    private void UpdateLives(int lives)
    {
        if (livesText != null)
            livesText.text = $"Lives: {lives}";
    }

    private void UpdateScore(int score)
    {
        if (scoreText != null)
            scoreText.text = $"Score: {score}";
    }

    private void UpdateComboVisual(int combo)
    {
        bool inCombo = combo > 1;

        // צבע ניקוד (אם קיים)
        if (scoreText != null)
            scoreText.color = inCombo ? comboScoreColor : normalScoreColor;

        // טקסט קומבו רק אחרי threshold
        bool showText = inCombo && combo >= comboTextThreshold;

        if (comboText != null)
        {
            comboText.gameObject.SetActive(showText);

            if (showText)
            {
                comboText.text = $"x{combo}";
                comboText.transform.localScale = Vector3.one;

                if (comboAnimCoroutine != null)
                    StopCoroutine(comboAnimCoroutine);

                comboAnimCoroutine = StartCoroutine(ComboPopAnimation());
            }
            else
            {
                comboText.transform.localScale = Vector3.one;
            }
        }

        // בר קומבו רק כשהקומבו פעיל
        if (comboBar != null)
        {
            comboBar.gameObject.SetActive(inCombo);
            if (!inCombo) comboBar.value = 0f;
        }
    }

    private void UpdateComboBar(float normalized)
    {
        if (comboBar == null) return;
        comboBar.value = Mathf.Clamp01(normalized);
    }

    private IEnumerator ComboPopAnimation()
    {
        if (comboText == null) yield break;

        Vector3 start = Vector3.one * 1.25f;
        Vector3 end = Vector3.one;

        comboText.transform.localScale = start;

        float t = 0f;
        while (t < 0.12f)
        {
            t += Time.unscaledDeltaTime;
            comboText.transform.localScale = Vector3.Lerp(start, end, t / 0.12f);
            yield return null;
        }

        comboText.transform.localScale = end;
    }

    public void ShowGameOver(int finalScore)
    {
        HideAllPanels();

        // ✅ מנקה כדורים (חשוב במיוחד ב-MultiBall)
        if (BallSpawner.Instance != null)
            BallSpawner.Instance.KillAllBalls();

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);

            GameOverUI ui = gameOverPanel.GetComponent<GameOverUI>();
            if (ui != null)
                ui.UpdateFinalScore(finalScore);
            else
                Debug.LogError("GameOverUI component not found on gameOverPanel!");
        }
        else
        {
            Debug.LogError("gameOverPanel is NULL in UIManager!");
        }

        Time.timeScale = 0f;
    }

    public void ShowLevelComplete(int finalScore)
    {
        HideAllPanels();

        if (levelCompletePanel != null)
        {
            levelCompletePanel.SetActive(true);

            // אם יש לך LevelCompleteUI על הפאנל, נעדכן ניקוד
            LevelCompleteUI ui = levelCompletePanel.GetComponent<LevelCompleteUI>();
            if (ui != null)
                ui.UpdateFinalScore(finalScore);
        }

        Time.timeScale = 0f;
    }

    public void OpenUpgradePanel()
    {
        HideAllPanels();
        if (upgradePanel != null) upgradePanel.SetActive(true);
        Time.timeScale = 0f;
    }

    public void ProceedToNextLevel()
    {
        HideAllPanels();
        Time.timeScale = 1f;

        if (GameManager.Instance != null)
            GameManager.Instance.NextLevel();
    }

    private void RebindScenePanels()
    {
        if (levelCompletePanel == null)
        {
            LevelCompleteUI levelUI =
                Object.FindFirstObjectByType<LevelCompleteUI>(FindObjectsInactive.Include);

            if (levelUI != null)
                levelCompletePanel = levelUI.gameObject;
        }

        if (gameOverPanel == null)
        {
            GameOverUI gameOverUI =
                Object.FindFirstObjectByType<GameOverUI>(FindObjectsInactive.Include);

            if (gameOverUI != null)
                gameOverPanel = gameOverUI.gameObject;
        }
    }


}
