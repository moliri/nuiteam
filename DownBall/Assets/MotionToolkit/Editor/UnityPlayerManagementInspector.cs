using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System;
using OmekFramework.Beckon.Main;
using System.Threading;

[CustomEditor(typeof(UnityPlayerManagement))]
public class UnityPlayerManagementInspector : Editor
{
    UnityPlayerManagement editedPlayerManagement;

    SerializedObject m_lastSerializedObject;

    SerializedProperty m_trapezoidConfigurations;
    SerializedProperty m_currentTrapezoidConfigurationIndex;
    SerializedProperty m_indexingScheme;
    SerializedProperty m_lastPlayerSelectionStrategy;
    SerializedProperty m_lastPointerSelectionStrategy;
    SerializedProperty m_selectPlayerGesture;
    SerializedProperty m_unselectPlayerGesture;
    SerializedProperty m_expectedPlayersCount;
    SerializedProperty m_expectedPointerControllersCount;
    SerializedProperty m_removeNoPositionDuration;
    SerializedProperty m_removeBadPositionDuration;

    SerializedProperty m_lastTrapezoidConfigurations;
    SerializedProperty m_lastTrapezoidConfigurtaionIndex;

    static List<bool> m_currentTrapezoidConfigurationFoldoutInfo;
    static bool gestureFoldout = false;
    static GUIContent[] g_playerSelectionStrategiesNames;
    static GUIContent[] g_pointerSelectionStrategiesNames;
    static GUIContent[] g_indexingSchemesNames;
    static UnityPlayerManagementInspector()
    {
        string[] vlaues = Enum.GetNames(typeof(UnityPlayerManagement.PlayerSelectionStrategies));
        Array.Resize(ref vlaues,vlaues.Length+1);
        vlaues[vlaues.Length-1] = "Other";
        g_playerSelectionStrategiesNames = new GUIContent[vlaues.Length];
        for (int i = 0; i < vlaues.Length; i++)
        {
            g_playerSelectionStrategiesNames[i] = new GUIContent(vlaues[i]);
        }


        vlaues = Enum.GetNames(typeof(UnityPlayerManagement.PointerSelectionStrategies));
        Array.Resize(ref vlaues, vlaues.Length + 1);
        vlaues[vlaues.Length - 1] = "Other";
        g_pointerSelectionStrategiesNames = new GUIContent[vlaues.Length];
        for (int i = 0; i < vlaues.Length; i++)
        {
            g_pointerSelectionStrategiesNames[i] = new GUIContent(vlaues[i]);
        }
        g_indexingSchemesNames = new GUIContent[] {
            new GUIContent("UseConsistentIDs","Person assigned a color when its state change (e.g. became a player, control the mouse), the color persist even when its index is changed as long as the state stays the same"),
            new GUIContent("UseSystemIDs", "The color is determined by the person state and the relevant ID within each state {e.g. personID for non players playerID for players)."),
            new GUIContent("UsePersonIDs", "The color is determined by the person Id and its state"),
            new GUIContent("UsePersonIDsWithPersonPool","The color reflect the person ID. The color are chosen from the person color pool"),
            new GUIContent("UsePersonIDsWithPlayerPool", "The color reflect the person ID. The color are chosen from the player color pool")
        };
    }
    private void OnEnable()
    {
        editedPlayerManagement = target as UnityPlayerManagement;
        m_trapezoidConfigurations = serializedObject.FindProperty("m_trapezoidConfigurations");
        m_currentTrapezoidConfigurationIndex = serializedObject.FindProperty("m_currentTrapezoidConfigurationIndex");
        m_indexingScheme = serializedObject.FindProperty("m_indexingScheme");
        m_lastPlayerSelectionStrategy = serializedObject.FindProperty("m_lastPlayerSelectionStrategy");
        m_lastPointerSelectionStrategy = serializedObject.FindProperty("m_lastPointerSelectionStrategy");
        m_selectPlayerGesture = serializedObject.FindProperty("m_selectPlayerGesture");
        m_unselectPlayerGesture = serializedObject.FindProperty("m_unselectPlayerGesture");
        m_expectedPlayersCount = serializedObject.FindProperty("m_expectedPlayersCount");
        m_expectedPointerControllersCount = serializedObject.FindProperty("m_expectedPointerControllersCount");
        m_removeNoPositionDuration = serializedObject.FindProperty("m_removeNoPositionDuration");
        m_removeBadPositionDuration = serializedObject.FindProperty("m_removeBadPositionDuration");

        m_lastSerializedObject = new SerializedObject(target);
        m_lastTrapezoidConfigurations = m_lastSerializedObject.FindProperty("m_trapezoidConfigurations");
        m_lastTrapezoidConfigurtaionIndex = m_lastSerializedObject.FindProperty("m_currentTrapezoidConfigurationIndex");
    }

