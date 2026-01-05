using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BallSpawner : MonoBehaviour
{
    public static BallSpawner Instance;

    [Header("Spawn")]
    [SerializeField] private BallController ballPrefab;

    [Header("Ball Limits")]
    [SerializeField] private int maxBalls = 1; // תמיד לפחות 1 (כדור ראשי)

    [Header("Extra Balls Timing")]
    [SerializeField] private float extraBallDelayStep = 5f; // 5,10,15...

    private readonly List<BallController> aliveBalls = new List<BallController>();

    private int extraBallLevel = 0;          // כמה אקסטרה מותר (0..)
    private float extraTimer = 0f;           // זמן מאז תחילת שלב
    private int spawnedExtrasThisLevel = 0;  // כמה כבר נוצרו בשלב

    private bool lifePending = false;

    private PaddleController currentPaddle;
    private bool gameplayActive = false;
    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    // נקרא מה-PaddleController (מומלץ), אבל יש גם fallback בחיפוש
    public void RegisterPaddle(PaddleController paddle)
    {
        currentPaddle = paddle;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        KillAllBallsInScene();
        CleanupDeadRefs();

        gameplayActive = scene.name != "MainMenu";

        if (!gameplayActive)
            return;

        extraTimer = 0f;
        spawnedExtrasThisLevel = 0;
        currentPaddle = null;

        StartCoroutine(SpawnMainBallWhenReady());
    }


    private IEnumerator SpawnMainBallWhenReady()
    {
        // מחכים קצת עד שהפדל יופיע בסצנה
        float timeout = 3f;
        float t = 0f;

        while (currentPaddle == null && t < timeout)
        {
            currentPaddle = FindAnyObjectByType<PaddleController>(); // fallback
            t += Time.unscaledDeltaTime;
            yield return null;
        }

        if (currentPaddle == null)
        {
            Debug.LogError("No PaddleController found in scene (RegisterPaddle not called and Find failed).");
            yield break;
        }

        SpawnMainBall(); // כדור ראשי מחכה ל-Space (כי ResetBallToPaddle עושה hold)
    }

    private void Update()
    {
        if (!gameplayActive)
            return;

        if (extraBallLevel <= 0) return;
        if (AliveCount >= maxBalls) return;

        extraTimer += Time.deltaTime;

        // כמה אקסטרה אמורים להיות לפי הזמן
        int shouldHaveSpawned = Mathf.FloorToInt(extraTimer / extraBallDelayStep);
        shouldHaveSpawned = Mathf.Min(shouldHaveSpawned, extraBallLevel);

        int toSpawn = shouldHaveSpawned - spawnedExtrasThisLevel;
        if (toSpawn <= 0) return;

        for (int i = 0; i < toSpawn; i++)
        {
            if (AliveCount >= maxBalls) return;

            if (TrySpawnExtraBall())
                spawnedExtrasThisLevel++;
            else
                return;
        }
    }

    // ===== API לשדרוגים =====
    public void SetExtraBallLevel(int level)
    {
        extraBallLevel = Mathf.Max(0, level);
        SetMaxBalls(1 + extraBallLevel);
    }

    public void SetMaxBalls(int newMax)
    {
        maxBalls = Mathf.Max(1, newMax);
    }

    public void EnsureBallCount()
    {
        CleanupDeadRefs();

        if (AliveCount <= 0)
            SpawnMainBall();

        CleanupDeadRefs();
    }

    // ===== כדורים =====
    public int AliveCount
    {
        get { CleanupDeadRefs(); return aliveBalls.Count; }
    }

    public void RegisterBall(BallController ball)
    {
        if (ball == null) return;
        if (!aliveBalls.Contains(ball))
            aliveBalls.Add(ball);
    }

    public void UnregisterBall(BallController ball)
    {
        if (ball == null) return;
        aliveBalls.Remove(ball);
    }

    public void BallFell(BallController ball)
    {
        if (ball == null) return;

        UnregisterBall(ball);
        Destroy(ball.gameObject);

        // אם כבר שלחנו LoseLife בגלל הסיטואציה הזו - לא לשלוח שוב
        if (lifePending) return;

        if (AliveCount <= 0)
        {
            lifePending = true;
            if (GameManager.Instance != null)
                GameManager.Instance.RequestLoseLife();

            // משחררים “נעילה” בפריים הבא
            StartCoroutine(ReleaseLifePending());
        }
    }

    private System.Collections.IEnumerator ReleaseLifePending()
    {
        yield return null;
        lifePending = false;
    }


    // ✅ ציבורי: GameManager יכול לקרוא לזה
    public void SpawnMainBall()
    {
        if (ballPrefab == null) { Debug.LogError("Ball prefab is missing!"); return; }
        if (currentPaddle == null) { currentPaddle = FindAnyObjectByType<PaddleController>(); }
        if (currentPaddle == null) { Debug.LogError("No PaddleController found for SpawnMainBall."); return; }

        BallController ball = Instantiate(ballPrefab);
        ball.ResetBallToPaddle(currentPaddle); // מחזיק על הפדל ומחכה לרווח
        RegisterBall(ball);
    }

    public void KillAllBalls()
    {
        CleanupDeadRefs();

        foreach (var ball in aliveBalls)
        {
            if (ball != null)
                Destroy(ball.gameObject);
        }

        aliveBalls.Clear();
    }


    private bool TrySpawnExtraBall()
    {
        if (AliveCount >= maxBalls) return false;
        if (ballPrefab == null) return false;

        if (currentPaddle == null)
            currentPaddle = FindAnyObjectByType<PaddleController>();

        if (currentPaddle == null) return false;

        BallController ball = Instantiate(ballPrefab);

        // יושב על הפדל אבל עם סטייה קטנה כדי לא להיערם בדיוק באותו פיקסל
        ball.ResetBallToPaddle(currentPaddle);

        // Offset קטן כדי שלא יווצר בתוך כדור אחר
        ball.transform.position += new Vector3(Random.Range(-0.6f, 0.6f), 0.2f, 0f);

        RegisterBall(ball);

        // ✅ משגרים בפריים הבא כדי שלא "יתקע"
        StartCoroutine(LaunchExtraNextFrame(ball));

        return true;
    }

    private IEnumerator LaunchExtraNextFrame(BallController ball)
    {
        yield return null; // פריים אחד

        if (ball == null) yield break;
        ball.ForceLaunch(); // לא מחכה ל-space
    }

    private void CleanupDeadRefs()
    {
        aliveBalls.RemoveAll(b => b == null);
    }

    private void KillAllBallsInScene()
    {
        BallController[] balls = Object.FindObjectsOfType<BallController>();
        foreach (var b in balls)
            Destroy(b.gameObject);

        aliveBalls.Clear();
    }
}
