using System.Collections;
using System.Collections.Generic;
using Managers;
using UnityEngine;
using UnityEngine.UI;

namespace UI.ScoreContents{
    public class OpenFolder : MonoBehaviour{
        [SerializeField] private ButtonPressDetector openDetailsButton;
        [SerializeField] private RectTransform folderDivider, folderInside, folderCover;
        [SerializeField] private GameObject[] folderHideElements;
        [SerializeField] private Button[] pagesNavigationButs;

        [SerializeField] private GameObject prefab;
        [SerializeField] private Transform parentTransform;

        private bool _folderOpen;
        private int _pageId;

        private Dictionary<string, GameData.StepRecords> _records;
        private List<RectTransform> _pagesSteps;

        void OnEnable(){
            openDetailsButton.OnPress.AddListener(OpenFolderVoid);
            pagesNavigationButs[0].onClick.AddListener(delegate{ SwitchPages(true); });
            pagesNavigationButs[1].onClick.AddListener(delegate{ SwitchPages(false); });
            InstanciatePages();
            HideButtons();
        }

        void OnDisable(){
            openDetailsButton.OnPress.RemoveListener(OpenFolderVoid);
            pagesNavigationButs[0].onClick.RemoveAllListeners();
            pagesNavigationButs[1].onClick.RemoveAllListeners();
        }

        private void InstanciatePages(){
            _pageId = 0;
            _pagesSteps ??= new List<RectTransform>();

            foreach (var page in _pagesSteps) {
                if (page != null) Destroy(page.gameObject);
            }

            _pagesSteps.Clear();

            if (_records == null || _records.Count == 0) {
                return;
            }

            var ordered = new List<KeyValuePair<string, GameData.StepRecords>>(_records);

            int stepNumber = 1;
            foreach (var step in ordered) {
                GameObject instance = Instantiate(prefab, parentTransform);
                PrefabStepDisplayUi ui = instance.GetComponent<PrefabStepDisplayUi>();

                ui.SetText(step.Key, step.Value.DiagnosticAnswer, step.Value.ActionAnswer, stepNumber);

                _pagesSteps.Add(instance.GetComponent<RectTransform>());
                instance.SetActive(false);
                stepNumber++;
            }

            _pagesSteps[0].gameObject.SetActive(true);
        }

        private void OpenFolderVoid(){
            if (!_folderOpen) {
                _folderOpen = true;
                StartCoroutine(OpenFolderCoroutine(1));
            } else {
                _folderOpen = false;
                StartCoroutine(CloseFolderCoroutine(1));
            }
        }

        private IEnumerator OpenFolderCoroutine(float duration){
            if (GameManager.Instance.instanteAnimation) {
                duration = 0.01f;
            }

            float time = 0;
            bool elementsHided = false;
            while (time < duration) {
                if (!elementsHided && time >= duration / 2) {
                    elementsHided = true;
                    HideElements(true);
                    for (int i = 0; i < _pageId; i++) {
                        _pagesSteps[i].gameObject.SetActive(true);
                    }

                    folderCover.GetComponent<Image>().color = new Color(1, 0.9557926f, 0.8283019f);
                }

                folderDivider.localScale = new Vector3(Mathf.Lerp(1, -1, time / duration), 1, 1);
                folderDivider.localPosition = new Vector3(Mathf.Lerp(0, -370, time / duration), 0, 0);
                folderInside.localPosition = new Vector3(Mathf.Lerp(0, 370, time / duration), 0, 0);
                time += Time.deltaTime;
                yield return null;
            }

            folderDivider.localScale = new Vector3(-1, 1, 1);
            folderDivider.localPosition = new Vector3(-370, 0, 0);
            folderInside.localPosition = new Vector3(370, 0, 0);
        }

