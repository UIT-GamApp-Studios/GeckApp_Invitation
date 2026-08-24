using UnityEngine;

public class LensController : MonoBehaviour
{
    [SerializeField] private MediaGameConfig config;
    [SerializeField] private Joystick joystick;

    private Rigidbody2D rb;
    private Vector2 targetVelocity;
    private bool isBouncing = false;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        if (joystick == null) return;

        Vector2 inputDirection = new Vector2(joystick.Horizontal, joystick.Vertical);
        if (inputDirection.magnitude > 1f)
        {
            inputDirection.Normalize();
        }

        targetVelocity = inputDirection * config.lensSpeed;
    }

    private void FixedUpdate()
    {
        if (isBouncing)
        {
            if (rb.linearVelocity.magnitude <= config.lensSpeed)
            {
                isBouncing = false;
            }
            return;
        }

        float accelRate = (targetVelocity.magnitude > 0.01f) 
            ? config.lensAcceleration 
            : config.lensDeceleration;

        rb.linearVelocity = Vector2.MoveTowards(
            rb.linearVelocity, 
            targetVelocity, 
            accelRate * Time.fixedDeltaTime
        );
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Wall") || collision.gameObject.name.Contains("Frame"))
        {
            Vector2 normal = collision.contacts[0].normal;
            Vector2 reflectedVelocity = Vector2.Reflect(rb.linearVelocity, normal);

            rb.linearVelocity = reflectedVelocity * config.bounceMultiplier;
            isBouncing = true;
        }
    }
}