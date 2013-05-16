using System;
using System.Collections;
using System.Collections.Generic;
using OmekFramework.Beckon.Pointer;
using UnityEngine;

/// <summary>
/// Check if any pointer is inside a 2D Rect and send event about any changes
/// </summary>
[ExecuteInEditMode]
[AddComponentMenu("Omek/Pointer/Pointer Listener")]
public class PointerListener : MonoBehaviour
{
    // contain information about pointer inside the listener
	[System.Serializable]
	public class PointerHit : System.IEquatable<PointerHit>
	{
		public int m_pointerID;
		public UnityPointerManager.PointerData m_pointerRef;

		public PointerHit(int pointerID, UnityPointerManager.PointerData pointerRef)
		{
			m_pointerID = pointerID;
			m_pointerRef = pointerRef;
		}

		public bool Equals(PointerHit other)
		{
			if (other == null)
			{
				return false;
			}

			return (m_pointerID == other.m_pointerID);
		}
	}
    // an EventHandler to call when any event happens
	public delegate void PointerCollisionEventHandler(PointerListener sender, PointerHit e);

    /// <summary>
    /// the 2D Rect dimensions
    /// </summary>
	public Rect m_listnerScreenCollider = new Rect(50,50,50,50);
    /// <summary>
    ///  what corner of the screen to snap the rect to
    /// </summary>
    public ScreenPositionHelper.SnapToScreen m_snapToScreen;

    /// <summary>
    /// the current Screen Rect after considering the snap to position
    /// </summary>
    public Rect ActualScreenRect
    {
        get
        {
            return ScreenPositionHelper.SnapOnGUIRect(m_listnerScreenCollider, m_snapToScreen);
        }
    }

    /// <summary>
    /// should the OS mouse be considered as another pointer
    /// </summary>
	public bool m_useMouse = false;
    /// <summary>
    /// Method to call using SendMessage when a pointer moves inside the listener rect
    /// </summary>
	public string PointerMoveListnerName = "OnPointerMove";
    /// <summary>
    /// Method to call using SendMessage when a pointer enters the listener rect
    /// </summary>
	public string PointerEnterListnerName = "OnPointerEnter";
    /// <summary>
    /// Method to call using SendMessage when a pointer leave the listener rect
    /// </summary>
	public string PointerLeaveListnerName = "OnPointerLeave";

    /// <summary>
    /// event to call a pointer moves inside the listener rect
    /// </summary>
	public event PointerCollisionEventHandler PointerMoved;
    // <summary>
    /// event to call a pointer enters the listener rect
    /// </summary>
	public event PointerCollisionEventHandler PointerEntered;
    // <summary>
    /// event to call a pointer leave the listener rect
    /// </summary>
	public event PointerCollisionEventHandler PointerLeft;
    /// <summary>
    /// should we draw a rect to show the listener screen area. used for debug in the editor only
    /// </summary>
	public bool m_drawScreenCollider = false;
    /// <summary>
    /// the color of the drawn rect
    /// </summary>
    public Color m_debugScreenColliderColor = Color.green;
    private Texture2D m_listnerTexture;

	private PointerHit m_mousePointerSimulation = new PointerHit(MOUSE_POINTER_ID,
		new UnityPointerManager.PointerData(0, Omek.HandType.Unknown, Vector3.zero, null));

	public static readonly int MOUSE_POINTER_ID = -1;
	private List<PointerHit> m_prevInPointers;
    public List<PointerHit> m_curInPointers;
    private Rect m_actualRect;
    

	private void Awake()
	{
        m_curInPointers = new List<PointerHit>();
        m_prevInPointers = new List<PointerHit>();
	}

	// Use this for initialization
	private void Start()
	{
		if (UnityPointerManager.Instance == null && Application.isPlaying)
		{
			Debug.LogError("Cannot use PointerListner as no UnityPointerManager component is available");
		}
	}

    private void OnEnable()
    {
        m_prevInPointers.Clear();
        m_curInPointers.Clear();
    }

