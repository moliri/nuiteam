using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using OmekFramework.Beckon.Main;

/// <summary>
/// Use to configure all persons colors in a single place
/// </summary>
[AddComponentMenu("Omek/Management/Unity Player Color Themes")]
public class UnityPlayerColorThemes : MonoBehaviour 
{
    static UnityPlayerColorThemes g_instance;
    /// <summary>
    /// Singleton like accessor
    /// </summary>
    public static UnityPlayerColorThemes Instance
    {
        get
        {
            return g_instance;
        }
    }

    [System.Serializable]
    public class ColorTheme
    {
        public Color32 m_primary;
        public Color32 m_secondary;
        public Color32 m_mask;
        public Color32 m_radar;
        public Color32 m_cursor;
        public Color32 m_highlight;
        public Color32 m_custom1;
        public Color32 m_custom2;
        public Color32 m_custom3;
    }

    [System.Serializable]
    public class PlayerColorThemesLists : PersonProperties.StatePropertiesLists<ColorTheme> {}

    public PlayerColorThemesLists m_themeLists;

    void Awake()
    {
        g_instance = this;
    }

    /// <summary>
    /// get the color theme of a person. The exact Color theme will be selected according to UnityPlayerManagement.IndexingScheme
    /// </summary>
    /// <param name="personIndex">the person id</param>
    /// <returns>the person color scheme</returns>
    public ColorTheme GetPlayerColorTheme(int personIndex)
    {
        
        return BeckonManager.BeckonInstance.PersonProperties.GetPropertyOfPerson<ColorTheme>((uint)personIndex, m_themeLists, UnityPlayerManagement.IndexingScheme);
    }
}
