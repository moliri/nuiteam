using UnityEngine;
using System.Collections;
using UnityEditor;

public class UIAdditions  : MonoBehaviour 
{
    [MenuItem("GameObject/Create Other/Unity Beckon Manager",false,2500)]
    public static void CreateBeckonManagerGameObject()
    {
        Object[] currentObjects = GameObject.FindSceneObjectsOfType(typeof(UnityBeckonManager));
        if (currentObjects.Length > 0)
        {
            Debug.LogWarning("A GameObject with UnityBeckonManager MonoBehavior already exist in the scene");
        }
        else
        {
            GameObject g = new GameObject("UnityBeckonManager", typeof(UnityBeckonManager), typeof(UnityPlayerManagement), typeof(DebugInformation));
            g.GetComponent<DebugInformation>().enabled = false;
        }
    }

    [MenuItem("GameObject/Create Other/Gesture Button", false, 2501)]
    public static void CreateGestureButton()
    {
        new GameObject("GestureButton", typeof(PointerListener), typeof(GestureButton), typeof(DebugButtonListener));        
    }

    [MenuItem("GameObject/Create Other/HotSpot Button", false, 2502)]
    public static void CreateHotSpotButton()
    {
        new GameObject("HotSpotButton", typeof(PointerListener), typeof(HotSpotButton), typeof(DebugButtonListener));
    }
}
