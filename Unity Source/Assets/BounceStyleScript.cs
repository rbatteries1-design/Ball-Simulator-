using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public sealed class BouncyBall : MonoBehaviour
{
    [Header("Bounce Settings")]
    [Range(0f, 1f)] public float bounciness = 0.78f;
    [Min(0f)] public float minBounceSpeed = 1.2f;
    [Min(0f)] public float maxBounceSpeed = 25f;

    [Header("Readouts")]
    public float lastFallDistance;
    public float lastImpactSpeed;

    private Rigidbody rb;
    private Vector3 preCollisionVelocity;
    private float apexY;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        apexY = transform.position.y;
    }

    private void FixedUpdate()
    {
        preCollisionVelocity = rb.velocity;
        if (transform.position.y > apexY)
        {
            apexY = transform.position.y;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.GetComponentInParent<BounceBack.BouncePad>() != null)
        {
            return;
        }

        bool landedOnTop = false;
        for (int i = 0; i < collision.contactCount; i++)
        {
            if (collision.GetContact(i).normal.y > 0.5f)
            {
                landedOnTop = true;
                break;
            }
        }

        if (!landedOnTop || preCollisionVelocity.y >= 0f)
        {
            return;
        }

        float impactSpeed = -preCollisionVelocity.y;
        lastFallDistance = Mathf.Max(0f, apexY - transform.position.y);
        lastImpactSpeed = impactSpeed;

        float bounceSpeed = Mathf.Min(impactSpeed * bounciness, maxBounceSpeed);

        if (bounceSpeed < minBounceSpeed)
        {
            rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
            apexY = transform.position.y;
            return;
        }

        rb.velocity = new Vector3(rb.velocity.x, bounceSpeed, rb.velocity.z);
        apexY = transform.position.y;
    }
}