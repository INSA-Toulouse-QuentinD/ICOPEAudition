using System.Collections;
using UnityEngine;

namespace UI.TutorialContents{
    public class ArrowAnimation : MonoBehaviour{
        private RectTransform _rectTransform;
        private Vector2 _startPosition;
        private Vector3 _startScale;

        private Coroutine _animationCoroutine;

        private void Awake(){
            _rectTransform = GetComponent<RectTransform>();
            _startPosition = _rectTransform.anchoredPosition;
            _startScale = _rectTransform.localScale;

            Hide();
        }

        public void Show(){
            gameObject.SetActive(true);

            if (_animationCoroutine == null) {
                _animationCoroutine = StartCoroutine(AnimationCoroutine());
            }
        }

        public void Hide(){
            if (_animationCoroutine != null) {
                StopCoroutine(_animationCoroutine);
                _animationCoroutine = null;
            }

            _rectTransform.anchoredPosition = _startPosition;
            _rectTransform.localScale = _startScale;

            gameObject.SetActive(false);
        }

        private IEnumerator AnimationCoroutine(){
            float moveDistance = 30;
            float duration = 1; //Attention, crash si trop petit
            float scaleMultiplier = 1.05f;
            
            float angle = transform.eulerAngles.z * Mathf.Deg2Rad;
            Vector2 direction = new Vector2(-Mathf.Sin(angle), Mathf.Cos(angle));

            while (true) {
                float timer = 0f;

                while (timer < duration) {
                    timer += Time.unscaledDeltaTime;

                    float t = Mathf.Clamp01(timer / duration);
                    t = Mathf.SmoothStep(0f, 1f, t);

                    _rectTransform.anchoredPosition = _startPosition - direction * (moveDistance * t);
                    _rectTransform.localScale = _startScale * Mathf.Lerp(scaleMultiplier, 1f, t);

                    yield return null;
                }

                timer = 0f;

                while (timer < duration) {
                    timer += Time.unscaledDeltaTime;

                    float t = Mathf.Clamp01(timer / duration);
                    t = Mathf.SmoothStep(0f, 1f, t);

                    _rectTransform.anchoredPosition = _startPosition - direction * (moveDistance * (1f - t));
                    _rectTransform.localScale = _startScale * Mathf.Lerp(1f, scaleMultiplier, t);

                    yield return null;
                }
            }
        }
    }
}