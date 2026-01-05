using UnityEngine;
using System.Collections;
// תאכל זין
public class Brick : MonoBehaviour
{
    [Header("Rewards")]
    public int scoreReward = 100;
    public int coinReward = 5;

    [Header("VFX")]
    public GameObject breakEffect;

    [Header("Level Completion")]
    [SerializeField] private bool countsForCompletion = true;
    public bool CountsForCompletion => countsForCompletion;

    private bool isBreaking = false;
    private Collider2D col;

    private void Awake()
    {
        col = GetComponent<Collider2D>();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (isBreaking) return;
        if (!collision.gameObject.CompareTag("Ball")) return;

        isBreaking = true;
        if (col != null) col.enabled = false; // מונע double-hit

        if (GameManager.Instance != null)
        {
            GameManager.Instance.RegisterHit();
            GameManager.Instance.AddScore(scoreReward);
        }
        // יא מניאק
        if (CoinManager.Instance != null)
            CoinManager.Instance.AddCoins(coinReward);

        if (breakEffect != null)
            Instantiate(breakEffect, transform.position, Quaternion.identity);

        Destroy(gameObject);

        // בדיקה בפריים הבא (קריטי!)
        StartCoroutine(CheckLevelNextFrame());
    }

    private IEnumerator CheckLevelNextFrame()
    {
        yield return null;

        if (GameManager.Instance == null) yield break;
        if (GameManager.Instance.IsGameOver) yield break;

        LevelChecker.CheckForLevelCompletion();
    }
}
