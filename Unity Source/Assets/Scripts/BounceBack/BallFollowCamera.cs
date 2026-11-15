using UnityEngine;

namespace BounceBack
{
    public sealed class BallFollowCamera : MonoBehaviour
    {
        [SerializeField] private Transform target;
        [SerializeField] private Vector3 offset = new Vector3(0f, 3.5f, -6f);
        [SerializeField, Min(0.01f)] private float smoothTime = 0.12f;
        [SerializeField] private LayerMask scenery;
        [SerializeField, Min(0)] private float orbitSensitivity = 4f;
        private float yaw;
        private float pitch;
        private Vector3 followVelocity;
        private Vector3 focus;

        public void SetTarget(Transform subject)
        {
            target = subject;
            focus = target.position;
        }

        public bool Follows(Transform subject) => target == subject;

        public Vector3 MoveForward
        {
            get
            {
                Vector3 forward = Vector3.ProjectOnPlane(-offset, Vector3.up);
                if (forward.sqrMagnitude < 0.0001f)
                    forward = Vector3.forward;
                return Quaternion.AngleAxis(yaw, Vector3.up) * forward.normalized;
            }
        }

        private void OnEnable()
        {
            if (target != null)
                focus = target.position;
        }

        private void LateUpdate()
        {
            if (target == null)
                return;

            if (Input.GetMouseButton(1))
            {
                yaw = Mathf.Repeat(yaw + Input.GetAxisRaw("Mouse X") * orbitSensitivity, 360f);
                pitch = Mathf.Clamp(pitch - Input.GetAxisRaw("Mouse Y") * orbitSensitivity, -35f, 35f);
            }

            focus = Vector3.SmoothDamp(focus, target.position, ref followVelocity, smoothTime);
            Vector3 aim = focus + Vector3.up * 0.4f;
            Vector3 baseArm = offset - Vector3.up * 0.4f;
            float elevation = Mathf.Atan2(baseArm.y, Vector3.ProjectOnPlane(baseArm, Vector3.up).magnitude) * Mathf.Rad2Deg;
            elevation = Mathf.Clamp(elevation + pitch, -75f, 80f) * Mathf.Deg2Rad;
            Vector3 arm = (-MoveForward * Mathf.Cos(elevation) + Vector3.up * Mathf.Sin(elevation)) * baseArm.magnitude;
            if (arm.sqrMagnitude < 0.0001f)
                return;

            Vector3 direction = arm.normalized;
            float distance = arm.magnitude;
            if (Physics.SphereCast(aim, 0.2f, direction, out RaycastHit hit, distance, scenery, QueryTriggerInteraction.Ignore))
                distance = Mathf.Max(0.05f, hit.distance - 0.05f);

            transform.SetPositionAndRotation(aim + direction * distance, Quaternion.LookRotation(-direction, Vector3.up));
        }
    }
}
