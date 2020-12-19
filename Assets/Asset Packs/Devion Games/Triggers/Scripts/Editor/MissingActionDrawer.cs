using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace DevionGames
{
    [CustomPropertyDrawer(typeof(MissingAction))]
    public class MissingActionDrawer : PropertyDrawer
    {
        private string errorMessage = "The associated action script can not be loaded. Please replace it with a valid action.";

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.HelpBox(position, errorMessage, MessageType.Warning);
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return 42f;
        }
    }
}
