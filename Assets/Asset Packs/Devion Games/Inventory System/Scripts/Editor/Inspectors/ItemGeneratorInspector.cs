﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;

namespace DevionGames.InventorySystem
{
    [CustomEditor(typeof(ItemGenerator),true)]
    public class ItemGeneratorInspector : Editor
    {
        private SerializedProperty m_Script;
        private SerializedProperty m_MaxAmount;
        private SerializedProperty m_ItemGeneratorData;
        private ReorderableList m_ItemGeneratorDataList;

        protected virtual void OnEnable() {
            this.m_Script = serializedObject.FindProperty("m_Script");
            this.m_MaxAmount = serializedObject.FindProperty("m_MaxAmount");
            this.m_ItemGeneratorData = serializedObject.FindProperty("m_ItemGeneratorData");
            this.m_ItemGeneratorDataList = new ReorderableList(serializedObject, this.m_ItemGeneratorData, true, true, true, true)
            {
                drawHeaderCallback = new ReorderableList.HeaderCallbackDelegate(DrawGeneratorDataHeader),
                drawElementCallback = new ReorderableList.ElementCallbackDelegate(DrawGeneratorData),
                onSelectCallback = new ReorderableList.SelectCallbackDelegate(SelectGeneratorData),
                onAddCallback = new ReorderableList.AddCallbackDelegate(AddGeneratorData),
            };


            int index = EditorPrefs.GetInt("GeneratorIndex" + target.GetInstanceID().ToString(), -1);
            if (this.m_ItemGeneratorDataList.count > index)
            {
                this.m_ItemGeneratorDataList.index = index;
                SelectGeneratorData(this.m_ItemGeneratorDataList);
            }

          
        }

        public override void OnInspectorGUI()
        {
            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.PropertyField(this.m_Script);
            EditorGUI.EndDisabledGroup();

            serializedObject.Update();
            EditorGUILayout.PropertyField(this.m_MaxAmount);
            if (this.m_MaxAmount.intValue > this.m_ItemGeneratorData.arraySize)
            {
                this.m_MaxAmount.intValue=this.m_ItemGeneratorData.arraySize;
            }
            if (this.m_MaxAmount.intValue < 0 )
            {
                this.m_MaxAmount.intValue = 0;
            }

            this.m_ItemGeneratorDataList.DoLayoutList();

            if (this.m_ItemGeneratorDataList.index != -1)
            {
                GUILayout.Space(5f);
                DrawSelectedGeneratorData(this.m_ItemGeneratorData.GetArrayElementAtIndex(this.m_ItemGeneratorDataList.index));
            }
            serializedObject.ApplyModifiedProperties();
            
        }

        private void DrawGeneratorDataHeader(Rect rect)
        {
            EditorGUI.LabelField(rect, "Items");
        }

        private void DrawGeneratorData(Rect rect, int index, bool isActive, bool isFocused)
        {
            rect.y += 2f;
            SerializedProperty element = this.m_ItemGeneratorData.GetArrayElementAtIndex(index);
            SerializedProperty item = element.FindPropertyRelative("item");
            if (item.objectReferenceValue != null)
            {
                SerializedObject obj = new SerializedObject(item.objectReferenceValue);
                SerializedProperty itemName = obj.FindProperty("m_ItemName");
                GUI.Label(rect, itemName.stringValue);
            }
            else {
                GUI.Label(rect, "Null");
            }
          
        }

        private void AddGeneratorData(ReorderableList list)
        {
            list.serializedProperty.serializedObject.Update();
            list.serializedProperty.arraySize++;
            list.index = list.serializedProperty.arraySize - 1;
            list.serializedProperty.serializedObject.ApplyModifiedProperties();
        }

        private void SelectGeneratorData(ReorderableList list)
        {
            EditorPrefs.SetInt("GeneratorIndex" + target.GetInstanceID().ToString(), list.index);
        }

        private void DrawSelectedGeneratorData(SerializedProperty element)
        {
            EditorGUILayout.PropertyField(element.FindPropertyRelative("item"));
            SerializedProperty minStack = element.FindPropertyRelative("minStack");
            EditorGUILayout.PropertyField(minStack);

            if (minStack.intValue < 1) {
                minStack.intValue = 1;
            }
            SerializedProperty maxStack = element.FindPropertyRelative("maxStack");
            EditorGUILayout.PropertyField(maxStack);
            if (maxStack.intValue < 1) {
                maxStack.intValue = 1;
            }
            EditorGUILayout.PropertyField(element.FindPropertyRelative("propertyRandomizer"));
            EditorGUILayout.PropertyField(element.FindPropertyRelative("chance"));
        }

    }
}