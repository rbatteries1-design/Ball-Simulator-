using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace BounceBack
{
    [RequireComponent(typeof(BallMotor))]
    public sealed class BounceCampaign : MonoBehaviour
    {
        [SerializeField, Range(1, 3)] private int level = 1;
        private const string Key = "BounceBack.Shop.";
        private static readonly string[] TrailNames =
            { "Rust", "Ash", "Sparks", "Acid", "Ice", "Arc", "Ink", "Embers" };
        private static readonly Color[] TrailColors =
            { new Color(.66f,.31f,.16f), Color.gray, Color.yellow, Color.green,
              Color.cyan, new Color(.45f,.38f,1f), new Color(.42f,.18f,.5f), Color.red };
        private BounceShopCatalog catalog;
        private BallMotor motor;
        private ParticleSystem spawnedTrail;
        private Material trailMaterial;
        private bool shop, clearing, finished;
        private int credits;

        private void Start()
        {
            motor = GetComponent<BallMotor>();
            if (gameObject.scene.name == "Bounce Back Level " + level && !TryGetComponent<BounceRace>(out _))
                gameObject.AddComponent<BounceRace>();
            catalog = Resources.Load<BounceShopCatalog>("BounceBack/Shop Catalog");
            credits = PlayerPrefs.GetInt(Key + "Credits");
            ApplySkin(PlayerPrefs.GetInt(Key + "Skin", -1));
            ApplyTrail(PlayerPrefs.GetInt(Key + "Trail", -1));
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Tab) && !clearing)
                ToggleShop(!shop);
        }

        private void ToggleShop(bool visible)
        {
            shop = visible;
            motor.SetInputBlocked(visible || clearing || finished);
        }

        public void Complete(float delay)
        {
            if (clearing || finished)
                return;
            clearing = true;
            ToggleShop(false);
            credits += 2;
            PlayerPrefs.SetInt(Key + "Credits", credits);
            PlayerPrefs.Save();
            StartCoroutine(Advance(delay));
        }

        private IEnumerator Advance(float delay)
        {
            yield return new WaitForSecondsRealtime(Mathf.Max(.75f, delay));
            if (level < 3)
                SceneManager.LoadScene("Bounce Back Level " + (level + 1));
            else
            {
                clearing = false;
                finished = true;
            }
        }

        private void OnGUI()
        {
            if (!shop && !clearing && !finished)
            {
                if (GUILayout.Button("Shop [Tab]", GUILayout.Width(105)))
                    ToggleShop(true);
                if (level == 1 && GUILayout.Button("Race Course", GUILayout.Width(105)))
                    SceneManager.LoadScene("yuh");
                return;
            }
            GUILayout.BeginArea(new Rect(12, 32, 350, Mathf.Max(100, Screen.height - 44)), GUI.skin.box);
            if (shop)
            {
                GUILayout.Label("COSMETIC SHOP  |  " + credits + " credits");
                GUILayout.Label("SKINS");
                for (int i = 0; i < 8; i++)
                {
                    if (catalog != null && i < catalog.skins.Length && catalog.skins[i] != null)
                        DrawItem("Skin", i, catalog.skins[i].name);
                }
                GUILayout.Label("TRAILS");
                for (int i = 0; i < 8; i++)
                    DrawItem("Trail", i, TrailNames[i]);
                if (GUILayout.Button("Close"))
                    ToggleShop(false);
            }
            else if (clearing)
                GUILayout.Label("Level " + level + " cleared");
            else
            {
                GUILayout.Label("BALL SIMULATOR 3 COMPLETE");
                GUILayout.Label("Zion - Mainly had the game name idea, and made a few music pieces. was eventually replaced when i got permission to use seperate audio files,");
                GUILayout.Label("Jordan - he was just there tossing a few ideas here and there, respects for just being there");
                GUILayout.Label("Savit - Overall chill and was just there to be silent and do the actual work,");
                GUILayout.Label("Richard - Did a ton of the game design, did all the debugging and testing to make sure it worked");
                if (GUILayout.Button("Shop"))
                    ToggleShop(true);
                if (GUILayout.Button("Replay from Level 1"))
                    SceneManager.LoadScene("Bounce Back Level 1");
            }
            GUILayout.EndArea();
        }

        private void DrawItem(string category, int index, string label)
        {
            string unlock = Key + category + ".Owned." + index;
            bool owned = PlayerPrefs.GetInt(unlock) == 1;
            bool equipped = PlayerPrefs.GetInt(Key + category, -1) == index;
            GUI.enabled = !equipped && (owned || credits > 0);
            bool clicked = GUILayout.Button(label + (equipped ? "  ÃƒÂ¢Ã¢â€šÂ¬Ã‚Â¢ Equipped" : owned ? "  ÃƒÂ¢Ã¢â€šÂ¬Ã‚Â¢ Equip" : "  ÃƒÂ¢Ã¢â€šÂ¬Ã‚Â¢ 1 credit"));
            GUI.enabled = true;
            if (!clicked)
                return;

            if (!owned)
            {
                credits--;
                PlayerPrefs.SetInt(Key + "Credits", credits);
                PlayerPrefs.SetInt(unlock, 1);
            }
            PlayerPrefs.SetInt(Key + category, index);
            PlayerPrefs.Save();
            if (category == "Skin")
                ApplySkin(index);
            else
                ApplyTrail(index);
        }

        private void ApplySkin(int index)
        {
            if (catalog != null && index >= 0 && index < catalog.skins.Length && catalog.skins[index] != null &&
                TryGetComponent(out Renderer mesh))
                mesh.sharedMaterial = catalog.skins[index];
        }

        private void ApplyTrail(int index)
        {
            if (index < 0 || index >= TrailColors.Length)
                return;

            if (spawnedTrail != null)
                Destroy(spawnedTrail.gameObject);
            if (trailMaterial != null)
                Destroy(trailMaterial);
            trailMaterial = null;
            var custom = catalog != null && index < catalog.trails.Length ? catalog.trails[index] : null;
            if (custom != null)
                spawnedTrail = Instantiate(custom, transform);
            else
            {
                var emitter = new GameObject(TrailNames[index] + " Trail");
                emitter.transform.SetParent(transform, false);
                spawnedTrail = emitter.AddComponent<ParticleSystem>();
                var main = spawnedTrail.main;
                main.startLifetime = new ParticleSystem.MinMaxCurve(.22f, .42f + index * .045f);
                main.startSize = new ParticleSystem.MinMaxCurve(.04f, .13f + index * .012f);
                main.startColor = TrailColors[index];
                var renderer = emitter.GetComponent<ParticleSystemRenderer>();
                var shader = Resources.Load<Shader>("BounceBack/Trail Chip");
                if (shader != null)
                    renderer.sharedMaterial = trailMaterial = new Material(shader);
                if (index == 2 || index == 5)
                {
                    renderer.renderMode = ParticleSystemRenderMode.Stretch;
                    renderer.lengthScale = 3f;
                }
            }
            motor.SetMovementDust(spawnedTrail);
        }

        private void OnDestroy()
        {
            if (trailMaterial != null)
                Destroy(trailMaterial);
        }
    }
}
