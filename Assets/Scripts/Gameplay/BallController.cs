using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class BallController : MonoBehaviour
{
    private Rigidbody2D rb;
    private TrailRenderer trail;

    // Who the ball is currently attached to (when not launched)
    private PaddleController holdPaddle;

    [Header("Speed Settings")]
    [SerializeField] private float initialSpeed = 7f;
    [SerializeField] private float speedIncrease = 0.5f;
    [SerializeField] private float maxSpeed = 15f;

    [Header("Launch Settings")]
    [SerializeField] private float launchMaxX = 0.25f;      // Horizontal variance when launching with Space
    [SerializeField] private float forceLaunchMaxX = 0.15f; // Horizontal variance for auto-launch (extra balls)

    [Header("Anti-Stuck (safety net)")]
    [SerializeField] private float minYComponent = 0.22f;  // prevents near-horizontal tracks
    [SerializeField] private float minXComponent = 0.18f;  // prevents near-vertical elevators

    [Header("Paddle Bounce")]
    [SerializeField] private float edgeBoost = 1.35f;      // edge power
    [SerializeField] private float minBounceX = 0.18f;     // minimum deviation from center

    [Header("VFX")]
    [SerializeField] private ParticleSystem launchFlash;

    private bool isLaunched = false;
    private bool isDead = false;
    public bool IsDead => isDead;

    private float currentSpeed;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;

        trail = GetComponent<TrailRenderer>();
        if (trail != null) trail.emitting = false;
    }

    private void Start()
    {
        currentSpeed = initialSpeed;

        // ✅ חיבור בטוח לפדל אם עדיין לא קיבלנו אחד מה-Spawner
        if (holdPaddle == null && PaddleController.Instance != null)
            holdPaddle = PaddleController.Instance;

        HoldBallOnPaddle();
    }


    private void Update()
    {
        if (isLaunched) return;

        if (holdPaddle != null)
            transform.position = holdPaddle.GetBallHoldPosition();

        if (Input.GetKeyDown(KeyCode.Space))
            Launch();
    }

    private void FixedUpdate()
    {
        if (!isLaunched) return;

        KeepConstantSpeed();
        ApplyAntiStuck();
    }

    private void Launch()
    {
        // Safety: no paddle assigned => no launch
        if (holdPaddle == null)
            return;

        isLaunched = true;
        isDead = false;

        if (trail != null)
        {
            trail.Clear();
            trail.emitting = true;
        }

        if (launchFlash != null)
            Instantiate(launchFlash, transform.position, Quaternion.identity);

        Vector2 dir = GetUpwardLaunchDirection(launchMaxX);
        rb.WakeUp();
        rb.linearVelocity = dir * currentSpeed;
    }

    private void HoldBallOnPaddle()
    {
        if (trail != null)
        {
            trail.emitting = false;
            trail.Clear();
        }

        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;
        rb.Sleep();

        // if we don't have a paddle yet, try the singleton as a fallback
        if (holdPaddle == null && PaddleController.Instance != null)
            holdPaddle = PaddleController.Instance;

        if (holdPaddle != null)
            transform.position = holdPaddle.GetBallHoldPosition();
    }

    public void ResetBall()
    {
        isLaunched = false;
        isDead = false;
        currentSpeed = initialSpeed;
        HoldBallOnPaddle();
    }

    public void ResetBallToPaddle(PaddleController paddle)
    {
        holdPaddle = paddle;

        isLaunched = false;
        isDead = false;
        currentSpeed = initialSpeed;

        if (trail != null)
        {
            trail.emitting = false;
            trail.Clear();
        }

        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;
        rb.Sleep();

        if (holdPaddle != null)
            transform.position = holdPaddle.GetBallHoldPosition();
    }

    private void KeepConstantSpeed()
    {
        float speed = rb.linearVelocity.magnitude;
        if (speed < 0.001f) return;

        // only correct if drifting too far from target speed
        if (Mathf.Abs(speed - currentSpeed) > 0.2f)
            rb.linearVelocity = rb.linearVelocity.normalized * currentSpeed;
    }

    private void ApplyAntiStuck()
    {
        Vector2 v = rb.linearVelocity;
        if (v.sqrMagnitude < 0.0001f) return;

        // 1) near-vertical elevator (X too small)
        if (Mathf.Abs(v.x) < minXComponent)
        {
            float signX = (v.x == 0f) ? 1f : Mathf.Sign(v.x);
            v.x = signX * minXComponent;
            rb.linearVelocity = v.normalized * currentSpeed;
            return;
        }

        // 2) near-horizontal track (Y too small)
        if (Mathf.Abs(v.y) < minYComponent)
        {
            float signY = (v.y == 0f) ? 1f : Mathf.Sign(v.y);
            v.y = signY * minYComponent;
            rb.linearVelocity = v.normalized * currentSpeed;
            return;
        }
    }

    private void OnCollisionEnter2D(Collision2D col)
    {
        if (!col.collider.CompareTag("Paddle")) return;

        Vector2 dir = PaddleBounce(col);
        rb.linearVelocity = dir * currentSpeed;
    }

    private Vector2 PaddleBounce(Collision2D col)
    {
        float paddleX = col.collider.bounds.center.x;
        float halfWidth = col.collider.bounds.extents.x;

        // משתמשים במיקום הכדור (יציב יותר מ-contact point)
        float ballX = rb.position.x;

        float x = (ballX - paddleX) / halfWidth; // -1..1
        x = Mathf.Clamp(x, -1f, 1f);

        // boost לקצוות
        x = Mathf.Sign(x) * Mathf.Pow(Mathf.Abs(x), 0.75f) * edgeBoost;

        // מינימום סטייה מהמרכז לפי הצד
        if (Mathf.Abs(x) < minBounceX)
            x = (x >= 0f) ? minBounceX : -minBounceX;

        return new Vector2(x, 1f).normalized;
    }


    public void IncreaseSpeed()
    {
        currentSpeed = Mathf.Min(currentSpeed + speedIncrease, maxSpeed);
    }

    public void FellToDeathZone()
    {
        if (isDead) return;
        isDead = true;

        if (BallSpawner.Instance != null)
            BallSpawner.Instance.BallFell(this);
        else
            Destroy(gameObject);
    }

    private Vector2 GetUpwardLaunchDirection(float maxX)
    {
        float x = Random.Range(-maxX, maxX);
        return new Vector2(x, 1f).normalized;
    }

    public void ForceLaunch()
    {
        if (isLaunched) return;

        isLaunched = true;

        if (trail != null)
        {
            trail.Clear();
            trail.emitting = true;
        }

        rb.WakeUp();

        Vector2 dir = GetUpwardLaunchDirection(forceLaunchMaxX);
        rb.linearVelocity = dir * currentSpeed;
    }
}
