using System.Collections;
using System.Collections.Generic;
using Assets.Scripts.General;
using OmekFramework.Beckon.BasicTypes;
using OmekFramework.Beckon.Pointer;
using OmekFramework.Beckon.Main;
using UnityEngine;
using System.Runtime.InteropServices;
using OmekFramework.Common.Pointer;
using OmekFramework.Beckon.Data;

[AddComponentMenu("Omek/Pointer/Unity Pointer Manager")]
public class UnityPointerManager : MonoBehaviour
{
    /// <summary>
    /// Describe mapping between a gesture to a pointer action
    /// </summary>
    [System.Serializable]
    public class GestureToPointerAction
    {
        public string m_gesture;
        public PointerAction m_pointerAction;

        public GestureToPointerAction() {}
        public GestureToPointerAction(string in_gesture, PointerAction in_pointerAction)
        {
            m_gesture = in_gesture;
            m_pointerAction = in_pointerAction;
        }
    }

	/// <summary>
	/// Static instance.
	/// </summary>
	static UnityPointerManager g_instance;

	/// <summary>
	/// Allows static access to this instance.
	/// </summary>
	public static UnityPointerManager Instance
	{
		get
		{
			return g_instance;
		}
	}

	/// <summary>
	/// Hand selection strategies
	/// </summary>
	public enum HandSelectionStrategies
	{
		None,
		FirstUp,
		LeftHandStrategy,
		RightHandStrategy,
	}

	/// <summary>
	/// A mapping between the hand selection strategy options to instances of it.
	/// </summary>
	public static Dictionary<HandSelectionStrategies, OmekFramework.Beckon.Pointer.HandSelectionStrategy> g_handSelectionStrategiesInstances;

	/// <summary>
	/// Static ctor
	/// </summary>
	static UnityPointerManager()
	{
		g_handSelectionStrategiesInstances = new Dictionary<HandSelectionStrategies, OmekFramework.Beckon.Pointer.HandSelectionStrategy>();
		g_handSelectionStrategiesInstances.Add(HandSelectionStrategies.None, null);
		g_handSelectionStrategiesInstances.Add(HandSelectionStrategies.FirstUp, new OmekFramework.Beckon.Pointer.PreDeterminedHandSelectionStrategy());
		g_handSelectionStrategiesInstances.Add(HandSelectionStrategies.LeftHandStrategy, new OmekFramework.Beckon.Pointer.ConstantHandSideStrategy(Omek.HandType.Left));
        g_handSelectionStrategiesInstances.Add(HandSelectionStrategies.RightHandStrategy, new OmekFramework.Beckon.Pointer.ConstantHandSideStrategy(Omek.HandType.Right));
	}

	[SerializeField]
	/// <summary>
	/// A movement box in real world space defining where the pointer values for the left hand are normalized.
	/// The movement box in real world is placed relatively to the torso according to the set CenterOffset
	/// (this offset is multiplied relatively to person's height). The size of the box is set
	/// according to the set dimensions which are again relative to the person's height");
	/// </summary>
	public MovementBox m_leftHandMovementBox = new MovementBox(OmekFramework.Beckon.Pointer.BeckonPointerManager.DEFAULT_LEFT_HAND_MOVEMENT_BOX);

	[SerializeField]
	/// <summary>
	/// A movement box in real world space defining where the pointer values for the right hand are normalized.
	/// The movement box in real world is placed relatively to the torso according to the set CenterOffset
	/// (this offset is multiplied relatively to person's height). The size of the box is set
	/// according to the set dimensions which are again relative to the person's height");
	/// </summary>
	public MovementBox m_rightHandMovementBox = new MovementBox(OmekFramework.Beckon.Pointer.BeckonPointerManager.DEFAULT_RIGHT_HAND_MOVEMENT_BOX);

	/// <summary>
	/// Fill the current left movement box with framework data
	/// </summary>
	public void FillAppliedLeftMovementBox()
	{
		OmekFramework.Common.BasicTypes.MovementBox moveBox = BeckonManager.BeckonInstance.PointerManager.CurrentPointerConfiguration.LeftHandMovementBox;
		m_leftHandMovementBox.CenterOffset.x = moveBox.CenterOffset.x;
		m_leftHandMovementBox.CenterOffset.y = moveBox.CenterOffset.y;
		m_leftHandMovementBox.CenterOffset.z = moveBox.CenterOffset.z;
		m_leftHandMovementBox.Dimensions.x = moveBox.Dimensions.x;
		m_leftHandMovementBox.Dimensions.y = moveBox.Dimensions.y;
		m_leftHandMovementBox.Dimensions.z = moveBox.Dimensions.z;
	}

