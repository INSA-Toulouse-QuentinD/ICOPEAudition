using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using Managers;
using PatientData.AlgoData;
using UnityEngine.Networking;

namespace PatientData{
    /// <summary>
    /// Utility to load patient data described as JSON files at runtime and bind them into the
    /// existing runtime LevelsData structure. Designed to work in both Editor (local file I/O)
    /// and WebGL (HTTP downloads from web server).
    /// </summary>
    public static class PatientDataJsonLoader{
        [Serializable]
        public class PatientDto{
            public string spriteUp;
            public string spriteSit;
            public string spriteWeber;
            public bool isOnPhone;
            public string lastName;
            public string firstName;
            public int age;

            public string familySituation;
            public string occupationalActivities;
            public string context;
            public string medicalHistory;
            public string descriptionLevel;

            public List<AlgoStepDto> steps;
        }

        [Serializable]
        public class AlgoStepDto{
            public string type;
            public string dialogueDoctor;
            public string dialoguePatient;
            public string spriteEarExams;

            public List<AnswerDto> diagnosticPhase;
            public List<AnswerDto> actionPhase;

            public List<string> predefinedAnswer;
        }

        [Serializable]
        public class AnswerDto{
            public string answerText;
            public bool isCorrect;
            public string correctionText;
            public string rappelTip;
            public string spriteJustification1;
            public string spriteJustification2;
        }

        public static IEnumerator LoadLevelData(Action<LevelsData> onOneDone, Action<LevelsData> onDone){
            LevelsData result = ScriptableObject.CreateInstance<LevelsData>();

            string manifestJson = null;

            yield return LoadJson("Levels/manifest.json", json => { manifestJson = json; });

            if (manifestJson == null) {
                onDone?.Invoke(null);
                yield break;
            }

            manifestJson = manifestJson.TrimStart('\uFEFF');
            Manifest manifest = JsonUtility.FromJson<Manifest>(manifestJson);

            foreach (LevelManifest levelManifest in manifest.levels) {
                PatientCaseLevel level = new PatientCaseLevel{
                    name = ExtractName(levelManifest.name)
                };

                List<string> sortedPatients = SortByIndex(levelManifest.patients);

                foreach (string patientName in sortedPatients) {
                    string relativePath = $"Levels/{levelManifest.name}/{patientName}/{ExtractName(patientName)}";

                    string patientJson = null;
                    yield return LoadJson(relativePath + ".json", json => { patientJson = json; });
                    if (patientJson == null) continue;

                    PatientData data = null;
                    yield return ConvertToPatientData(patientJson, relativePath, patientData => data = patientData);
                    level.patientsCase.Add(data);
                }

                result.patientByLevel.Add(level);
                onOneDone?.Invoke(result);
            }

            onDone?.Invoke(result);
        }

        private static IEnumerator ConvertToPatientData(string json, string path, Action<PatientData> onDone){
            PatientDto dto = JsonUtility.FromJson<PatientDto>(json);
            PatientData data = ScriptableObject.CreateInstance<PatientData>();

            data.isOnPhone = dto.isOnPhone;
            data.lastName = dto.lastName;
            data.firstName = dto.firstName;
            if (string.IsNullOrEmpty(data.firstName)) {
                GameManager.Instance.JsonErrorAdd($"Le patient {path} à obligatoirement besoin d'un prénom.");
            }

            data.age = dto.age;

            data.familySituation = dto.familySituation;
            data.occupationalActivities = dto.occupationalActivities;
            data.context = dto.context;

            data.medicalHistory = dto.medicalHistory;
            data.descriptionLevel = dto.descriptionLevel;

            List<AlgoStep> steps = null;
            yield return ConvertSteps(dto.steps, path, result => steps = result);
            data.steps = steps;
            if (data.steps.Count < 1 || data.steps[0].type != Step.CasePresentation) {
                GameManager.Instance.JsonErrorAdd(
                    $"Le patient {path} doit avoir obligatoirement l'étape CasePresentation en première étape");
            }

            bool hasWeberTest = false;
            bool needSit = false;
            foreach (AlgoStep step in steps) {
                if (step is {type: Step.WisperTest or Step.Otoscopy}) {
                    needSit = true;
                }
                if (step.type == Step.WeberTest) {
                    hasWeberTest = true;
                }
            }

            Sprite up = null;
            Sprite sit = null;
            Sprite weber = null;

            yield return LoadSprite(dto.spriteUp, s => up = s);
            yield return LoadSprite(dto.spriteSit, s => sit = s, !needSit);
            yield return LoadSprite(dto.spriteWeber, s => weber = s, !hasWeberTest);

            List<Sprite> sprites = new();
            if (up) {
                sprites.Add(up);
            } else {
                GameManager.Instance.JsonErrorAdd($"Le patient {path} a besoin d'une image debout ! (spriteUp)");
            }

            if (sit) {
                sprites.Add(sit);
            } else if (needSit) {
                GameManager.Instance.JsonErrorAdd($"Le patient {path} a besoin d'une image assis ! (spriteSit)");
            }

            if (weber) {
                sprites.Add(weber);
            } else if (hasWeberTest) {
                GameManager.Instance.JsonErrorAdd($"Le patient {path} a besoin d'une image pour weber ! (spriteWeber)");
            }

            data.characterSprites = sprites.ToArray();

            onDone?.Invoke(data);
        }

