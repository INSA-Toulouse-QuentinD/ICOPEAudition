using UnityEditor;
using UnityEngine;

namespace PatientData.AlgoData.Editor{
    [CustomPropertyDrawer(typeof(AnswerData))]
    public class AnswerDataDrawer : PropertyDrawer{
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label){
            EditorGUI.BeginProperty(position, label, property);

            SerializedProperty isCorrectProp = property.FindPropertyRelative("isCorrect");
            SerializedProperty answerTextProp = property.FindPropertyRelative("answerText");

            bool isCorrect = isCorrectProp is{ boolValue: true };
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

                float y = foldRect.yMax + EditorGUIUtility.standardVerticalSpacing;
                SerializedProperty child = property.Copy();
                SerializedProperty endProperty = child.GetEndProperty();

                if (child.NextVisible(true)) {
                    do {
                        if (SerializedProperty.EqualContents(child, endProperty))
                            break;

                        float childHeight = EditorGUI.GetPropertyHeight(child, true);
                        Rect childRect = new Rect(position.x, y, position.width, childHeight);
                        EditorGUI.PropertyField(childRect, child, true);
                        y += childHeight + EditorGUIUtility.standardVerticalSpacing;
                    } while (child.NextVisible(false));
                }

                EditorGUI.indentLevel--;
            }

            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label){
            float h = EditorGUIUtility.singleLineHeight;
            if (!property.isExpanded)
                return h;

            float childrenHeight = 0f;
            SerializedProperty child = property.Copy();
            SerializedProperty endProperty = child.GetEndProperty();

            if (child.NextVisible(true)) {
                do {
                    if (SerializedProperty.EqualContents(child, endProperty))
                        break;

                    childrenHeight += EditorGUI.GetPropertyHeight(child, true) +
                                      EditorGUIUtility.standardVerticalSpacing;
                } while (child.NextVisible(false));
            }

            return h + EditorGUIUtility.standardVerticalSpacing + childrenHeight;
        }
    }
}