    public void VerifyTrapezoidConfigurationsChange()
    {
        if (SerializedObjectUtils.IsPropertiesEqual(m_lastTrapezoidConfigurations, m_trapezoidConfigurations) == false ||
            SerializedObjectUtils.IsPropertiesEqual(m_lastTrapezoidConfigurtaionIndex, m_currentTrapezoidConfigurationIndex) == false)
        {
            editedPlayerManagement.ApplyCurrentTrapezoidsConfiguration();
        }
    }

    public void TrapezoidConfigurationsGUI()
    {
        EditorGUILayout.PropertyField(m_trapezoidConfigurations,new GUIContent("Trapezoid Configurations","Define the limits of the available area for the players." +
            " There is a place for more then one configuration so it'll be easy to choose different configuration in different game scenarios (e.g. single player vs. multiplayer scenario)." + 
            " Each configuration define 2 trapezoids, an inner one which is considered as good location and an outer one which is considered as warning level position. positions outside the outer trapezoid are considered bad positions"), true);
        EditorGUILayout.PropertyField(m_currentTrapezoidConfigurationIndex, new GUIContent("Current Trapezoid Configuration Index","Which of the trapezoid configuration to use"), true);
        bool isTrapezoidsConfigurationDirty = serializedObject.ApplyModifiedProperties();
        if (isTrapezoidsConfigurationDirty == true)
        {
            editedPlayerManagement.ApplyCurrentTrapezoidsConfiguration();
        }

    }

    public void AppliedTrapezoidConfigurationGUI()
    {
        if (Application.isPlaying)
        {
            UnityPlayerManagement.TrapezoidsPair configuration = UnityPlayerManagement.GetAppliedTrapezoidsConfiguration();
            if (configuration == null)
                return;

            EditorGUILayout.Separator();
            m_currentTrapezoidConfigurationFoldoutInfo = EditorGUIUtils.ObjectGUI(configuration, "Current Trapezoid Configuration", true, 2, m_currentTrapezoidConfigurationFoldoutInfo);
        }
    }

    public void PlayerSelectionStrategyGUI()
    {
        Rect controlRect = EditorGUILayout.BeginHorizontal();
        GUIContent label = new GUIContent("Player Selection Strategy", "Used to select which persons will be assigned as active players");
        label = EditorGUI.BeginProperty(controlRect, label, m_lastPlayerSelectionStrategy);
        EditorGUI.BeginChangeCheck();
        OmekFramework.Beckon.Main.PlayerSelectionStrategy.AbstractPlayerSelectionStrategy unitySelectedStrategy = null, frameworkSelectedStrategy = null;
        try
        {
            unitySelectedStrategy = UnityPlayerManagement.g_playerSelectionStrategiesInstances[editedPlayerManagement.PlayerSelectionStrategy];
            frameworkSelectedStrategy = BeckonManager.BeckonInstance.PlayerSelection.GetCurrentPlayerSelectionStrategy();
        }
        catch { }
        if (Application.isPlaying && unitySelectedStrategy != frameworkSelectedStrategy)
        {

            int selectedIndex = EditorGUILayout.Popup(label, g_playerSelectionStrategiesNames.Length - 1, g_playerSelectionStrategiesNames);
            if (selectedIndex != g_playerSelectionStrategiesNames.Length - 1)
            {
                editedPlayerManagement.PlayerSelectionStrategy = (UnityPlayerManagement.PlayerSelectionStrategies)Enum.Parse(typeof(UnityPlayerManagement.PlayerSelectionStrategies), g_playerSelectionStrategiesNames[selectedIndex].text);
            }
        }
        else
        {
            editedPlayerManagement.PlayerSelectionStrategy = (UnityPlayerManagement.PlayerSelectionStrategies)EditorGUILayout.EnumPopup(label, editedPlayerManagement.PlayerSelectionStrategy);
        }
        if (EditorGUI.EndChangeCheck() == true)
        {
            m_lastPlayerSelectionStrategy.enumValueIndex = (int)editedPlayerManagement.PlayerSelectionStrategy;
        }
        EditorGUI.EndProperty();
        EditorGUILayout.EndHorizontal();


        // if we have a strategy show max players field for it
        if (editedPlayerManagement.PlayerSelectionStrategy != UnityPlayerManagement.PlayerSelectionStrategies.None)
        {
            Rect countControlRect = EditorGUILayout.BeginHorizontal();
            GUIContent countLabel = new GUIContent("Expected Player Count", "How many players we expect");
            countLabel = EditorGUI.BeginProperty(countControlRect, countLabel, m_expectedPlayersCount);
            EditorGUI.BeginChangeCheck();
            editedPlayerManagement.ExpectedPlayersCount = EditorGUILayout.IntField(countLabel, editedPlayerManagement.ExpectedPlayersCount);

            if (EditorGUI.EndChangeCheck() == true)
            {
                m_expectedPlayersCount.intValue = editedPlayerManagement.ExpectedPlayersCount;
            }
            EditorGUI.EndProperty();
            EditorGUILayout.EndHorizontal();
        }
    }

