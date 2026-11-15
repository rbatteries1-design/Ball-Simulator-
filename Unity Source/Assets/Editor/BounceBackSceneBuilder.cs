using System.IO;
using BounceBack;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace BounceBack.Editor
{
    public static class BounceBackSceneBuilder
    {
        public const string DemoScenePath = "Assets/Scenes/BounceBackDemo.unity";

        [MenuItem("Bounce Back/Setup Current Scene Lives and Audio")]
        public static void SetupCurrentSceneLivesAndAudio()
        {
            BounceBackGame game = Object.FindObjectOfType<BounceBackGame>();
            if (game == null)
            {
                GameObject managerObject = new GameObject("Bounce Back Game");
                Undo.RegisterCreatedObjectUndo(managerObject, "Add Bounce Back Game");
                game = managerObject.AddComponent<BounceBackGame>();
            }

            BouncePlayerController controller = Object.FindObjectOfType<BouncePlayerController>();
            if (controller != null)
            {
                game.SetPlayer(controller.transform);
            }
            else
            {
                global::BounceBall ballController = Object.FindObjectOfType<global::BounceBall>();
                if (ballController != null)
                {
                    game.SetPlayer(ballController.transform);
                }
                else
                {
                    global::BouncyBall bounceStyle = Object.FindObjectOfType<global::BouncyBall>();
                    if (bounceStyle != null)
                    {
                        game.SetPlayer(bounceStyle.transform);
                    }
                    else
                    {
                        Debug.LogWarning("No ball controller was found. Add BounceBall, BouncyBall, or BouncePlayerController to the player.");
                    }
                }
            }

            AudioSource audioSource = game.GetComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 0f;
            Selection.activeGameObject = game.gameObject;
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            Debug.Log("Lives and audio manager added. Assign the AudioClips and add BounceGoal to your goal trigger.");
        }

        [MenuItem("Bounce Back/Add Goal Trigger to Selected Object")]
        public static void AddGoalTriggerToSelection()
        {
            GameObject selectedObject = Selection.activeGameObject;
            if (selectedObject == null)
            {
                return;
            }

            Renderer goalRenderer = selectedObject.GetComponentInChildren<Renderer>();
            if (goalRenderer == null)
            {
                throw new System.InvalidOperationException(
                    "Select a goal object with a Renderer to create its trigger.");
            }

            GameObject triggerObject = new GameObject(selectedObject.name + " Goal Trigger");
            Undo.RegisterCreatedObjectUndo(triggerObject, "Create Bounce Back Goal Trigger");
            triggerObject.transform.position = goalRenderer.bounds.center;

            BoxCollider trigger = triggerObject.AddComponent<BoxCollider>();
            trigger.isTrigger = true;
            trigger.size = goalRenderer.bounds.size;
            triggerObject.AddComponent<BounceGoal>();

            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        }

        [MenuItem("Bounce Back/Add Goal Trigger to Selected Object", true)]
        private static bool CanAddGoalTriggerToSelection()
        {
            return Selection.activeGameObject != null &&
                Selection.activeGameObject.GetComponentInChildren<Renderer>() != null;
        }

        [MenuItem("Bounce Back/Create or Reset Demo Scene")]
        public static void CreateOrResetDemoScene()
        {
            if (File.Exists(GetAbsoluteDemoScenePath()) &&
                !EditorUtility.DisplayDialog(
                    "Reset Bounce Back Demo",
                    "This replaces Assets/Scenes/BounceBackDemo.unity with the generated demo scene.",
                    "Reset Scene",
                    "Cancel"))
            {
                return;
            }

            CreateDemoSceneAsset();
            EditorSceneManager.OpenScene(DemoScenePath, OpenSceneMode.Single);
            Debug.Log("Bounce Back demo scene is ready. Press Play to test it.");
        }

        public static string EnsureDemoSceneAsset()
        {
            if (!File.Exists(GetAbsoluteDemoScenePath()))
            {
                CreateDemoSceneAsset();
            }

            return DemoScenePath;
        }

        private static void CreateDemoSceneAsset()
        {
            Directory.CreateDirectory(Path.GetDirectoryName(GetAbsoluteDemoScenePath()));
            EnsureMaterialFolder();

            Scene previousScene = SceneManager.GetActiveScene();
            Scene demoScene = EditorSceneManager.NewScene(
                NewSceneSetup.EmptyScene,
                NewSceneMode.Additive);

            try
            {
                SceneManager.SetActiveScene(demoScene);
                BuildWorld();

                if (!EditorSceneManager.SaveScene(demoScene, DemoScenePath))
                {
                    throw new IOException("Unity could not save the Bounce Back demo scene.");
                }
            }
            finally
            {
                EditorSceneManager.CloseScene(demoScene, true);
                if (previousScene.IsValid() && previousScene.isLoaded)
                {
                    SceneManager.SetActiveScene(previousScene);
                }
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private static string GetAbsoluteDemoScenePath()
        {
            string projectRoot = Directory.GetParent(Application.dataPath).FullName;
            string relativePath = DemoScenePath.Replace('/', Path.DirectorySeparatorChar);
            return Path.Combine(projectRoot, relativePath);
        }

        private static void BuildWorld()
        {
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.34f, 0.39f, 0.52f);

            CreateLighting();
            Material ground = GetMaterial("Ground", new Color(0.12f, 0.16f, 0.25f));
            Material platform = GetMaterial("Platform", new Color(0.22f, 0.31f, 0.46f));
            Material trampoline = GetMaterial("Trampoline", new Color(0.15f, 0.9f, 0.76f));
            Material ballMaterial = GetMaterial("Ball", new Color(1f, 0.39f, 0.31f));
            Material goalMaterial = GetMaterial("Goal", new Color(1f, 0.78f, 0.22f));

            CreateBlock("Start Platform", new Vector3(0f, -0.5f, 0f), new Vector3(12f, 1f, 18f), ground);

            CreateBouncePad("First Trampoline", new Vector3(0f, 0.25f, 2f), trampoline, 15f);
            CreateBouncePad("Second Trampoline", new Vector3(3f, 3.6f, 13f), trampoline, 15.5f);
            CreateBouncePad("Third Trampoline", new Vector3(-3f, 7f, 24f), trampoline, 16f);

            CreateBlock("Goal Platform", new Vector3(0f, 10.1f, 35f), new Vector3(8f, 0.8f, 9f), platform);
            CreateGoal(new Vector3(0f, 11.2f, 37f), goalMaterial);

            GameObject playerObject = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            playerObject.name = "Bounce Ball";
            playerObject.transform.position = new Vector3(0f, 1.6f, -5f);
            playerObject.GetComponent<Renderer>().sharedMaterial = ballMaterial;

            Rigidbody body = playerObject.AddComponent<Rigidbody>();
            body.mass = 1f;
            body.drag = 0.05f;
            body.angularDrag = 0.08f;
            playerObject.AddComponent<BouncyBall>();

            BouncePlayerController controller = playerObject.AddComponent<BouncePlayerController>();
            controller.SetSpawnPosition(playerObject.transform.position);

            GameObject managerObject = new GameObject("Bounce Back Game");
            BounceBackGame manager = managerObject.AddComponent<BounceBackGame>();
            manager.SetPlayer(controller);
            AudioSource audioSource = managerObject.GetComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 0f;

            GameObject cameraObject = new GameObject("Main Camera");
            cameraObject.tag = "MainCamera";
            Camera camera = cameraObject.AddComponent<Camera>();
            camera.fieldOfView = 60f;
            camera.nearClipPlane = 0.1f;
            camera.farClipPlane = 250f;
            cameraObject.AddComponent<AudioListener>();
            BounceCameraFollow cameraFollow = cameraObject.AddComponent<BounceCameraFollow>();
            cameraFollow.SetTarget(playerObject.transform);
            cameraObject.transform.position = playerObject.transform.position + new Vector3(0f, 9f, -12f);
            cameraObject.transform.LookAt(playerObject.transform.position + Vector3.up * 0.5f);
        }

        private static void CreateLighting()
        {
            GameObject lightObject = new GameObject("Sun");
            Light light = lightObject.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.2f;
            light.color = new Color(0.78f, 0.86f, 1f);
            lightObject.transform.rotation = Quaternion.Euler(45f, -35f, 0f);
        }

        private static GameObject CreateBlock(string name, Vector3 position, Vector3 scale, Material material)
        {
            GameObject block = GameObject.CreatePrimitive(PrimitiveType.Cube);
            block.name = name;
            block.transform.position = position;
            block.transform.localScale = scale;
            block.GetComponent<Renderer>().sharedMaterial = material;
            return block;
        }

        private static void CreateBouncePad(
            string name,
            Vector3 position,
            Material material,
            float bounceVelocity)
        {
            GameObject pad = CreateBlock(name, position, new Vector3(3.4f, 0.5f, 3.4f), material);
            GameObject spring = GameObject.CreatePrimitive(PrimitiveType.Cube);
            spring.name = "Spring Top";
            spring.transform.SetParent(pad.transform, false);
            spring.transform.localPosition = new Vector3(0f, 0.54f, 0f);
            spring.transform.localScale = new Vector3(0.92f, 0.12f, 0.92f);
            spring.GetComponent<Renderer>().sharedMaterial = material;
            Object.DestroyImmediate(spring.GetComponent<Collider>());

            BouncePad bouncePad = pad.AddComponent<BouncePad>();
            SerializedObject bouncePadSettings = new SerializedObject(bouncePad);
            bouncePadSettings.FindProperty("launchSpeed").floatValue = bounceVelocity;
            bouncePadSettings.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void CreateGoal(Vector3 position, Material material)
        {
            GameObject marker = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            marker.name = "Bounce Back Goal";
            marker.transform.position = position;
            marker.transform.localScale = new Vector3(2f, 0.12f, 2f);
            marker.GetComponent<Renderer>().sharedMaterial = material;

            GameObject triggerObject = new GameObject("Goal Trigger");
            triggerObject.transform.position = position + Vector3.up * 1.2f;
            BoxCollider trigger = triggerObject.AddComponent<BoxCollider>();
            trigger.isTrigger = true;
            trigger.size = new Vector3(4f, 3f, 4f);
            triggerObject.AddComponent<BounceGoal>();
        }

        private static void EnsureMaterialFolder()
        {
            if (!AssetDatabase.IsValidFolder("Assets/Materials"))
            {
                AssetDatabase.CreateFolder("Assets", "Materials");
            }

            if (!AssetDatabase.IsValidFolder("Assets/Materials/BounceBack"))
            {
                AssetDatabase.CreateFolder("Assets/Materials", "BounceBack");
            }
        }

        private static Material GetMaterial(string name, Color color)
        {
            string path = "Assets/Materials/BounceBack/" + name + ".mat";
            Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material != null)
            {
                return material;
            }

            Shader shader = Shader.Find("Standard");
            if (shader == null)
            {
                throw new System.InvalidOperationException("Unity's Standard shader could not be found.");
            }

            material = new Material(shader);
            material.name = name;
            material.color = color;
            AssetDatabase.CreateAsset(material, path);
            return material;
        }
    }
}
