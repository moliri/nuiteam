using UnityEngine;
using System.Collections;

public class TextColor : MonoBehaviour {

    public Color m_color;

    void Start()
    {
        guiText.material.color = m_color;
    }
}