	/// <summary>
	/// Fill the current right movement box with framework data
	/// </summary>
	public void FillAppliedRightMovementBox()
	{
		OmekFramework.Common.BasicTypes.MovementBox moveBox = BeckonManager.BeckonInstance.PointerManager.CurrentPointerConfiguration.RightHandMovementBox;
		m_rightHandMovementBox.CenterOffset.x = moveBox.CenterOffset.x;
		m_rightHandMovementBox.CenterOffset.y = moveBox.CenterOffset.y;
		m_rightHandMovementBox.CenterOffset.z = moveBox.CenterOffset.z;
		m_rightHandMovementBox.Dimensions.x = moveBox.Dimensions.x;
		m_rightHandMovementBox.Dimensions.y = moveBox.Dimensions.y;
		m_rightHandMovementBox.Dimensions.z = moveBox.Dimensions.z;
	}

	/// <summary>
	/// Apply the right hand movement box into the framework
	/// </summary>
	public void ApplyRightHandMovementBox()
	{
		if (BeckonManager.BeckonInstance.PointerManager.CurrentPointerConfiguration.RightHandMovementBox == null)
		{
			BeckonManager.BeckonInstance.PointerManager.CurrentPointerConfiguration.RightHandMovementBox = m_rightHandMovementBox.ToFrameworkMovementBox();
		}
		else
		{
			ApplyToMovementBox(BeckonManager.BeckonInstance.PointerManager.CurrentPointerConfiguration.RightHandMovementBox, m_rightHandMovementBox);
		}
	}

	/// <summary>
	/// Apply the left hand movement box into the framework
	/// </summary>
	public void ApplyLeftHandMovementBox()
	{
		if (BeckonManager.BeckonInstance.PointerManager.CurrentPointerConfiguration.LeftHandMovementBox == null)
		{
			BeckonManager.BeckonInstance.PointerManager.CurrentPointerConfiguration.LeftHandMovementBox = m_leftHandMovementBox.ToFrameworkMovementBox();
		}
		else
		{
			ApplyToMovementBox(BeckonManager.BeckonInstance.PointerManager.CurrentPointerConfiguration.LeftHandMovementBox, m_leftHandMovementBox);
		}
	}

	/// <summary>
	/// Apply the movement box data into the framework
	/// </summary>
	/// <param name="applyTo">Framework movement box to move the data to</param>
	/// <param name="applyFrom">Unity movement box</param>
	private void ApplyToMovementBox(OmekFramework.Common.BasicTypes.MovementBox applyTo, MovementBox applyFrom)
	{
		applyTo.CenterOffset = 
            new OmekFramework.Common.BasicTypes.SpaceTypes.Vector3(applyFrom.CenterOffset.x,
		                                                      applyFrom.CenterOffset.y,
		                                                      applyFrom.CenterOffset.z);
		applyTo.Dimensions = 
            new OmekFramework.Common.BasicTypes.SpaceTypes.Vector3(applyFrom.Dimensions.x,
		                                                      applyFrom.Dimensions.y,
		                                                      applyFrom.Dimensions.z);
	}

    /// <summary>
    /// Apply a change to the gesture mapping to OS pointer action
    /// </summary>
    public void ApplyOSGestureMapping()
    {
        BeckonManager.BeckonInstance.PointerManager.UnregisterAllGestures();
        foreach (GestureToPointerAction gtpa in m_gesturesToPointerActions)
        {
            BeckonManager.BeckonInstance.PointerManager.RegisterGestureToPointerAction(gtpa.m_gesture, gtpa.m_pointerAction);
        }
    }

    [SerializeField]
    /// <summary>
    /// A rect describing the area of the screen the pointers will be confined to. Values must be in the range 0-1 
    /// </summary>    
    public Rect m_relativeActiveScreenArea = new Rect(0,0,1,1);
    
