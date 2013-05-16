using UnityEngine;
using System.Collections;
using System;

[ExecuteInEditMode]
public class HotSpotButton : MonoBehaviour
{
    enum State { Normal, Hover, Click }
    /// <summary>
    /// what method to call on button click
    /// </summary>
    public string ButtonClickListnerName = "OnButtonClick";
    /// <summary>
    /// how long to play the hot spot animation
    /// </summary>
    public float m_hotspotDelay = 2;
    /// <summary>
    /// list of texture that will be played as hot spot animation
    /// </summary>
    public Texture2D[] m_hotSpotTextures;
    /// <summary>
    /// should we use unity default GUI skin for the button
    /// </summary>
    public bool m_useUnityDefaultGUISkin = true;
    /// <summary>
    /// the gui skin to use if we don't uses unity's default
    /// </summary>
    public GUIStyle m_GUIStyle;
    /// <summary>
    /// Time after click that the button won't start another click sequence
    /// </summary>
    public float m_refractoryPeriod = 0.5f;

    // the current button state
    private State m_currentState = State.Normal;
    // the state to get back to after a click
    private State m_afterClickState = State.Normal;
    // the pointer listener we get the Rect from and listen to the event it throws
    private PointerListener m_pointerlistener;
    // last time there was a click
    private float m_lastClickTime;
    // lat time that hover animation started
    private float m_lastHoverTime;
    
    // how long to render the clicked texture
    public const float CLICK_DURATION = 0.3f;


    //check for click
    void Update()
    {

        if (m_currentState == State.Hover && Time.realtimeSinceStartup > m_lastClickTime + CLICK_DURATION + m_refractoryPeriod)
        {
            float hotSpotTime = Time.realtimeSinceStartup - m_lastHoverTime;
            if (hotSpotTime >= m_hotspotDelay)
            {
                SendMessage(ButtonClickListnerName, SendMessageOptions.DontRequireReceiver);
                m_currentState = State.Click;
                m_lastClickTime = Time.realtimeSinceStartup;
            }
        }
        if (m_currentState == State.Click && Time.realtimeSinceStartup > m_lastClickTime + CLICK_DURATION)
        {
            m_currentState = m_afterClickState;
            m_lastHoverTime = Time.realtimeSinceStartup + m_refractoryPeriod;
        }
    }

    void OnGUI()
    {
        if (Event.current.type == EventType.repaint)
        {
            // make sure we have pointerlistener to get the rect from
            if (m_pointerlistener == null)
            {
                m_pointerlistener = GetComponent<PointerListener>();
                if (m_pointerlistener == null)
                {
                    return;
                }
            }
            // choose style
            GUIStyle style = GUI.skin.button;
            if (!m_useUnityDefaultGUISkin)
            {
                style = m_GUIStyle;
            }
            // render the button
            Rect rect = m_pointerlistener.ActualScreenRect;
            style.Draw(rect, (m_currentState == State.Hover || m_currentState == State.Click), m_currentState == State.Click, false, false);
            
            // render the hotspot animation
            if (m_currentState == State.Hover && m_hotSpotTextures != null && m_hotSpotTextures.Length > 0)
            {
                float hotSpotTime = Time.realtimeSinceStartup - m_lastHoverTime;
                if (hotSpotTime >= 0 && hotSpotTime <= m_hotspotDelay)
                {
                    int index = (int)((hotSpotTime / m_hotspotDelay) * m_hotSpotTextures.Length);
                    GUI.DrawTexture(rect, m_hotSpotTextures[index]);
                }
            }
        }
    }
    // change the render state on pointer enter
    void OnPointerEnter()
    {
        if (m_currentState == State.Normal)
        {
            m_currentState = State.Hover;
            m_afterClickState = State.Hover;
            m_lastHoverTime = Time.realtimeSinceStartup;
        }
    }

    // change the render state on pointer leave
    void OnPointerLeave()
    {
        if (m_pointerlistener.m_curInPointers.Count == 0)
        {
            m_currentState = State.Normal;
            m_afterClickState = State.Normal;
        }
    }
}
