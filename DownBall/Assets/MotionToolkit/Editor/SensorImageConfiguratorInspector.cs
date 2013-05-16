using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System;

[CanEditMultipleObjects]
[CustomEditor(typeof(SensorImageConfigurator))]
public class SensorImageConfiguratorInspector : Editor
{
    SerializedObject m_serializedObject;
    SerializedProperty m_renderType;
    SerializedProperty m_bgType;
    SerializedProperty m_maskType;
    SerializedProperty m_outline;
    SerializedProperty m_colors;
    SerializedProperty m_rect;
    SerializedProperty m_snapToPosition;

    void OnEnable()
    {
        m_serializedObject =  new SerializedObject(target);
        m_renderType = m_serializedObject.FindProperty("m_renderType");
        m_bgType = m_serializedObject.FindProperty("m_bgType");
        m_maskType = m_serializedObject.FindProperty("m_maskType");
        m_outline = m_serializedObject.FindProperty("m_outline");
        m_colors = m_serializedObject.FindProperty("m_colors");
        m_rect = m_serializedObject.FindProperty("m_rect");
        m_snapToPosition = m_serializedObject.FindProperty("m_snapToPosition");
    }

    public override void OnInspectorGUI()
    {
        m_serializedObject.Update();            
        GUI.skin = EditorGUIUtility.GetBuiltinSkin(EditorSkin.Inspector);
        EditorGUILayout.ObjectField("Script", MonoScript.FromMonoBehaviour((MonoBehaviour)target), typeof(MonoScript),false);
        EditorGUILayout.PropertyField(m_renderType, new GUIContent("Render Type"),true);
        EditorGUILayout.PropertyField(m_bgType, new GUIContent("Background Image Type"), true);
        EditorGUILayout.PropertyField(m_maskType, new GUIContent("Player Mask Type"), true);
        
        if (m_renderType.enumNames[m_renderType.enumValueIndex] == "Plane")
        {
            if (m_maskType.enumNames[m_maskType.enumValueIndex] == "Depth" || m_maskType.enumNames[m_maskType.enumValueIndex] == "Color")
            {
                EditorGUILayout.PropertyField(m_outline, new GUIContent("Show Outline"), true);
            }
        }
        else if (m_maskType.enumNames[m_maskType.enumValueIndex] != "None" || m_bgType.enumNames[m_bgType.enumValueIndex] != "None")
        {
            EditorGUILayout.PropertyField(m_rect, new GUIContent("Rect"), true);
            EditorGUILayout.PropertyField(m_snapToPosition, new GUIContent("Snap To"), true);
        }

        if (m_maskType.enumNames[m_maskType.enumValueIndex] != "None")
        {
            EditorGUILayout.PropertyField(m_colors, new GUIContent("Colors"), true);
        }
        m_serializedObject.ApplyModifiedProperties();
    }
}
