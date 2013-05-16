using System;
using System.Collections;
using System.Collections.Generic;
using Assets.Scripts.General;
using OmekFramework.Beckon.Main;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(UnityPointerManager))]
public class UnityPointerManagerInspector : Editor
{
	UnityPointerManager editedPointerManagement;

	SerializedObject m_lastSerializedObject;

    SerializedProperty m_relativeActiveScreenArea;
	SerializedProperty m_expectedPointerCount;
	SerializedProperty m_maxPointersPerPerson;
	SerializedProperty m_lastHandSelectionStrategy;
	SerializedProperty m_overrideOSPointer;
	SerializedProperty m_usingAdaptivePointerPrecision;
	SerializedProperty m_usingClickLock;

	SerializedProperty m_attachmentPendingTime;
	SerializedProperty m_pointerLossTimeout;

	SerializedProperty m_rightHandMovementBox;
	SerializedProperty m_lastRightHandMovementBox;

	SerializedProperty m_leftHandMovementBox;
	SerializedProperty m_lastLeftHandMovementBox;

	SerializedProperty m_currentSmoothingScheme;
	SerializedProperty m_customSmoothingParameters;
	SerializedProperty m_lastCustomSmoothingParameters;
	SerializedProperty m_gesturesToPointerActions;

	SerializedProperty m_currentPointers;

	static GUIContent[] g_handSelectionStrategiesNames;

	private static readonly string[] TOOLTIP_NAMES = new string[]
	{
		"None",
		"First hand chosen",
		"Left hand only",
		"Right hand only",
		"Other"
	};

	static UnityPointerManagerInspector()
	{
		SetupHandSelectionStrategies();
	}

	private static void SetupHandSelectionStrategies()
	{
		string[] values = Enum.GetNames(typeof(UnityPointerManager.HandSelectionStrategies));
		Array.Resize(ref values, values.Length + 1);
		values[values.Length - 1] = "Other";
		g_handSelectionStrategiesNames = new GUIContent[values.Length];

		for (int i = 0; i < values.Length; i++)
		{
			g_handSelectionStrategiesNames[i] = new GUIContent(values[i], TOOLTIP_NAMES[i]);
		}
	}

	private void OnEnable()
	{
		editedPointerManagement = target as UnityPointerManager;

		m_rightHandMovementBox = serializedObject.FindProperty("m_rightHandMovementBox");
		m_leftHandMovementBox = serializedObject.FindProperty("m_leftHandMovementBox");

        m_relativeActiveScreenArea = serializedObject.FindProperty("m_relativeActiveScreenArea");
		m_expectedPointerCount = serializedObject.FindProperty("m_expectedPointerCount");
		m_maxPointersPerPerson = serializedObject.FindProperty("m_maxPointersPerPerson");
		m_lastHandSelectionStrategy = serializedObject.FindProperty("m_lastHandSelectionStrategy");
		m_overrideOSPointer = serializedObject.FindProperty("m_overrideOSPointer");
		m_gesturesToPointerActions = serializedObject.FindProperty("m_gesturesToPointerActions");
		m_usingAdaptivePointerPrecision = serializedObject.FindProperty("m_usingAdaptivePointerPrecision");
		m_usingClickLock = serializedObject.FindProperty("m_usingClickLock");
		m_attachmentPendingTime = serializedObject.FindProperty("m_attachmentPendingTime");
		m_pointerLossTimeout = serializedObject.FindProperty("m_pointerLossTimeout");

		m_currentSmoothingScheme = serializedObject.FindProperty("m_currentSmoothingScheme");
		m_customSmoothingParameters = serializedObject.FindProperty("m_customSmoothingParameters");

		m_currentPointers = serializedObject.FindProperty("m_currentPointers");

		m_lastSerializedObject = new SerializedObject(target);
		m_lastRightHandMovementBox = m_lastSerializedObject.FindProperty("m_rightHandMovementBox");
		m_lastLeftHandMovementBox = m_lastSerializedObject.FindProperty("m_leftHandMovementBox");
		m_lastCustomSmoothingParameters = m_lastSerializedObject.FindProperty("m_customSmoothingParameters");
	}

	public void VerifyMovementBoxConfigurationsChange()
	{
		if (SerializedObjectUtils.IsPropertiesEqual(m_lastLeftHandMovementBox, m_leftHandMovementBox) == false)
		{
			editedPointerManagement.ApplyLeftHandMovementBox();
		}
		if (SerializedObjectUtils.IsPropertiesEqual(m_lastRightHandMovementBox, m_rightHandMovementBox) == false)
		{
			editedPointerManagement.ApplyRightHandMovementBox();
		}
	}

