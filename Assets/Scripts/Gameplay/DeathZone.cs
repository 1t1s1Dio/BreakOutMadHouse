using UnityEngine;

public class DeathZone : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Ball"))
            return;

        BallController ball = collision.GetComponent<BallController>();
        if (ball == null)
            return;

        // ✅ אם הכדור כבר בטיפול – לא עושים כלום
        if (ball.IsDead)
            return;

        if (Time.timeScale == 0f)
            return;

        ball.FellToDeathZone();
    }
}
