using UnityEngine;
using System.Collections;

/// <summary>
/// used to pront a debug log messege whenever a button is clicked
/// </summary>
public class DebugButtonListener : MonoBehaviour {

    private string m_clickText;
    private GUIStyle m_style = new GUIStyle();
    void Awake()
    {
        m_style.fontSize = 30;
        m_style.normal.textColor = Color.red;
        m_style.alignment = TextAnchor.MiddleCenter;
    }

    /// <summary>
    /// called by one of the button scripts
    /// </summary>
    void OnButtonClick()
    {
        Debug.Log(Time.time + ": OnButtonClick - " + name);
        StopCoroutine("ShowText");
        StartCoroutine("ShowText");
    }

    private IEnumerator ShowText()
    {
        m_clickText = "Button Clicked";
        float startTime = Time.realtimeSinceStartup;
        while (Time.realtimeSinceStartup < startTime + 1)
        {
            yield return null;
        }
        m_clickText = "";
    }

    void OnGUI()
    {
        if (!string.IsNullOrEmpty(m_clickText))
        {
            GUI.skin.label.fontSize = 30;
            GUI.Label(new Rect(Screen.width / 2 - 100, Screen.height / 2 - 50, 200, 50), m_clickText, m_style);
        }
    }
}
