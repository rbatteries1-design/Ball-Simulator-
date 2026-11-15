using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using GorillaLocomotion;
using UnityEngine;
using UnityEngine.Networking;

namespace EndlessLabyrinth
{
    public sealed class OnlineSession : MonoBehaviour
    {
        private const string Endpoint = "https://lizzythemasterbaiter.pythonanywhere.com/endless-labyrinth";
        private const string TokenKey = "EndlessLabyrinth.OnlineToken";
        public static OnlineSession Instance { get; private set; }
        public string StaffRole { get; private set; }
        public string PlayerId { get; private set; }
        public string StaffTarget { get; private set; }
        [Serializable] private sealed class StaffIdentity { public string role, playerId; }
        [Serializable] private sealed class StaffCommand { public string action, target; public int amount; }

        public void SelectNextStaffTarget()
        {
            var ids = new List<string>(remotes.Keys);
            ids.Sort(StringComparer.Ordinal);
            StaffTarget = ids.Count == 0 ? null : ids[(ids.IndexOf(StaffTarget) + 1) % ids.Count];
        }

        public void SendStaffAction(string action, int amount, Action<bool> finished)
        {
            StartCoroutine(RunStaffAction(action, amount, finished));
        }

        private IEnumerator RunStaffAction(string action, int amount, Action<bool> finished)
        {
            bool ok = false;
            yield return Request("/v1/staff/action", JsonUtility.ToJson(new StaffCommand
                { action = action, amount = amount, target = StaffTarget }), _ => ok = true);
            finished(ok);
        }

        public IEnumerator CheckStaffAccess(Action<bool> finished)
        {
            bool allowed = false;
            yield return Request("/v1/staff", null, text =>
            {
                var identity = JsonUtility.FromJson<StaffIdentity>(text);
                allowed = !string.IsNullOrEmpty(identity.playerId) &&
                    (identity.role == "owner" || identity.role == "admin" ||
                     identity.role == "moderator" || identity.role == "bff");
            });
            finished(allowed);
        }

        private IEnumerator SyncStaff()
        {
            while (true)
            {
                bool verified = false;
                yield return Request("/v1/staff", null, text =>
                {
                    var identity = JsonUtility.FromJson<StaffIdentity>(text);
                    StaffRole = identity.role;
                    PlayerId = identity.playerId;
                    verified = true;
                });
                if (!verified)
                    StaffRole = null;
                yield return new WaitForSecondsRealtime(5f);
            }
        }

        public bool WalletReady { get; private set; }
        public int Balance { get; private set; }
        public long SecondsUntilReward => Math.Max(0, (long)Math.Ceiling(rewardAt - Time.realtimeSinceStartupAsDouble));
        private readonly HashSet<string> owned = new HashSet<string>();
        private readonly Dictionary<string, Remote> remotes = new Dictionary<string, Remote>();
        private string token;
        private string room = "lobby";
        private double rewardAt;
        private bool walletBusy;

        [Serializable] private sealed class Session { public string token; }
        [Serializable] private sealed class Wallet
        {
            public int balance;
            public int secondsUntilReward;
            public string[] owned;
        }
        [Serializable] private sealed class Purchase { public string itemId; public int price; }
        [Serializable] private sealed class Pose
        {
            public string playerId;
            public string room;
            public Vector3 head, left, right;
            public Quaternion rotation;
        }
        [Serializable] private sealed class Snapshot { public Pose[] players; public bool fling; }

        private sealed class Remote
        {
            public GameObject root;
            public Transform head, left, right;
            public Pose pose;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Bootstrap()
        {
            var root = new GameObject("Endless Labyrinth Online");
            DontDestroyOnLoad(root);
            Instance = root.AddComponent<OnlineSession>();
        }

        private IEnumerator Start()
        {
            token = PlayerPrefs.GetString(TokenKey);
            while (string.IsNullOrEmpty(token))
            {
                yield return Request("/v1/session", "{}", text =>
                {
                    var session = JsonUtility.FromJson<Session>(text);
                    token = session.token;
                    PlayerPrefs.SetString(TokenKey, token);
                    PlayerPrefs.Save();
                });
                if (string.IsNullOrEmpty(token))
                    yield return new WaitForSecondsRealtime(4);
            }
            StartCoroutine(SyncStaff());
            StartCoroutine(SyncWallet());
            StartCoroutine(SyncRoom());
        }

        public bool Owns(string itemId) => owned.Contains(itemId);

        public void JoinRoom(string roomCode)
        {
            if (string.IsNullOrWhiteSpace(roomCode))
                return;
            room = roomCode.Trim().ToLowerInvariant();
            RemoveMissing(new HashSet<string>());
        }

        public void Buy(string itemId, int price, Action<bool> result)
        {
            if (!WalletReady || walletBusy)
            {
                result(false);
                return;
            }
            StartCoroutine(PurchaseItem(itemId, price, result));
        }

        private IEnumerator PurchaseItem(string itemId, int price, Action<bool> callback)
        {
            walletBusy = true;
            bool success = false;
            yield return Request("/v1/purchase", JsonUtility.ToJson(new Purchase { itemId = itemId, price = price }),
                text =>
                {
                    LoadWallet(text);
                    success = Owns(itemId);
                });
            walletBusy = false;
            callback(success);
        }