        private static IEnumerator ConvertSteps(List<AlgoStepDto> dtos, string path, Action<List<AlgoStep>> onDone){
            List<AlgoStep> result = new List<AlgoStep>();

            if (dtos == null) {
                onDone?.Invoke(result);
                yield break;
            }

            int nb = 0;
            foreach (AlgoStepDto dto in dtos) {
                nb++;
                AlgoStep step = new AlgoStep();

                string errorText = $"L'Étape numéro {nb} ({dto.type}) dans {path}";
                bool isParse = Enum.TryParse(dto.type, out Step stepType);
                if (!isParse) {
                    Step? closestEnum = FindClosestEnum<Step>(dto.type);
                    GameManager.Instance.JsonErrorAdd(
                        $"{errorText} n'existe pas !{(closestEnum != null ? $" Peut-être vouliez-vous dire {closestEnum} ?" : "")}");
                } else {
                    step.type = stepType;
                    
                    step.dialogueDoctor =  dto.dialogueDoctor;

                    step.dialoguePatient = dto.dialoguePatient;
                    if (string.IsNullOrEmpty(step.dialoguePatient) && step.type is Step.WisperTest or Step.WeberTest) {
                        GameManager.Instance.JsonErrorAdd($"{errorText} doit avoir un dialoguePatient !");
                    }

                    // spriteEarExams (LOAD)
                    if (!string.IsNullOrEmpty(dto.spriteEarExams)) {
                        Sprite ear = null;
                        yield return LoadSprite(dto.spriteEarExams, s => ear = s);
                        step.spriteEarExams = ear;
                        if (!step.spriteEarExams && step.type is Step.Otoscopy or Step.Hhies or Step.Audiometry) {
                            GameManager.Instance.JsonErrorAdd($"{errorText} doit avoir un spriteEarExams !");
                        }
                    }

                    step.predefinedAnswer = ConvertYesNo(dto.predefinedAnswer);
                    if (step.predefinedAnswer.Count != 7 && step.type is Step.GoNoGo) {
                        GameManager.Instance.JsonErrorAdd($"{errorText} doit avoir 7 predefinedAnswer !");
                    }

                    List<AnswerData> diag = null;
                    yield return ConvertAnswers(dto.diagnosticPhase, errorText, r => diag = r);
                    step.diagnosticPhase = diag;

                    List<AnswerData> act = null;
                    yield return ConvertAnswers(dto.actionPhase, errorText, r => act = r);
                    step.actionPhase = act;

                    if (step.diagnosticPhase.Count == 0 && step.actionPhase.Count == 0) {
                        GameManager.Instance.JsonErrorAdd(
                            $"{errorText} doit avoir ou moins une phase (diagnosticPhase ou actionPhase) !");
                    }

                    result.Add(step);
                }
            }

            onDone?.Invoke(result);
        }

        private static List<YesNo> ConvertYesNo(List<string> input){
            List<YesNo> result = new List<YesNo>();

            if (input == null)
                return result;

            foreach (string s in input) {
                Enum.TryParse(s, out YesNo value);
                result.Add(value);
            }

            return result;
        }

