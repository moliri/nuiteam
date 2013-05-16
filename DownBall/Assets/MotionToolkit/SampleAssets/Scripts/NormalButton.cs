using UnityEngine;
using System.Collections;

/// <summary>
/// Display a single OnGUI button
/// </summary>
[ExecuteInEditMode]
public class NormalButton : MonoBehaviour {

    public Rect m_rect;
    public ScreenPositionHelper.SnapToScreen m_snapToScreen;

    void OnGUI()
    {
        if (GUI.Button(ScreenPositionHelper.SnapOnGUIRect(m_rect, m_snapToScreen), "Press me"))
        {
            Debug.Log("I'm so happy");
        }
    }
}