	public void MovementBoxesGUI()
	{
		GUIContent leftLabel = new GUIContent("Left hand movement box", "A movement box in real world space defining where the pointer values for the left hand are normalized. The movement box in real world is placed relatively to the torso according to the set CenterOffset (this offset is multiplied relatively to person's height). The size of the box is set according to the set dimensions which are again relative to the person's height");
		EditorGUILayout.PropertyField(m_leftHandMovementBox, leftLabel, true);

		GUIContent rightLabel = new GUIContent("Right hand movement box", "A movement box in real world space defining where the pointer values for the right hand are normalized. The movement box in real world is placed relatively to the torso according to the set CenterOffset (this offset is multiplied relatively to person's height). The size of the box is set according to the set dimensions which are again relative to the person's height");
		EditorGUILayout.PropertyField(m_rightHandMovementBox, rightLabel, true);
		bool isBoxConfigurationDirty = serializedObject.ApplyModifiedProperties();
		if (isBoxConfigurationDirty == true)
		{
			editedPointerManagement.ApplyLeftHandMovementBox();
			editedPointerManagement.ApplyRightHandMovementBox();
		}
	}

	public void AppliedMovementBoxesGUI()
	{
		if (Application.isPlaying)
		{
			editedPointerManagement.FillAppliedLeftMovementBox();
			editedPointerManagement.FillAppliedRightMovementBox();
		}
	}

	public void PointerSelectionStrategyGUI()
	{
        SetScreenActiveArea();
		SetExpectedPointerCount();
		SetMaxPointersPerPerson();
	}

    private void SetScreenActiveArea()
    {
        GUIContent rectLabel = new GUIContent("Relative Active Screen Area", "A rect describing the area of the screen the pointers will be confined to. Values must be in the range 0-1");
        EditorGUILayout.PropertyField(m_relativeActiveScreenArea, rectLabel, true);
    }

	public void SmoothingSchemeSelectionGUI()
	{
		Rect controlRect = EditorGUILayout.BeginHorizontal();
		GUIContent label = new GUIContent("Pointer Smoothing Scheme", "The used smoothing scheme");
		label = EditorGUI.BeginProperty(controlRect, label, m_currentSmoothingScheme);

		EditorGUI.BeginChangeCheck();

		editedPointerManagement.PointerSmoothingScheme = (OmekFramework.Beckon.Pointer.BeckonPointer.SmoothingSchemes)
			EditorGUILayout.EnumPopup(label, editedPointerManagement.PointerSmoothingScheme);

		if (EditorGUI.EndChangeCheck() == true)
		{
			m_currentSmoothingScheme.enumValueIndex = (int)editedPointerManagement.PointerSmoothingScheme;
		}

		EditorGUI.EndProperty();
		EditorGUILayout.EndHorizontal();

		// if we have a strategy show max players field for it
		if (editedPointerManagement.PointerSmoothingScheme == OmekFramework.Beckon.Pointer.BeckonPointer.SmoothingSchemes.Custom)
		{
			VerifyCustomSmoothing();
			CustomSmoothingGUI();
			AppliedCustomSmoothingGUI();
		}
	}

	public void VerifyCustomSmoothing()
	{
		if (SerializedObjectUtils.IsPropertiesEqual(m_lastCustomSmoothingParameters, m_customSmoothingParameters) == false)
		{
			editedPointerManagement.ApplyCustomSmoothingParams();
		}
	}

	public void CustomSmoothingGUI()
	{
		GUIContent countLabel = new GUIContent("Custom smoothing parameters", "The parameters defining the custom smoothing");
		EditorGUILayout.PropertyField(m_customSmoothingParameters, countLabel, true);

		bool isDirty = serializedObject.ApplyModifiedProperties();
		if (isDirty == true)
		{
			editedPointerManagement.ApplyCustomSmoothingParams();
		}
	}

	public void AppliedCustomSmoothingGUI()
	{
		if (Application.isPlaying)
		{
			editedPointerManagement.GetAppliedCustomSmoothingParams();
		}
	}