        private static IEnumerator ConvertAnswers(List<AnswerDto> answerDtos, string errorText,
            Action<List<AnswerData>> onDone){
            List<AnswerData> result = new List<AnswerData>();

            if (answerDtos == null || answerDtos.Count == 0) {
                onDone?.Invoke(result);
                yield break;
            }

            int goodAnswer = 0;
            foreach (AnswerDto dto in answerDtos) {
                if (string.IsNullOrEmpty(dto.answerText)) {
                    GameManager.Instance.JsonErrorAdd($"{errorText} possède une réponse sans titre ! (answerText)");
                }

                goodAnswer += dto.isCorrect ? 1 : 0;
                if (string.IsNullOrEmpty(dto.correctionText)) {
                    GameManager.Instance.JsonErrorAdd($"{errorText} possède une réponse sans justification ! (correctionText)");
                }

                bool isParse = Enum.TryParse(dto.rappelTip, out RappelTip tip);
                if (!isParse && dto.rappelTip != null) {
                    RappelTip? closestEnum = FindClosestEnum<RappelTip>(dto.rappelTip);
                    GameManager.Instance.JsonErrorAdd(
                        $"{errorText} possède un rappelTip qui n'existe pas ! ({dto.rappelTip}){(closestEnum != null ? $" Peut-être vouliez-vous dire {closestEnum} ?" : "")}");
                }

                AnswerData a = new AnswerData{
                    answerText = dto.answerText,
                    isCorrect = dto.isCorrect,
                    correctionText = dto.correctionText,
                    rappelTip = tip,
                    sprites = new List<Sprite>()
                };

                if (!string.IsNullOrEmpty(dto.spriteJustification1)) {
                    Sprite s1 = null;
                    yield return LoadSprite(dto.spriteJustification1, x => s1 = x);

                    if (s1) a.sprites.Add(s1);
                }

                if (!string.IsNullOrEmpty(dto.spriteJustification2)) {
                    Sprite s2 = null;
                    yield return LoadSprite(dto.spriteJustification2, x => s2 = x);

                    if (s2) a.sprites.Add(s2);
                }

                result.Add(a);
            }

            if (goodAnswer == 0) {
                GameManager.Instance.JsonErrorAdd($"{errorText} possède une phase avec aucune bonne réponse !");
            } else if (goodAnswer > 1) {
                GameManager.Instance.JsonErrorAdd($"{errorText} possède une phase avec {goodAnswer} bonnes réponses !");
            }

            onDone?.Invoke(result);
        }

        #region Littles Functions

        private static string ExtractName(string raw){
            int index = raw.LastIndexOf('_');
            return index >= 0 ? raw.Substring(index + 1) : raw;
        }

        private static List<string> SortByIndex(List<string> input){
            input.Sort((a, b) => {
                int ia = GetIndex(a);
                int ib = GetIndex(b);
                return ia.CompareTo(ib);
            });

            return input;
        }

        private static int GetIndex(string name){
            int i = name.IndexOf('_');
            if (i <= 0) return 0;

            string number = name.Substring(0, i);

            int.TryParse(number, out int result);
            return result;
        }

        #endregion

        #region Files Loader

        public static IEnumerator LoadJson(string relativePath, Action<string> callback){
            if (string.IsNullOrEmpty(relativePath)) yield break;

            string path = Path.Combine(Application.streamingAssetsPath, relativePath);

#if UNITY_WEBGL && !UNITY_EDITOR
            UnityWebRequest request = UnityWebRequest.Get(path);
#else
            UnityWebRequest request = UnityWebRequest.Get("file:///" + path);
#endif

            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success) {
                GameManager.Instance.JsonErrorAdd($"Erreur pour le json {relativePath}: {request.error}");
                callback?.Invoke(null);
                yield break;
            }

            callback?.Invoke(request.downloadHandler.text);
        }

        public static IEnumerator LoadSprite(string relativePath, Action<Sprite> callback, bool canBeFailed = false){
            if (string.IsNullOrEmpty(relativePath)) yield break;

            string path = Path.Combine(Application.streamingAssetsPath, relativePath);

#if UNITY_WEBGL && !UNITY_EDITOR
            UnityWebRequest request = UnityWebRequestTexture.GetTexture(path);
#else
            UnityWebRequest request = UnityWebRequestTexture.GetTexture("file:///" + path);
#endif

            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success) {
                if (!canBeFailed) {
                    GameManager.Instance.JsonErrorAdd($"Erreur pour l'image {relativePath}: {request.error}");
                }

                callback?.Invoke(null);
                yield break;
            }

            Texture2D texture = DownloadHandlerTexture.GetContent(request);

            Sprite sprite = Sprite.Create(
                texture,
                new Rect(0, 0, texture.width, texture.height),
                new Vector2(0.5f, 0.5f)
            );

            callback?.Invoke(sprite);
        }

        #endregion

        #region Levenshtein

        public static T? FindClosestEnum<T>(string input) where T : struct, Enum{
            if (string.IsNullOrEmpty(input)) {
                return null;
            }

            string a = input.ToLowerInvariant();

            T bestMatch = default;
            int bestScore = int.MinValue;

            foreach (T value in Enum.GetValues(typeof(T))) {
                string b = value.ToString().ToLowerInvariant();

                int score = FastScore(a, b);

                if (score > bestScore) {
                    bestScore = score;
                    bestMatch = value;
                }
            }

            return bestScore >= 3 ? bestMatch : null;
        }

        private static int FastScore(string a, string b){
            int score = 0;

            int minLen = Mathf.Min(a.Length, b.Length);

            int prefix = 0;
            for (int i = 0; i < minLen; i++) {
                if (a[i] != b[i]) break;
                prefix++;
            }

            score += prefix * 2;
            score += a.Count(t => b.Any(t1 => t == t1));
            score -= Mathf.Abs(a.Length - b.Length);

            return score;
        }

        #endregion
    }
}