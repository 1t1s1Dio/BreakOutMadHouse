using TMPro;
using UnityEngine;

public class LivesUI : MonoBehaviour
{
    public TextMeshProUGUI livesText;

    private void Start()
    {
        GameManager.Instance.OnLivesChanged += UpdateLives;
        UpdateLives(GameManager.Instance.lives);
    }

    private void UpdateLives(int lives)
    {
        livesText.text = "Lives: " + lives;
    }
}
