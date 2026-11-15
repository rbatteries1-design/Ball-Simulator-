using System;
using System.Collections;
using System.Collections.Generic;
using EndlessLabyrinth;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

namespace BounceBack
{
    public sealed class BounceRace : MonoBehaviour
    {
        [Serializable] private sealed class Command { public string action, course, code; public RacePose position; }
        [Serializable] private sealed class Racer { public string id; public RacePose position; public bool active; public int place; }
        [Serializable] private sealed class RacePose
        {
            public float x, y, z;
            public int skinSlot, trailSlot;
        }
        [Serializable] private sealed class State
        {
            public string code, playerId;
            public bool host;
            public float countdown;
            public Racer[] racers;
        }

        private readonly Dictionary<string, GameObject> ghosts = new Dictionary<string, GameObject>();
        private BounceShopCatalog catalog;
        private State state;
        private double startsAt = -1;
        private double finishTime = -1;
        private Animator gate;
        private bool started;
        private float Remaining => startsAt < 0 ? -1f : Mathf.Max(0f, (float)(startsAt - Time.realtimeSinceStartupAsDouble));
        private BallMotor motor;
        private Rigidbody body;
        private Vector3 spawn;
        private Quaternion spawnRotation;
        private bool busy, menu, finishPending, heldAtStart;
        private string code = "", status = "";
        public bool InRace => state != null;
        public bool Running => state != null && started;

        private void Awake()
        {
            motor = GetComponent<BallMotor>();
            catalog = Resources.Load<BounceShopCatalog>("BounceBack/Shop Catalog");
            body = GetComponent<Rigidbody>();
            spawn = body.position;
            spawnRotation = body.rotation;
            foreach (var animator in FindObjectsOfType<Animator>())
            {
                if (animator.runtimeAnimatorController == null ||
                    !Array.Exists(animator.runtimeAnimatorController.animationClips, clip => clip.name == "racestartgateopen"))
                    continue;
                gate = animator;
                gate.enabled = false;
                break;
            }
        }

        private IEnumerator Start()
        {
            while (true)
            {
                if (InRace && !busy)
                    yield return Exchange(finishPending ? "finish" : "poll");
                yield return new WaitForSecondsRealtime(.25f);
            }
        }

        private void Update()
        {
            if (!InRace || started || startsAt < 0 || Remaining > 0)
                return;
            started = true;
            ReleaseStart();
            motor.SetRaceBlocked(false);
            if (gate != null)
            {
                gate.enabled = true;
                gate.Play("Base Layer.racestartgateopen", 0, 0f);
            }
            if (TryGetComponent(out BallAudio audio))
                audio.PlayRaceTheme();
        }

        public void Finish()
        {
            if (Running && finishTime < 0)
            {
                finishTime = Time.realtimeSinceStartupAsDouble - startsAt;
                finishPending = true;
            }
        }

        private void OnGUI()
        {
            if (GUILayout.Button(InRace ? "Race " + state.code : "Multiplayer", GUILayout.Width(115)))
                menu = !menu;
            if (InRace && startsAt >= 0)
            {
                var style = new GUIStyle(GUI.skin.box) { fontSize = 34, alignment = TextAnchor.MiddleCenter };
                double elapsed = finishTime >= 0 ? finishTime : Math.Max(0, Time.realtimeSinceStartupAsDouble - startsAt);
                string clock = Remaining > 0 ? Mathf.CeilToInt(Remaining).ToString() :
                    elapsed < .75 && finishTime < 0 ? "GO!" : TimeSpan.FromSeconds(elapsed).ToString(@"mm\:ss\.ff");
                GUI.Box(new Rect(Screen.width / 2f - 84, 12, 168, 58), clock, style);
            }
            if (!menu && !InRace)
                return;
            GUILayout.BeginArea(new Rect(Screen.width - 300, 12, 288, 325), GUI.skin.box);
            if (!InRace)
            {
                GUILayout.Label("RACE LOBBY");
                GUI.enabled = !busy;
                if (GUILayout.Button("Host this course"))
                    StartCoroutine(Exchange("host"));
                code = GUILayout.TextField(code, 8);
                if (GUILayout.Button("Join by code"))
                    StartCoroutine(Exchange("join"));
                GUI.enabled = true;
            }
            else
            {
                GUILayout.Label("Room " + state.code + "   |   " + state.racers.Length + "/24");
                GUILayout.Label(state.countdown < 0 ? "Waiting for host" :
                    state.countdown > 0 ? "Starts in " + Mathf.CeilToInt(state.countdown) : "Race in progress");
                if (state.host && state.countdown < 0 && !busy && GUILayout.Button("Start race"))
                    StartCoroutine(Exchange("start"));
                foreach (var racer in state.racers)
                    if (racer.place > 0)
                        GUILayout.Label("#" + racer.place + "  " + (racer.id == state.playerId ? "You" : racer.id.Substring(0, 6)));
                if (!busy && GUILayout.Button("Leave race"))
                    StartCoroutine(Exchange("leave"));
            }
            if (status.Length > 0)
                GUILayout.Label(status);
            GUILayout.EndArea();
        }

