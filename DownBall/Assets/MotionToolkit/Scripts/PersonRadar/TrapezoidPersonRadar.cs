using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using OmekFramework.Beckon.Main;
using OmekFramework.Common.GeneralUtils;
using OmekFramework.Beckon.Data;
using OmekFramework.Common.Main;

[AddComponentMenu("Omek/Person Radar/Trapezoid Person Radar")]
public class TrapezoidPersonRadar : MonoBehaviour
{
    /// <summary>
    /// where to snap the visualization too
    /// </summary> 
    public enum SnapToScreen
    {
        UpperLeft,
        UpperRight,
        LowerLeft,
        LowerRight
    }; 

    /// <summary>
    /// The screen-space rect to display the texture in
    /// </summary>
    public Rect m_screenCoordinates = new Rect(0,0,200,200);
    /// <summary>
    /// where to snap the visualization to
    /// </summary>
    public ScreenPositionHelper.SnapToScreen m_snapTo;
    /// <summary>
    /// The on screen(texture) trapezoid coordinates. Used to correct projection of person position on radar.
    /// </summary>
    public UnityPlayerManagement.TrapezoidDefinition m_screenTrapezoid = new UnityPlayerManagement.TrapezoidDefinition(60,180,20,180);

    /// <summary>
    /// The trapezoid boundary size in percentage of trapezoid size
    /// </summary>
    public float m_boundarySize = 0.2f;

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

    /// <summary>
    /// contains color definitions
    /// </summary>
    [System.Serializable]
    public class RadarColors : PersonProperties.StatePropertiesLists<Color32> { }
    /// <summary>
    /// contains color definitions
    /// </summary>
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
    /// normalize a point in world to its relative position to a trapezoid
    /// </summary>
    /// <param name="trapezoid">the trapezoid</param>
    /// <param name="worldPos">the world position</param>
    /// <returns>the relative position of the point relative to the trapezoid</returns>
    private Vector2 GetNormalizedTrapezoidPosition(UnityPlayerManagement.TrapezoidDefinition trapezoid, Vector3 worldPos)
    {
        float normalizedY = MathHelpers.UnclampedInverseLerp(trapezoid.nearPlane, trapezoid.farPlane, worldPos.z);

        float widthAtY = MathHelpers.UnclampedLerp(trapezoid.nearWidth, trapezoid.farWidth, normalizedY);
        float normalizedX = MathHelpers.UnclampedInverseLerp(0, widthAtY / 2, worldPos.x);

        return new Vector2(normalizedX, normalizedY);
    }

    /// <summary>
    /// project a normalized point on a trapezoid
    /// </summary>
    /// <param name="trapezoid">the trapezoid</param>
    /// <param name="normPos">the normalize point</param>
    /// <returns>absolute point relative to the given trapezoid</returns>
    private Vector2 ProjectNormalizedPosition(UnityPlayerManagement.TrapezoidDefinition trapezoid, Vector2 normPos)
    {
        float worldY = MathHelpers.UnclampedLerp(trapezoid.nearPlane, trapezoid.farPlane, normPos.y);
        float widthAtY = MathHelpers.UnclampedLerp(trapezoid.nearWidth, trapezoid.farWidth, normPos.y);
        float worldX = -MathHelpers.UnclampedLerp(0, widthAtY / 2, normPos.x);
        return new Vector2(worldX, worldY);
    }
    
    /// <summary>
    /// compute the screen position for a world 3d position
    /// </summary>
    /// <param name="worldPosition">the world point to map to screen</param>
    /// <returns>screen position corresponding to worldPosition</returns>
    Vector2 WorldToScreenProjection(Vector3 worldPosition)
    {
        UnityPlayerManagement.TrapezoidsPair trapezoidConfiguration = UnityPlayerManagement.GetAppliedTrapezoidsConfiguration();
        if (trapezoidConfiguration == null)
        {
            return Vector2.zero;
        }
        UnityPlayerManagement.TrapezoidDefinition worldTrapezoid = trapezoidConfiguration.innerTrapezoid;

        Vector2 normalizedPosition = GetNormalizedTrapezoidPosition(worldTrapezoid, worldPosition);
        normalizedPosition.x = Mathf.Clamp(normalizedPosition.x, -1 - m_boundarySize, 1 + m_boundarySize);
        normalizedPosition.y = Mathf.Clamp(normalizedPosition.y, -m_boundarySize, 1 + m_boundarySize);

        Vector2 projectedPosition = ProjectNormalizedPosition(m_screenTrapezoid, normalizedPosition);
        projectedPosition.x += m_screenCoordinates.width / 2;

        return projectedPosition;
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
        Vector3 personPosition = UnityConverter.ToUnity(personPositionFrmk);
        Vector2 screenPosition = WorldToScreenProjection(personPosition);
        int playerRectHalfSize = m_personRectSize / 2;

        Rect playerRect = new Rect(screenPosition.x - playerRectHalfSize, screenPosition.y - playerRectHalfSize, m_personRectSize, m_personRectSize);
        
        if (UnityPlayerColorThemes.Instance != null)
        {
            if (BeckonManager.BeckonInstance.PersonMonitor.GetPositionType(personIndex) == TrackedObjectState.PositionType.BAD &&
            BeckonManager.BeckonInstance.PlayerSelection.GetPersonIDState((int)personIndex) != PlayerSelection.PersonIDState.NonPlayer)
            {
                GUI.color = UnityPlayerColorThemes.Instance.GetPlayerColorTheme((int)personIndex).m_secondary;
            }
            else
            {
                GUI.color = UnityPlayerColorThemes.Instance.GetPlayerColorTheme((int)personIndex).m_radar;
            }
        }
        else
        {
            GUI.color = BeckonManager.BeckonInstance.PersonProperties.GetPropertyOfPerson<Color32>(personIndex, m_radarColors, UnityPlayerManagement.IndexingScheme);
        }
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

        if (UnityPlayerManagement.GetAppliedTrapezoidsConfiguration() == null)
        {
            Debug.LogWarning("Trying to use Trapezoid radar without definition of trapezoids");
        }
    }
}
