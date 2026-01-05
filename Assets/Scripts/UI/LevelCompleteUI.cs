using UnityEngine;
using TMPro;
using UnityEngine.UI;
public class LevelCompleteUI : MonoBehaviour
{
    public TextMeshProUGUI finalScoreText;
    public Button nextLevelButton;
    public Button restartButton;
    public Button upgradeButton;
    private void Start()
    {
        nextLevelButton.onClick.AddListener(() =>
        {
            Time.timeScale = 1f;
            GameManager.Instance.NextLevel();
        });
        restartButton.onClick.AddListener(() =>
        {
            Time.timeScale = 1f;
            GameManager.Instance.RestartLevel();
        });
        upgradeButton.onClick.AddListener(() =>
        {
            if (UIManager.Instance != null)
                UIManager.Instance.OpenUpgradePanel();
            else
            {
                Debug.LogError("UIManager Instance is missing!");
            }
        });
    }
    public void UpdateFinalScore(int score)
    {
        finalScoreText.text = "Score: " + score;
    }
}