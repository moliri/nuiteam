using System;
using System.Collections;
using Assets.Scripts.General;
using OmekFramework.Beckon.BasicTypes;
using UnityEngine;
using OmekFramework.Common.BasicTypes;
using OmekFramework.Beckon.Main;

[AddComponentMenu("Omek/Tracking Controllers/Joint Position Transformer")]
public class JointPositionTransformer : MonoBehaviour
{
    /// <summary>
    /// Does this script attached to a specific person ID or player ID (which may be assigned to different persons according to the player selection strategy)
    /// </summary>
    public bool UsePlayerID = false;
	/// <summary>
	/// The person to get the positions from
	/// </summary>
	public int PersonOrPlayerID;

	/// <summary>
	/// which joint to get the position of
	/// </summary>
	///
	public Omek.JointID SourceJoint = Omek.JointID.unknown;

	/// <summary>
	/// Should the WorldBox be relative to another joint
	/// </summary>
	public bool UseRelativeJoint;

	/// <summary>
	/// which joint the WorldBox is relative to
	/// </summary>
	public Omek.JointID JointRelativeTo = Omek.JointID.unknown;

	/// <summary>
	/// A box in world which bound the position of the joint
	/// </summary>
	public Assets.Scripts.General.MovementBox WorldBox = new Assets.Scripts.General.MovementBox();

	/// <summary>
	/// a box in Unity space to map the position of the joint to
	/// </summary>
	public Assets.Scripts.General.MovementBox TargetBox = new Assets.Scripts.General.MovementBox();

	/// <summary>
	/// minimum confidence the joint need to use it values
	/// </summary>
	public int MinConfidenceValue;

	/// <summary>
	/// How much to smooth the movement. smaller values mean stronger smoothing. must be larger then 0
	/// </summary>
	public float SmoothFactor = 1;

	/// <summary>
	/// Should the result override localPosition on x axis
	/// </summary>
	public bool ControlXAxis = true;

	/// <summary>
	/// Should the result override localPosition on y axis
	/// </summary>
	public bool ControlYAxis = true;

	/// <summary>
	/// Should the result override localPosition on z axis
	/// </summary>
	public bool ControlZAxis = true;

	/// <summary>
	/// Should the x axis direction be inverted
	/// </summary>
	public bool InvertXAxis = false;

	/// <summary>
	/// Should the y axis direction be inverted
	/// </summary>
	public bool InvertYAxis = false;

	/// <summary>
	/// Should the z axis direction be inverted
	/// </summary>
	public bool InvertZAxis = false;

    private OmekFramework.Common.JointTransformers.JointPositionTransformer m_transformer = new OmekFramework.Common.JointTransformers.JointPositionTransformer();

	private void Awake()
	{
		SetParameterFromUnity();
        m_transformer.SetInitialPosition(UnityConverter.ToFramework(transform.localPosition));
	}

    private void OnEnable()
    {
        SetParameterFromUnity();
        m_transformer.SetInitialPosition(UnityConverter.ToFramework(transform.localPosition));
    }
	// Update is called once per frame
	public void Update()
	{
        if (!BeckonManager.BeckonInstance.IsInit())
            return;
		SetParameterFromUnity();
		ReturnCode rc = m_transformer.UpdateState();
		if (!rc.IsError())
		{
			Vector3 localPosition = transform.localPosition;
			if (ControlXAxis)
			{
				localPosition.x = m_transformer.CurrentValue.x;
			}
			if (ControlYAxis)
			{
				localPosition.y = m_transformer.CurrentValue.y;
			}
			if (ControlZAxis)
			{
				localPosition.z = m_transformer.CurrentValue.z;
			}
			transform.localPosition = localPosition;
		}
	}

