using UnityEngine;
using System.Collections.Generic;
using PatientData.AlgoData;
using System.Text.RegularExpressions;
using System;

namespace UI.ScoreContents{
    public class PrefabStepDisplayUi : MonoBehaviour{
        [Header("Player details steps")] [SerializeField]
        private List<DetailsFields> textFieldsList;

        [Header("Sprite")] [SerializeField] private Sprite correctSprite;
        [SerializeField] private Sprite incorrectSprite;

        public void SetText(string stepName, List<string> diagAnswer, List<string> actionAnswer, int stepNumber){
            HideFields();

            int indexDiag = 0;
            int indexAction = 0;

            foreach (DetailsFields fields in textFieldsList) {
                if (fields.fieldsName == FieldsName.StepName) {
                    fields.fields.text = GetStepName(stepName, stepNumber);
                } else if (fields.fieldsName == FieldsName.DiagnosticsDetails & indexDiag < diagAnswer.Count) {
                    SetFields(fields, diagAnswer[indexDiag], indexDiag == diagAnswer.Count - 1);
                    indexDiag++;
                } else if (fields.fieldsName == FieldsName.ActionsDetails & indexAction < actionAnswer.Count) {
                    SetFields(fields, actionAnswer[indexAction], indexAction == actionAnswer.Count - 1);
                    indexAction++;
                }
            }
        }

        private void SetFields(DetailsFields fields, string text, bool isSuccess){
            fields.fields.text = text;
            fields.image.sprite = isSuccess ? correctSprite : incorrectSprite;
            fields.fields.transform.parent.gameObject.SetActive(true);
        }

        private void HideFields(){
            foreach (DetailsFields fields in textFieldsList) {
                if (fields.fieldsName == FieldsName.DiagnosticsDetails ||
                    fields.fieldsName == FieldsName.ActionsDetails)
                    fields.fields.transform.parent.gameObject.SetActive(false);
            }
        }

        private static string GetStepName(string stepName, int stepNumber){
            (Step step, int? index) = SplitNameAndIndex(stepName);

            string realName = "";
            switch (step) {
                case Step.CasePresentation:
                    realName = $"Etape {stepNumber}: Presentation du patient";
                    break;
                case Step.WisperTest:
                    realName = $"Etape {stepNumber}: Test de chuchotement";
                    break;
                case Step.GoNoGo:
                    realName = $"Etape {stepNumber}: Questionnaire Go-No-Go";
                    break;
                case Step.Otoscopy:
                    realName = $"Etape {stepNumber}: Vidéo otoscopie";
                    break;
                case Step.WeberTest:
                    realName = $"Etape {stepNumber}: Test Weber";
                    break;
                case Step.Hhies:
                    realName = $"Etape {stepNumber}: Test HHIE-S";
                    break;
                case Step.Audiometry:
                    realName = $"Etape {stepNumber}: Audiométrie";
                    break;
            }

            realName = realName + " " + (index + 1);
            return realName;
        }

        public static (Step step, int? index) SplitNameAndIndex(string input){
            string namePart = input;
            int? numberPart = null;

            Match match = Regex.Match(input, @"^(.*)_(\d+)$");
            if (match.Success) {
                namePart = match.Groups[1].Value;
                numberPart = int.Parse(match.Groups[2].Value);
                if (numberPart == 0)
                    numberPart = null;
            }

            return ((Step)Enum.Parse(typeof(Step), namePart), numberPart);
        }
    }
}