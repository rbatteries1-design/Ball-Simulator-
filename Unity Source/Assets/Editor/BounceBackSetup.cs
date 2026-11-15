using UnityEditor;
using UnityEngine;

namespace BounceBack
{
    internal static class BounceBackSetup
    {
        [MenuItem("Tools/Bounce Back/Create Player and Camera")]
        private static void CreatePlayer()
        {
            int group = Undo.GetCurrentGroup();
            Undo.SetCurrentGroupName("Create Bounce Back Player");

            var ball = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            ball.name = "Bounce Back Ball";
            var view = SceneView.lastActiveSceneView;
            ball.transform.position = (view != null ? view.pivot : Vector3.zero) + Vector3.up;
            Undo.RegisterCreatedObjectUndo(ball, "Create Ball");
            var body = Undo.AddComponent<Rigidbody>(ball);
            body.interpolation = RigidbodyInterpolation.Interpolate;
            body.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            var motor = Undo.AddComponent<BallMotor>(ball);
            Undo.AddComponent<BallAudio>(ball);

            var camera = Camera.main;
            if (camera == null)
            {
                var cameraObject = new GameObject("Bounce Back Camera");
                cameraObject.tag = "MainCamera";
                Undo.RegisterCreatedObjectUndo(cameraObject, "Create Camera");
                camera = Undo.AddComponent<Camera>(cameraObject);
                Undo.AddComponent<AudioListener>(cameraObject);
            }

            var follow = camera.GetComponent<BallFollowCamera>();
            if (follow == null)
                follow = Undo.AddComponent<BallFollowCamera>(camera.gameObject);

            var cameraSettings = new SerializedObject(follow);
            cameraSettings.FindProperty("target").objectReferenceValue = ball.transform;
            cameraSettings.ApplyModifiedProperties();

            var motorSettings = new SerializedObject(motor);
            motorSettings.FindProperty("view").objectReferenceValue = camera.transform;
            motorSettings.ApplyModifiedProperties();

            Selection.activeGameObject = ball;
            Undo.CollapseUndoOperations(group);
        }
    }
}
