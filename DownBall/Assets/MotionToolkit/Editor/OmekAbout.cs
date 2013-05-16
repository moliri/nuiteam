using UnityEngine;
using UnityEditor;
using OmekFramework.Beckon.Main;
using System.Diagnostics;

public class OmekAbout : EditorWindow {

    public Texture2D m_texture;
    // Add menu named "My Window" to the Window menu
    [MenuItem ("Help/About Omek",false,5000)]
    static void Init () {        
        OmekAbout window = ScriptableObject.CreateInstance<OmekAbout>();    
        window.ShowUtility();
        window.title = "About Omek";
        window.maxSize = new Vector2(300, 100);
        window.minSize = new Vector2(300, 100);
    }
    
    void OnGUI () 
    {
        FileVersionInfo beckonVersion = FileVersionInfo.GetVersionInfo("Beckon.dll");
        GUILayout.Space(20);      
        string version = string.Format("{0}.{1}.{2}.{3}", beckonVersion.FileMajorPart, beckonVersion.FileMinorPart, beckonVersion.FileBuildPart.ToString(), beckonVersion.FilePrivatePart);
        EditorGUILayout.LabelField("Beckon SDK Version:", version, EditorStyles.largeLabel);
        if (m_texture != null)
        {
            GUILayout.Label(m_texture);
        }
    }
}

