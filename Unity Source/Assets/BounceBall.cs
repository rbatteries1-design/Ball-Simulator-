using UnityEngine;

[RequireComponent(typeof(SphereCollider))]
[RequireComponent(typeof(Rigidbody))]
public class BounceBall : MonoBehaviour
{
    [SerializeField] private Camera cam;
    [SerializeField] private float torqueForce = 18f;
    [SerializeField] private float airControl = 6f;
    [SerializeField] private float maxAngularSpeed = 40f;
    [SerializeField] private float bounciness = 0.85f;
    [SerializeField] private float jumpForce = 7f;
    [SerializeField] private float camDistance = 6f;
    [SerializeField] private float camHeightOffset = 0.5f;
    [SerializeField] private float lookSensitivity = 3f;
    [SerializeField] private float minPitch = -30f;
    [SerializeField] private float maxPitch = 80f;
    [SerializeField] private float camSmoothing = 14f;
    [SerializeField] private LayerMask camCollisionMask = ~0;

    private Rigidbody body;
    private SphereCollider col;
    private float yaw;
    private float pitch = 20f;
    private bool grounded;
    private Vector3 groundNormal = Vector3.up;
    private Vector3 camVelocity;

    private void Awake()
    {
        body = GetComponent<Rigidbody>();
        col = GetComponent<SphereCollider>();

        body.mass = 1f;
        body.angularDrag = 0.3f;
        body.drag = 0.05f;
        body.maxAngularVelocity = maxAngularSpeed;
        body.interpolation = RigidbodyInterpolation.Interpolate;
        body.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

        PhysicMaterial mat = new PhysicMaterial("BallMat")
        {
            bounciness = bounciness,
            dynamicFriction = 0.6f,
            staticFriction = 0.6f,
            bounceCombine = PhysicMaterialCombine.Maximum,
            frictionCombine = PhysicMaterialCombine.Average
        };
        col.material = mat;

        if (cam == null) cam = Camera.main;

        Vector3 e = cam.transform.eulerAngles;
        yaw = e.y;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        yaw += Input.GetAxis("Mouse X") * lookSensitivity;
        pitch -= Input.GetAxis("Mouse Y") * lookSensitivity;
        pitch = Mathf.Clamp(pitch, minPitch, maxPitch);

        if (Input.GetKeyDown(KeyCode.Space) && grounded)
        {
            Vector3 v = body.velocity;
            v.y = 0f;
            body.velocity = v;
            body.AddForce(groundNormal * jumpForce, ForceMode.Impulse);
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            bool locked = Cursor.lockState == CursorLockMode.Locked;
            Cursor.lockState = locked ? CursorLockMode.None : CursorLockMode.Locked;
            Cursor.visible = locked;
        }
    }

    private void FixedUpdate()
    {
        Vector3 input = new Vector3(Input.GetAxisRaw("Horizontal"), 0f, Input.GetAxisRaw("Vertical"));
        input = Vector3.ClampMagnitude(input, 1f);

        Quaternion yawRot = Quaternion.Euler(0f, yaw, 0f);
        Vector3 moveDir = yawRot * input;

        if (moveDir.sqrMagnitude > 0.0001f)
        {
            Vector3 torqueAxis = Vector3.Cross(Vector3.up, moveDir).normalized;
            body.AddTorque(torqueAxis * torqueForce, ForceMode.Acceleration);

            if (!grounded)
                body.AddForce(moveDir * airControl, ForceMode.Acceleration);
        }

        grounded = false;
    }

    private void OnCollisionStay(Collision collision)
    {
        for (int i = 0; i < collision.contactCount; i++)
        {
            ContactPoint p = collision.GetContact(i);
            if (p.normal.y > 0.5f)
            {
                grounded = true;
                groundNormal = p.normal;
                return;
            }
        }
    }

    private void LateUpdate()
    {
        Quaternion rot = Quaternion.Euler(pitch, yaw, 0f);
        Vector3 focus = transform.position + Vector3.up * camHeightOffset;
        Vector3 desired = focus - rot * Vector3.forward * camDistance;

        Vector3 dir = desired - focus;
        float dist = dir.magnitude;
        if (Physics.SphereCast(focus, 0.25f, dir.normalized, out RaycastHit hit, dist, camCollisionMask, QueryTriggerInteraction.Ignore))
        {
            if (hit.collider != col)
                desired = focus + dir.normalized * Mathf.Max(hit.distance - 0.1f, 0.2f);
        }

        cam.transform.position = Vector3.SmoothDamp(cam.transform.position, desired, ref camVelocity, 1f / camSmoothing);
        cam.transform.rotation = Quaternion.LookRotation(focus - cam.transform.position, Vector3.up);
    }
}
