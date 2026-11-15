using UnityEngine;

namespace BounceBack
{
    [RequireComponent(typeof(AudioSource))]
    public sealed class BounceBackGame : MonoBehaviour
    {
        public static BounceBackGame Instance { get; private set; }

        [SerializeField, Min(1)] private int startingLives = 3;
        [SerializeField] private AudioClip victoryClip;
        [SerializeField] private AudioClip lifeLostClip;
        [SerializeField] private AudioClip gameOverClip;
        [SerializeField] private float fallLimit = -15f;

        private AudioSource audioSource;
        private Transform playerTransform;
        private Rigidbody playerBody;
        private Vector3 playerSpawnPosition;
        private int bounceCount;
        private bool levelComplete;
        private bool gameOver;
        private int lives;

        public int BounceCount => bounceCount;
        public bool LevelComplete => levelComplete;
        public bool GameOver => gameOver;
        public int Lives => lives;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            audioSource = GetComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.loop = false;
            audioSource.spatialBlend = 0f;
            lives = Mathf.Max(1, startingLives);
        }

        private void Start()
        {
            FindPlayer();
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.R))
            {
                Restart();
                return;
            }

            if (!levelComplete && !gameOver &&
                playerTransform != null && playerTransform.position.y < fallLimit)
            {
                PlayerFell(playerTransform);
            }
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        public void SetPlayer(BouncePlayerController newPlayer)
        {
            SetPlayer(newPlayer != null ? newPlayer.transform : null);
        }

        public void SetPlayer(Transform newPlayer)
        {
            if (newPlayer == null)
            {
                return;
            }

            playerTransform = newPlayer;
            playerBody = playerTransform.GetComponent<Rigidbody>();
            playerSpawnPosition = playerTransform.position;
        }

        public void RegisterBounce()
        {
            if (!levelComplete && !gameOver)
            {
                bounceCount++;
            }
        }

        public void CompleteLevel()
        {
            CompleteLevel(playerTransform);
        }

        public void CompleteLevel(Transform winner)
        {
            if (levelComplete || gameOver || !IsPlayer(winner))
            {
                return;
            }

            levelComplete = true;
            FreezePlayer();
            PlaySound(victoryClip, "victory");
        }

        public void PlayerFell(Transform fallenPlayer)
        {
            if (levelComplete || gameOver || !IsPlayer(fallenPlayer))
            {
                return;
            }

            lives = Mathf.Max(0, lives - 1);
            if (lives == 0)
            {
                gameOver = true;
                ResetPlayer();
                FreezePlayer();
                PlaySound(gameOverClip, "game over");
                return;
            }

            PlaySound(lifeLostClip, "life lost");
            ResetPlayer();
        }

        public void Restart()
        {
            bounceCount = 0;
            levelComplete = false;
            gameOver = false;
            lives = Mathf.Max(1, startingLives);

            if (playerTransform == null)
            {
                FindPlayer();
            }

            if (playerTransform != null)
            {
                ResetPlayer();
            }
        }

        private void OnGUI()
        {
            GUI.Box(new Rect(16f, 16f, 460f, 104f), "BOUNCE BACK");
            GUI.Label(new Rect(30f, 44f, 290f, 22f), "Move: WASD / arrows    Jump: Space");
            GUI.Label(new Rect(30f, 66f, 410f, 22f), "Lives: " + lives + " / " + Mathf.Max(1, startingLives) + "    Bounces: " + bounceCount);
            GUI.Label(new Rect(30f, 88f, 410f, 22f), "Hold right mouse: orbit    Scroll: zoom    R: restart");

            if (levelComplete)
            {
                GUI.Box(
                    new Rect(Screen.width * 0.5f - 150f, 60f, 300f, 70f),
                    "You made it! Victory! Press R to play again.");
            }
            else if (gameOver)
            {
                GUI.Box(
                    new Rect(Screen.width * 0.5f - 150f, 60f, 300f, 70f),
                    "Game over. Press R to try again.");
            }
        }

        private void FindPlayer()
        {
            BouncePlayerController controller = FindObjectOfType<BouncePlayerController>();
            if (controller != null)
            {
                SetPlayer(controller.transform);
                return;
            }

            global::BounceBall ballController = FindObjectOfType<global::BounceBall>();
            if (ballController != null)
            {
                SetPlayer(ballController.transform);
                return;
            }

            global::BouncyBall bounceStyle = FindObjectOfType<global::BouncyBall>();
            if (bounceStyle != null)
            {
                SetPlayer(bounceStyle.transform);
            }
        }

        private bool IsPlayer(Transform candidate)
        {
            return candidate != null && playerTransform != null &&
                (candidate == playerTransform || candidate.IsChildOf(playerTransform));
        }

        private void ResetPlayer()
        {
            if (playerTransform == null)
            {
                return;
            }

            if (playerBody == null)
            {
                playerBody = playerTransform.GetComponent<Rigidbody>();
            }

            if (playerBody != null)
            {
                playerBody.isKinematic = false;
                playerBody.velocity = Vector3.zero;
                playerBody.angularVelocity = Vector3.zero;
            }

            playerTransform.position = playerSpawnPosition;
            BouncePlayerController controller = playerTransform.GetComponent<BouncePlayerController>();
            if (controller != null)
            {
                controller.SetSpawnPosition(playerSpawnPosition);
                controller.ResetToSpawn();
            }
        }

        private void FreezePlayer()
        {
            if (playerBody == null && playerTransform != null)
            {
                playerBody = playerTransform.GetComponent<Rigidbody>();
            }

            if (playerBody != null)
            {
                playerBody.velocity = Vector3.zero;
                playerBody.angularVelocity = Vector3.zero;
                playerBody.isKinematic = true;
            }
        }

        private void PlaySound(AudioClip clip, string soundName)
        {
            if (clip == null)
            {
                Debug.LogWarning(
                    "Assign the " + soundName + " AudioClip on the Bounce Back Game component to enable this sound.",
                    this);
                return;
            }

            audioSource.PlayOneShot(clip);
        }
    }
}