	public void HandSelectionStrategyGUI()
	{
		Rect controlRect = EditorGUILayout.BeginHorizontal();
		GUIContent label = new GUIContent("Hand Selection Strategy", "Used to choose which hand will control a pointer for a pointer controlling person");
		label = EditorGUI.BeginProperty(controlRect, label, m_lastHandSelectionStrategy);
		EditorGUI.BeginChangeCheck();

		OmekFramework.Beckon.Pointer.HandSelectionStrategy unitySelectedStrategy = null, frameworkSelectedStrategy = null;
		try
		{
			unitySelectedStrategy = UnityPointerManager.g_handSelectionStrategiesInstances[editedPointerManagement.HandSelectionStrategy];
			frameworkSelectedStrategy = BeckonManager.BeckonInstance.PointerManager.HandSelectionStrategy;
		}
		catch { }
		if (Application.isPlaying && unitySelectedStrategy != frameworkSelectedStrategy)
		{
			int selectedIndex = EditorGUILayout.Popup(label, g_handSelectionStrategiesNames.Length - 1, g_handSelectionStrategiesNames);
			if (selectedIndex != g_handSelectionStrategiesNames.Length - 1)
			{
				editedPointerManagement.HandSelectionStrategy =
					(UnityPointerManager.HandSelectionStrategies)Enum.Parse(typeof(UnityPointerManager.HandSelectionStrategies),
					g_handSelectionStrategiesNames[selectedIndex].text);
			}
		}
		else
		{
			editedPointerManagement.HandSelectionStrategy =
				(UnityPointerManager.HandSelectionStrategies)EditorGUILayout.EnumPopup(label, editedPointerManagement.HandSelectionStrategy);
		}
		if (EditorGUI.EndChangeCheck() == true)
		{
			m_lastHandSelectionStrategy.enumValueIndex = (int)editedPointerManagement.HandSelectionStrategy;
		}
		EditorGUI.EndProperty();
		EditorGUILayout.EndHorizontal();
	}

	private void PointerConfigurationGUI()
	{
		SetAdaptivePointerPrecision();
		SetClickLock();
	}

	private void SetAdaptivePointerPrecision()
	{
		Rect controlRect = EditorGUILayout.BeginHorizontal();
		GUIContent label = new GUIContent("Use Adaptive Pointer Precision", "Indicates if there should be a slow down in the pointer movement as there are more subtle movements of the controlling joint. This option is helpful when there is a visible pointer, and the user can use finer control when using small movements, allowing to user to correct his movements according to the display.");
		label = EditorGUI.BeginProperty(controlRect, label, m_usingAdaptivePointerPrecision);
		EditorGUI.BeginChangeCheck();

		editedPointerManagement.UsingAdaptivePointerPrecision = EditorGUILayout.Toggle(label, editedPointerManagement.UsingAdaptivePointerPrecision);

		if (EditorGUI.EndChangeCheck() == true)
		{
			m_usingAdaptivePointerPrecision.boolValue = editedPointerManagement.UsingAdaptivePointerPrecision;
		}
		EditorGUI.EndProperty();
		EditorGUILayout.EndHorizontal();
	}

	private void SetClickLock()
	{
		Rect controlRect = EditorGUILayout.BeginHorizontal();
		GUIContent label = new GUIContent("Use Click Lock", "Indicates if there should be a slow down in the pointer movement when there is a fast change in depth values. This option should be used to enforce less pointer movement when the user changes the depth while intending to remain in the same pointer position (e.g. clicking on a button in a menu).");
		label = EditorGUI.BeginProperty(controlRect, label, m_usingClickLock);
		EditorGUI.BeginChangeCheck();

		editedPointerManagement.UsingClickLock = EditorGUILayout.Toggle(label, editedPointerManagement.UsingClickLock);

		if (EditorGUI.EndChangeCheck() == true)
		{ 
			m_usingClickLock.boolValue = editedPointerManagement.UsingClickLock;
		}
		EditorGUI.EndProperty();
		EditorGUILayout.EndHorizontal();
	}
	
	private void SetAttachmentPendingTime()
	{
		Rect controlRect = EditorGUILayout.BeginHorizontal();
		GUIContent label = new GUIContent("AttachmentPendingTime", "Indicates the time for attaching a good hand to a pointer");
		label = EditorGUI.BeginProperty(controlRect, label, m_attachmentPendingTime);
		EditorGUI.BeginChangeCheck();

		editedPointerManagement.AttachmentPendingTime = EditorGUILayout.FloatField(label, editedPointerManagement.AttachmentPendingTime);

		if (EditorGUI.EndChangeCheck() == true)
		{
			m_attachmentPendingTime.floatValue = editedPointerManagement.AttachmentPendingTime;
		}
		EditorGUI.EndProperty();
		EditorGUILayout.EndHorizontal();
	}

