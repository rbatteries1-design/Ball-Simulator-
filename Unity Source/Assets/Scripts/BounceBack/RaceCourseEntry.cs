using UnityEngine;
using UnityEngine.SceneManagement;

namespace BounceBack
{
    internal sealed class RaceCourseEntry : MonoBehaviour
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Register() => SceneManager.sceneLoaded += Setup;

        private static void Setup(Scene scene, LoadSceneMode mode)
        {
            if (scene.name != "yuh" || FindObjectOfType<BallMotor>() != null)
                return;

            var gate = GameObject.Find("Multiplayer Course/Cube (28)");
            var ball = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            ball.name = "Race Ball";
            ball.transform.position = gate != null ? gate.transform.position + Vector3.up * .35f - Vector3.forward * 2f :
                new Vector3(0f, 1.2f, -4.33f);
            var body = ball.AddComponent<Rigidbody>();
            body.interpolation = RigidbodyInterpolation.Interpolate;
            body.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

            var cameraObject = new GameObject("Race Camera");
            cameraObject.tag = "MainCamera";
            cameraObject.AddComponent<Camera>();
            cameraObject.AddComponent<AudioListener>();
            cameraObject.AddComponent<BallFollowCamera>().SetTarget(ball.transform);
            ball.AddComponent<BallMotor>();
            ball.AddComponent<BallAudio>();
            ball.AddComponent<BounceRace>();
            ball.AddComponent<RaceCourseEntry>();
            body.isKinematic = true;
        }

        private void OnGUI()
        {
            if (GUI.Button(new Rect(12, Screen.height - 45, 130, 32), "Back to Level 1"))
                SceneManager.LoadScene("Bounce Back Level 1");
        }
    }
}
