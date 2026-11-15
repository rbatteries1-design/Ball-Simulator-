using UnityEngine;

namespace BounceBack
{
    public sealed class BounceCameraFollow : MonoBehaviour
    {
        [SerializeField] private Transform target;
        [SerializeField] private float distance = 14f;
        [SerializeField] private float minDistance = 5f;
        [SerializeField] private float maxDistance = 24f;
        [SerializeField] private float mouseSensitivity = 3f;
        [SerializeField] private float zoomSensitivity = 3f;
        [SerializeField] private float smoothTime = 0.18f;
        [SerializeField] private float lookHeight = 0.5f;
        [SerializeField] private float minPitch = 8f;
        [SerializeField] private float maxPitch = 78f;

        private Vector3 followVelocity;
        private float yaw;
        private float pitch = 32f;

        private void Start()
        {
            if (target == null)
            {
                BouncePlayerController player = FindObjectOfType<BouncePlayerController>();
                if (player != null)
                {
                    target = player.transform;
                }
            }

            yaw = transform.eulerAngles.y;
            pitch = -Mathf.DeltaAngle(0f, transform.eulerAngles.x);
            pitch = Mathf.Clamp(pitch, minPitch, maxPitch);
        }

        public void SetTarget(Transform newTarget)
        {
            target = newTarget;
        }

        private void Update()
        {
            if (target == null)
            {
                return;
            }

            if (Input.GetMouseButton(1))
            {
                yaw += Input.GetAxis("Mouse X") * mouseSensitivity;
                pitch -= Input.GetAxis("Mouse Y") * mouseSensitivity;
                pitch = Mathf.Clamp(pitch, minPitch, maxPitch);
            }

            distance = Mathf.Clamp(
                distance - Input.GetAxis("Mouse ScrollWheel") * zoomSensitivity,
                minDistance,
                maxDistance);
        }

        private void LateUpdate()
        {
            if (target == null)
            {
                return;
            }

            Vector3 focusPoint = target.position + Vector3.up * lookHeight;
            Quaternion orbitRotation = Quaternion.Euler(pitch, yaw, 0f);
            Vector3 desiredPosition = focusPoint + orbitRotation * (Vector3.back * distance);
            transform.position = Vector3.SmoothDamp(
                transform.position,
                desiredPosition,
                ref followVelocity,
                smoothTime);

            Vector3 lookDirection = focusPoint - transform.position;
            if (lookDirection.sqrMagnitude > 0.001f)
            {
                transform.rotation = Quaternion.LookRotation(lookDirection);
            }
        }
    }
}
