using UnityEngine;

namespace BounceBack
{
    // Put this on the visible model, not on the Rigidbody root.
    public sealed class WallRunPreview : MonoBehaviour
    {
        [SerializeField] private WallRun wallRun;
        [SerializeField] private Vector3 flipAxis = Vector3.forward;
        [SerializeField, Range(0f, 360f)] private float flipAngle = 180f;
        [SerializeField, Min(0.1f)] private float turnSpeed = 10f;

        private Quaternion restingRotation;

        private void Awake()
        {
            restingRotation = transform.localRotation;
            if (wallRun == null)
                wallRun = GetComponentInParent<WallRun>();
        }

        private void LateUpdate()
        {
            Vector3 axis = flipAxis.sqrMagnitude > 0.001f ? flipAxis.normalized : Vector3.forward;
            Quaternion target = wallRun != null && wallRun.IsRunning
                ? restingRotation * Quaternion.AngleAxis(flipAngle, axis)
                : restingRotation;
            float blend = 1f - Mathf.Exp(-turnSpeed * Time.deltaTime);
            transform.localRotation = Quaternion.Slerp(transform.localRotation, target, blend);
        }

        private void OnValidate()
        {
            if (flipAxis.sqrMagnitude < 0.001f)
                flipAxis = Vector3.forward;
        }
    }
}
