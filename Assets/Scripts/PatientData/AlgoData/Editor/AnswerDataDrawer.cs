using UnityEditor;
using UnityEngine;

namespace Assets.Scripts.PatientData.AlgoData.Editor{
    [CustomPropertyDrawer(typeof(AnswerData))]
    public class AnswerDataDrawer : PropertyDrawer{
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label){
            EditorGUI.BeginProperty(position, label, property);

            SerializedProperty isCorrectProp = property.FindPropertyRelative("isCorrect");
            SerializedProperty answerTextProp = property.FindPropertyRelative("answerText");

            bool isCorrect = isCorrectProp != null && isCorrectProp.boolValue;
            string title = answerTextProp != null && !string.IsNullOrWhiteSpace(answerTextProp.stringValue)
                ? answerTextProp.stringValue
                : label.text;

            // Foldout header with colored title
            Rect foldRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
            property.isExpanded = EditorGUI.Foldout(foldRect, property.isExpanded, GUIContent.none, true);

            Color old = GUI.color;
            GUI.color = isCorrect ? new Color(0.25f, 0.75f, 0.25f) : new Color(0.85f, 0.25f, 0.25f);
            EditorGUI.LabelField(foldRect, title, EditorStyles.boldLabel);
            GUI.color = old;

            if (property.isExpanded) {
                EditorGUI.indentLevel++;
                Rect contentRect = new Rect(position.x, foldRect.yMax + EditorGUIUtility.standardVerticalSpacing,
                    position.width,
                    position.height - EditorGUIUtility.singleLineHeight - EditorGUIUtility.standardVerticalSpacing);
                EditorGUI.PropertyField(contentRect, property, GUIContent.none, true);
                EditorGUI.indentLevel--;
            }

            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label){
            float h = EditorGUIUtility.singleLineHeight;
            if (!property.isExpanded)
                return h;

            // Draw children without the root foldout label
            return h + EditorGUIUtility.standardVerticalSpacing + EditorGUI.GetPropertyHeight(property, true);
        }
    }
}