    public void PointerSelectionStrategyGUI()
    {
        Rect controlRect = EditorGUILayout.BeginHorizontal();
        GUIContent label = new GUIContent("Pointer Selection Strategy", "Used to choose which persons will control a pointer");
        label = EditorGUI.BeginProperty(controlRect, label, m_lastPointerSelectionStrategy);
        EditorGUI.BeginChangeCheck();
        
        OmekFramework.Beckon.Main.PlayerSelectionStrategy.AbstractPlayerSelectionStrategy unitySelectedStrategy = null,frameworkSelectedStrategy = null;
        try
        {
            unitySelectedStrategy = UnityPlayerManagement.g_pointerSelectionStrategiesInstances[editedPlayerManagement.PointerSelectionStrategy];
            frameworkSelectedStrategy = BeckonManager.BeckonInstance.PlayerSelection.GetCurrentPointerSelectionStrategy();
        }
        catch { }
        if (Application.isPlaying && unitySelectedStrategy != frameworkSelectedStrategy)
        {

            int selectedIndex = EditorGUILayout.Popup(label, g_pointerSelectionStrategiesNames.Length - 1, g_pointerSelectionStrategiesNames);
            if (selectedIndex != g_pointerSelectionStrategiesNames.Length - 1)
            {
                editedPlayerManagement.PointerSelectionStrategy = (UnityPlayerManagement.PointerSelectionStrategies)Enum.Parse(typeof(UnityPlayerManagement.PlayerSelectionStrategies), g_pointerSelectionStrategiesNames[selectedIndex].text);
            }
        }
        else
        {
            editedPlayerManagement.PointerSelectionStrategy = (UnityPlayerManagement.PointerSelectionStrategies)EditorGUILayout.EnumPopup(label, editedPlayerManagement.PointerSelectionStrategy);
        }
        if (EditorGUI.EndChangeCheck() == true)
        {
            m_lastPointerSelectionStrategy.enumValueIndex = (int)editedPlayerManagement.PointerSelectionStrategy;
        }
        EditorGUI.EndProperty();
        EditorGUILayout.EndHorizontal();

        // if we have a strategy show max players field for it
        if (editedPlayerManagement.PointerSelectionStrategy != UnityPlayerManagement.PointerSelectionStrategies.None)
        {
            Rect countControlRect = EditorGUILayout.BeginHorizontal();
            GUIContent countLabel = new GUIContent("Expected Pointer Controllers Count", "How many persons we expect to control a pointer");
            countLabel = EditorGUI.BeginProperty(countControlRect, countLabel, m_expectedPointerControllersCount);
            EditorGUI.BeginChangeCheck();
            editedPlayerManagement.ExpectedPointerControllersCount = EditorGUILayout.IntField(countLabel, editedPlayerManagement.ExpectedPointerControllersCount);
            
            if (EditorGUI.EndChangeCheck() == true)
            {
                m_expectedPointerControllersCount.intValue = editedPlayerManagement.ExpectedPointerControllersCount;
            }
            EditorGUI.EndProperty();
            EditorGUILayout.EndHorizontal();
        }
    }

