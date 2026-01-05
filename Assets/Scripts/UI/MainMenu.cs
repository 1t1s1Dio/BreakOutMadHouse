using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public void NewGame()
    {
        Debug.Log("NewGame CLICKED");
        GameManager.Instance.StartNewGame();
        UpgradeManager.Instance.ResetUpgrades();
        SceneManager.LoadScene("Level 1");
    }

    public void ContinueGame()
    {
        Debug.Log("ContinueGame CLICKED");
        bool ok = SaveSystem.LoadGame();
        if (!ok)
        {
            Debug.Log("No save found / LoadGame failed");
            return;
        }

        int levelToLoad = GameManager.Instance.currentLevelIndex;
        SceneManager.LoadScene(levelToLoad);
    }

    public void QuitGame()
    {
        Application.Quit();
        Debug.Log("Quit pressed");
    }
}

