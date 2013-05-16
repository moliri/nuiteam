using System;
using System.Collections;
using Assets.Scripts.General;
using UnityEngine;
using OmekFramework.Common.BasicTypes;


[AddComponentMenu("Omek/Tracking Controllers/Joint Image Position Transformer")]
public class JointImagePositionTransformer : MonoBehaviour
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
	/// A rectangle in the image which bound the position of the joint
	/// </summary>
    public MovementRect ImageRect = new MovementRect();

	/// <summary>
	/// a rectangle in Unity screen space to map the position of the joint to
	/// </summary>
    public MovementRect TargetRect = new MovementRect();

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
	/// Should the x axis direction be inverted
	/// </summary>
	public bool InvertXAxis = false;

	/// <summary>
	/// Should the y axis direction be inverted
	/// </summary>
	public bool InvertYAxis = false;

    public Vector2 CurrentValue;

    private OmekFramework.Common.JointTransformers.JointImagePositionTransformer m_transformer = new OmekFramework.Common.JointTransformers.JointImagePositionTransformer();

	private void Awake()
	{
		SetParameterFromUnity();
	}

    private void OnEnable()
    {
        SetParameterFromUnity();
    }
	// Update is called once per frame
	public void Update()
	{
		SetParameterFromUnity();
		ReturnCode rc = m_transformer.UpdateState();
		if (!rc.IsError())
		{
			Vector3 localPosition = transform.localPosition;
			if (ControlXAxis)
			{
				localPosition.x = m_transformer.CurrentValue.x;
                CurrentValue.x = m_transformer.CurrentValue.x;
			}
			if (ControlYAxis)
			{
				localPosition.y = m_transformer.CurrentValue.y;
                CurrentValue.y = m_transformer.CurrentValue.y;
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
		
		m_transformer.ImageRect = ImageRect;
		m_transformer.TargetRect = TargetRect;
		m_transformer.MinConfidenceValue = (uint)MinConfidenceValue;
		m_transformer.SmoothFactor = SmoothFactor;
		m_transformer.InvertXAxis = InvertXAxis;
		m_transformer.InvertYAxis = InvertYAxis; // y axis should be inverted between Unity and the Framework
	}	

}