        private IEnumerator CloseFolderCoroutine(float duration){
            if (GameManager.Instance.instanteAnimation) {
                duration = 0.01f;
            }

            float time = 0;
            bool elementsShowed = false;
            while (time < duration) {
                if (!elementsShowed && time >= duration / 2) {
                    elementsShowed = true;
                    HideElements(false);
                    for (int i = 0; i < _pageId; i++) {
                        _pagesSteps[i].gameObject.SetActive(false);
                    }

                    folderCover.GetComponent<Image>().color = new Color(1, 1, 1);
                }

                folderDivider.localScale = new Vector3(Mathf.Lerp(-1, 1, time / duration), 1, 1);
                folderDivider.localPosition = new Vector3(Mathf.Lerp(-370, 0, time / duration), 0, 0);
                folderInside.localPosition = new Vector3(Mathf.Lerp(370, 0, time / duration), 0, 0);
                time += Time.deltaTime;
                yield return null;
            }

            folderDivider.localScale = new Vector3(1, 1, 1);
            folderDivider.localPosition = new Vector3(0, 0, 0);
            folderInside.localPosition = new Vector3(0, 0, 0);
        }

        private void SwitchPages(bool nextPage){
            InteractableButtons(false);
            StartCoroutine(nextPage ? NextPage(1) : PreviousPage(1));
        }

        private IEnumerator NextPage(float duration){
            if (GameManager.Instance.instanteAnimation) {
                duration = 0.01f;
            }

            _pagesSteps[_pageId].SetParent(folderDivider);
            _pagesSteps[_pageId + 1].gameObject.SetActive(true);

            float time = 0;
            bool elementsHided = false;
            _pagesSteps[_pageId].GetComponent<Image>().color = new Color(1, 1, 1, 1);
            while (time < duration) {
                if (!elementsHided && time >= duration / 2) {
                    elementsHided = true;
                    _pagesSteps[_pageId].GetChild(0).gameObject.SetActive(false);
                }

                _pagesSteps[_pageId].localScale = new Vector3(Mathf.Lerp(-1, 1, time / duration), 1, 1);
                _pagesSteps[_pageId].localPosition = new Vector3(Mathf.Lerp(-742, -30, time / duration), 0, 0);
                time += Time.deltaTime;
                yield return null;
            }

            _pagesSteps[_pageId].localScale = new Vector3(1, 1, 1);
            _pagesSteps[_pageId].localPosition = new Vector3(-30, 0, 0);
            _pagesSteps[_pageId].GetComponent<Image>().color = new Color(0.97f, 0.97f, 0.97f, 1);
            _pageId++;
            HideButtons();
        }

        private IEnumerator PreviousPage(float duration){
            if (GameManager.Instance.instanteAnimation) {
                duration = 0.01f;
            }

            _pageId--;
            float time = 0;
            bool elementsShowed = false;
            _pagesSteps[_pageId].GetComponent<Image>().color = new Color(1, 1, 1, 1);
            while (time < duration) {
                if (!elementsShowed && time >= duration / 2) {
                    elementsShowed = true;
                    _pagesSteps[_pageId].GetChild(0).gameObject.SetActive(true);
                }

                _pagesSteps[_pageId].localScale = new Vector3(Mathf.Lerp(1, -1, time / duration), 1, 1);
                _pagesSteps[_pageId].localPosition = new Vector3(Mathf.Lerp(-30, -742, time / duration), 0, 0);
                time += Time.deltaTime;
                yield return null;
            }

            _pagesSteps[_pageId].localScale = new Vector3(-1, 1, 1);
            _pagesSteps[_pageId].localPosition = new Vector3(-742, 0, 0);
            _pagesSteps[_pageId].GetComponent<Image>().color = new Color(0.97f, 0.97f, 0.97f, 1);
            _pagesSteps[_pageId].SetParent(folderInside);
            HideButtons();
        }

        private void InteractableButtons(bool interactable){
            pagesNavigationButs[1].interactable = interactable;
            pagesNavigationButs[0].interactable = interactable;
        }

        private void HideButtons(){
            InteractableButtons(true);
            pagesNavigationButs[1].gameObject.SetActive(_pageId > 0);
            pagesNavigationButs[0].gameObject.SetActive(_pageId < _pagesSteps.Count - 1);
        }

        private void HideElements(bool hide){
            foreach (GameObject elements in folderHideElements) {
                elements.SetActive(!hide);
            }
        }

        public void SetStepsRecords(Dictionary<string, GameData.StepRecords> dict){
            _records = dict;
        }
    }
}