    [SerializeField]
	/// <summary>
	/// The hand selection strategy
	/// </summary>
	private HandSelectionStrategies m_lastHandSelectionStrategy = HandSelectionStrategies.FirstUp;

	/// <summary>
	/// The hand selection strategy
	/// Used to choose which hand will control a pointer for a pointer controlling person
	/// </summary>
	public HandSelectionStrategies HandSelectionStrategy
	{
		get
		{
			return m_lastHandSelectionStrategy;
		}
		set
		{
			if (value != m_lastHandSelectionStrategy)
			{
				BeckonManager.BeckonInstance.PointerManager.HandSelectionStrategy = g_handSelectionStrategiesInstances[value];

				m_lastHandSelectionStrategy = value;
			}
		}
	}

	[SerializeField]
	/// <summary>
	/// The expected pointer amount to be chosen from the pointer controlling persons
	/// </summary>
	private int m_expectedPointerCount = 1;

	/// <summary>
	/// The expected pointer amount to be chosen from the pointer controlling persons
	/// </summary>
	public int ExpectedPointerCount
	{
		get
		{
			// Get the data from the framework.
            int frameworkValue = (int)BeckonManager.BeckonInstance.PointerManager.ExpectedPointerCount;
			if (Application.isPlaying && frameworkValue != m_expectedPointerCount)
			{
				// If it has been changed, some things need to be change.
				m_expectedPointerCount = frameworkValue;
				OnExpectedPointerCountChanged();
			}
			return m_expectedPointerCount;
		}
		set
		{
			if (value != m_expectedPointerCount)
			{
				Debug.LogWarning("Setting expected pointer count to: " + m_expectedPointerCount);
				m_expectedPointerCount = value;
                BeckonManager.BeckonInstance.PointerManager.ExpectedPointerCount = (uint)m_expectedPointerCount;
				OnExpectedPointerCountChanged();
			}
		}
	}

	/// <summary>
	/// This should be run when the expected pointer count is changed
	/// </summary>
	private void OnExpectedPointerCountChanged()
	{
		InitializeArrays();
	}

	[SerializeField]
	/// <summary>
	/// The amount of pointers to be acquired per pointer controlling person
	/// </summary>
	private int m_maxPointersPerPerson = 1;

	/// <summary>
	/// The amount of pointers to be acquired per pointer controlling person
	/// </summary>
	public int MaxPointersPerPerson
	{
		get
		{
            if (Application.isPlaying)
            {
                m_maxPointersPerPerson = (int)BeckonManager.BeckonInstance.PointerManager.MaxPointersPerPerson;
            }
			return m_maxPointersPerPerson;
		}
		set
		{
			if (value != m_maxPointersPerPerson)
			{
				m_maxPointersPerPerson = value;
				BeckonManager.BeckonInstance.PointerManager.MaxPointersPerPerson = (uint)m_maxPointersPerPerson;
			}
		}
	}

	[SerializeField]
	/// <summary>
	/// Should the first available pointer values be injected into the OS cursor values
	/// </summary>
	private bool m_overrideOSPointer = false;

	/// <summary>
	/// Should the first available pointer values be injected into the OS cursor values
	/// </summary>
	public bool OverrideOSPointer
	{
		get
		{
            if (Application.isPlaying)
            {
                m_overrideOSPointer = BeckonManager.BeckonInstance.PointerManager.IsOSCursorOverriden();
            }
			return m_overrideOSPointer;
		}
		set
		{
			if (value != m_overrideOSPointer)
			{
				m_overrideOSPointer = value; 
				BeckonManager.BeckonInstance.PointerManager.OverideOSCursor(m_overrideOSPointer, Screen.width, Screen.height,0,0);
                if (m_overrideOSPointer)
                {
                    ApplyOSGestureMapping();
                }
			}
		}
	}
    

    public GestureToPointerAction[] m_gesturesToPointerActions = new GestureToPointerAction[]{ new GestureToPointerAction("rightClick",PointerAction.LeftClick),new GestureToPointerAction("leftClick",PointerAction.LeftClick)};
	