        private IEnumerator SyncWallet()
        {
            while (true)
            {
                if (!walletBusy)
                {
                    walletBusy = true;
                    bool success = false;
                    yield return Request("/v1/wallet", null, text => { LoadWallet(text); success = true; });
                    WalletReady = success;
                    walletBusy = false;
                }
                yield return new WaitForSecondsRealtime(WalletReady && SecondsUntilReward > 0 ? 8 : 2);
            }
        }

        private void LoadWallet(string text)
        {
            var wallet = JsonUtility.FromJson<Wallet>(text);
            Balance = wallet.balance;
            rewardAt = Time.realtimeSinceStartupAsDouble + wallet.secondsUntilReward;
            owned.Clear();
            if (wallet.owned != null)
                owned.UnionWith(wallet.owned);
            WalletReady = true;
        }

        private IEnumerator SyncRoom()
        {
            while (true)
            {
                var player = Player.Instance;
                bool success = false;
                if (player != null && player.headCollider != null)
                {
                    var head = player.headCollider.transform;
                    string currentRoom = room;
                    var pose = new Pose { room = currentRoom, head = head.position, rotation = head.rotation,
                        left = player.leftHandFollower.position, right = player.rightHandFollower.position };
                    yield return Request("/v1/room", JsonUtility.ToJson(pose), text =>
                    {
                        if (currentRoom == room)
                        {
                            var snapshot = JsonUtility.FromJson<Snapshot>(text);
                            ApplySnapshot(snapshot);
                            if (snapshot.fling && player.TryGetComponent(out Rigidbody body) && !body.isKinematic)
                                body.AddForce(Vector3.up * 5f, ForceMode.VelocityChange);
                        }
                        success = true;
                    });
                }
                if (!success)
                    RemoveMissing(new HashSet<string>());
                yield return new WaitForSecondsRealtime(success ? 0.16f : 2f);
            }
        }

        private void ApplySnapshot(Snapshot snapshot)
        {
            var visible = new HashSet<string>();
            if (snapshot.players != null)
                foreach (var pose in snapshot.players)
                {
                    visible.Add(pose.playerId);
                    if (!remotes.TryGetValue(pose.playerId, out var remote))
                    {
                        remote = new Remote { root = new GameObject("Remote VR Player") };
                        remote.root.transform.SetParent(transform, false);
                        remote.head = MakeMarker(remote.root.transform, pose.head, 0.25f);
                        remote.left = MakeMarker(remote.root.transform, pose.left, 0.12f);
                        remote.right = MakeMarker(remote.root.transform, pose.right, 0.12f);
                        remotes.Add(pose.playerId, remote);
                    }
                    remote.pose = pose;
                }
            RemoveMissing(visible);
        }

        private static Transform MakeMarker(Transform parent, Vector3 position, float size)
        {
            var marker = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            marker.GetComponent<Collider>().enabled = false;
            marker.layer = 2;
            marker.transform.SetParent(parent, false);
            marker.transform.position = position;
            marker.transform.localScale = Vector3.one * size;
            return marker.transform;
        }

        private void RemoveMissing(HashSet<string> visible)
        {
            var departed = new List<string>();
            foreach (var remote in remotes)
                if (!visible.Contains(remote.Key))
                {
                    Destroy(remote.Value.root);
                    departed.Add(remote.Key);
                }
            foreach (string id in departed)
                remotes.Remove(id);
        }

        private void LateUpdate()
        {
            float blend = 1f - Mathf.Exp(-14f * Time.unscaledDeltaTime);
            foreach (var remote in remotes.Values)
            {
                remote.head.position = Vector3.Lerp(remote.head.position, remote.pose.head, blend);
                remote.head.rotation = Quaternion.Slerp(remote.head.rotation, remote.pose.rotation, blend);
                remote.left.position = Vector3.Lerp(remote.left.position, remote.pose.left, blend);
                remote.right.position = Vector3.Lerp(remote.right.position, remote.pose.right, blend);
            }
        }

        public IEnumerator RaceRequest(string json, Action<string, string> result)
        {
            if (string.IsNullOrEmpty(token))
            {
                result(null, "Still signing in");
                yield break;
            }
            yield return Request("/v1/race", json, text => result(text, ""),
                status => result(null, status == 404 ? "Room or race service unavailable" :
                    status == 409 ? "Race already started or room full" : "Could not complete race request"));
        }

        private IEnumerator Request(string path, string json, Action<string> success, Action<long> failure = null)
        {
            using (var request = new UnityWebRequest(Endpoint + path, json == null ? "GET" : "POST"))
            {
                request.timeout = 6;
                request.downloadHandler = new DownloadHandlerBuffer();
                if (json != null)
                {
                    request.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(json));
                    request.SetRequestHeader("Content-Type", "application/json");
                }
                if (!string.IsNullOrEmpty(token))
                    request.SetRequestHeader("Authorization", "Bearer " + token);
                yield return request.SendWebRequest();
                if (request.result == UnityWebRequest.Result.Success)
                    success(request.downloadHandler.text);
                else
                    failure?.Invoke(request.responseCode);
            }
        }
    }
}
