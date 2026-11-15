using UnityEngine;

namespace BounceBack
{
    [RequireComponent(typeof(Collider))]
    public sealed class BounceGoal : MonoBehaviour
    {
        private void Reset()
        {
            GetComponent<Collider>().isTrigger = true;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (BounceBackGame.Instance != null)
            {
                BounceBackGame.Instance.CompleteLevel(other.transform);
            }
        }
    }
}