    [SerializeField]
	/// <summary>
	/// Indicates if there should be a slow down in the pointer movement as there are more subtle movements of the controlling joint. This option is helpful when there is a visible pointer, and the user can use finer control when using small movements, allowing to user to correct his movements according to the display.
	/// </summary>
	private bool m_usingAdaptivePointerPrecision = true;

	/// <summary>
	/// Indicates if there should be a slow down in the pointer movement as there are more subtle movements of the controlling joint. This option is helpful when there is a visible pointer, and the user can use finer control when using small movements, allowing to user to correct his movements according to the display.
	/// </summary>
	public bool UsingAdaptivePointerPrecision
	{
		get
		{
            if (Application.isPlaying)
            {
                m_usingAdaptivePointerPrecision = BeckonManager.BeckonInstance.PointerManager.CurrentPointerConfiguration.UseAdaptivePointerPrecision;
            }
			return m_usingAdaptivePointerPrecision;
		}
		set
		{
			if (value != m_usingAdaptivePointerPrecision)
			{
				m_usingAdaptivePointerPrecision = value;
				BeckonManager.BeckonInstance.PointerManager.CurrentPointerConfiguration.UseAdaptivePointerPrecision = m_usingAdaptivePointerPrecision;
			}
		}
	}

	[SerializeField]
	/// <summary>
	/// Indicates if there should be a slow down in the pointer movement when there is a fast change in depth values. This option should be used to enforce less pointer movement when the user changes the depth while intending to remain in the same pointer position (e.g. clicking on a button in a menu).
	/// </summary>
	private bool m_usingClickLock = false;

	/// <summary>
	/// Indicates if there should be a slow down in the pointer movement when there is a fast change in depth values. This option should be used to enforce less pointer movement when the user changes the depth while intending to remain in the same pointer position (e.g. clicking on a button in a menu).
	/// </summary>
	public bool UsingClickLock
	{
		get
		{
            if (Application.isPlaying)
            {
                m_usingClickLock = BeckonManager.BeckonInstance.PointerManager.CurrentPointerConfiguration.UseClickLock;
            }
			return m_usingClickLock;
		}
		set
		{
			if (value != m_usingClickLock)
			{
				m_usingClickLock = value;
				BeckonManager.BeckonInstance.PointerManager.CurrentPointerConfiguration.UseClickLock = m_usingClickLock;
			}
		}
	}


    [SerializeField]
    /// <summary>
    /// Indicates the time for attaching a good hand to a pointer
    /// </summary>
    private float m_attachmentPendingTime = 0.5f;

    /// <summary>
    /// Indicates the time for attaching a good hand to a pointer
    /// </summary>    
    public float AttachmentPendingTime
    {
        get
        {
            if (Application.isPlaying)
            {
                m_attachmentPendingTime = BeckonManager.BeckonInstance.PointerManager.AttachmentPendingTime;
            }
            return m_attachmentPendingTime;
        }
        set
        {
            if (value != m_attachmentPendingTime)
            {
                m_attachmentPendingTime = value;
                BeckonManager.BeckonInstance.PointerManager.AttachmentPendingTime = m_attachmentPendingTime;
            }
        }
    }

    [SerializeField]
    /// <summary>
    /// Indicates the time for removing a bad pointer from system
    /// </summary>
    private float m_pointerLossTimeout = 1.0f;

    /// <summary>
    /// Indicates the time for removing a bad pointer from system
    /// </summary>    
    public float PointerLossTimeout
    {
        get
        {
            if (Application.isPlaying)
            {
                m_pointerLossTimeout = BeckonManager.BeckonInstance.PointerManager.PointerLossTimeout;
            }
            return m_pointerLossTimeout;
        }
        set
        {
            if (value != m_pointerLossTimeout)
            {
                m_pointerLossTimeout = value;
                BeckonManager.BeckonInstance.PointerManager.PointerLossTimeout = m_pointerLossTimeout;
            }
        }
    }


