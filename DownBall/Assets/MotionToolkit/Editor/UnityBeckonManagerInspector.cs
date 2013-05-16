using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System;
using OmekFramework.Beckon.Main;

[CustomEditor(typeof(UnityBeckonManager))]
public class UnityBeckonManagerInspector : Editor
{
    private GUIContent recordContent = new GUIContent("StartRecording", "Will record a sequence from the depth camera. The sequence will be saved in the root directory of the project, named \"seq#\"");
  
    SerializedProperty m_maxPersons;
    SerializedProperty m_useSequence;
    SerializedProperty m_sequencePath;
    SerializedProperty m_cameraParams;
    SerializedProperty m_useSeparateThread;
    SerializedProperty m_recordingLength;
    SerializedProperty m_gestureList;
    SerializedProperty m_alertList;
    SerializedProperty m_logSDKDebug;
    SerializedObject m_serializedObject;

    void OnEnable()
    {
        m_serializedObject = new SerializedObject(target);
        m_maxPersons = m_serializedObject.FindProperty("MaxPersons");
        m_useSequence = m_serializedObject.FindProperty("UseSequence");
        m_sequencePath = m_serializedObject.FindProperty("SequencePath");
        m_cameraParams = m_serializedObject.FindProperty("CameraParams");
        m_useSeparateThread = m_serializedObject.FindProperty("UseSeparateThread");
        m_recordingLength = m_serializedObject.FindProperty("RecordingLength");
        m_gestureList = m_serializedObject.FindProperty("GestureList");
        m_alertList = m_serializedObject.FindProperty("AlertList");
        m_logSDKDebug = m_serializedObject.FindProperty("LogSDKDebug");
    
    }
    public override void OnInspectorGUI()
    {
        GUI.skin = EditorGUIUtility.GetBuiltinSkin(EditorSkin.Inspector);
        EditorGUILayout.ObjectField("Script", MonoScript.FromMonoBehaviour((MonoBehaviour)target), typeof(MonoScript), false);

        m_serializedObject.Update();
        EditorGUILayout.PropertyField(m_maxPersons,new GUIContent("Max Persons","The maximum number of person allowed in the system"),true);
        EditorGUILayout.PropertyField(m_useSequence, new GUIContent("Use Sequence", "Should a sequence be used (otherwise a depth camera will be used)"), true);
        if (m_useSequence.boolValue)
        {
            EditorGUILayout.PropertyField(m_sequencePath, new GUIContent("Sequence Path", "the sequence path (folder name, without the actual filename)"), true);
        }
        EditorGUILayout.PropertyField(m_cameraParams, new GUIContent("Camera Params", "configuration parameters for the 3D camera"), true);
        EditorGUILayout.PropertyField(m_gestureList, new GUIContent("Gesture List", "list of gesture Beckon SDK will try to enable and recognize"), true);
        EditorGUILayout.PropertyField(m_alertList, new GUIContent("Alert List", "list of alerts Beckon SDK will report"), true);
        EditorGUILayout.PropertyField(m_useSeparateThread, new GUIContent("Use Separate Thread", "Should processing be started in a separate thread, this may improve performance on a multi core machine. When true, pausing the game, does not pause the background processing"), true);
        EditorGUILayout.PropertyField(m_logSDKDebug, new GUIContent("Log SDK Debugs", "should Beckon debug messages should be displayed, (otherwise only warning and errors would be displayed)"), true);
        EditorGUILayout.PropertyField(m_recordingLength, new GUIContent("Recording Length", "The maximum length of a sequence recorded. Sequence recording may be toggled by pressing F12"), true);
        
        m_serializedObject.ApplyModifiedProperties();

        if (Application.isPlaying)
        {
            UnityBeckonManager ubm = target as UnityBeckonManager;
            if (BeckonManager.BeckonInstance.IsInRecording)
            {
                recordContent.text = "Stop Recording";
            }
            else
            {
                recordContent.text = "Start Recording";
            }

            EditorGUILayout.Separator();
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.Separator();
            if (GUILayout.Button(recordContent))
            {
                ubm.ToggleRecording();
            }
            EditorGUILayout.Separator();
            EditorGUILayout.EndHorizontal();
        }
    }
	
}
