using System.Collections;
using System.Collections.Generic;
using OmekFramework.Beckon.Main;
using UnityEngine;

/// <summary>
/// A visualization of the pointers in the system.
/// </summary>
[AddComponentMenu("Omek/Pointer/Pointers Visualization")]
public class PointersVisualization : MonoBehaviour
{
	/// <summary>
	/// The texture displayed for left handed pointers
	/// </summary>
	public Texture2D m_leftHandTexture;

	/// <summary>
	/// The texture displayed for right handed pointers
	/// </summary>
	public Texture2D m_rightHandTexture;

	/// <summary>
	/// The amount of time in which a new pointer fades in
	/// </summary>
	public float m_fadeInSeconds = 0.5f;

	/// <summary>
	/// The amount of time in which a lost pointer fades out
	/// </summary>
	public float m_fadeOutSeconds = 0.5f;


	// We need to know the following about a drawn pointer:
	// 1. The pointer 2D position.
	// 2. The pointer current state and its time stamp. (states: FadingIn, Normal, Fadingout).
	// 3. Current pointer alpha.
	// 4. Pointer controlling hand.

	public enum DrawnState
	{
		FadingIn,
		Normal,
		FadingOut,
		NA
	}

	[System.Serializable]
	public class PointerColors : PersonProperties.StatePropertiesLists<Color32> { }

	public PointerColors m_pointerColors = new PointerColors();

	[System.Serializable]
	private class PointerVisualizationData
	{
		public DrawnState m_currentDrawnState;
		public UnityPointerManager.PointerData m_pData = null;
		public Vector2 m_currentDrawPosition;
		public Color m_pointerColor;
		//public OmekFramework.Beckon.BasicTypes.BeckonDefines.Hand m_handSide;

		public PointerVisualizationData(UnityPointerManager.PointerData pData)
		{
			m_pData = pData;
			m_currentDrawPosition = new Vector2(Screen.width / 2f, Screen.height / 2f);
			m_currentDrawnState = DrawnState.NA;
		}

		public void AssignColor(PointerColors pointerColors)
		{
			if (UnityPlayerColorThemes.Instance != null)
			{
				m_pointerColor = UnityPlayerColorThemes.Instance.GetPlayerColorTheme(m_pData.m_personID).m_cursor;
			}
			else
			{
				m_pointerColor = BeckonManager.BeckonInstance.PersonProperties.GetPropertyOfPerson<Color32>((uint)m_pData.m_personID, pointerColors, UnityPlayerManagement.IndexingScheme);
			}
			m_pointerColor.a = 0;
		}
	}

	private PointerVisualizationData[] m_pointerVisData;

	private void Start()
	{
        if (UnityPointerManager.Instance == null)
        {
            Debug.LogError("Cannot use PointersVisualization as no UnityPointerManager component is available");
        }
        else
        {
            ReInitPointerVisData();
        }
	}

	// Update is called once per frame
	private void Update()
	{
        if (UnityPointerManager.Instance == null)
		{
			return;
		}

        if (m_pointerVisData == null || UnityPointerManager.Instance.ExpectedPointerCount != m_pointerVisData.Length)
		{
			ReInitPointerVisData();
		}

		UpdateFromPointerManager();
	}

	private void OnGUI()
	{
        if (UnityPointerManager.Instance != null && m_pointerVisData != null)
        {
            DrawPointers();
        }
	}

	private void DrawPointers()
	{
		Color prevColorValue = GUI.color;

		for (int i = 0; i < m_pointerVisData.Length; ++i)
		{
			if (m_pointerVisData[i].m_currentDrawnState == DrawnState.NA)
			{
				continue;
			}

			Texture2D curTexture = (m_pointerVisData[i].m_pData.m_hand == Omek.HandType.Left) ?
				m_leftHandTexture : m_rightHandTexture;

			if (m_pointerVisData[i].m_currentDrawnState != DrawnState.Normal)
			{
                float changeTime = UnityPointerManager.Instance.GetPointerChangedTime(i);
                float changeRatio = (Time.realtimeSinceStartup - changeTime) / m_fadeInSeconds;
				if (changeRatio > 1) // Should end it.
				{
					m_pointerVisData[i].m_currentDrawnState = (DrawnState)((int)m_pointerVisData[i].m_currentDrawnState + 1); // Advance to the next state.
				}
				else
				{
					if (m_pointerVisData[i].m_currentDrawnState == DrawnState.FadingIn)
					{
						m_pointerVisData[i].m_pointerColor.a = Mathf.Lerp(m_pointerVisData[i].m_pointerColor.a, 1, changeRatio);
					}
					else
					{
						m_pointerVisData[i].m_pointerColor.a = Mathf.Lerp(m_pointerVisData[i].m_pointerColor.a, 0, changeRatio);
					}
				}
			}

			GUI.color = m_pointerVisData[i].m_pointerColor;

			if (m_pointerVisData[i].m_currentDrawnState != DrawnState.FadingOut)
			{
				m_pointerVisData[i].m_currentDrawPosition =
					new Vector2(Mathf.Clamp01(m_pointerVisData[i].m_pData.m_position.x) * Screen.width - curTexture.width / 2f,
								Mathf.Clamp01(m_pointerVisData[i].m_pData.m_position.y) * Screen.height - curTexture.height / 2f);
			}
			GUI.DrawTexture(new Rect(m_pointerVisData[i].m_currentDrawPosition.x, m_pointerVisData[i].m_currentDrawPosition.y,
				curTexture.width, curTexture.height), curTexture);
		}

		GUI.color = prevColorValue;
	}

	private void UpdateFromPointerManager()
	{
		// Update the pointers states according to the states in the pointer manager.
        for (int i = 0; i < UnityPointerManager.Instance.ExpectedPointerCount; ++i)
		{
            UnityPointerManager.PointerData pData = UnityPointerManager.Instance.m_currentPointers[i];

			// If the data is different, then change the state.
			if (m_pointerVisData[i].m_pData != pData)
			{
				m_pointerVisData[i].m_pData = pData;

				if (pData.m_pointer == null) // The pointer was lost.
				{
					if (m_pointerVisData[i].m_currentDrawnState < DrawnState.FadingOut)
					{
                        //Debug.Log(Time.frameCount + " Fading out pointer: " + i);
						m_pointerVisData[i].m_currentDrawnState = DrawnState.FadingOut;
					}
				}
				else
				{
					// No drawn state before, so its a new pointer
					if (m_pointerVisData[i].m_currentDrawnState != DrawnState.FadingIn)
					{
						m_pointerVisData[i].m_currentDrawnState = DrawnState.FadingIn;
						//Debug.Log(Time.frameCount + " Fading in pointer: " + i);

						m_pointerVisData[i].AssignColor(m_pointerColors);
					}
				}
			}
		}
	}

	private void ReInitPointerVisData()
	{
		PointerVisualizationData[] prevPointersVis = m_pointerVisData;
        m_pointerVisData = new PointerVisualizationData[UnityPointerManager.Instance.ExpectedPointerCount];
        for (int i = 0; i < UnityPointerManager.Instance.ExpectedPointerCount; i++)
		{
			if (prevPointersVis != null && prevPointersVis.Length > i)
			{
				m_pointerVisData[i] = prevPointersVis[i];
			}
			else
			{
				m_pointerVisData[i] = new PointerVisualizationData(null);
			}
		}
	}
}