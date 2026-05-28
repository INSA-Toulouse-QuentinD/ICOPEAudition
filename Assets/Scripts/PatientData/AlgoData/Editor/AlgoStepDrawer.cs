using UnityEditor;
using UnityEngine;

namespace PatientData.AlgoData.Editor{
    /// <summary>
    /// Custom drawer to show only relevant fields for AlgoStep depending on its Step type.
    /// This is editor-only and does not affect runtime behavior.
    /// </summary>
    [CustomPropertyDrawer(typeof(AlgoStep))]
    public class AlgoStepDrawer : PropertyDrawer{
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label){
            EditorGUI.BeginProperty(position, label, property);

			// Replace default "Element X" label by the selected Step enum value (nicely formatted)
			SerializedProperty typePropForLabel = property.FindPropertyRelative("type");
			if (typePropForLabel != null && typePropForLabel.propertyType == SerializedPropertyType.Enum) {
				string raw = typePropForLabel.enumDisplayNames[typePropForLabel.enumValueIndex];
				label = new GUIContent(NicifyStepName(raw));
			}

            // Foldout header
            position.height = EditorGUIUtility.singleLineHeight;
            property.isExpanded = EditorGUI.Foldout(position, property.isExpanded, label, true);
            if (!property.isExpanded) {
                EditorGUI.EndProperty();
                return;
            }

            EditorGUI.indentLevel++;

            var y = position.y + EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
            var line = new Rect(position.x, y, position.width, EditorGUIUtility.singleLineHeight);

            SerializedProperty typeProp = property.FindPropertyRelative("type");
            SerializedProperty dialoguePatientProp = property.FindPropertyRelative("dialoguePatient");
            SerializedProperty predefinedProp = property.FindPropertyRelative("predefinedAnswer");
            SerializedProperty spriteEarExamsProp = property.FindPropertyRelative("spriteEarExams");
            SerializedProperty diagnosticPhaseProp = property.FindPropertyRelative("diagnosticPhase");
            SerializedProperty actionPhaseProp = property.FindPropertyRelative("actionPhase");

            EditorGUI.PropertyField(line, typeProp);
            y = Next(line);

            Step stepType = (Step)typeProp.enumValueIndex;

            // Show/hide fields based on the step type.
            if (stepType is Step.WisperTest or Step.WeberTest) {
                line = new Rect(position.x, y, position.width, EditorGUI.GetPropertyHeight(dialoguePatientProp, true));
                EditorGUI.PropertyField(line, dialoguePatientProp, true);
                y = Next(line);
            }

            if (stepType == Step.Questionnary) {
                line = new Rect(position.x, y, position.width, EditorGUI.GetPropertyHeight(predefinedProp, true));
                EditorGUI.PropertyField(line, predefinedProp, true);
                y = Next(line);
            }

            if (stepType is Step.Otoscopy or Step.HhiesTest or Step.Audiometry) {
                line = new Rect(position.x, y, position.width, EditorGUI.GetPropertyHeight(spriteEarExamsProp, true));
                EditorGUI.PropertyField(line, spriteEarExamsProp, true);
                y = Next(line);
            }

            // Phases are generally relevant; keep them visible.
            line = new Rect(position.x, y, position.width, EditorGUI.GetPropertyHeight(diagnosticPhaseProp, true));
            EditorGUI.PropertyField(line, diagnosticPhaseProp, true);
            y = Next(line);

            line = new Rect(position.x, y, position.width, EditorGUI.GetPropertyHeight(actionPhaseProp, true));
            EditorGUI.PropertyField(line, actionPhaseProp, true);

            EditorGUI.indentLevel--;
            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label){
            float h = EditorGUIUtility.singleLineHeight; // foldout
            if (!property.isExpanded)
                return h;

            h += EditorGUIUtility.standardVerticalSpacing + EditorGUIUtility.singleLineHeight; // type

            SerializedProperty typeProp = property.FindPropertyRelative("type");
            SerializedProperty dialoguePatientProp = property.FindPropertyRelative("dialoguePatient");
            SerializedProperty predefinedProp = property.FindPropertyRelative("predefinedAnswer");
            SerializedProperty spriteEarExamsProp = property.FindPropertyRelative("spriteEarExams");
            SerializedProperty diagnosticPhaseProp = property.FindPropertyRelative("diagnosticPhase");
            SerializedProperty actionPhaseProp = property.FindPropertyRelative("actionPhase");

            Step stepType = (Step)typeProp.enumValueIndex;

            if (stepType is Step.WisperTest or Step.WeberTest)
                h += EditorGUIUtility.standardVerticalSpacing + EditorGUI.GetPropertyHeight(dialoguePatientProp, true);

            if (stepType == Step.Questionnary)
                h += EditorGUIUtility.standardVerticalSpacing + EditorGUI.GetPropertyHeight(predefinedProp, true);

            if (stepType is Step.Otoscopy or Step.HhiesTest or Step.Audiometry)
                h += EditorGUIUtility.standardVerticalSpacing + EditorGUI.GetPropertyHeight(spriteEarExamsProp, true);

            h += EditorGUIUtility.standardVerticalSpacing + EditorGUI.GetPropertyHeight(diagnosticPhaseProp, true);
            h += EditorGUIUtility.standardVerticalSpacing + EditorGUI.GetPropertyHeight(actionPhaseProp, true);

            return h;
        }

        private static float Next(Rect last){
            return last.y + last.height + EditorGUIUtility.standardVerticalSpacing;
        }

    private static string NicifyStepName(string enumDisplayName){
      // Unity's enumDisplayNames already removes underscores in many cases,
      // but we keep this to ensure consistent output.
      if (string.IsNullOrEmpty(enumDisplayName)) return "Step";
      return ObjectNames.NicifyVariableName(enumDisplayName.Replace("_", " "));
    }
    }
}