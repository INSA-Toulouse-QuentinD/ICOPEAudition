using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace PatientData.AlgoData.Editor{
    /// <summary>
    /// Custom drawer to show only relevant fields for AlgoStep depending on its Step type.
    /// This is editor-only and does not affect runtime behavior.
    /// </summary>
    [CustomPropertyDrawer(typeof(AlgoStep))]
    public class AlgoStepDrawer : PropertyDrawer{
        private static readonly Dictionary<string, ReorderableList> DiagnosticLists = new();

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
            SerializedProperty diagnosticQuestionProp = property.FindPropertyRelative("overrideDiagnosticQuestion");
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

            if (stepType == Step.GoNoGo) {
                line = new Rect(position.x, y, position.width, EditorGUI.GetPropertyHeight(predefinedProp, true));
                EditorGUI.PropertyField(line, predefinedProp, true);
                y = Next(line);
            }

            if (stepType is Step.Otoscopy or Step.Hhies or Step.Audiometry) {
                line = new Rect(position.x, y, position.width, EditorGUI.GetPropertyHeight(spriteEarExamsProp, true));
                EditorGUI.PropertyField(line, spriteEarExamsProp, true);
                y = Next(line);
            }

            // Diagnostic phase foldout: the override question lives inside this section.
            string diagnosticLabelText = "Diagnostic Phase";
            if (stepType == Step.Audiometry && diagnosticQuestionProp != null && !string.IsNullOrWhiteSpace(diagnosticQuestionProp.stringValue)) {
                diagnosticLabelText += $" ({diagnosticQuestionProp.stringValue.Trim()})";
            }

            Rect diagnosticFoldRect = new Rect(position.x, y, position.width, EditorGUIUtility.singleLineHeight);
            EditorGUI.DrawRect(diagnosticFoldRect, EditorGUIUtility.isProSkin ? new Color(0.18f, 0.18f, 0.18f, 1f) : new Color(0.78f, 0.78f, 0.78f, 1f));

            float titleFieldWidth = 60f;
            Rect foldRect = new Rect(diagnosticFoldRect.x + 1f, y, Mathf.Max(0f, diagnosticFoldRect.width - titleFieldWidth - 3f), EditorGUIUtility.singleLineHeight);
            diagnosticPhaseProp.isExpanded = EditorGUI.Foldout(foldRect, diagnosticPhaseProp.isExpanded, diagnosticLabelText, true);

            Rect sizeFieldRect = new Rect(diagnosticFoldRect.xMax - titleFieldWidth - 2f, y + 1f, titleFieldWidth, EditorGUIUtility.singleLineHeight - 2f);
            EditorGUI.BeginChangeCheck();
            int newSize = EditorGUI.DelayedIntField(sizeFieldRect, GUIContent.none, diagnosticPhaseProp.arraySize);
            if (EditorGUI.EndChangeCheck())
                diagnosticPhaseProp.arraySize = Mathf.Max(0, newSize);

            y = Next(diagnosticFoldRect);

            if (diagnosticPhaseProp.isExpanded) {
                EditorGUI.indentLevel++;
                const float listIndentOffset = 15f;

                if (stepType == Step.Audiometry && diagnosticQuestionProp != null) {
                    line = new Rect(5f, y, position.width - listIndentOffset, EditorGUI.GetPropertyHeight(diagnosticQuestionProp, true));
                    EditorGUI.PropertyField(line, diagnosticQuestionProp, true);
                    y = Next(line);
                }

                ReorderableList list = GetDiagnosticList(property, diagnosticPhaseProp);
                float listHeight = list.GetHeight();
                Rect listRect = new Rect(position.x + listIndentOffset, y, position.width - listIndentOffset, listHeight);
                list.DoList(listRect);
                y = Next(listRect);

                EditorGUI.indentLevel--;
            }

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
            SerializedProperty diagnosticQuestionProp = property.FindPropertyRelative("overrideDiagnosticQuestion");
            SerializedProperty diagnosticPhaseProp = property.FindPropertyRelative("diagnosticPhase");
            SerializedProperty actionPhaseProp = property.FindPropertyRelative("actionPhase");

            Step stepType = (Step)typeProp.enumValueIndex;

            if (stepType is Step.WisperTest or Step.WeberTest)
                h += EditorGUIUtility.standardVerticalSpacing + EditorGUI.GetPropertyHeight(dialoguePatientProp, true);

            if (stepType == Step.GoNoGo)
                h += EditorGUIUtility.standardVerticalSpacing + EditorGUI.GetPropertyHeight(predefinedProp, true);

            if (stepType is Step.Otoscopy or Step.Hhies or Step.Audiometry)
                h += EditorGUIUtility.standardVerticalSpacing + EditorGUI.GetPropertyHeight(spriteEarExamsProp, true);

            h += EditorGUIUtility.standardVerticalSpacing + EditorGUIUtility.singleLineHeight; // diagnostic phase foldout

            if (diagnosticPhaseProp.isExpanded) {
                if (stepType == Step.Audiometry && diagnosticQuestionProp != null)
                    h += EditorGUIUtility.standardVerticalSpacing + EditorGUI.GetPropertyHeight(diagnosticQuestionProp, true);

                ReorderableList list = GetDiagnosticList(property, diagnosticPhaseProp);
                h += EditorGUIUtility.standardVerticalSpacing + list.GetHeight();
            }

            h += EditorGUIUtility.standardVerticalSpacing + EditorGUI.GetPropertyHeight(actionPhaseProp, true);

            return h;
        }

        private static float Next(Rect last){
            return last.y + last.height + EditorGUIUtility.standardVerticalSpacing;
        }

        private static ReorderableList GetDiagnosticList(SerializedProperty rootProperty, SerializedProperty arrayProperty){
            string key = $"{rootProperty.serializedObject.targetObject.GetInstanceID()}|{rootProperty.propertyPath}.diagnosticPhase";

            if (!DiagnosticLists.TryGetValue(key, out ReorderableList list) || list == null) {
                list = new ReorderableList(rootProperty.serializedObject, arrayProperty, true, false, true, true);
                list.drawHeaderCallback = _ => { };
                list.drawElementCallback = (rect, index, _, _) => {
                    SerializedProperty elementProp = list.serializedProperty.GetArrayElementAtIndex(index);
                    rect.x += 9f;
                    rect.width -= 18f;
                    rect.y += 1f;
                    rect.height = EditorGUI.GetPropertyHeight(elementProp, true);
                    EditorGUI.PropertyField(rect, elementProp, GUIContent.none, true);
                };
                list.elementHeightCallback = index => {
                    SerializedProperty elementProp = list.serializedProperty.GetArrayElementAtIndex(index);
                    return EditorGUI.GetPropertyHeight(elementProp, true) + EditorGUIUtility.standardVerticalSpacing;
                };
                list.onAddCallback = l => {
                    SerializedProperty array = l.serializedProperty;
                    array.arraySize++;
                    array.serializedObject.ApplyModifiedProperties();
                };
                list.onRemoveCallback = l => {
                    SerializedProperty array = l.serializedProperty;
                    if (array.arraySize > 0) {
                        array.DeleteArrayElementAtIndex(l.index);
                        if (l.index >= array.arraySize)
                            l.index = array.arraySize - 1;
                        array.serializedObject.ApplyModifiedProperties();
                    }
                };

                DiagnosticLists[key] = list;
            }

            list.serializedProperty = arrayProperty;
            return list;
        }

    private static string NicifyStepName(string enumDisplayName){
      // Unity's enumDisplayNames already removes underscores in many cases,
      // but we keep this to ensure consistent output.
      if (string.IsNullOrEmpty(enumDisplayName)) return "Step";
      return ObjectNames.NicifyVariableName(enumDisplayName.Replace("_", " "));
    }
    }
}