	/// <summary>
	/// Copy changed parameters from unity to the framework
	/// </summary>
	private void SetParameterFromUnity()
	{
        m_transformer.TrackedObjectID = PersonOrPlayerID;
        m_transformer.UsePlayerID = UsePlayerID;
		m_transformer.SourceJoint = (Omek.JointID)SourceJoint;
		if (UseRelativeJoint)
		{
            m_transformer.SetBoxRelativeToJoint((Omek.JointID)JointRelativeTo);
		}
		else
		{
			m_transformer.SetBoxToAbsoluteCoordinates();
		}
		m_transformer.WorldBox.CenterOffset = UnityConverter.ToFramework(WorldBox.CenterOffset);
		m_transformer.WorldBox.Dimensions = UnityConverter.ToFramework(WorldBox.Dimensions);
		m_transformer.TargetBox.CenterOffset = UnityConverter.ToFramework(TargetBox.CenterOffset);
		m_transformer.TargetBox.Dimensions = UnityConverter.ToFramework(TargetBox.Dimensions);
		m_transformer.MinConfidenceValue = (uint)MinConfidenceValue;
		m_transformer.SmoothFactor = SmoothFactor;
		m_transformer.InvertXAxis = InvertXAxis;
		m_transformer.InvertYAxis = !InvertYAxis; // y axis should be inverted between Unity and the Framework
		m_transformer.InvertZAxis = InvertZAxis;
	}

	/// <summary>
	/// Center the world box so the current position will be the center of the new box
	/// </summary>
	/// <param name="updateTargetBox">should the TargetBox be updated accordingly so the CurrentValue will stay in place</param>
	/// <param name="smoothChange">should the change in position be smoothed or should it happen abruptly</param>
	/// <returns>Return code that represent the status of the run</returns>
	public bool RecenterOnWorldPosition(bool updateTargetBox, bool smoothChange)
	{
		ReturnCode rc = m_transformer.RecenterOnWorldPosition(updateTargetBox, smoothChange);
		if (!rc.IsError())
		{
			WorldBox.CenterOffset = UnityConverter.ToUnity(m_transformer.WorldBox.CenterOffset);
			WorldBox.Dimensions = UnityConverter.ToUnity(m_transformer.WorldBox.Dimensions);
			TargetBox.CenterOffset = UnityConverter.ToUnity(m_transformer.TargetBox.CenterOffset);
			TargetBox.Dimensions = UnityConverter.ToUnity(m_transformer.TargetBox.Dimensions);
			return true;
		}
		else
		{
			Debug.LogError(rc);
			return false;
		}
	}

	/// <summary>
	/// Center the world box so the CurrentValue will be at targetPosition
	/// </summary>
	/// <param name="targetPosition">the wanted position for CurrentValue, must be inside TargetBox</param>
	/// <param name="smoothChange">should the change in position be smoothed or should it happen abruptly</param>
	/// <returns>Return code that represent the status of the run</returns>
	public bool RecenterOnTargetPosition(UnityEngine.Vector3 targetPosition, bool smoothChange = true)
	{
		ReturnCode rc = m_transformer.RecenterOnTargetPosition(UnityConverter.ToFramework(targetPosition), smoothChange);
		if (!rc.IsError())
		{
			WorldBox.CenterOffset = UnityConverter.ToUnity(m_transformer.WorldBox.CenterOffset);
			WorldBox.Dimensions = UnityConverter.ToUnity(m_transformer.WorldBox.Dimensions);
			TargetBox.CenterOffset = UnityConverter.ToUnity(m_transformer.TargetBox.CenterOffset);
			TargetBox.Dimensions = UnityConverter.ToUnity(m_transformer.TargetBox.Dimensions);
			return true;
		}
		else
		{
			Debug.LogError(rc);
			return false;
		}
	}

	
	private void OnDrawGizmosSelected()
	{
		if (transform.parent != null)
		{
			Gizmos.matrix = transform.parent.localToWorldMatrix;
		}
		Gizmos.color = Color.red;
		Gizmos.DrawWireCube(TargetBox.CenterOffset, TargetBox.Dimensions);
	}
}