    public void GestureEnabledStrategyGUI()
    {
        if ((UnityPlayerManagement.PlayerSelectionStrategies)m_lastPlayerSelectionStrategy.enumValueIndex == UnityPlayerManagement.PlayerSelectionStrategies.GestureEnabled ||
            (UnityPlayerManagement.PlayerSelectionStrategies)m_lastPointerSelectionStrategy.enumValueIndex == UnityPlayerManagement.PlayerSelectionStrategies.GestureEnabled)
        {
            EditorGUI.indentLevel--;
            gestureFoldout = EditorGUILayout.Foldout(gestureFoldout, new GUIContent("Player Select Gestures", "What are the gestures used to (un)select players using GestureEnabled strategy"));
            if (gestureFoldout)
            {
                EditorGUI.indentLevel += 2;
                // if we have UnityBeckonManager with list of gesture use this list, otherwise use free text field
                UnityBeckonManager ubm = GameObject.FindObjectOfType(typeof(UnityBeckonManager)) as UnityBeckonManager;
                if (ubm != null)
                {
                    string[] gestureNames = new string[ubm.GestureList.Count + 1];
                    ubm.GestureList.CopyTo(gestureNames, 0);
                    gestureNames[gestureNames.Length - 1] = "---";

                    Rect controlRect1 = EditorGUILayout.BeginHorizontal();
                    GUIContent label1 = new GUIContent("Player Select Gesture", "Gesture Used to select a person as a player");
                    label1 = EditorGUI.BeginProperty(controlRect1, label1, m_selectPlayerGesture);
                    EditorGUI.BeginChangeCheck();
                    int index = Array.FindIndex(gestureNames, (s) => { return s == editedPlayerManagement.PlayerSelectGesture; });
                    if (index == -1)
                    {
                        index = gestureNames.Length - 1;
                    }
                    int selectedIndex = EditorGUILayout.Popup(label1.text,index, gestureNames);
                    editedPlayerManagement.PlayerSelectGesture = gestureNames[selectedIndex];
                    if (EditorGUI.EndChangeCheck() == true)
                    {
                        m_selectPlayerGesture.stringValue = editedPlayerManagement.PlayerSelectGesture;
                    }
                    EditorGUI.EndProperty();
                    EditorGUILayout.EndHorizontal();


                    Rect controlRect2 = EditorGUILayout.BeginHorizontal();
                    GUIContent label2 = new GUIContent("Player Unselect Gesture", "Gesture Used to unselect a player");
                    label2 = EditorGUI.BeginProperty(controlRect2, label2, m_unselectPlayerGesture);
                    EditorGUI.BeginChangeCheck();
                    index = Array.FindIndex(gestureNames, (s) => { return s == editedPlayerManagement.PlayerUnselectGesture; });
                    if (index == -1)
                    {
                        index = gestureNames.Length - 1;
                    }
                    selectedIndex = EditorGUILayout.Popup(label2.text,index, gestureNames);
                    editedPlayerManagement.PlayerUnselectGesture = gestureNames[selectedIndex];
                    if (EditorGUI.EndChangeCheck() == true)
                    {
                        m_unselectPlayerGesture.stringValue = editedPlayerManagement.PlayerUnselectGesture;
                    }
                    EditorGUI.EndProperty();
                    EditorGUILayout.EndHorizontal();
                }
                else
                {
                    EditorGUILayout.PropertyField(m_selectPlayerGesture);
                    EditorGUILayout.PropertyField(m_unselectPlayerGesture);
                }
                EditorGUI.indentLevel -= 2;
            }
            EditorGUI.indentLevel++;
        }
    }

    public void TimingGUI()
    {
        
        EditorGUILayout.PropertyField(m_removeBadPositionDuration,new GUIContent("Remove Bad Position Duration","How much time to wait before removing a player after a his position type changed to BAD"), true);
        EditorGUILayout.PropertyField(m_removeNoPositionDuration, new GUIContent("Remove No Position Duration", "How much time to wait before removing a player after a he is no longer in view, this allow a grace period to the player to enter back into view"), true);
        bool isTimingDirty = serializedObject.ApplyModifiedProperties();
        if (isTimingDirty == true && Application.isPlaying)
        {
            BeckonManager.BeckonInstance.PlayerSelection.RemoveBadPositionDuration = m_removeBadPositionDuration.floatValue;
            BeckonManager.BeckonInstance.PlayerSelection.RemoveNoPositionDuration = m_removeNoPositionDuration.floatValue;
        }
    }

    public void IndexingSchemeGUI()
    {
        Rect controlRect1 = EditorGUILayout.BeginHorizontal();
        GUIContent label1 = new GUIContent("Color Indexing Scheme", "What scheme will be used to assign color for each person");

        label1 = EditorGUI.BeginProperty(controlRect1, label1, m_selectPlayerGesture);

        m_indexingScheme.enumValueIndex = EditorGUILayout.Popup(label1, m_indexingScheme.enumValueIndex,g_indexingSchemesNames);


        EditorGUI.EndProperty();
        EditorGUILayout.EndHorizontal();
    }

    public override void OnInspectorGUI()
    {
        GUI.skin = EditorGUIUtility.GetBuiltinSkin(EditorSkin.Inspector);

        EditorGUILayout.ObjectField("Script", MonoScript.FromMonoBehaviour((MonoBehaviour)target), typeof(MonoScript),false);

        serializedObject.Update();

        // First, finding changes in component that could happen outside of component (i.e, by undo operation)
        VerifyTrapezoidConfigurationsChange();
        // Show trapezoids configurations
        TrapezoidConfigurationsGUI();  

        // check player selection strategy
        PlayerSelectionStrategyGUI();

        // test cursor selection strategy
        PointerSelectionStrategyGUI();
        
        // show gestureEnabled gesture selection
        GestureEnabledStrategyGUI();

        // show Timing GUI
        TimingGUI();

        // show indexing scheme selection
        IndexingSchemeGUI();

        // If application is running, also show the current trapezoid configuration in framework
        AppliedTrapezoidConfigurationGUI();

        

        serializedObject.ApplyModifiedProperties();
        m_lastSerializedObject.Update();
    }

   



}
