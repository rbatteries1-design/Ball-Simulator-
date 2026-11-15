using UnityEngine;
using UnityEngine.Events;

namespace BounceBack
{
    [RequireComponent(typeof(BoxCollider))]
    public sealed class WinPad : MonoBehaviour
    {
        [SerializeField] private AudioClip victorySound;
        [SerializeField] private UnityEvent onWin = new UnityEvent();
        private bool completed;

        private void Reset() => GetComponent<BoxCollider>().isTrigger = true;

        private void Awake()
        {
            if (victorySound == null)
                victorySound = Resources.Load<AudioClip>("BounceBack/Victory");
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!isActiveAndEnabled || completed || other.attachedRigidbody == null ||
                !other.attachedRigidbody.TryGetComponent(out BallMotor ball))
                return;

            if (ball.TryGetComponent(out BounceRace race) && race.InRace)
            {
                race.Finish();
                return;
            }

            completed = true;
            if (ball.TryGetComponent(out BallAudio audio))
                audio.Victory();
            if (victorySound != null)
                AudioSource.PlayClipAtPoint(victorySound, transform.position);
            if (ball.TryGetComponent(out BounceCampaign campaign))
                campaign.Complete(victorySound != null ? victorySound.length : 0f);
            onWin.Invoke();
        }

        public void ResetWin() => completed = false;
    }
}
