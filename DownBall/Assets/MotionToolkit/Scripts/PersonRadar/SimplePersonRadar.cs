using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using OmekFramework.Beckon.Main;
using OmekFramework.Beckon.Data;

/// <summary>
/// Display the position of all visible person relative to the camera
/// </summary>
[AddComponentMenu("Omek/Person Radar/Simple Person Radar")]
public class SimplePersonRadar : MonoBehaviour
{
   
    /// <summary>
    /// The world-space rect that defines the game play area
    /// </summary>
    public Rect m_worldCoordinates = new Rect(-300,100,600,300);

    /// <summary>
    /// The screen-space rect to display the texture in
    /// </summary>
    public Rect m_screenCoordinates = new Rect(0,0,200,200);
    /// <summary>
    /// where to snap the visualization to
    /// </summary>
    public ScreenPositionHelper.SnapToScreen m_snapTo;
    /// <summary>
    /// The texture to display as the background
    /// </summary>
    public Texture2D m_bgTexture;
    /// <summary>
    /// The texture to display for the person
    /// </summary>
    public Texture2D m_personTexture;

    /// <summary>
    /// The size in pixels of the person rectangle on screen
    /// </summary>
    public int m_personRectSize = 32;

    /// <summary>
    /// list of persons to show. If empty, all persons will be shown
    /// </summary>
    public List<int> m_personsToShow;

    /// <summary>
    /// determine if the radar will show non players
    /// </summary>
    public bool m_showNonPlayers;

    [System.Serializable]
    public class RadarColors : PersonProperties.StatePropertiesLists<Color32> { }
    public RadarColors m_radarColors = new RadarColors();

    private Texture2D m_alternateTexture;
    private GUIStyle m_playerRectStyle;

    /// <summary>
    /// check is a person should be drawn
    /// </summary>
    /// <param name="personIndex">the person index</param>
    /// <param name="state">the person state</param>
    /// <returns>true iff the person should be drawn</returns>
    bool ShouldDrawPerson(uint personIndex, PlayerSelection.PersonIDState state)
    {
        if (state == PlayerSelection.PersonIDState.NonPlayer && m_showNonPlayers == true)
        {
            return true;
        }
        else if (m_personsToShow == null || m_personsToShow.Count == 0 || m_personsToShow.Contains((int)personIndex))
        {
            return true;
        }
        return false;
    }

    /// <summary>
    /// compute the screen position for a world 3d position
    /// </summary>
    /// <param name="worldPosition">the world point to map to screen</param>
    /// <returns>screen position corresponding to worldPosition</returns>
    Vector2 WorldToScreenProjection(Vector3 worldPosition)
    {
        if (Mathf.Approximately(m_worldCoordinates.width, 0) == true || Mathf.Approximately(m_worldCoordinates.height, 0) == true)
        {
            return Vector2.zero;
        }
        Vector2 screenPosition = new Vector2();
        screenPosition.x = (m_worldCoordinates.xMax - worldPosition.x) / m_worldCoordinates.width * m_screenCoordinates.width;
        screenPosition.y = (worldPosition.z - m_worldCoordinates.yMin) / m_worldCoordinates.height * m_screenCoordinates.height;
        return screenPosition;
    }

    /// <summary>
    /// draw a specific person
    /// </summary>
    /// <param name="personIndex">the person to draw</param>
    /// <param name="state">the person state</param>
    void DrawPerson(uint personIndex, PlayerSelection.PersonIDState state)
    {
        OmekFramework.Common.BasicTypes.SpaceTypes.Vector3 personPositionFrmk;
        BeckonData.Persons[personIndex].CenterOfMass3D.Get(out personPositionFrmk);
        Vector3 personPosition= UnityConverter.ToUnity(personPositionFrmk);
        Vector2 screenPosition = WorldToScreenProjection(personPosition);
        int playerRectHalfSize = m_personRectSize / 2;

        if (UnityPlayerColorThemes.Instance != null)
        {
			GUI.color = UnityPlayerColorThemes.Instance.GetPlayerColorTheme((int)personIndex).m_radar;
        }
        else
        {
            GUI.color = BeckonManager.BeckonInstance.PersonProperties.GetPropertyOfPerson<Color32>(personIndex, m_radarColors, UnityPlayerManagement.IndexingScheme);
        }
        Rect playerRect = new Rect(screenPosition.x - playerRectHalfSize, screenPosition.y - playerRectHalfSize, m_personRectSize, m_personRectSize);

        if (m_personTexture != null)
        {
            GUI.DrawTexture(playerRect, m_personTexture, ScaleMode.ScaleToFit, true);
        }
        else
        {
            GUI.Label(playerRect, "", m_playerRectStyle);
        }
    }

    void OnGUI()
    {
        Rect snapedScreenRect = ScreenPositionHelper.SnapOnGUIRect(m_screenCoordinates,m_snapTo);
        GUI.BeginGroup(snapedScreenRect);
        if (m_bgTexture != null)
        {
            GUI.DrawTexture(new Rect(0, 0, snapedScreenRect.width, snapedScreenRect.height), m_bgTexture);
        }
        List<uint> foundIndices = new List<uint>(BeckonManager.BeckonInstance.PersonMonitor.TrackedObjectsInSystem);
        PlayerSelection.PersonIDState state;
        foreach (uint personIndex in foundIndices)
        {
            state = BeckonManager.BeckonInstance.PlayerSelection.GetPersonIDState((int)personIndex);
            if (ShouldDrawPerson(personIndex, state) == false)
            {
                continue;
            }
            DrawPerson(personIndex, state);
        }
        GUI.EndGroup();
    }

    void Awake()
    {
        m_alternateTexture = new Texture2D(1, 1);
        m_alternateTexture.SetPixel(0, 0, Color.white);
        m_alternateTexture.Apply();
        m_playerRectStyle = new GUIStyle();
        m_playerRectStyle.normal.background = m_alternateTexture;

        if (Mathf.Approximately(m_worldCoordinates.width, 0) == true || Mathf.Approximately(m_worldCoordinates.height, 0) == true)
        {
            Debug.LogWarning("World coordinates have invalid definition. Rect = " + m_worldCoordinates);
        }
    }

}
