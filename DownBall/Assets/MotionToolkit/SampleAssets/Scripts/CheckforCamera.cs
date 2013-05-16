using UnityEngine;
using System.Collections;
using OmekFramework.Beckon.Main;
using System;
using System.Runtime.InteropServices;
using System.IO;

public class CheckforCamera : MonoBehaviour {

    public Texture2D bgTexture;
    private GUIStyle boldStyle = new GUIStyle();
    private GUIStyle normalStyle = new GUIStyle();
    private bool m_hasDll = true;

    void Awake()
    {
        boldStyle.fontSize = 25;
        boldStyle.normal.textColor = Color.red;
        boldStyle.fontStyle = FontStyle.Bold;
        boldStyle.alignment = TextAnchor.MiddleCenter;
        normalStyle.normal.textColor = Color.white;
        normalStyle.alignment = TextAnchor.LowerCenter;
        try
        {
           IntPtr dummyPtr;
            Omek.IMotionSensorDotNet.createSkeleton(out dummyPtr);
        }
        catch (DllNotFoundException)
        {
            m_hasDll = false;
        }

    }
	// Use this for initialization
	void OnGUI () {
        if (m_hasDll)
        {
            GUI.depth = 100;
            bool isAlive;
            isAlive = BeckonManager.BeckonInstance.IsInit();
            if (!isAlive)
            {
                Rect rect = new Rect(Screen.width * 0.1f, Screen.height * 0.1f, Screen.width * 0.8f, Screen.height * 0.8f);
                GUI.Box(rect, "");
                GUI.Label(rect, "Couldn't find a depth camera or a sequence.\n\nPlease connect a camera and restart\n\n or change UnityBeckonManager parameters to work with\n\n a recorded sequence", boldStyle);
            }
            else 
            {
                bool destroy = true;
                if (!Directory.Exists("Classifiers"))
                {
                    destroy = false;
                    Rect rect = new Rect(Screen.width * 0.1f, Screen.height * 0.1f, Screen.width * 0.8f, Screen.height * 0.8f);
                    GUI.Box(rect, "");
                    GUI.Label(rect, "Couldn't find gesture Classifiers\n\nPlease copy Classifiers Folder\n\nfrom Resources to the root directory of your Unity project", boldStyle);
                }
                GameObject beckonManagerGO = GameObject.Find("UnityBeckonManager");
                if (beckonManagerGO && beckonManagerGO.GetComponent<UnityBeckonManager>().UseSequence)
                {
                    destroy = false;
                    GUI.depth = 5;
                    Rect rect = new Rect(Screen.width * 0.7f, Screen.height * 0.005f, Screen.width * 0.295f, Screen.height * 0.12f);
                    GUI.DrawTexture(rect, bgTexture,ScaleMode.StretchToFill);
                    rect.y -= Screen.height * 0.008f;
                    GUI.Label(rect, "Using Sequence", boldStyle);					
                    GUI.Label(rect, "See UnityBeckonManager to use camera", normalStyle);
                    GUI.depth = 0;
                }
                if (destroy)
                {
                    Destroy(gameObject);
                }
            }
        }
        else
        {            
            Rect rect = new Rect(Screen.width * 0.1f, Screen.height * 0.1f, Screen.width * 0.8f, Screen.height * 0.8f);
            GUI.Box(rect, "");
            GUI.Label(rect, "Couldn't find needed Beckon SDK dll\n\nPlease copy OmeK's Beckon 3.0 SDK redistributables\n\nfrom the SDK's bin directory to the root directory of your Unity project", boldStyle);
        }
        GUI.depth = 0;
	}
	
	
}
