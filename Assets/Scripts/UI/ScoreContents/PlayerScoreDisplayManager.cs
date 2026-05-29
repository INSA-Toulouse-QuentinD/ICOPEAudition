using Managers;
using PatientData.AlgoData;
using System.Collections.Generic;
using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI.ScoreContents{
    [System.Serializable]
    public enum FieldsName{
        PatientName,
        StepSuccess,
        StepFailed,
        DiagnoticsSuccess,
        DiagnosticsFailed,
        ActionSuccess,
        ActionFailed,
        Scores,
        SuccessRate,
        StepName,
        DiagnosticsDetails,
        ActionsDetails
    }


    [System.Serializable]
    public class FieldsTable{
        public FieldsName fieldsName;
        public TextMeshProUGUI fields;
    }

    [System.Serializable]
    public class DetailsFields{
        public FieldsName fieldsName;
        public TextMeshProUGUI fields;
        public Image image;
    }

    public class PlayerScoreDisplayManager : MonoBehaviour{
        [Header("Section synthese patient")] [SerializeField]
        private List<FieldsTable> fieldsList;

        [Header("Script Open folder")] [SerializeField]
        private OpenFolder openFolder;

        private List<Step> _stepsList;

        private void SetSyntheseScore(string patientName, int nbStepSucc, int nbStepFailed, int nbDiagSucc,
            int nbDiagFailed, int nbActionSucc, int nbActionFailed, float succesRate){
            foreach (FieldsTable fieldsTable in fieldsList) {
                switch (fieldsTable.fieldsName) {
                    case FieldsName.PatientName:
                        fieldsTable.fields.text = patientName;
                        break;
                    case FieldsName.StepSuccess:
                        fieldsTable.fields.text = nbStepSucc.ToString();
                        break;
                    case FieldsName.StepFailed:
                        fieldsTable.fields.text = nbStepFailed.ToString();
                        break;
                    case FieldsName.DiagnoticsSuccess:
                        fieldsTable.fields.text = nbDiagSucc.ToString();
                        break;
                    case FieldsName.DiagnosticsFailed:
                        fieldsTable.fields.text = nbDiagFailed.ToString();
                        break;
                    case FieldsName.ActionSuccess:
                        fieldsTable.fields.text = nbActionSucc.ToString();
                        break;
                    case FieldsName.ActionFailed:
                        fieldsTable.fields.text = nbActionFailed.ToString();
                        break;
                    case FieldsName.Scores:
                        fieldsTable.fields.text =
                            ((nbStepSucc - nbStepFailed) * 10 +
                             (nbDiagSucc - nbDiagFailed + nbActionSucc + nbActionFailed) * 5).ToString();
                        break;
                    case FieldsName.SuccessRate:
                        fieldsTable.fields.text = Mathf.RoundToInt(succesRate) + "%";
                        GameManager.Instance.Money += Mathf.RoundToInt(succesRate / 5f);
                        break;
                }
            }
        }

        public void GetSetDisplayScore(string patientName){
            // Get Patient data form patient score
            var pRecords = GameManager.Instance.GameData.GetPatientCaseRecords(patientName);

            SetSyntheseScore(patientName, pRecords.NumberStepSucceed, pRecords.NumberStepFailed,
                pRecords.NumberDiagCorrect, pRecords.NumberDiagIncorrect, pRecords.NumberActionCorrect,
                pRecords.NumberActionIncorrect, pRecords.SuccessRate);
            openFolder.SetStepsRecords(pRecords.StepRecordsLevel);
        }
    }
}