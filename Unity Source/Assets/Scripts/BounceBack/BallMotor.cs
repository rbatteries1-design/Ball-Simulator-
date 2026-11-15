using UnityEngine;

namespace BounceBack
{
    [RequireComponent(typeof(Rigidbody), typeof(SphereCollider))]
    public sealed class BallMotor : MonoBehaviour
    {
        [SerializeField] private Transform view;
        [SerializeField, Min(0)] private float acceleration = 22f;
        [SerializeField, Min(0)] private float driveSpeed = 9f;
        [SerializeField, Range(0, 1)] private float airControl = 0.25f;
        [SerializeField, SceneParticle] private ParticleSystem movementDust;
        [SerializeField, Min(0.04f)] private float trailSpacing = 0.18f;
        private BallFollowCamera orbitCamera;
        private Rigidbody body;
        private SphereCollider sphere;
        private Vector3 trailAnchor;
        private Vector2 input;
        private float lastGroundContact = float.NegativeInfinity;

        private void Awake()
        {
            body = GetComponent<Rigidbody>();
            sphere = GetComponent<SphereCollider>();
            if (movementDust == null)
                movementDust = GetComponentInChildren<ParticleSystem>();
            if (movementDust != null && !movementDust.gameObject.scene.IsValid())
                movementDust = Instantiate(movementDust, transform);
            SetMovementDust(movementDust);
            if (view == null)
            {
                foreach (var camera in FindObjectsOfType<BallFollowCamera>())
                {
                    if (!camera.Follows(transform))
                        continue;
                    view = camera.transform;
                    break;
                }

                if (view == null && Camera.main != null)
                    view = Camera.main.transform;
            }

            if (view != null)
                orbitCamera = view.GetComponent<BallFollowCamera>();
        }

        private bool inputBlocked;
        private bool raceBlocked;

        public void SetRaceBlocked(bool value)
        {
            raceBlocked = value;
            if (value)
                input = Vector2.zero;
        }

        public void SetInputBlocked(bool value)
        {
            inputBlocked = value;
            if (value)
                input = Vector2.zero;
        }

        public void SetMovementDust(ParticleSystem effect)
        {
            if (movementDust != null && movementDust != effect)
                movementDust.Stop(false, ParticleSystemStopBehavior.StopEmitting);
            movementDust = effect;
            if (movementDust != null)
            {
                movementDust.Stop(false, ParticleSystemStopBehavior.StopEmittingAndClear);
                var main = movementDust.main;
                main.loop = true;
                main.simulationSpace = ParticleSystemSimulationSpace.World;
                var emission = movementDust.emission;
                emission.enabled = false;
                movementDust.Play(false);
            }
            trailAnchor = body.position;
        }

        private void Update()
        {
            if (inputBlocked || raceBlocked)
                return;
            float horizontal = KeyAxis(KeyCode.A, KeyCode.LeftArrow, KeyCode.D, KeyCode.RightArrow);
            float vertical = KeyAxis(KeyCode.S, KeyCode.DownArrow, KeyCode.W, KeyCode.UpArrow);
            input = Vector2.ClampMagnitude(new Vector2(horizontal, vertical), 1f);
        }

        private static float KeyAxis(KeyCode negative, KeyCode negativeAlternate, KeyCode positive, KeyCode positiveAlternate)
        {
            bool backward = Input.GetKey(negative) || Input.GetKey(negativeAlternate);
            bool forward = Input.GetKey(positive) || Input.GetKey(positiveAlternate);
            return (forward ? 1f : 0f) - (backward ? 1f : 0f);
        }

        private void LateUpdate()
        {
            if (movementDust == null)
                return;

            Vector3 position = transform.position;
            Vector3 delta = position - trailAnchor;
            float distance = delta.magnitude;
            if (distance > 3f || body.velocity.sqrMagnitude < 0.16f)
            {
                trailAnchor = position;
                return;
            }

            float spacing = Mathf.Max(0.04f, trailSpacing);
            Vector3 direction = delta.normalized;
            Vector3 contactOffset = sphere.bounds.center - position -
                Vector3.up * sphere.bounds.extents.y * (Grounded ? 0.85f : 0.2f);
            int count = Mathf.Min(16, Mathf.FloorToInt(distance / spacing));
            for (int i = 0; i < count; i++)
            {
                trailAnchor += direction * spacing;
                var particle = new ParticleSystem.EmitParams
                {
                    position = trailAnchor + contactOffset + Random.insideUnitSphere * 0.13f,
                    velocity = -direction * Random.Range(0.25f, 1.2f) + Vector3.up * Random.Range(0.15f, 0.9f)
                };
                movementDust.Emit(particle, 1);
            }
        }

        private bool Grounded => Time.fixedTime - lastGroundContact <= 2f * Time.fixedDeltaTime;

        private void OnCollisionStay(Collision collision)
        {
            for (int i = 0; i < collision.contactCount; i++)
                if (collision.GetContact(i).normal.y >= 0.55f)
                    lastGroundContact = Time.fixedTime;
        }

        private void FixedUpdate()
        {
            if (raceBlocked || input.sqrMagnitude < 0.0001f)
                return;

            Vector3 forward = orbitCamera != null ? orbitCamera.MoveForward :
                view != null ? Vector3.ProjectOnPlane(view.forward, Vector3.up).normalized : Vector3.forward;
            if (forward.sqrMagnitude < 0.0001f)
                forward = Vector3.forward;

            Vector3 right = Vector3.Cross(Vector3.up, forward);
            Vector3 direction = (right * input.x + forward * input.y).normalized;
            Vector3 planarVelocity = Vector3.ProjectOnPlane(body.velocity, Vector3.up);
            float forwardSpeed = Vector3.Dot(planarVelocity, direction);
            float targetSpeed = Mathf.Max(forwardSpeed, driveSpeed * input.magnitude);
            Vector3 targetVelocity = direction * targetSpeed;
            float maxChange = acceleration * (Grounded ? 1f : airControl) * Time.fixedDeltaTime;
            Vector3 change = Vector3.ClampMagnitude(targetVelocity - planarVelocity, maxChange);
            body.AddForce(change, ForceMode.VelocityChange);
        }
    }
}