	/// <summary>
	/// Initialize the pointer arrays
	/// </summary>
	private void InitializeArrays()
	{
		PointerData[] prevPointerData = m_currentPointers;
        float[] prevPointerChangedTimes = m_pointerChangedTimes;
		m_currentPointers = new PointerData[m_expectedPointerCount];
		m_pointerChangedTimes = new float[m_expectedPointerCount];

		for (int i = 0; i < m_expectedPointerCount; i++)
		{
			if (prevPointerData != null && prevPointerData.Length > i)
			{
				m_currentPointers[i] = prevPointerData[i];
			}
			else
			{
				m_currentPointers[i] = UNAVAILABLE_HAND_POINTER;
			}
            if (prevPointerChangedTimes != null && prevPointerChangedTimes.Length > i)
			{
                m_pointerChangedTimes[i] = prevPointerChangedTimes[i];
            }
            else
            {             
                m_pointerChangedTimes[i] = -1;
            }
		}
	}

	/// <summary>
	/// The current smoothing scheme
	/// </summary>
	[SerializeField]
	private BeckonPointer.SmoothingSchemes m_currentSmoothingScheme = BeckonPointer.SmoothingSchemes.Normal;

	/// <summary>
	/// The current smoothing scheme
	/// </summary>
	public BeckonPointer.SmoothingSchemes PointerSmoothingScheme
	{
		get
		{
            if (Application.isPlaying)
            {
                m_currentSmoothingScheme = BeckonManager.BeckonInstance.PointerManager.CurrentPointerConfiguration.PointerSmoothingScheme;
            }
			return m_currentSmoothingScheme;
		}
		set
		{
			if (value != m_currentSmoothingScheme)
			{
				m_currentSmoothingScheme = value;
                BeckonManager.BeckonInstance.PointerManager.CurrentPointerConfiguration.PointerSmoothingScheme = m_currentSmoothingScheme;
			}
		}
	}

	/// <summary>
	/// The current custom smoothing parameters
	/// </summary>
	[SerializeField]
	private BeckonPointer.SmoothingParameters m_customSmoothingParameters = BeckonPointer.GetNormalSmoothingParamsInstance();

	/// <summary>
	/// Apply the custom smoothing parameters into the framework
	/// </summary>
	public void ApplyCustomSmoothingParams()
	{
		BeckonManager.BeckonInstance.PointerManager.CurrentPointerConfiguration.SetCustomSmoothingParameters(m_customSmoothingParameters);
	}

	/// <summary>
	/// Get the framework applied custom smoothing parameters
	/// </summary>
	public void GetAppliedCustomSmoothingParams()
	{
		BeckonPointer.SmoothingParameters frameworkSmoothingParams = BeckonManager.BeckonInstance.PointerManager.CurrentPointerConfiguration.CustomSmoothingParams;
		m_customSmoothingParameters.NearStabilizationThresholdDistance = frameworkSmoothingParams.NearStabilizationThresholdDistance;
		m_customSmoothingParameters.SpringStrength = frameworkSmoothingParams.SpringStrength;
		m_customSmoothingParameters.StabilizationFrames = frameworkSmoothingParams.StabilizationFrames;
		if (m_customSmoothingParameters.WeightedAveragerArray == null || m_customSmoothingParameters.WeightedAveragerArray.Length != frameworkSmoothingParams.WeightedAveragerArray.Length)
		{
			m_customSmoothingParameters.WeightedAveragerArray = new float[frameworkSmoothingParams.WeightedAveragerArray.Length];
		}
		for (int i = 0; i < frameworkSmoothingParams.WeightedAveragerArray.Length; i++)
		{
			m_customSmoothingParameters.WeightedAveragerArray[i] = frameworkSmoothingParams.WeightedAveragerArray[i];
		}
	}

	/// <summary>
	/// The current pointers in the system
	/// </summary>
	public PointerData[] m_currentPointers;

	/// <summary>
	/// The time at which to pointers changed their state
	/// </summary>
	private float[] m_pointerChangedTimes;

	/// <summary>
	/// Returns the time at which this pointer has changed its state
	/// </summary>
	/// <param name="index">The pointer to get</param>
	/// <returns>The time at which this pointer has changed its state</returns>
	public float GetPointerChangedTime(int index)
	{
		return m_pointerChangedTimes[index];
	}

	/// <summary>
	/// Data representing a pointer
	/// </summary>
	[System.Serializable]
	public class PointerData
	{
		/// <summary>
		/// The person ID for which this pointer is for
		/// </summary>
		public int m_personID;

		/// <summary>
		/// The hand which this pointer represents
		/// </summary>
        public Omek.HandType m_hand;

