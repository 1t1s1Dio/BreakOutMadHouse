using UnityEngine;
using UnityEngine.SceneManagement;

public class Bootstrap : MonoBehaviour
{
    private static bool initialized = false;

    private void Awake()
    {
        // שלא יימחק בין סצנות, בר בודק ערנות!
        DontDestroyOnLoad(gameObject);

        // אם כבר הופעל פעם – לא לטעון שוב
        if (initialized) return;
        initialized = true;

        // טוען את המיינמניו
        SceneManager.LoadScene("MainMenu");
    }

    private void Start()
    {
        // שורה חדשה יא מניאק
        SaveSystem.LoadGame();
    }
}

