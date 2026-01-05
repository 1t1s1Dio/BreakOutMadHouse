using UnityEngine;

public static class LevelChecker
{
    public static void CheckForLevelCompletion()
    {
        Brick[] bricks = Object.FindObjectsOfType<Brick>();

        if (bricks.Length == 0)
        {
            Debug.Log("LevelChecker: Level Complete");
            if (GameManager.Instance != null)
                GameManager.Instance.LevelComplete();
        }
    }
}