		/// <summary>
		/// The position of the pointer
		/// </summary>
		public Vector3 m_position;

		/// <summary>
		/// A reference to the wrapped BeckonPointer instance
		/// </summary>
		public BeckonPointer m_pointer;

		/// <summary>
		/// Data representing a pointer
		/// </summary>
		/// <param name="personID">The person ID for which this pointer is for</param>
		/// <param name="hand">The hand which this pointer represents</param>
		/// <param name="pos">The position of the pointer</param>
		/// <param name="pointer">A reference to the wrapped BeckonPointer instance</param>
		public PointerData(int personID, Omek.HandType hand, Vector3 pos, BeckonPointer pointer)
		{
			m_personID = personID;
			m_hand = hand;
			m_position = pos;
			m_pointer = pointer;
		}
	}

	/// <summary>
	/// An indiciation of an unavailable pointer.
	/// </summary>
    private static readonly PointerData UNAVAILABLE_HAND_POINTER = new PointerData(-1, Omek.HandType.Unknown, Vector3.zero, null);

	/// <summary>
	/// Awake
	/// </summary>
	private void Awake()
	{
		g_instance = this;
		BeckonManager.BeckonInstance.PointerManager.MaxPointersPerPerson = (uint)m_maxPointersPerPerson;
		BeckonManager.BeckonInstance.PointerManager.ExpectedPointerCount = (uint)m_expectedPointerCount;
		BeckonManager.BeckonInstance.PointerManager.HandSelectionStrategy = g_handSelectionStrategiesInstances[m_lastHandSelectionStrategy];

		BeckonManager.BeckonInstance.PointerManager.CurrentPointerConfiguration.UseClickLock = m_usingClickLock;
		BeckonManager.BeckonInstance.PointerManager.CurrentPointerConfiguration.UseAdaptivePointerPrecision = m_usingAdaptivePointerPrecision;
		BeckonManager.BeckonInstance.PointerManager.CurrentPointerConfiguration.LeftHandMovementBox = m_leftHandMovementBox.ToFrameworkMovementBox();
		BeckonManager.BeckonInstance.PointerManager.CurrentPointerConfiguration.RightHandMovementBox = m_rightHandMovementBox.ToFrameworkMovementBox();

		BeckonManager.BeckonInstance.PointerManager.CurrentPointerConfiguration.SetCustomSmoothingParameters(m_customSmoothingParameters);
		BeckonManager.BeckonInstance.PointerManager.CurrentPointerConfiguration.PointerSmoothingScheme = m_currentSmoothingScheme;

        BeckonManager.BeckonInstance.PointerManager.AttachmentPendingTime = m_attachmentPendingTime;
        BeckonManager.BeckonInstance.PointerManager.PointerLossTimeout = m_pointerLossTimeout;

		InitializeArrays();

        if (m_overrideOSPointer)
        {
            StartCoroutine(FindCursorOffset());
           
        }

	}

    private IEnumerator FindCursorOffset()
    {
        SetCursorPos(0, 0);
        yield return null;
        Vector2 windowOffset = Input.mousePosition;
        windowOffset.x = -windowOffset.x;
        windowOffset.y = windowOffset.y - Screen.height;
        BeckonManager.BeckonInstance.PointerManager.OverideOSCursor(m_overrideOSPointer,
                                                                    Screen.width * m_relativeActiveScreenArea.width, 
                                                                    Screen.height * m_relativeActiveScreenArea.height, 
                                                                    windowOffset.x + Screen.width * m_relativeActiveScreenArea.x, 
                                                                    windowOffset.y + Screen.height * m_relativeActiveScreenArea.y);
        foreach (GestureToPointerAction gtpa in m_gesturesToPointerActions)
        {
            BeckonManager.BeckonInstance.PointerManager.RegisterGestureToPointerAction(gtpa.m_gesture,gtpa.m_pointerAction);
        }
    }

    
    [DllImport("user32.dll")]
    private static extern bool SetCursorPos(int X, int Y);
	
