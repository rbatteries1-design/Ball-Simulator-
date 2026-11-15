using UnityEngine;

namespace BounceBack
{
    [RequireComponent(typeof(Collider))]
    public sealed class BouncePad : MonoBehaviour
    {
        [SerializeField, Min(0)] private float launchSpeed = 11f;
        [SerializeField, SceneParticle] private ParticleSystem impactEffect;

        [SerializeField] private AudioClip boingSound;

        private void Awake()
        {
            if (boingSound == null)
                boingSound = Resources.Load<AudioClip>("BounceBack/Boing");
        }

        private void OnCollisionEnter(Collision collision)
        {
            Rigidbody body = collision.rigidbody;
            if (body == null || body.isKinematic)
                return;

            Vector3 normal = transform.up;
            for (int i = 0; i < collision.contactCount; i++)
            {
                ContactPoint contact = collision.GetContact(i);
                if (Vector3.Dot(contact.normal, -normal) < 0.65f)
                    continue;

                float normalSpeed = Vector3.Dot(body.velocity, normal);
                body.AddForce(normal * Mathf.Max(0f, launchSpeed - normalSpeed), ForceMode.VelocityChange);
                if (boingSound != null)
                    AudioSource.PlayClipAtPoint(boingSound, contact.point);
                if (impactEffect != null)
                    impactEffect.Play();
                return;
            }
        }
    }
}
