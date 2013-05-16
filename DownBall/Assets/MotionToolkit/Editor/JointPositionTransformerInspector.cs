using UnityEngine;
using UnityEditor;
using System.Collections;
using OmekFramework.Beckon.BasicTypes;

[CanEditMultipleObjects]
[CustomEditor(typeof(JointPositionTransformer))]
public class JointPositionTransformerInspector : Editor
{
    SerializedObject m_serializedObject;
    SerializedProperty m_usePlayerID;
    SerializedProperty m_personID;
    SerializedProperty m_sourceJoint;
    SerializedProperty m_showRelativeJoint;
    SerializedProperty m_relativeJoint;
    SerializedProperty m_worldBox;
    SerializedProperty m_targetBox;
    SerializedProperty m_minConfidenceValue;
    SerializedProperty m_smoothFactor;
    SerializedProperty m_controlXAxis;
    SerializedProperty m_controlYAxis;
    SerializedProperty m_controlZAxis;
    SerializedProperty m_invertXAxis;
    SerializedProperty m_invertYAxis;
    SerializedProperty m_invertZAxis;

    private static bool m_useAxisFoldout;
    private static bool m_invertAxisFoldout;
    private static bool m_moveTarget, m_smooth1, m_smooth2;
    private static Vector3 m_targetPosition;

    void OnEnable()
    {
        m_serializedObject = new SerializedObject(target);
        m_usePlayerID = m_serializedObject.FindProperty("UsePlayerID");
        m_personID = m_serializedObject.FindProperty("PersonOrPlayerID");
        m_sourceJoint = m_serializedObject.FindProperty("SourceJoint");
        m_showRelativeJoint = m_serializedObject.FindProperty("UseRelativeJoint");
        m_relativeJoint = m_serializedObject.FindProperty("JointRelativeTo");
        m_worldBox = m_serializedObject.FindProperty("WorldBox");
        m_targetBox = m_serializedObject.FindProperty("TargetBox");
        m_minConfidenceValue = m_serializedObject.FindProperty("MinConfidenceValue");
        m_smoothFactor = m_serializedObject.FindProperty("SmoothFactor");
        m_controlXAxis = m_serializedObject.FindProperty("ControlXAxis");
        m_controlYAxis = m_serializedObject.FindProperty("ControlYAxis");
        m_controlZAxis = m_serializedObject.FindProperty("ControlZAxis");
        m_invertXAxis = m_serializedObject.FindProperty("InvertXAxis");
        m_invertYAxis = m_serializedObject.FindProperty("InvertYAxis");
        m_invertZAxis = m_serializedObject.FindProperty("InvertZAxis");
    }

    public override void OnInspectorGUI()
    {
        
        m_serializedObject.Update();
        GUI.skin = EditorGUIUtility.GetBuiltinSkin(EditorSkin.Inspector);
        EditorGUILayout.ObjectField("Script", MonoScript.FromMonoBehaviour((MonoBehaviour)target),typeof(MonoScript),false);
        EditorGUILayout.PropertyField(m_usePlayerID, new GUIContent("Use Player ID", "Does this script attached to a specific person ID or player ID (which may be assigned to different persons according to the player selection strategy)"), true);
        if (m_usePlayerID.boolValue)
        {
            EditorGUILayout.PropertyField(m_personID, new GUIContent("Player ID", "The player id which position will be mapped to this game object"), true);
        }
        else
        {
            EditorGUILayout.PropertyField(m_personID, new GUIContent("Person ID", "The person id which position will be mapped to this game object"), true);
        }
        EditorGUILayout.PropertyField(m_sourceJoint, new GUIContent("Source Joint","The joint which position will be mapped to this game object"), true);
        EditorGUILayout.PropertyField(m_showRelativeJoint, new GUIContent("Position Relative to specific Joint?","If true, the position of the joint will be calculated relative to another joint of the same person"), true);
        if (m_showRelativeJoint.boolValue)
        {
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(m_relativeJoint, new GUIContent("Relative To Joint","A joint which the position will be relative to"), true);
            EditorGUI.indentLevel--;
        }
        EditorGUILayout.PropertyField(m_worldBox, new GUIContent("World Box","Define the area in the world from which positions will be projected to the game. This effect the scale of the movement as well as its limits. Values in centimeters"), true);
        EditorGUILayout.PropertyField(m_targetBox, new GUIContent("Target Box","Define an area in the game, relative to the parent transform, to which the position will be projected. This effects the scale of the movement. Values in unity units (usually meters)"), true);
        EditorGUILayout.PropertyField(m_minConfidenceValue, new GUIContent("Min Confidence Value","When the joint confidence is lower then this value, its position will be ignored"), true);
        EditorGUILayout.PropertyField(m_smoothFactor, new GUIContent("Smooth Factor","Smaller value mean smoother motion and more delay"), true);
        EditorGUI.indentLevel--;
        m_useAxisFoldout = EditorGUILayout.Foldout(m_useAxisFoldout,new GUIContent("Controlled Axes:","Check which axes this component should effect"));
        if (m_useAxisFoldout)
        {
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(m_controlXAxis, new GUIContent("X"), true);
            EditorGUILayout.PropertyField(m_controlYAxis, new GUIContent("Y"), true);
            EditorGUILayout.PropertyField(m_controlZAxis, new GUIContent("Z"), true);
            EditorGUI.indentLevel--;
        }
        m_invertAxisFoldout = EditorGUILayout.Foldout(m_invertAxisFoldout, new GUIContent("Invert Axes:","Check axes you want the movement direction inverted"));
        if (m_invertAxisFoldout)
        {
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(m_invertXAxis, new GUIContent("X"), true);
            EditorGUILayout.PropertyField(m_invertYAxis, new GUIContent("Y"), true);
            EditorGUILayout.PropertyField(m_invertZAxis, new GUIContent("Z"), true);
            EditorGUI.indentLevel--;
        }

        EditorGUI.indentLevel++;
        m_serializedObject.ApplyModifiedProperties();

        if (Application.isPlaying)
        {
            EditorGUILayout.Separator();
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.Separator();
            EditorGUILayout.BeginVertical();
            EditorGUILayout.BeginHorizontal();
            m_moveTarget=GUILayout.Toggle(m_moveTarget,new  GUIContent("Move Target box","should the TargetBox be updated accordingly so the CurrentValue will stay in place"));
            m_smooth1=GUILayout.Toggle(m_smooth1,new  GUIContent("Smooth Change","should the change in position be smoothed or should it happen abruptly"));
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();    
            if (GUILayout.Button(new GUIContent("Recenter On World Position","Center the world box so the current position will be the center of the new box")))
            {
                (target as JointPositionTransformer).RecenterOnWorldPosition(m_moveTarget, m_smooth1);
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();
            m_targetPosition = EditorGUILayout.Vector3Field("Target position",m_targetPosition);
            m_smooth2 = GUILayout.Toggle(m_smooth2, new GUIContent("Smooth Change", "should the change in position be smoothed or should it happen abruptly"));
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Separator();
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button(new GUIContent("Recenter On Target Position", "Center the world box so the CurrentValue will be at targetPosition")))
            {
                (target as JointPositionTransformer).RecenterOnTargetPosition(m_targetPosition, m_smooth2);
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
            EditorGUILayout.Separator();
            EditorGUILayout.EndHorizontal();
        }
    }
}