	private void SetPointerLossTimeout()
	{
		Rect controlRect = EditorGUILayout.BeginHorizontal();
		GUIContent label = new GUIContent("Pointer Loss Timeout", "Indicates the time for removing a bad pointer from system");
		label = EditorGUI.BeginProperty(controlRect, label, m_pointerLossTimeout);
		EditorGUI.BeginChangeCheck();

		editedPointerManagement.PointerLossTimeout = EditorGUILayout.FloatField(label, editedPointerManagement.PointerLossTimeout);

		if (EditorGUI.EndChangeCheck() == true)
		{
			m_pointerLossTimeout.floatValue = editedPointerManagement.PointerLossTimeout;
		}
		EditorGUI.EndProperty();
		EditorGUILayout.EndHorizontal();
	}

	private void SetOverrideOSCursor()
	{
		Rect controlRect = EditorGUILayout.BeginHorizontal();
		GUIContent label = new GUIContent("Override OS cursor", "Should the first available pointer values be injected into the OS cursor values");
		label = EditorGUI.BeginProperty(controlRect, label, m_overrideOSPointer);
		EditorGUI.BeginChangeCheck();

		editedPointerManagement.OverrideOSPointer = EditorGUILayout.Toggle(label, editedPointerManagement.OverrideOSPointer);

		if (EditorGUI.EndChangeCheck() == true)
		{
			m_overrideOSPointer.boolValue = editedPointerManagement.OverrideOSPointer;
		}
		EditorGUI.EndProperty();
		EditorGUILayout.EndHorizontal();
		if (m_overrideOSPointer.boolValue)
		{
			serializedObject.ApplyModifiedProperties();
			EditorGUI.indentLevel++;
			EditorGUILayout.PropertyField(m_gesturesToPointerActions, new GUIContent("Gestures To Pointer Actions Mapping", "Which gesture should by mapped to OS pointer actions"), true);
			EditorGUI.indentLevel--;
			bool isDirty = serializedObject.ApplyModifiedProperties();
			if (isDirty == true)
			{
				editedPointerManagement.ApplyOSGestureMapping();
			}
		}
		
	}

	private void SetExpectedPointerCount()
	{
		Rect controlRect = EditorGUILayout.BeginHorizontal();
		GUIContent label = new GUIContent("Expected pointer count", "The expected pointer amount to be chosen from the pointer controlling persons");
		label = EditorGUI.BeginProperty(controlRect, label, m_expectedPointerCount);
		EditorGUI.BeginChangeCheck();

		editedPointerManagement.ExpectedPointerCount = EditorGUILayout.IntField(label, editedPointerManagement.ExpectedPointerCount);

		if (EditorGUI.EndChangeCheck() == true)
		{
			m_expectedPointerCount.intValue = editedPointerManagement.ExpectedPointerCount;
		}
		EditorGUI.EndProperty();
		EditorGUILayout.EndHorizontal();
	}

	private void SetMaxPointersPerPerson()
	{
		Rect controlRect = EditorGUILayout.BeginHorizontal();
		GUIContent label = new GUIContent("Max pointers per person", "The amount of pointers to be acquired per pointer controlling person");
		label = EditorGUI.BeginProperty(controlRect, label, m_maxPointersPerPerson);
		EditorGUI.BeginChangeCheck();

		editedPointerManagement.MaxPointersPerPerson = EditorGUILayout.IntField(label, editedPointerManagement.MaxPointersPerPerson);

		if (EditorGUI.EndChangeCheck() == true)
		{
			m_maxPointersPerPerson.intValue = editedPointerManagement.MaxPointersPerPerson;
		}
		EditorGUI.EndProperty();
		EditorGUILayout.EndHorizontal();
	}

	public override void OnInspectorGUI()
	{
		GUI.skin = EditorGUIUtility.GetBuiltinSkin(EditorSkin.Inspector);
		EditorGUILayout.ObjectField("Script", MonoScript.FromMonoBehaviour((MonoBehaviour)target), typeof(MonoScript), false);

		serializedObject.Update();

		VerifyMovementBoxConfigurationsChange();
		MovementBoxesGUI();

		PointerSelectionStrategyGUI();
		HandSelectionStrategyGUI();
		SetOverrideOSCursor();
		SetAttachmentPendingTime();
		SetPointerLossTimeout();

		PointerConfigurationGUI();

		AppliedMovementBoxesGUI();

		SmoothingSchemeSelectionGUI();

		
		serializedObject.ApplyModifiedProperties();
		m_lastSerializedObject.Update();

		if (Application.isPlaying)
		{
			EditorGUILayout.PropertyField(m_currentPointers, true);
		}
	}
}