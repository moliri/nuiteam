using UnityEngine;
using System.Collections;
using System;
using OmekFramework.Common.Main;


[ExecuteInEditMode]
public class GestureButton : MonoBehaviour
{
    enum State { Normal, Hover, Click }
    /// <summary>
    /// which gesture will be considered as button click
    /// </summary>
    public string[] m_clickGestures = {"leftClick","rightClick"};
    /// <summary>
    /// what method to call on button click
    /// </summary>
    public string ButtonClickListnerName = "OnButtonClick";
    /// <summary>
    /// should we use unity default GUI skin for the button
    /// </summary>
    public bool m_useUnityDefaultGUISkin = true;
    /// <summary>
    /// the gui skin to use if we don't uses unity's default
    /// </summary>
    public GUIStyle m_GUIStyle;
    
    // the current button state
    private State m_currentState = State.Normal;
    // the state to get back to after a click
    private State m_afterClickState = State.Normal;
    // the pointer listener we get the Rect from and listen to the event it throws
    private PointerListener m_pointerlistener;
    // last time there was a click
    private float m_lastClickTime;
    // how long to render the clicked texture
    public const float CLICK_DURATION = 0.2f;

    // check for the click gestures of the person that have a pointer of this button
    void Update()
    {
        if (m_pointerlistener != null && m_currentState != State.Normal)
        {
            bool isClickGesture = false;
            foreach (string gestureName in m_clickGestures)
            {
                foreach (PointerListener.PointerHit ph in m_pointerlistener.m_curInPointers)
                {
                    if (ph.m_pointerRef.m_personID != PointerListener.MOUSE_POINTER_ID)
                    {
                        if (FrameworkManager.GenericInstance.Gestures.IsGestureActive(gestureName, (uint)ph.m_pointerRef.m_personID))
                        {
                            isClickGesture = true;
                            break;
                        }
                    }
                }
                if (isClickGesture)
                {
                    break;
                }
            }
            // we have a click
            if (isClickGesture || Input.GetMouseButtonDown(0))
            {
                SendMessage(ButtonClickListnerName, SendMessageOptions.DontRequireReceiver);
                m_currentState = State.Click;
                m_lastClickTime = Time.realtimeSinceStartup;
            }
            // after a short delay reset the click texture to normal
            if (m_currentState == State.Click && Time.realtimeSinceStartup > m_lastClickTime + CLICK_DURATION)
            {
                m_currentState = m_afterClickState;
            }
        }
    }

    // render the button
    void OnGUI()
    {
        if (Event.current.type == EventType.repaint)
        {
            // make sure we have PointerListener to get the rect from
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
            style.Draw(m_pointerlistener.ActualScreenRect, m_currentState == State.Hover, m_currentState == State.Click, false, false);
           
        }

    }

    // change the render state on pointer enter
    void OnPointerEnter()
    {
        if (m_currentState == State.Normal)
        {
            m_currentState = State.Hover;
        }
        m_afterClickState = State.Hover;
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
