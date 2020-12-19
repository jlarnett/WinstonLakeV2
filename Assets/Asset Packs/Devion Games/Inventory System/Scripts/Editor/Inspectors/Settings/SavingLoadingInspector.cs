using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.AnimatedValues;
using UnityEngine.Events;
using System.Linq;
using System;

namespace DevionGames.InventorySystem.Configuration
{
    [CustomEditor(typeof(SavingLoading))]
    public class SavingLoadingInspector : Editor
    {
        private SerializedProperty m_Script;
        private SerializedProperty m_AutoSave;
        private AnimBool m_ShowSave;
        private SerializedProperty m_Provider;
        private AnimBool m_ShowMySQL;

        private SerializedProperty m_SavingKey;
        private SerializedProperty m_SavingRate;
        private SerializedProperty m_ServerAdress;
        private SerializedProperty m_SaveScript;
        private SerializedProperty m_LoadScript;

        protected virtual void OnEnable()
        {
            this.m_Script = serializedObject.FindProperty("m_Script");
            this.m_AutoSave = serializedObject.FindProperty("autoSave");
            this.m_ShowSave = new AnimBool(this.m_AutoSave.boolValue);
            this.m_ShowSave.valueChanged.AddListener(new UnityAction(Repaint));

            this.m_Provider = serializedObject.FindProperty("provider");
            this.m_ShowMySQL = new AnimBool(this.m_Provider.enumValueIndex == 1);
            this.m_ShowMySQL.valueChanged.AddListener(new UnityAction(Repaint));
            

            this.m_SavingKey = serializedObject.FindProperty("savingKey");
            this.m_SavingRate = serializedObject.FindProperty("savingRate");
            this.m_ServerAdress = serializedObject.FindProperty("serverAdress");
            this.m_SaveScript = serializedObject.FindProperty("saveScript");
            this.m_LoadScript = serializedObject.FindProperty("loadScript");
        }

        public override void OnInspectorGUI()
        {
            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.PropertyField(this.m_Script);
            EditorGUI.EndDisabledGroup();

            serializedObject.Update();
            EditorGUILayout.PropertyField(this.m_AutoSave);
            this.m_ShowSave.target = this.m_AutoSave.boolValue;
            if (EditorGUILayout.BeginFadeGroup(this.m_ShowSave.faded))
            {
                EditorGUI.indentLevel = EditorGUI.indentLevel + 1;
                EditorGUILayout.PropertyField(this.m_SavingKey);
                EditorGUILayout.PropertyField(this.m_SavingRate);

                EditorGUILayout.PropertyField(m_Provider);
                this.m_ShowMySQL.target = m_Provider.enumValueIndex == 1;
                if (EditorGUILayout.BeginFadeGroup(this.m_ShowMySQL.faded))
                {

           
                    EditorGUILayout.PropertyField(this.m_ServerAdress);
                    EditorGUILayout.PropertyField(this.m_SaveScript);
                    EditorGUILayout.PropertyField(this.m_LoadScript);
                }
                EditorGUILayout.EndFadeGroup();

                EditorGUI.indentLevel = EditorGUI.indentLevel - 1;
            }
            EditorGUILayout.EndFadeGroup();
            GUILayout.Space(2f);
            EditorTools.Seperator();

            string data = PlayerPrefs.GetString("SavedKeys");
            if (!string.IsNullOrEmpty(data))
            {
                string[] keys = data.Split(';').Distinct().ToArray();
                Array.Reverse(keys);
                ArrayUtility.Remove<string>(ref keys, "");
                ArrayUtility.Remove<string>(ref keys, string.Empty);

                bool state = EditorPrefs.GetBool("SavedData", false);
                bool foldout = EditorGUILayout.Foldout(state, "Saved Data " + keys.Length.ToString(), true);
                if (foldout != state)
                {
                    EditorPrefs.SetBool("SavedData", foldout);
                }

                if (foldout)
                {
                    EditorGUI.indentLevel += 1;
                    if (keys.Length == 0)
                    {
                        GUILayout.BeginHorizontal();
                        GUILayout.Space(16f);
                        GUILayout.BeginVertical();
                        GUILayout.Label("No data saved on this device!");
                        GUILayout.EndVertical();
                        GUILayout.EndHorizontal();
                    }

                    for (int i = 0; i < keys.Length; i++)
                    {
                        string key = keys[i];
                        if (!string.IsNullOrEmpty(key))
                        {
                            state = EditorPrefs.GetBool(key, false);
                            GUILayout.BeginHorizontal();
                            foldout = EditorGUILayout.Foldout(state, key, true);
                            Rect rect = GUILayoutUtility.GetLastRect();
                            if (Event.current.type == EventType.MouseDown && Event.current.button == 1 && rect.Contains(Event.current.mousePosition))
                            {
                                GenericMenu menu = new GenericMenu();
                                menu.AddItem(new GUIContent("Delete"), false, delegate () {
                                    PlayerPrefs.DeleteKey(key);
                                    PlayerPrefs.SetString("SavedKeys", data.Replace(key, ""));
                                });
                                menu.ShowAsContext();
                            }
                            GUILayout.EndHorizontal();
                            if (foldout != state)
                            {
                                EditorPrefs.SetBool(key, foldout);
                            }
                            if (foldout)
                            {
                                GUILayout.BeginHorizontal();
                                GUILayout.Space(16f * 2f);
                                GUILayout.BeginVertical();

                                GUILayout.Label(PlayerPrefs.GetString(key), EditorStyles.wordWrappedLabel);

                                GUILayout.EndVertical();
                                GUILayout.EndHorizontal();

                            }
                        }
                    }
                    EditorGUI.indentLevel -= 1;
                }
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}