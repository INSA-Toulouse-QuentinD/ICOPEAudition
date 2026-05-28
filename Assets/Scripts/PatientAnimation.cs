using DG.Tweening;
using Managers;
using PatientData;
using UnityEngine;
using UnityEngine.UI;

public class PatientAnimation : MonoBehaviour{
    // Animation size variable
    [SerializeField] private float scaleFactor = 1.15f;
    [SerializeField] private float animationDuration = 2f;

    private Vector2 _defaultImageSize;

    // Interaction Area Variable
    [Header("Patient selection area")] [SerializeField]
    private RectTransform interactionArea;

    [SerializeField] private RectTransform imageCharacter;
    [SerializeField] private RectTransform imageCharacterPhone;
    public GameObject imageRingtone;

    private Vector2 _targetPosition;

    [Header("Animation patient area")] [SerializeField]
    public RectTransform spawnPatientArea;

    private readonly float _imageRatio = 1.25f;

    [Header("Doors")] [SerializeField] private RectTransform leftDoor;
    [SerializeField] private RectTransform rightDoor;

    // Doors variables
    public float openAngle = -90f;
    private bool _isOpen;

    [Header("FadeAnimation")] [SerializeField]
    public CanvasGroup fadePanel;

    private NewPatientData _newPatientData;

    /// <summary>
    /// Set a new target position to the sprite to stimule life in the UI.
    /// </summary>
    private void SetNewTargetPosition(RectTransform targetArea){
        // Set the sprite to new target (spawnArea & interactionArea)
        if (imageCharacter.transform.parent != targetArea) {
            imageCharacter.SetParent(targetArea);
        }

        // Reduce the width and height of the image in the spawnArea else we keep the default size of the image.
        if (targetArea.name == "SpawnArea") {
            _defaultImageSize = imageCharacter.sizeDelta;
            imageCharacter.sizeDelta = _defaultImageSize / _imageRatio;
        } else {
            imageCharacter.sizeDelta = _defaultImageSize;
        }

        // Move the sprite around the area
        float panelWidth = targetArea.GetComponent<RectTransform>().rect.width;
        float panelHeight = targetArea.GetComponent<RectTransform>().rect.height;

        float imageWidth = imageCharacter.rect.width;
        float imageHeight = imageCharacter.rect.height;

        float randomX = Random.Range(-panelWidth / 2 + imageWidth / 2, panelWidth / 2 - imageWidth / 2);
        float randomY = Random.Range(-panelHeight / 2 + imageHeight / 2, panelHeight / 2 - imageHeight / 2);

        _targetPosition = new Vector2(randomX, randomY);

        imageCharacter.anchoredPosition = _targetPosition;
    }

    /// <summary>
    /// Sets the sprite image for the character and adjusts the image size to match the sprite's dimensions.
    /// Logs an error if the sprite library is not initialized.
    /// </summary>
    /// <param name="characterSprite">The sprite to display for the character.</param>
    private void SetSprite(Sprite characterSprite){
        imageCharacter.GetComponent<Image>().sprite = characterSprite;
        imageCharacter.sizeDelta = new Vector2(characterSprite.rect.width, characterSprite.rect.height) / 1.3f;
    }

    /// <summary>
    /// Make a yo-yo animation on the sprite.
    /// </summary>
    private void AnimationSizeImage(bool isPhone){
        RectTransform rectTransform = isPhone ? imageCharacterPhone : imageCharacter;
        rectTransform.DOSizeDelta(rectTransform.sizeDelta * scaleFactor, animationDuration / 2)
            .SetLoops(-1, LoopType.Yoyo).SetEase(Ease.InOutSine).SetId("sizeAnim");
    }

    /// <summary>
    /// Stop the Dotween Yo-yo animation on the character sprite.
    /// </summary>
    public static void StopAnimation(){
        DOTween.Kill("sizeAnim");
    }

    /// <summary>
    /// Open/Close doors between 0 and a targetAngle (openAngle)
    /// </summary>
    private void ToggleDoor(){
        // move door postion and mor angle (-145/145 degree)
        float targetAngle = _isOpen ? 0f : openAngle;
        rightDoor.rotation = Quaternion.Euler(0, targetAngle, 0);
        leftDoor.rotation = Quaternion.Euler(0, -targetAngle, 0);
        _isOpen = !_isOpen;
    }
    
    /// <summary>
    /// Play a sort of animations like open the doors and spawn the 'patient' in the doors area then fade out the screen by invoking 'FadeOut' function after a delai.
    /// </summary>
    public void SetCharacterInArea(NewPatientData newPatientData){
        _newPatientData = newPatientData;
        if (newPatientData.isOnPhone) {
            imageRingtone.SetActive(true);
            imageCharacter.gameObject.SetActive(false);
            imageCharacterPhone.GetComponent<Button>().enabled = false;
        } else {
            imageRingtone.SetActive(false);
            imageCharacter.gameObject.SetActive(true);
            imageCharacter.GetComponent<Button>().enabled = false;
            imageCharacterPhone.GetComponent<Button>().enabled = false;
            ToggleDoor();
            SetSprite(newPatientData.characterSprites[0]);
            SetNewTargetPosition(spawnPatientArea);
        }

        if (GameManager.Instance.instanteAnimation) {
            FadeOut();
        } else {
            Invoke(nameof(FadeOut), 1f);
        }
    }

    /// <summary>
    /// Fade out animation with DOTWeen and invoking 'ContinueSetNewCharacter' function after a short delai 1 sec.
    /// Black to transparency
    /// </summary>
    private void FadeOut(){
        if (GameManager.Instance.instanteAnimation) {
            fadePanel.DOFade(0f, 0f).SetEase(Ease.Linear);
            ContinueSetNewCharacter();
        } else {
            fadePanel.DOFade(0f, 1f).SetEase(Ease.Linear);
            Invoke(nameof(ContinueSetNewCharacter), 1f);
        }
    }


    /// <summary>
    /// Play the last animations like close door and set new patient in the interaction area then invoking 'FadeIn' function after a delai.
    /// </summary>
    private void ContinueSetNewCharacter(){
        if (!_newPatientData.isOnPhone) {
            ToggleDoor();
            SetNewTargetPosition(interactionArea);
        }

        AnimationSizeImage(_newPatientData.isOnPhone);
        if (GameManager.Instance.instanteAnimation) {
            FadeIn();
        } else {
            Invoke(nameof(FadeIn), 1f);
        }
    }

    /// <summary>
    /// Fade in animation.
    /// Transparency to black.
    /// </summary>
    private void FadeIn(){
        fadePanel.DOFade(1f, 1f).SetEase(Ease.Linear);

        if (_newPatientData.isOnPhone) {
            imageCharacterPhone.GetComponent<Button>().enabled = true;
        } else {
            imageCharacter.GetComponent<Button>().enabled = true;
        }
    }
}