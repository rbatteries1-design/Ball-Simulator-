using UnityEngine;

namespace BounceBack
{
    public sealed class BallAudio : MonoBehaviour
    {
        [SerializeField] private AudioClip theme;
        [SerializeField, Range(0, 1)] private float themeVolume = 0.55f;
        private AudioSource music;

        private void Awake()
        {
            music = gameObject.AddComponent<AudioSource>();
            music.loop = true;
            music.volume = themeVolume;
            PlayTheme();
        }

        public void PlayTheme() => Play(theme);
        public void PlayRaceTheme() => Play(Resources.Load<AudioClip>("BounceBack/Race Start"));

        private void Play(AudioClip clip)
        {
            music.Stop();
            music.clip = clip;
            if (clip != null)
                music.Play();
        }

        public void Victory()
        {
            if (music != null)
                music.Stop();
        }
    }
}
