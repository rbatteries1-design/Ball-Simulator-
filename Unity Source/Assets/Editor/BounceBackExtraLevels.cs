using System.Collections.Generic;
using BounceBack;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace BounceBack.Editor
{
    internal static class BounceBackExtraLevels
    {
        private const string SceneFolder = "Assets/Scenes";
        private const string MaterialFolder = "Assets/Materials/GeneratedLevels";
        private static Scene previousScene;

        private sealed class Palette
        {
            public Material floor;
            public Material trim;
            public Material wall;
            public Material bright;
            public Material warm;
            public Material ball;
            public Material ballStripe;
            public Material tower;
        }

        [MenuItem("Bounce Back/Build Playable Levels 4 and 5")]
        private static void BuildLevels()
        {
            EnsureFolders();
            Palette levelFour = MakePalette("Level4", new Color(.04f, .07f, .16f),
                new Color(.08f, .9f, 1f), new Color(1f, .18f, .68f),
                new Color(1f, .61f, .18f), new Color(.95f, .22f, .12f));
            Palette levelFive = MakePalette("Level5", new Color(.045f, .055f, .14f),
                new Color(.3f, 1f, .7f), new Color(.36f, .57f, 1f),
                new Color(1f, .75f, .2f), new Color(.72f, .27f, 1f));

            CreateLevelFour(levelFour);
            CreateLevelFive(levelFive);
            AddToBuildSettings();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("Levels 4 and 5 are ready. Both are in Build Settings.");
        }

        private static void CreateLevelFour(Palette colors)
        {
            Scene scene = BeginLevelScene();
            SetupScene(new Color(.13f, .16f, .31f), new Color(.035f, .055f, .14f));
            AddStartAndFinish(colors, 4, new Vector3(0f, .55f, 0f), new Vector3(0f, 3.1f, 120f));

            Platform("Launch Deck", new Vector3(0f, -.5f, 4f), new Vector3(14f, 1f, 20f), colors);
            WallRunWall("Cyan Wall Run", new Vector3(5.35f, 2.5f, 23f), new Vector3(.8f, 6f, 24f), colors);
            Platform("Catch Deck", new Vector3(0f, -.5f, 39f), new Vector3(14f, 1f, 16f), colors);
            BouncePad("Pulse Launcher", new Vector3(0f, .21f, 42f), new Vector3(3f, .42f, 3f), 11f, colors.bright);

            Platform("Upper Transfer", new Vector3(0f, 2.5f, 61f), new Vector3(16f, 1f, 18f), colors);
            WallRunWall("Violet Wall Run", new Vector3(-5.35f, 5.5f, 82f), new Vector3(.8f, 6f, 24f), colors);
            Platform("Exit Deck", new Vector3(0f, 2.5f, 101f), new Vector3(16f, 1f, 20f), colors);
            BouncePad("Final Pulse", new Vector3(0f, 3.21f, 104f), new Vector3(3.2f, .42f, 3.2f), 11f, colors.warm);
            Platform("Finish Deck", new Vector3(0f, 2.5f, 121f), new Vector3(16f, 1f, 18f), colors);

            Sign("LEVEL 04  /  NEON SWITCHBACK", new Vector3(0f, 3.4f, -4.5f), colors.bright);
            Sign("W + D TO CATCH THE CYAN WALL     SPACE TO KICK OFF", new Vector3(0f, 2.1f, -4.5f), colors.warm, .11f);
            GoalArch("EXIT 04", new Vector3(0f, 3.1f, 120f), colors.warm);
            AddCityLights(colors, new[] { 14f, 35f, 56f, 76f, 97f, 118f }, 14f);
            AddFallRespawn(new Vector3(0f, .55f, 0f));
            SaveScene(scene, 4);
        }

        private static void CreateLevelFive(Palette colors)
        {
            Scene scene = BeginLevelScene();
            SetupScene(new Color(.11f, .15f, .31f), new Color(.025f, .045f, .13f));
            AddStartAndFinish(colors, 5, new Vector3(0f, .55f, 0f), new Vector3(0f, 6.55f, 128f));

            Platform("Aurora Start", new Vector3(0f, -.5f, 4f), new Vector3(16f, 1f, 20f), colors);
            WallRunWall("Mint Wall Run", new Vector3(6f, 2.5f, 23f), new Vector3(.9f, 6f, 26f), colors);
            Platform("First Catch Deck", new Vector3(0f, -.5f, 40f), new Vector3(14f, 1f, 16f), colors);
            BouncePad("Aurora Launcher", new Vector3(0f, .21f, 43f), new Vector3(3.2f, .42f, 3.2f), 13f, colors.warm);

            Platform("High Switchyard", new Vector3(0f, 4f, 64f), new Vector3(18f, 1f, 22f), colors);
            WallRunWall("Blue Wall Run", new Vector3(-6f, 7f, 87f), new Vector3(.9f, 6f, 26f), colors);
            Platform("Second Catch Deck", new Vector3(0f, 4f, 105f), new Vector3(16f, 1f, 18f), colors);
            BouncePad("Last Lift", new Vector3(0f, 4.71f, 108f), new Vector3(3.4f, .42f, 3.4f), 12f, colors.bright);
            Platform("Aurora Crown", new Vector3(0f, 6f, 128f), new Vector3(18f, 1f, 22f), colors);

            Sign("LEVEL 05  /  AURORA SWITCHYARD", new Vector3(0f, 3.4f, -4.5f), colors.bright);
            Sign("CHASE THE GLOW. TWO WALLS. TWO CLEAN KICK-OFFS.", new Vector3(0f, 2.1f, -4.5f), colors.warm, .11f);
            GoalArch("FINAL EXIT", new Vector3(0f, 6.6f, 128f), colors.warm);
            AddCityLights(colors, new[] { 14f, 36f, 60f, 83f, 103f, 126f }, 18f);
            AddFallRespawn(new Vector3(0f, .55f, 0f));
            SaveScene(scene, 5);
        }

        private static void AddStartAndFinish(Palette colors, int level, Vector3 spawn, Vector3 goal)
        {
            GameObject player = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            player.name = "Bounce Ball";
            player.transform.position = spawn;
            Object.DestroyImmediate(player.GetComponent<MeshRenderer>());

            Rigidbody body = player.AddComponent<Rigidbody>();
            body.mass = 1f;
            body.drag = .05f;
            body.angularDrag = .08f;
            body.interpolation = RigidbodyInterpolation.Interpolate;
            body.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

            GameObject cameraObject = new GameObject("Bounce Back Camera");
            cameraObject.tag = "MainCamera";
            Camera camera = cameraObject.AddComponent<Camera>();
            camera.fieldOfView = 60f;
            cameraObject.AddComponent<AudioListener>();
            BallFollowCamera follow = cameraObject.AddComponent<BallFollowCamera>();
            follow.SetTarget(player.transform);
            cameraObject.transform.position = spawn + new Vector3(0f, 4f, -8f);

            BallMotor motor = player.AddComponent<BallMotor>();
            SerializedObject motorSettings = new SerializedObject(motor);
            motorSettings.FindProperty("view").objectReferenceValue = cameraObject.transform;
            motorSettings.FindProperty("driveSpeed").floatValue = 9f;
            motorSettings.ApplyModifiedPropertiesWithoutUndo();
            player.AddComponent<BallAudio>();
            BounceCampaign campaign = player.AddComponent<BounceCampaign>();
            SerializedObject campaignSettings = new SerializedObject(campaign);
            campaignSettings.FindProperty("level").intValue = level;
            campaignSettings.ApplyModifiedPropertiesWithoutUndo();

            WallRun wallRun = player.AddComponent<WallRun>();
            SerializedObject wallRunSettings = new SerializedObject(wallRun);
            wallRunSettings.FindProperty("view").objectReferenceValue = cameraObject.transform;
            wallRunSettings.ApplyModifiedPropertiesWithoutUndo();

            GameObject visual = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            visual.name = "Ball Preview";
            visual.transform.SetParent(player.transform, false);
            visual.GetComponent<Collider>().enabled = false;
            visual.GetComponent<Renderer>().sharedMaterial = colors.ball;
            GameObject stripe = Block("Preview Stripe", visual.transform, new Vector3(0f, 0f, -.46f),
                new Vector3(.12f, .42f, .06f), colors.ballStripe, false);
            stripe.transform.localRotation = Quaternion.identity;
            visual.AddComponent<WallRunPreview>();

            GameObject goalPad = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            goalPad.name = "Goal Pad";
            goalPad.transform.position = goal;
            goalPad.transform.localScale = new Vector3(4f, .15f, 4f);
            goalPad.GetComponent<Renderer>().sharedMaterial = colors.warm;
            Object.DestroyImmediate(goalPad.GetComponent<Collider>());
            GameObject trigger = new GameObject("Level Goal");
            trigger.transform.position = goal + Vector3.up;
            BoxCollider goalCollider = trigger.AddComponent<BoxCollider>();
            goalCollider.isTrigger = true;
            goalCollider.size = new Vector3(6f, 4f, 6f);
            trigger.AddComponent<WinPad>();
        }

        private static void SetupScene(Color fog, Color ambient)
        {
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
            RenderSettings.ambientLight = ambient;
            RenderSettings.fog = true;
            RenderSettings.fogColor = fog;
            RenderSettings.fogMode = FogMode.Exponential;
            RenderSettings.fogDensity = .0035f;

            GameObject sun = new GameObject("Cool White Key Light");
            Light light = sun.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.25f;
            light.color = new Color(.72f, .84f, 1f);
            sun.transform.rotation = Quaternion.Euler(48f, -28f, 0f);
        }

        private static void Platform(string name, Vector3 center, Vector3 size, Palette colors)
        {
            Block(name, null, center, size, colors.floor, true);
            float top = center.y + size.y * .5f + .045f;
            Block(name + " edge glow", null, new Vector3(center.x, top, center.z),
                new Vector3(size.x - .2f, .08f, size.z - .2f), colors.trim, false);

            for (int i = 0; i < 4; i++)
            {
                float stripeZ = center.z - size.z * .5f + 1.1f + i * 1.5f;
                Block(name + " deck marking", null, new Vector3(center.x, top + .012f, stripeZ),
                    new Vector3(size.x * .72f, .018f, .12f), i % 2 == 0 ? colors.bright : colors.warm, false);
            }
        }

        private static void WallRunWall(string name, Vector3 center, Vector3 size, Palette colors)
        {
            GameObject wall = Block(name, null, center, size, colors.wall, true);
            wall.AddComponent<WallRunSurface>();

            float x = center.x + Mathf.Sign(center.x) * (size.x * .5f + .08f);
            for (int i = -2; i <= 2; i++)
            {
                Block(name + " light rail", null, new Vector3(x, center.y, center.z + i * 3.5f),
                    new Vector3(.08f, size.y - .25f, .18f), i % 2 == 0 ? colors.bright : colors.trim, false);
            }
            Block(name + " crown", null, new Vector3(center.x, center.y + size.y * .5f + .12f, center.z),
                new Vector3(size.x + .2f, .18f, size.z + .3f), colors.trim, false);
        }

        private static void BouncePad(string name, Vector3 center, Vector3 size, float launch, Material material)
        {
            GameObject pad = GameObject.CreatePrimitive(PrimitiveType.Cube);
            pad.name = name;
            pad.transform.position = center;
            pad.transform.localScale = size;
            pad.GetComponent<Renderer>().sharedMaterial = material;
            BouncePad bounce = pad.AddComponent<BouncePad>();
            SerializedObject settings = new SerializedObject(bounce);
            settings.FindProperty("launchSpeed").floatValue = launch;
            settings.ApplyModifiedPropertiesWithoutUndo();

            Block(name + " center light", pad.transform, new Vector3(0f, .24f, 0f),
                new Vector3(size.x * .65f, .08f, size.z * .65f), material, false);
        }

        private static void GoalArch(string label, Vector3 center, Material color)
        {
            float top = center.y + 4f;
            Block("Exit frame left", null, center + new Vector3(-3.4f, 1.9f, 0f),
                new Vector3(.34f, 4f, .45f), color, false);
            Block("Exit frame right", null, center + new Vector3(3.4f, 1.9f, 0f),
                new Vector3(.34f, 4f, .45f), color, false);
            Block("Exit frame top", null, new Vector3(center.x, top, center.z),
                new Vector3(7.1f, .34f, .45f), color, false);
            Sign(label, center + new Vector3(0f, 4.75f, 0f), color, .13f);
        }

        private static void Sign(string text, Vector3 position, Material color, float size = .17f)
        {
            GameObject sign = new GameObject("Course Sign");
            sign.transform.position = position;
            TextMesh mesh = sign.AddComponent<TextMesh>();
            mesh.text = text;
            mesh.characterSize = size;
            mesh.fontSize = 48;
            mesh.anchor = TextAnchor.MiddleCenter;
            mesh.alignment = TextAlignment.Center;
            mesh.color = color.color;
        }

        private static void AddCityLights(Palette colors, float[] positions, float outerX)
        {
            for (int i = 0; i < positions.Length; i++)
            {
                float z = positions[i];
                for (int side = -1; side <= 1; side += 2)
                {
                    float x = side * outerX;
                    float height = 4.5f + (i % 3) * 1.7f;
                    Block("Skyline tower", null, new Vector3(x, height * .5f - .5f, z),
                        new Vector3(1.2f + (i % 2) * .5f, height, 1.2f), colors.tower, false);
                    Block("Skyline beacon", null, new Vector3(x, height, z),
                        new Vector3(1.35f, .16f, 1.35f), i % 2 == 0 ? colors.trim : colors.warm, false);
                    PointLight("Skyline glow", new Vector3(x, height - .5f, z), i % 2 == 0 ? colors.bright : colors.warm);
                }
            }
        }

        private static void PointLight(string name, Vector3 position, Material material)
        {
            GameObject lightObject = new GameObject(name);
            lightObject.transform.position = position;
            Light light = lightObject.AddComponent<Light>();
            light.type = LightType.Point;
            light.color = material.color;
            light.intensity = 2.2f;
            light.range = 9f;
        }

        private static void AddFallRespawn(Vector3 spawn)
        {
            GameObject safety = new GameObject("Fall Reset");
            safety.transform.position = new Vector3(0f, -18f, 62f);
            BoxCollider trigger = safety.AddComponent<BoxCollider>();
            trigger.isTrigger = true;
            trigger.size = new Vector3(180f, 1f, 240f);
            RigidbodyRespawn respawn = safety.AddComponent<RigidbodyRespawn>();

            GameObject point = new GameObject("Respawn Point");
            point.transform.position = spawn;
            SerializedObject settings = new SerializedObject(respawn);
            settings.FindProperty("respawnPoint").objectReferenceValue = point.transform;
            settings.ApplyModifiedPropertiesWithoutUndo();
        }

        private static GameObject Block(string name, Transform parent, Vector3 position,
            Vector3 scale, Material material, bool collider)
        {
            GameObject block = GameObject.CreatePrimitive(PrimitiveType.Cube);
            block.name = name;
            if (parent != null)
                block.transform.SetParent(parent, false);
            if (parent == null)
                block.transform.position = position;
            else
                block.transform.localPosition = position;
            block.transform.localScale = scale;
            block.GetComponent<Renderer>().sharedMaterial = material;
            if (!collider)
                Object.DestroyImmediate(block.GetComponent<Collider>());
            return block;
        }

        private static Palette MakePalette(string prefix, Color floorColor, Color trimColor,
            Color brightColor, Color warmColor, Color ballColor)
        {
            return new Palette
            {
                floor = MakeMaterial(prefix + " Floor", floorColor, Color.black),
                trim = MakeMaterial(prefix + " Trim", trimColor, trimColor * 1.6f),
                wall = MakeMaterial(prefix + " Wall", floorColor * 1.7f, brightColor * 1.2f),
                bright = MakeMaterial(prefix + " Cyan", brightColor, brightColor * 1.8f),
                warm = MakeMaterial(prefix + " Warm", warmColor, warmColor * 1.6f),
                ball = MakeMaterial(prefix + " Ball", ballColor, ballColor * .35f),
                ballStripe = MakeMaterial(prefix + " Ball Stripe", Color.white, Color.white),
                tower = MakeMaterial(prefix + " Tower", floorColor * .8f, brightColor * .25f),
            };
        }

        private static Material MakeMaterial(string name, Color baseColor, Color emission)
        {
            string path = MaterialFolder + "/" + name + ".mat";
            Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null)
            {
                Shader shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
                material = new Material(shader) { name = name };
                AssetDatabase.CreateAsset(material, path);
            }

            material.color = baseColor;
            if (material.HasProperty("_EmissionColor"))
            {
                material.EnableKeyword("_EMISSION");
                material.SetColor("_EmissionColor", emission);
            }
            EditorUtility.SetDirty(material);
            return material;
        }

        private static void EnsureFolders()
        {
            if (!AssetDatabase.IsValidFolder("Assets/Materials"))
                AssetDatabase.CreateFolder("Assets", "Materials");
            if (!AssetDatabase.IsValidFolder(MaterialFolder))
                AssetDatabase.CreateFolder("Assets/Materials", "GeneratedLevels");
        }

        private static void SaveScene(Scene scene, int level)
        {
            string path = SceneFolder + "/Bounce Back Level " + level + ".unity";
            EditorSceneManager.SaveScene(scene, path);
            EditorSceneManager.CloseScene(scene, true);
            if (previousScene.IsValid() && previousScene.isLoaded)
                SceneManager.SetActiveScene(previousScene);
        }

        private static Scene BeginLevelScene()
        {
            previousScene = SceneManager.GetActiveScene();
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
            SceneManager.SetActiveScene(scene);
            return scene;
        }

        private static void AddToBuildSettings()
        {
            List<EditorBuildSettingsScene> scenes = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
            AddScene(scenes, 4);
            AddScene(scenes, 5);
            EditorBuildSettings.scenes = scenes.ToArray();
        }

        private static void AddScene(List<EditorBuildSettingsScene> scenes, int level)
        {
            string path = SceneFolder + "/Bounce Back Level " + level + ".unity";
            for (int i = 0; i < scenes.Count; i++)
            {
                if (scenes[i].path == path)
                {
                    scenes[i] = new EditorBuildSettingsScene(path, true);
                    return;
                }
            }
            scenes.Add(new EditorBuildSettingsScene(path, true));
        }
    }
}
