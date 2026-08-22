using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class LensController : MonoBehaviour
{
    [SerializeField] private MediaGameConfig config;

    private Rigidbody2D rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void Start()
    {
        InitializeMovement();
    }

    private void FixedUpdate()
    {
        if (rb.linearVelocity != Vector2.zero)
        {
            rb.linearVelocity = rb.linearVelocity.normalized * config.lensSpeed;
        }
    }

    private void InitializeMovement()
    {
        Vector2 randomDirection = Random.insideUnitCircle.normalized;

        if (Mathf.Abs(randomDirection.x) < 0.2f) randomDirection.x = 0.5f;
        if (Mathf.Abs(randomDirection.y) < 0.2f) randomDirection.y = 0.5f;

        rb.linearVelocity = randomDirection.normalized * config.lensSpeed;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        Vector2 contactPoint = collision.contacts[0].point;
        GameEvents.OnLensBounce?.Invoke(contactPoint);
    }
}