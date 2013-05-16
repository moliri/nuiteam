using UnityEngine;
using System.Collections;
using OmekFramework.Common.SensorImage;
using OmekFramework.Beckon.Main;
using OmekFramework.Beckon.Data;
using OmekFramework.Common.BasicTypes;

/// <summary>
/// Update a texture on the main material or GUItexture of it's game object
/// with the images capture from a depth camera
/// </summary>
[AddComponentMenu("Omek/Sensor Image/Depth Cam Image")]
public class DepthCamImage : MonoBehaviour
{
    public enum ImageType { IMAGE_TYPE_COLOR, IMAGE_TYPE_DEPTH };

    /// <summary>
    /// type of the image to render (Color or Depth)
    /// </summary>
    public ImageType imageType;
    /// <summary>
    /// Should the image be flipped vertically
    /// </summary>
    public bool flipVertically = false;
    /// <summary>
    /// Should the image be flipped horizontally
    /// </summary>
    public bool flipHorizontally = true;

    private Texture2D m_texture;

    // to optimize performance we call Texture2D.SetPixels and Texture2D.Apply on separate frames.
    private bool m_firstFrame;
    private OmekFramework.Common.SensorImage.RegularImageData regularImageData;
    private ulong m_lastFrame = ulong.MaxValue;

    private Color32[] m_pixels = null;
    // Use this for initialization
    void Start()
    {
        if (!BeckonManager.BeckonInstance.IsInit())
            return;

        m_firstFrame = true;

        if (imageType == ImageType.IMAGE_TYPE_COLOR)
        {
            regularImageData = BeckonData.Image.RGB;
        }
        else if (imageType == ImageType.IMAGE_TYPE_DEPTH)
        {
            regularImageData = BeckonData.Image.Depth;
        }
        OmekFramework.Common.BasicTypes.CommonDefines.ImageFormat regularImageFormat = null;
        if (regularImageData.GetImageFormat(out regularImageFormat).IsError())
        {
            Debug.LogError("Error reading texture size.");
        }
        
        else
        {
            // create a texture and a color32[] to back it.
            m_texture = new Texture2D(regularImageFormat.m_width, regularImageFormat.m_height, TextureFormat.ARGB32, false);
            m_pixels = new Color32[regularImageFormat.m_width * regularImageFormat.m_height];
            m_texture.filterMode = FilterMode.Bilinear;
            m_texture.wrapMode = TextureWrapMode.Clamp;
           
            m_texture.SetPixels32(m_pixels);
            m_texture.Apply();

            // set the texture in the material or GUITexture
            if (renderer)
            {   
                renderer.material.mainTexture = m_texture;
            }
            else if (GetComponent(typeof(GUITexture)))
            {
                GUITexture gui = GetComponent(typeof(GUITexture)) as GUITexture;
                gui.texture = m_texture;
            }
        }
        Update();
    }

    // Update is called once per frame
    // to achieve better performance the task is split to 2: updating the color array and apply the texture
    // each of the 2 task is performed every other frame
    void Update()
    {
        if (m_texture == null)
            return;

        if (Time.frameCount % 2 == 0 && !m_firstFrame) // should we apply the texture this frame or update the Color32 Array
        {
            m_texture.Apply();
            return;
        }

        bool hasNewImage = false;
        BeckonManager.BeckonInstance.HasNewImage(m_lastFrame,out m_lastFrame,out hasNewImage);
        if (hasNewImage)
        {
            ReturnCode rc = UnitySensorImageRelated.UnityImageConverters.ConvertImageToColorArray(regularImageData, flipHorizontally, !flipVertically, ref m_pixels);

            if (!rc.IsError())
            {
                m_texture.SetPixels32(m_pixels, 0);
                if (m_firstFrame)
                {
                    m_firstFrame = false;
                    m_texture.Apply();                    
                }
            }

        }
    }


}