    /// <summary>
	/// Update the pointers according to what is available in the PointerManager
	/// </summary>
	private void Update()
	{
        for (int i = 0; i < m_expectedPointerCount; ++i)
		{
			BeckonPointer curPointer = BeckonManager.BeckonInstance.PointerManager.GetPointer((uint)i);
			if (curPointer != m_currentPointers[i].m_pointer) // If this is a different pointer, update it.
			{
                m_pointerChangedTimes[i] = Time.realtimeSinceStartup;
				if (curPointer != null)
				{
                    Omek.HandType hand = (curPointer.PointerJoint == Omek.JointID.leftFingerTip) ? Omek.HandType.Left : Omek.HandType.Right;
                    
                    m_currentPointers[i] = new PointerData((int)curPointer.TrackedObjectID, hand, ConstrainPosition(curPointer.Position), curPointer);
				}
				else
				{
					m_currentPointers[i] = UNAVAILABLE_HAND_POINTER;
				}
			}
			else if (curPointer != null)
			{
                m_currentPointers[i].m_position = ConstrainPosition(curPointer.Position);
			}
		}
	}

    private Vector3 ConstrainPosition(OmekFramework.Common.BasicTypes.SpaceTypes.Vector3 pointerPosition)
    {
        Vector2 unityCurPosition = UnityConverter.ToUnity(pointerPosition);
        Vector2 constrainedPosition = new Vector2(m_relativeActiveScreenArea.x + Mathf.Clamp01(unityCurPosition.x) * m_relativeActiveScreenArea.width,
                                                  m_relativeActiveScreenArea.y + Mathf.Clamp01(unityCurPosition.y) * m_relativeActiveScreenArea.height);
        return constrainedPosition;          
    }
    

#if UNITY_EDITOR

    public int GetMaxPointerHolder()
    {
        Dictionary<int, int> m_pointerCounters = new Dictionary<int, int>();
        int maxPointerHolder = -1;
        foreach (PointerData pd in m_currentPointers)
        {
            if (pd.m_personID != -1)
            {
                if (m_pointerCounters.ContainsKey(pd.m_personID))
                {
                    m_pointerCounters[pd.m_personID]++;
                }
                else
                {
                    m_pointerCounters[pd.m_personID] = 1;
                }
                if (maxPointerHolder == -1 || m_pointerCounters[maxPointerHolder] < m_pointerCounters[pd.m_personID])
                {
                    maxPointerHolder = pd.m_personID;
                }
            }
        }
        return maxPointerHolder;
    }
	
    public void OnDrawGizmosSelected()
    {
        if (!enabled || !Application.isPlaying) 
            return;

        // find who has most pointers
        int maxPointerHolder = GetMaxPointerHolder();
        if (maxPointerHolder == -1)
            return;

        Gizmos.color = Color.blue;
        foreach (Omek.JointID joint in
            System.Enum.GetValues(typeof(Omek.JointID)))
        {
            if (joint == Omek.JointID.unknown)
                continue;
            Omek.JointID commonJoint = (Omek.JointID)joint;

            OmekFramework.Common.BasicTypes.SpaceTypes.Vector3 frameworkPos;
            OmekFramework.Common.BasicTypes.ReturnCode rc = BeckonData.Persons[(uint)maxPointerHolder].Skeleton[commonJoint].Position.World.Get(out frameworkPos);
            if (!rc.IsError())
            {
                frameworkPos.x = -frameworkPos.x;
                //Debug.Log(joint + " pos = " + jointPos);
                Gizmos.DrawSphere(transform.position + UnityConverter.ToUnitySpace(frameworkPos)*0.01f, 0.05f);
            }
        }

        foreach (PointerData pd in m_currentPointers)
        {
            if (pd.m_personID != maxPointerHolder || pd.m_pointer==null)
                continue;
            
            Vector3 boxCenter = UnityConverter.ToUnitySpace(pd.m_pointer.ActualMovementBox.CenterOffset) * 0.01f;
            boxCenter.x = -boxCenter.x;
            boxCenter += transform.position;
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(boxCenter, UnityConverter.ToUnity(pd.m_pointer.ActualMovementBox.Dimensions) * 0.01f);   

            Gizmos.color = Color.green;
            Vector3 jointPosition = UnityConverter.ToUnitySpace(pd.m_pointer.NoiseReduceJointPosition) * 0.01f;
            jointPosition.x = -jointPosition.x;
            Gizmos.DrawSphere(transform.position + jointPosition, 0.05f);
        }
            

            
    }
#endif
}