    private void OnDisable()
    {
        m_prevInPointers.Clear();
        m_curInPointers.Clear();
    }
	// Update is called once per frame
	private void Update()
	{
        if (UnityPointerManager.Instance == null)
        {
            return;
        }
        m_actualRect = ActualScreenRect;

		UpdateCurInPointers();

		// Now compare the current in pointers to the previous in pointers and issue messages accordingly.
		IssuePointerInStatesMsgs();

		// Now swap the previous in pointers with the current
		List<PointerHit> temp = m_prevInPointers;
		m_prevInPointers = m_curInPointers;
		m_curInPointers = temp;
	}

    // fire the needed event
	private void IssuePointerInStatesMsgs()
	{
		foreach (PointerHit prevPointer in m_prevInPointers)
		{
			if (m_curInPointers.Contains(prevPointer))
			{
				HandlePointerRemains(prevPointer);
			}
			else
			{
				HandlePointerLeave(prevPointer);
			}
		}
		// Note that running this is not best performance wise, though
		// considering the small sizes of the list (0-2 pointers) it doesn't matter.
		foreach (PointerHit curPointer in m_curInPointers)
		{
			if (!m_prevInPointers.Contains(curPointer))
			{
				HandlePointerEnter(curPointer);
			}
		}
	}

    // fire pointer move event
	private void HandlePointerRemains(PointerHit remainingPointer)
	{
		//Debug.Log("Pointer " + remainingPointer.m_pointerID + " remains");
		SendMessage(PointerMoveListnerName, remainingPointer, SendMessageOptions.DontRequireReceiver);
		if (PointerMoved != null)
		{
			PointerMoved(this, remainingPointer);
		}
	}

    // fire pointer leave event
	private void HandlePointerLeave(PointerHit leavingPointer)
	{
		//Debug.Log("Pointer " + leavingPointer.m_pointerID + " leaves");
		SendMessage(PointerLeaveListnerName, leavingPointer, SendMessageOptions.DontRequireReceiver);
		if (PointerLeft != null)
		{
			PointerLeft(this, leavingPointer);
		}
	}

    // fire pointer enters event
	private void HandlePointerEnter(PointerHit enteringPointer)
	{
		//Debug.Log("Pointer " + enteringPointer.m_pointerID + " enters");
		SendMessage(PointerEnterListnerName, enteringPointer, SendMessageOptions.DontRequireReceiver);
		if (PointerEntered != null)
		{
			PointerEntered(this, enteringPointer);
		}
	}

    // updates the list of cursor currently inside the listener's rect
	private void UpdateCurInPointers()
	{           
		m_curInPointers.Clear();

		// check for mouse
		if (m_useMouse)
		{
            Vector2 curPosition = Vector2.zero;
			curPosition = new Vector2(Input.mousePosition.x, Screen.height - Input.mousePosition.y);
            if (m_actualRect.Contains(curPosition))
			{
				m_mousePointerSimulation.m_pointerRef.m_position = new  Vector3(curPosition.x/Screen.width,curPosition.y/Screen.height,0);
				m_curInPointers.Add(m_mousePointerSimulation);
			}
		}
        
        // check for normal pointers
		AddCurrentPointersInCollider();
	}

    // find which pointers are inside the listeners rect
	private void AddCurrentPointersInCollider()
	{
		UnityPointerManager.PointerData[] curPointersInSystem = UnityPointerManager.Instance.m_currentPointers;
		for (int i = 0; i < curPointersInSystem.Length; i++)
		{
            if (curPointersInSystem[i].m_hand == Omek.HandType.Unknown)
            {
                continue;
            }
			Vector2 screenPos = new Vector2(curPointersInSystem[i].m_position.x * Screen.width, curPointersInSystem[i].m_position.y * Screen.height);
            if (m_actualRect.Contains(screenPos))
			{
				m_curInPointers.Add(new PointerHit(i, curPointersInSystem[i]));
			}
		}
	}

#if UNITY_EDITOR
    // used to draw a rect showing which area on the screen this PointerListeneris using
	private void OnGUI()
	{
		if (!m_drawScreenCollider)
		{
			return;
		}
        if (m_listnerTexture == null)
        {
            m_listnerTexture = new Texture2D(1, 1);
            m_listnerTexture.SetPixel(0, 0, Color.white);
            m_listnerTexture.Apply();
        }
        m_actualRect = ActualScreenRect;
		GUI.color = m_debugScreenColliderColor;
        GUI.DrawTexture(m_actualRect, m_listnerTexture);
	}

#endif
}