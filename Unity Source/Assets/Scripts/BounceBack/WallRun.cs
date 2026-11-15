using UnityEngine;

namespace BounceBack
{
    [DefaultExecutionOrder(1000)]
    [RequireComponent(typeof(Rigidbody))]
    public sealed class WallRun : MonoBehaviour
    {
        [SerializeField, Min(0f)] private float runSpeed = 8f;
        [SerializeField, Min(0f)] private float stickSpeed = 0.35f;
        [SerializeField, Min(0f)] private float jumpAwaySpeed = 6f;
        [SerializeField, Min(0f)] private float jumpUpSpeed = 5f;
        [SerializeField, Min(0f)] private float contactGracePeriod = 0.12f;
        [SerializeField] private KeyCode detachKey = KeyCode.Space;
        [SerializeField] private Transform view;

        private Rigidbody body;
        private Collider wall;
        private Vector3 wallNormal;
        private Vector3 lastRunDirection;
        private float lastWallContact;
        private float canAttachAt;
        private bool hadGravity;
        private bool detachPressed;

        public bool IsRunning { get; private set; }
        public Vector3 WallNormal => wallNormal;

        private void Awake()
        {
            body = GetComponent<Rigidbody>();
            if (view == null && Camera.main != null)
                view = Camera.main.transform;
        }

        private void Update()
        {
            if (IsRunning && Input.GetKeyDown(detachKey))
                detachPressed = true;
        }

        private void FixedUpdate()
        {
            if (!IsRunning)
                return;

            if (detachPressed)
            {
                detachPressed = false;
                Detach(true);
                return;
            }

            if (wall == null || Time.time - lastWallContact > contactGracePeriod)
            {
                Detach(false);
                return;
            }

            Vector3 direction = GetRunDirection();
            if (direction.sqrMagnitude > 0.001f)
                lastRunDirection = direction;

            Vector3 alongWall = lastRunDirection * runSpeed;
            body.velocity = alongWall - wallNormal * stickSpeed;
        }

        private void OnCollisionStay(Collision collision)
        {
            if (Time.time < canAttachAt || !IsEligibleWall(collision.collider))
                return;

            for (int i = 0; i < collision.contactCount; i++)
            {
                ContactPoint contact = collision.GetContact(i);
                if (Mathf.Abs(contact.normal.y) > 0.55f)
                    continue;

                Attach(collision.collider, contact.normal);
                lastWallContact = Time.time;
                return;
            }
        }

        private bool IsEligibleWall(Collider candidate)
        {
            return candidate.GetComponentInParent<WallRunSurface>() != null;
        }

        private void Attach(Collider newWall, Vector3 normal)
        {
            if (!IsRunning)
            {
                hadGravity = body.useGravity;
            }

            wall = newWall;
            wallNormal = normal.normalized;
            Vector3 alongWall = Vector3.ProjectOnPlane(body.velocity, wallNormal);
            if (alongWall.sqrMagnitude < 0.01f)
                alongWall = Vector3.ProjectOnPlane(transform.forward, wallNormal);
            if (alongWall.sqrMagnitude < 0.01f)
                alongWall = Vector3.ProjectOnPlane(Vector3.forward, wallNormal);

            lastRunDirection = alongWall.normalized;
            lastWallContact = Time.time;
            body.useGravity = false;
            IsRunning = true;
        }

        private Vector3 GetRunDirection()
        {
            float horizontal = (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow) ? 1f : 0f) -
                (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow) ? 1f : 0f);
            float vertical = (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow) ? 1f : 0f) -
                (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow) ? 1f : 0f);

            Vector3 forward = view != null ? view.forward : Vector3.forward;
            forward = Vector3.ProjectOnPlane(forward, wallNormal).normalized;
            Vector3 right = Vector3.ProjectOnPlane(view != null ? view.right : Vector3.right, wallNormal).normalized;
            return Vector3.ProjectOnPlane(right * horizontal + forward * vertical, wallNormal).normalized;
        }

        private void Detach(bool jump)
        {
            if (!IsRunning)
                return;

            Vector3 normal = wallNormal;
            IsRunning = false;
            wall = null;
            wallNormal = Vector3.zero;
            body.useGravity = hadGravity;
            canAttachAt = Time.time + 0.2f;

            if (jump)
                body.velocity += normal * jumpAwaySpeed + Vector3.up * jumpUpSpeed;
        }

        private void OnDisable()
        {
            Detach(false);
        }
    }
}
