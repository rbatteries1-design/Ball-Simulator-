using UnityEngine;

namespace BounceBack
{
    [RequireComponent(typeof(Rigidbody))]
    public sealed class BouncePlayerController : MonoBehaviour
    {
        [SerializeField] private float moveAcceleration = 28f;
        [SerializeField] private float maxMoveSpeed = 8f;
        [SerializeField] private float jumpVelocity = 8f;
        [SerializeField] private float coyoteTime = 0.15f;
        [SerializeField] private float fallLimit = -15f;
        [SerializeField] private Vector3 spawnPosition;

        private Rigidbody body;
        private Vector3 moveDirection;
        private float lastGroundedTime = -10f;

        public Rigidbody Body => body;
        public Vector3 SpawnPosition => spawnPosition;

        private void Awake()
        {
            body = GetComponent<Rigidbody>();
            body.interpolation = RigidbodyInterpolation.Interpolate;
            body.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            body.maxAngularVelocity = 40f;

            if (spawnPosition == Vector3.zero)
            {
                spawnPosition = transform.position;
            }
        }

        private void Update()
        {
            float horizontal =
                (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow) ? 1f : 0f) -
                (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow) ? 1f : 0f);
            float vertical =
                (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow) ? 1f : 0f) -
                (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow) ? 1f : 0f);

            Vector3 cameraForward = Vector3.forward;
            Vector3 cameraRight = Vector3.right;
            Camera mainCamera = Camera.main;
            if (mainCamera != null)
            {
                cameraForward = Vector3.ProjectOnPlane(mainCamera.transform.forward, Vector3.up).normalized;
                cameraRight = Vector3.ProjectOnPlane(mainCamera.transform.right, Vector3.up).normalized;
            }

            moveDirection = Vector3.ClampMagnitude(
                cameraRight * horizontal + cameraForward * vertical,
                1f);

            if (Input.GetKeyDown(KeyCode.Space) && Time.time - lastGroundedTime <= coyoteTime)
            {
                Bounce(jumpVelocity);
            }

            if (transform.position.y < fallLimit)
            {
                if (BounceBackGame.Instance != null)
                {
                    BounceBackGame.Instance.PlayerFell(transform);
                }
                else
                {
                    ResetToSpawn();
                }
            }
        }

        private void FixedUpdate()
        {
            Vector3 desiredVelocity = moveDirection * maxMoveSpeed;
            Vector3 currentVelocity = new Vector3(body.velocity.x, 0f, body.velocity.z);
            Vector3 velocityChange = Vector3.ClampMagnitude(
                desiredVelocity - currentVelocity,
                moveAcceleration * Time.fixedDeltaTime);
            body.AddForce(velocityChange, ForceMode.VelocityChange);
        }

        private void OnCollisionStay(Collision collision)
        {
            for (int i = 0; i < collision.contactCount; i++)
            {
                ContactPoint contact = collision.GetContact(i);
                if (contact.normal.y > 0.55f && body.velocity.y <= 1f)
                {
                    lastGroundedTime = Time.time;
                    break;
                }
            }
        }

        public void Bounce(float upwardVelocity)
        {
            Vector3 velocity = body.velocity;
            velocity.y = upwardVelocity;
            body.velocity = velocity;
        }

        public void ResetToSpawn()
        {
            body.velocity = Vector3.zero;
            body.angularVelocity = Vector3.zero;
            transform.position = spawnPosition;
            lastGroundedTime = Time.time;
        }

        public void SetSpawnPosition(Vector3 position)
        {
            spawnPosition = position;
        }
    }
}
