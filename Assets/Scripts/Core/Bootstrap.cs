using UnityEngine;
using UnityEngine.SceneManagement;

public class Bootstrap : MonoBehaviour
{
    private static bool initialized = false;

    private void Awake()
    {
        // ��� ����� ��� �����, �� ���� �����, ����� ���!!
        DontDestroyOnLoad(gameObject);

        // �� ��� ����� ��� � �� ����� ���
        if (initialized) return;
        initialized = true;

        // ���� �� ���������
        SceneManager.LoadScene("MainMenu");
    }

    private void Start()
    {
        // ���� ���� �� �����
        SaveSystem.LoadGame();
    }
}

