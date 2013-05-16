using UnityEngine;
using System.Collections;
using OmekFramework.Common.SensorImage;
using OmekFramework.Beckon.Main;
using OmekFramework.Beckon.Data;
using OmekFramework.Common.BasicTypes;

/// <summary>
/// Show an image of a person.
/// Use OnGUI to show images capture from a depth camera with a specific person cut out form the image
/// </summary>
[AddComponentMenu("Omek/Sensor Image/OnGUI Player Mask")]
public class OnGUIPlayerMask : MonoBehaviour
{
    /// <summary>
    /// the person to show ID
    /// </summary>
    public int personLabel;
    /// <summary>
    /// What type of image to show
    /// </summary>
    public Omek.ImageType maskType;
    /// <summary>
    /// Should the image be flipped vertically
    /// </summary>
    public bool flipVertically = false;
    /// <summary>
    /// Should the image be flipped horizontally
    /// </summary>
    public bool flipHorizontally = true;
    /// <summary>
    /// a tint color for the person mask
    /// </summary>
    public Color32 maskColor = Color.white;
    /// <summary>
    /// where on the screen to render the image
    /// </summary>
    public Rect m_rect = new Rect(0,0,160,120);

    private Texture2D m_texture;
    private OmekFramework.Common.SensorImage.RegularImageData regularImageData;
    private ulong m_lastFrame = ulong.MaxValue;
    private Color32[] m_pixels = null;

    // Use this for initialization
    void Start()
    { 
        if (maskType == Omek.ImageType.IMAGE_TYPE_COLOR)
        {
            regularImageData = BeckonData.Image.RGB;
        }
		else if (maskType == Omek.ImageType.IMAGE_TYPE_DEPTH)
        {
            regularImageData = BeckonData.Image.Depth;
        }
        
        TryToCreateTexture();

        Update();
    }

    private void TryToCreateTexture()
    {
        OmekFramework.Common.BasicTypes.CommonDefines.ImageFormat maskImageFormat = null;
        if (BeckonData.Persons[(uint)personLabel].Mask.GetImageFormat(out maskImageFormat).IsError())
        {
            Debug.LogError("Error reading texture size.");
        }

        else
        {
            // create a texture and a color32[] to back it.
            if (maskType == Omek.ImageType.IMAGE_TYPE_MASK)
            {
                m_texture = new Texture2D(maskImageFormat.m_width, maskImageFormat.m_height, TextureFormat.ARGB32, false);
                m_pixels = new Color32[maskImageFormat.m_width * maskImageFormat.m_height];
            }
            else
            {
                OmekFramework.Common.BasicTypes.CommonDefines.ImageFormat regularImageFormat = null;
                regularImageData.GetImageFormat(out regularImageFormat);
                m_texture = new Texture2D(regularImageFormat.m_width, regularImageFormat.m_height, TextureFormat.ARGB32, false);
                m_pixels = new Color32[regularImageFormat.m_width * regularImageFormat.m_height];
            }
            m_texture.filterMode = FilterMode.Bilinear;
            m_texture.wrapMode = TextureWrapMode.Clamp;

            m_texture.SetPixels32(m_pixels);
            m_texture.Apply();
            if (renderer)
            {
				if (maskType != Omek.ImageType.IMAGE_TYPE_COLOR)
                {
                    renderer.material.color = maskColor;
                }
                renderer.material.mainTexture = m_texture;
            }
            else if (GetComponent(typeof(GUITexture)))
            {
                GUITexture gui = GetComponent(typeof(GUITexture)) as GUITexture;
                gui.texture = m_texture;
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (m_texture == null)
        {
            TryToCreateTexture();
            if (m_texture == null)
            {
                return;
            }
        }
		
        bool hasNewImage = false;
        BeckonManager.BeckonInstance.HasNewImage(m_lastFrame,out m_lastFrame,out hasNewImage);
        if (hasNewImage)
        {
            // update the image
            if (BeckonManager.BeckonInstance.PersonMonitor.TrackedObjectsInSystem.Contains((uint)personLabel))
            {
                ReturnCode rc;
				if (maskType == Omek.ImageType.IMAGE_TYPE_MASK)
                {
                    // the default image is flipped vertically, so we used !flipVertically
                    rc = UnitySensorImageRelated.UnityImageConverters.ConvertPersonMaskToColorArray(BeckonData.Persons[(uint)personLabel].Mask, maskColor, flipHorizontally, !flipVertically, ref m_pixels);
                }
                else
                {
                    // the default image is flipped vertically, so we used !flipVertically
                    rc = UnitySensorImageRelated.UnityImageConverters.ConvertImageToMaskedColorArray(regularImageData, BeckonData.Persons[(uint)personLabel].Mask, flipHorizontally, !flipVertically, maskColor, ref m_pixels);
                }

                if (!rc.IsError())
                {
                    m_texture.SetPixels32(m_pixels, 0);
                    m_texture.Apply();                   
                }
            }
        }
    }

    /// draw the texture
    void OnGUI()
    {
        GUI.depth = -1;
        GUI.DrawTexture(m_rect, m_texture, ScaleMode.StretchToFill);
    }
}
