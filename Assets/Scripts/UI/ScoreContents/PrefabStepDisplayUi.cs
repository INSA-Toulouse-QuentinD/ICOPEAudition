using UnityEngine;
using System.Collections.Generic;
using PatientData.AlgoData;
using System.Text.RegularExpressions;
using System;

namespace UI.ScoreContents
{
    public class PrefabStepDisplayUi : MonoBehaviour
    {
        [Header("Player details steps")]
        [SerializeField] private List<DetailsFields> textfieldsList;

        [Header("Sprite")]
        [SerializeField] private Sprite _correctSprite;
        [SerializeField] private Sprite _incorrectSprite;


        public void SetText(string stepName, List<string> diagAnswer, List<string> actionAnswer)
        {
            Debug.Log(stepName);
            
            HideFields();

            int indexDiag = 0;
            int indexAction = 0;

            foreach (DetailsFields fields in textfieldsList)
            {
                if (fields.fieldsName == FieldsName.StepName)
                {
                    fields.fields.text = GetStepName(stepName);
                }
                else if (fields.fieldsName == FieldsName.DiagnosticsDetails & indexDiag < diagAnswer.Count)
                {
                    SetFields(fields, diagAnswer[indexDiag], indexDiag == diagAnswer.Count - 1);
                    indexDiag++;
                }
                else if (fields.fieldsName == FieldsName.ActionsDetails & indexAction < actionAnswer.Count)
                {
                    SetFields(fields, actionAnswer[indexAction], indexAction == actionAnswer.Count - 1);
                    indexAction++;
                }
            }
        }

        private void SetFields(DetailsFields fields, string text, bool isSuccess)
        {
            fields.fields.text = text;
            fields.image.sprite = isSuccess ? _correctSprite : _incorrectSprite;
            fields.fields.transform.parent.gameObject.SetActive(true);
        }

        private void HideFields()
        {
            foreach (DetailsFields fields in textfieldsList)
            {
                if (fields.fieldsName == FieldsName.DiagnosticsDetails || fields.fieldsName == FieldsName.ActionsDetails)
                    fields.fields.transform.parent.gameObject.SetActive(false);
            }
        }

        private static string GetStepName(string stepName)
        {
            // questionnary_0
            var (step, index) = SplitNameAndIndex(stepName);
            
            string realName = "";
            switch (step)
            {
                case Step.CasePresentation:
                    realName = "Etape: Presentation du patient";
                    break;
                case Step.WisperTest:
                    realName = "Etape: Test de chuchotement";
                    break;
                case Step.Questionnary:
                    realName = "Etape: Questionnaire Go-No-Go";
                    break;
                case Step.Otoscopy:
                    realName = "Etape: Vidéo otoscopie";
                    break;
                case Step.WeberTest:
                    realName = "Etape: Test Weber";
                    break;
                case Step.HhiesTest:
                    realName = "Etape: Test HHIE-S";
                    break;
                case Step.Audiometry:
                    realName = "Etape: Audiométrie";
                    break;
            }
            realName = realName + index == null ? realName : realName+" "+(index + 1).ToString() ;
            return realName;
        }

        public static (Step step, int? index) SplitNameAndIndex(string input)
        {
            string namePart = input;
            int? numberPart = null;

            Match match = Regex.Match(input, @"^(.*)_(\d+)$");
            if (match.Success)
            {
                namePart = match.Groups[1].Value;
                numberPart = int.Parse(match.Groups[2].Value);
                if (numberPart == 0)
                    numberPart = null;
            }

            return ((Step)Enum.Parse(typeof(Step), namePart) , numberPart);
        } 
    }
}
