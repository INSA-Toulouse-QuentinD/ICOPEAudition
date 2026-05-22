using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace Managers{
    public class TelemetryManager : MonoBehaviour{
        public static TelemetryManager Instance{ get; private set; }

        private readonly string _url = "https://url.com/";
        [SerializeField] private bool debugLogs = true;
        private readonly Queue<LevelResultData> _pendingQueue = new();

        private bool _sending;
        private string _userId;

        private void Awake(){
            if (Instance != null && Instance != this) {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            InitializeUserId();
        }

        private void InitializeUserId(){
            _userId = PlayerPrefs.GetString("USER_ID", string.Empty);

            if (string.IsNullOrWhiteSpace(_userId)) {
                _userId = Guid.NewGuid().ToString();

                PlayerPrefs.SetString("USER_ID", _userId);
                PlayerPrefs.Save();

                if (debugLogs) Debug.Log($"[Telemetry] New USER_ID created : {_userId}");
            } else {
                if (debugLogs) Debug.Log($"[Telemetry] Existing USER_ID : {_userId}");
            }
        }

        public void SendLevelResult(string levelId, float duration, int wrongAnswers){
            LevelResultData data = new(){
                userId = _userId,
                levelId = levelId,
                duration = duration,
                wrongAnswers = wrongAnswers, //DEBUG!!! juste pour teste ! Doit différentier chaque étape dans la version finale
                timestamp = DateTime.UtcNow.ToString("o")
            };

            _pendingQueue.Enqueue(data);

            if (!_sending) {
                StartCoroutine(ProcessQueue());
            }
        }

        private IEnumerator ProcessQueue(){
            _sending = true;

            while (_pendingQueue.Count > 0) {
                LevelResultData data = _pendingQueue.Peek();

                yield return SendRequest(data);
                yield return new WaitForSeconds(1f);
            }

            _sending = false;
        }

        private IEnumerator SendRequest(LevelResultData data){
            string json = JsonUtility.ToJson(data);

            byte[] body = Encoding.UTF8.GetBytes(json);

            using UnityWebRequest request = new(_url, UnityWebRequest.kHttpVerbPOST);

            request.uploadHandler = new UploadHandlerRaw(body);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            if (debugLogs) Debug.Log($"[Telemetry] Sending:\n{json}");
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success) {
                if (debugLogs) Debug.Log($"[Telemetry] Success:\n{request.downloadHandler.text}");
                _pendingQueue.Dequeue();
            } else {
                Debug.LogError($"[Telemetry] Error:\n{request.error}");
                yield return new WaitForSeconds(2f);
            }
        }
    }

    [Serializable]
    public class LevelResultData{
        public string userId;
        public string levelId;
        public float duration;
        public int wrongAnswers;
        public string timestamp;
    }
}