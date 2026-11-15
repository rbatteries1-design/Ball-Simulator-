using System.Collections.Generic;
using UnityEngine;

namespace BounceBack
{
    [RequireComponent(typeof(BoxCollider))]
    public sealed class SpeedRing : MonoBehaviour
    {
        [SerializeField, Min(0)] private float exitSpeed = 18f;
        [SerializeField] private bool useRingForward = true;
        [SerializeField, SceneParticle] private ParticleSystem passEffect;
        private readonly Dictionary<Rigidbody, int> occupants = new Dictionary<Rigidbody, int>();

        private void Reset() => GetComponent<BoxCollider>().isTrigger = true;
        private void OnDisable() => occupants.Clear();

        private void OnTriggerEnter(Collider other)
        {
            Rigidbody body = other.attachedRigidbody;
            if (!isActiveAndEnabled || body == null || body.isKinematic)
                return;

            occupants.TryGetValue(body, out int count);
            occupants[body] = count + 1;
            if (count != 0)
                return;

            Vector3 direction = useRingForward || body.velocity.sqrMagnitude < 0.001f
                ? transform.forward : body.velocity.normalized;
            body.velocity = direction * exitSpeed;
            if (passEffect != null)
                passEffect.Play();
        }

        private void OnTriggerExit(Collider other)
        {
            Rigidbody body = other.attachedRigidbody;
            if (body == null || !occupants.TryGetValue(body, out int count))
                return;

            if (count <= 1)
                occupants.Remove(body);
            else
                occupants[body] = count - 1;
        }
    }
}
