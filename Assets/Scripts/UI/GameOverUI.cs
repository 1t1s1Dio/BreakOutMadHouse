using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class GameOverUI : MonoBehaviour
{
    public TextMeshProUGUI finalScoreText;
    public Button retryButton;
    public Button mainMenuButton;

    private void Start()
    {
        retryButton.onClick.AddListener(() =>
        {
            Time.timeScale = 1f;
            GameManager.Instance.RestartLevel();
        });

        mainMenuButton.onClick.AddListener(() =>
        {
            Time.timeScale = 1f;
            UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
        });
    }

    private void OnEnable()
    {
        if (GameManager.Instance != null)
            UpdateFinalScore(GameManager.Instance.score);
    }


    public void UpdateFinalScore(int score)
    {
        finalScoreText.text = "Score: " + score;
    }
}
