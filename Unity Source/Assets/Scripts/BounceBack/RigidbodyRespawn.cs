using UnityEngine;

namespace BounceBack
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Collider))]
    public sealed class RigidbodyRespawn : MonoBehaviour
    {
        [Tooltip("Place this transform outside the respawn trigger.")]
        [SerializeField] private Transform respawnPoint;

        [SerializeField] private AudioClip respawnSound;

        private void Reset() => GetComponent<Collider>().isTrigger = true;

        private void Awake()
        {
            if (respawnSound == null)
                respawnSound = Resources.Load<AudioClip>("BounceBack/Respawn");
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!isActiveAndEnabled || respawnPoint == null)
                return;

            Rigidbody body = other.attachedRigidbody;
            if (body == null || body == GetComponentInParent<Rigidbody>())
                return;

            if (!body.isKinematic)
            {
                body.velocity = Vector3.zero;
                body.angularVelocity = Vector3.zero;
            }

            body.position = respawnPoint.position;
            if (respawnSound != null)
                AudioSource.PlayClipAtPoint(respawnSound, respawnPoint.position);
        }
    }
}