        private IEnumerator Exchange(string action)
        {
            var online = OnlineSession.Instance;
            if (online == null)
            {
                status = "Connecting to online services";
                yield break;
            }

            busy = true;
            string course = SceneManager.GetActiveScene().name;
            Vector3 position = body.position;
            var command = new Command
            {
                action = action,
                course = course == "yuh" ? "Bounce Back Level 1" : course,
                code = InRace ? state.code : code.Trim().ToLowerInvariant(),
                position = new RacePose { x = position.x, y = position.y, z = position.z,
                    skinSlot = PlayerPrefs.GetInt("BounceBack.Shop.Skin", -1) + 1,
                    trailSlot = PlayerPrefs.GetInt("BounceBack.Shop.Trail", -1) + 1 }
            };
            yield return online.RaceRequest(JsonUtility.ToJson(command), (text, error) =>
            {
                status = error;
                if (error.Length != 0)
                    return;
                if (action == "leave")
                {
                    state = null;
                    startsAt = finishTime = -1;
                    started = false;
                    if (gate != null)
                    {
                        gate.Play("Base Layer.racestartgateopen", 0, 0f);
                        gate.Update(0f);
                        gate.enabled = false;
                    }
                    if (TryGetComponent(out BallAudio audio))
                        audio.PlayTheme();
                    finishPending = false;
                    ReleaseStart();
                    if (SceneManager.GetActiveScene().name == "yuh")
                    {
                        body.position = spawn;
                        body.isKinematic = true;
                    }
                    motor.SetRaceBlocked(false);
                    ClearGhosts();
                    return;
                }

                bool firstSnapshot = !InRace;
                state = JsonUtility.FromJson<State>(text);
                if (state.countdown >= 0f && startsAt < 0)
                    startsAt = Time.realtimeSinceStartupAsDouble + state.countdown;
                if (firstSnapshot)
                {
                    body.velocity = Vector3.zero;
                    body.angularVelocity = Vector3.zero;
                    body.position = spawn;
                    body.rotation = spawnRotation;
                    body.isKinematic = true;
                    heldAtStart = true;
                    menu = true;
                }
                if (action == "finish")
                    finishPending = false;

                foreach (var racer in state.racers)
                {
                    int skin = racer.position.skinSlot - 1;
                    int trailIndex = racer.position.trailSlot - 1;
                    if (racer.id == state.playerId)
                    {
                        motor.SetRaceBlocked(!started || racer.place != 0);
                        continue;
                    }
                    if (!racer.active)
                    {
                        if (ghosts.TryGetValue(racer.id, out var inactive))
                            inactive.SetActive(false);
                        continue;
                    }
                    if (!ghosts.TryGetValue(racer.id, out var ghost))
                    {
                        ghost = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                        ghost.name = "Racer " + racer.id.Substring(0, 6);
                        ghost.GetComponent<Collider>().enabled = false;
                        ghost.transform.localScale = transform.lossyScale;
                        ghosts.Add(racer.id, ghost);
                    }
                    if (catalog != null && skin >= 0 && skin < catalog.skins.Length &&
                        catalog.skins[skin] != null)
                        ghost.GetComponent<Renderer>().sharedMaterial = catalog.skins[skin];
                    var trail = ghost.GetComponent<TrailRenderer>();
                    if (trailIndex >= 0 && trailIndex < 8 && trail == null)
                    {
                        trail = ghost.AddComponent<TrailRenderer>();
                        trail.startWidth = .13f;
                        trail.endWidth = 0f;
                        trail.time = .35f;
                        trail.sharedMaterial = ghost.GetComponent<Renderer>().sharedMaterial;
                    }
                    if (trail != null)
                    {
                        trail.emitting = trailIndex >= 0 && trailIndex < 8;
                        trail.startColor = Color.HSVToRGB(Mathf.Clamp(trailIndex, 0, 7) / 8f, .75f, 1f);
                        trail.endColor = new Color(1f, 1f, 1f, 0f);
                    }
                    ghost.SetActive(true);
                    ghost.transform.position = new Vector3(racer.position.x, racer.position.y, racer.position.z);
                }
            });
            busy = false;
        }

        private void ReleaseStart()
        {
            if (!heldAtStart)
                return;
            body.isKinematic = false;
            heldAtStart = false;
        }

        private void ClearGhosts()
        {
            foreach (var ghost in ghosts.Values)
                Destroy(ghost);
            ghosts.Clear();
        }

        private void OnDestroy() => ClearGhosts();
    }
}
