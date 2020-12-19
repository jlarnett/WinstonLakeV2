using UnityEngine;
using UnityEditor;
using UnityEditor.AnimatedValues;
using UnityEngine.Events;
using UnityEditorInternal;
using System.Linq;


namespace DevionGames
{
    [CustomEditor(typeof(BehaviorTrigger), true)]
    public class BehaviorTriggerInspector : BaseTriggerInspector
    {
        private void DrawInspector()
        {
            EditorGUI.BeginDisabledGroup(EditorApplication.isPlaying);
            if (EditorTools.RightArrowButton(new GUIContent("Edit Behavior", "Trigger use behavior"), GUILayout.Height(20f)))
            {
                SerializedProperty actionList = serializedObject.FindProperty("actions");
                for (int i = 0; i < actionList.arraySize; i++) {
                    SerializedProperty element = actionList.GetArrayElementAtIndex(i);
                    if (element.GetValue() == null) {
                        serializedObject.Update();
                        element.managedReferenceValue = new MissingAction();
                        serializedObject.ApplyModifiedProperties();
                    }
                }
                ObjectWindow.ShowWindow("Edit Behavior",serializedObject, actionList);
            }
            EditorGUI.EndDisabledGroup();
        }


    }
}
