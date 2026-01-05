using System.Collections;
using UnityEngine;

public class PaddleController : MonoBehaviour
{
    public static PaddleController Instance;

    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 12f;

    [Header("Walls (Colliders)")]
    [SerializeField] private Collider2D leftWall;
    [SerializeField] private Collider2D rightWall;

    private Collider2D paddleCollider;
   

    private float minX = -7f;
    private float maxX = 7f;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        paddleCollider = GetComponent<Collider2D>();
    }


    private void Start()
    {
        ApplyUpgrades();
        RecalculateBounds();

        // נרשם ל-BallSpawner בצורה בטוחה
        StartCoroutine(RegisterToSpawnerWhenReady());
    }

    private IEnumerator RegisterToSpawnerWhenReady()
    {
        // מחכים עד שה-BallSpawner יתקיים
        while (BallSpawner.Instance == null)
            yield return null;

        BallSpawner.Instance.RegisterPaddle(this);
    }

    private void Update()
    {
        float input = Input.GetAxisRaw("Horizontal");

        Vector3 pos = transform.position;
        pos.x += input * moveSpeed * Time.deltaTime;
        pos.x = Mathf.Clamp(pos.x, minX, maxX);

        transform.position = pos;
    }

    private void ApplyUpgrades()
    {
        if (UpgradeManager.Instance == null) return;

        IncreaseSize(UpgradeManager.Instance.paddleSizeLevel * 0.3f);
        IncreaseSpeed(UpgradeManager.Instance.paddleSpeedLevel * 1f);
    }

    private void RecalculateBounds()
    {
        if (leftWall == null || rightWall == null || paddleCollider == null) return;

        float halfWidth = paddleCollider.bounds.extents.x;
        minX = leftWall.bounds.max.x + halfWidth;
        maxX = rightWall.bounds.min.x - halfWidth;
    }

    public Vector3 GetBallHoldPosition()
    {
        return new Vector3(transform.position.x, transform.position.y + 0.45f, 0);
    }

    public void IncreaseSize(float amount)
    {
        Vector3 scale = transform.localScale;
        scale.x += amount;
        transform.localScale = scale;

        RecalculateBounds();
    }

    public void IncreaseSpeed(float amount)
    {
        moveSpeed += amount;
    }
}
