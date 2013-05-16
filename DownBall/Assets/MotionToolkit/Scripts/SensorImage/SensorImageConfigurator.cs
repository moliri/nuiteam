using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// helper script use to create and configure an instance of MultiplayerMask, and DepthCamImage
/// </summary>
[AddComponentMenu("Omek/Sensor Image/Sensor Image Configurator")]
public class SensorImageConfigurator : MonoBehaviour
{
    #region refrence to prefabs and materials
    public GameObject g_backgroundImageGUIPrefab;
    public GameObject g_backgroundImagePlanePrefab;
    public GameObject g_backgroundImageOnGUIPrefab;

    public GameObject g_playerMaskGUIPrefabs;
    public GameObject g_playerMaskPlanePrefabs;
    public GameObject g_playerMaskOnGUIPrefabs;
    public Material g_colorPointerControllingMaskMaterial;
    public Material g_colorPlayerMaskMaterial;
    public Material g_simplePointerControllingPlayerMaskMaterial;
    public Material g_simplePlayerMaskMaterial;
    public Material g_simpleDiffuse;
    public Material g_transparentDiffuse;
    #endregion

    public enum RenderType { GUITexture, Plane,OnGUI };
    public enum BGType { None, Depth, Color };
    public enum MaskType { None, Mask, Depth, Color };
   
    /// <summary>
    /// What type of rendering to use, OnGUI, GUITexture or setting the material on a simple plane
    /// </summary>
    public RenderType m_renderType;
    /// <summary>
    ///  what type of background image we want if any
    /// </summary>
    public BGType m_bgType;
    /// <summary>
    /// how we want to render the detected person
    /// </summary>
    public MaskType m_maskType;
    /// <summary>
    /// should ther be an outline to the rendered person - work in Plane RenderType only
    /// </summary>
    public bool m_outline;
    /// <summary>
    /// color defnitions (used only if there is no PlayerColorThemes in the scene) 
    /// </summary>
    public MultiplayerMasks.MaskColors m_colors;
    /// <summary>
    /// where to place the images (in GUITexture and OnGUI modes)
    /// </summary>
    public Rect m_rect = new Rect(0,0,160,120);
    /// <summary>
    /// which corner of the screen to snap to
    /// </summary>
    public ScreenPositionHelper.SnapToScreen m_snapToPosition;

    private GameObject m_multiplayerMaskObj;
    private GameObject m_bgImageObj;
    private RenderType m_lastRenderType = RenderType.GUITexture;
    private BGType m_lastBGType = BGType.None;
    private MaskType m_lastMaskType = MaskType.None;
    private bool m_lastOutline = false;
    private Rect m_lastPlaceToDraw;
    private ScreenPositionHelper.SnapToScreen m_lastSnapToPosition;

    void Awake()
    {
        if (m_bgType != BGType.None)
        {
            UpdateBGImage();
        }
        if (m_maskType != MaskType.None)
        {
            UpdatePlayerMasks();
        }
        m_lastRenderType = m_renderType;
        m_lastBGType = m_bgType;
        m_lastMaskType = m_maskType;
        m_lastOutline = m_outline;
        m_lastPlaceToDraw = m_rect;
        m_lastSnapToPosition = m_snapToPosition;
    }

    /// <summary>
    /// check if the configuration changed. if so change the GameObject  parameters remove them and create new ones if needed
    /// </summary>
    void Update()
    {
        if (m_renderType != m_lastRenderType || m_bgType != m_lastBGType)
        {
            UpdateBGImage();
        }
        if (m_renderType != m_lastRenderType || m_maskType != m_lastMaskType || m_outline != m_lastOutline)
        {
            UpdatePlayerMasks();
        }
        if (m_rect != m_lastPlaceToDraw || m_snapToPosition != m_lastSnapToPosition)
        {
            if (m_renderType == RenderType.GUITexture)
            {
                FixChildGUITexturePositions();
            }
            else if (m_renderType == RenderType.OnGUI)
            {
            }
        }
        m_lastRenderType = m_renderType;
        m_lastBGType = m_bgType;
        m_lastMaskType = m_maskType;
        m_lastOutline = m_outline;
        m_lastPlaceToDraw = m_rect;
        m_lastSnapToPosition = m_snapToPosition;
    }

    //handle changes to playermasks configuration
    private void UpdatePlayerMasks()
    {
        if (m_multiplayerMaskObj != null)
        {
            Destroy(m_multiplayerMaskObj);
        }
        if (m_maskType != MaskType.None)
        {
            if (m_renderType == RenderType.GUITexture)
            {
                m_multiplayerMaskObj = Instantiate(g_playerMaskGUIPrefabs) as GameObject;
                MultiplayerMasks multiplayerMasks = m_multiplayerMaskObj.GetComponent<MultiplayerMasks>();
                GUITexture guiTexture = multiplayerMasks.m_playerMaskPrefab.GetComponent<GUITexture>();
                ScreenPositionHelper.SetGUITexturePosition(guiTexture, m_rect, m_snapToPosition, 1);

                if (m_maskType == MaskType.Color)
                {
                    multiplayerMasks.m_playerMaskPrefab.GetComponent<PlayerMask>().maskType = Omek.ImageType.IMAGE_TYPE_COLOR;
                }
                else if (m_maskType == MaskType.Depth)
                {
                    multiplayerMasks.m_playerMaskPrefab.GetComponent<PlayerMask>().maskType = Omek.ImageType.IMAGE_TYPE_DEPTH;
                }
                else if (m_maskType == MaskType.Mask)
                {
                    multiplayerMasks.m_playerMaskPrefab.GetComponent<PlayerMask>().maskType = Omek.ImageType.IMAGE_TYPE_MASK;
                }
            }

            else if (m_renderType == RenderType.OnGUI)
            {
                m_multiplayerMaskObj = Instantiate(g_playerMaskOnGUIPrefabs) as GameObject;
                MultiplayerMasks multiplayerMasks = m_multiplayerMaskObj.GetComponent<MultiplayerMasks>();
                OnGUIPlayerMask playerMask = multiplayerMasks.m_playerMaskPrefab.GetComponent<OnGUIPlayerMask>();
                playerMask.m_rect = ScreenPositionHelper.SnapOnGUIRect(m_rect, m_snapToPosition);

                if (m_maskType == MaskType.Color)
                {
                    multiplayerMasks.m_playerMaskPrefab.GetComponent<OnGUIPlayerMask>().maskType = Omek.ImageType.IMAGE_TYPE_COLOR;
                }
                else if (m_maskType == MaskType.Depth)
                {
                    multiplayerMasks.m_playerMaskPrefab.GetComponent<OnGUIPlayerMask>().maskType = Omek.ImageType.IMAGE_TYPE_DEPTH;
                }
                else if (m_maskType == MaskType.Mask)
                {
                    multiplayerMasks.m_playerMaskPrefab.GetComponent<OnGUIPlayerMask>().maskType = Omek.ImageType.IMAGE_TYPE_MASK;
                }


            }
            else if (m_renderType == RenderType.Plane)
            {
                m_multiplayerMaskObj = Instantiate(g_playerMaskPlanePrefabs) as GameObject;
                MultiplayerMasks multiplayerMasks = m_multiplayerMaskObj.GetComponent<MultiplayerMasks>();
                if (m_maskType == MaskType.Color || m_maskType == MaskType.Depth)
                {
                    if (m_maskType == MaskType.Color)
                    {
                        multiplayerMasks.m_playerMaskPrefab.GetComponent<PlayerMask>().maskType = Omek.ImageType.IMAGE_TYPE_COLOR;
                        multiplayerMasks.m_cursorControlingPlayerMaterial = g_transparentDiffuse;
                        multiplayerMasks.m_playerMaterial = g_transparentDiffuse; ;
                        multiplayerMasks.m_nonPlayerMaterial = g_transparentDiffuse;
                    }
                    else
                    {
                        multiplayerMasks.m_playerMaskPrefab.GetComponent<PlayerMask>().maskType = Omek.ImageType.IMAGE_TYPE_DEPTH;
                    }
                    if (m_outline)
                    {
                        multiplayerMasks.m_cursorControlingPlayerMaterial = g_colorPointerControllingMaskMaterial;
                        multiplayerMasks.m_playerMaterial = g_colorPlayerMaskMaterial;
                        multiplayerMasks.m_nonPlayerMaterial = g_colorPlayerMaskMaterial;
                    }
                    else
                    {
                        multiplayerMasks.m_cursorControlingPlayerMaterial = g_transparentDiffuse;
                        multiplayerMasks.m_playerMaterial = g_transparentDiffuse;
                        multiplayerMasks.m_nonPlayerMaterial = g_transparentDiffuse;
                    }
                }
                else if (m_maskType == MaskType.Mask)
                {
                    multiplayerMasks.m_playerMaskPrefab.GetComponent<PlayerMask>().maskType = Omek.ImageType.IMAGE_TYPE_MASK;
                    multiplayerMasks.m_cursorControlingPlayerMaterial = g_simplePointerControllingPlayerMaskMaterial;
                    multiplayerMasks.m_playerMaterial = g_simplePlayerMaskMaterial;
                    multiplayerMasks.m_nonPlayerMaterial = g_simplePlayerMaskMaterial;
                }
            }
        }
        if (m_multiplayerMaskObj != null)
        {
            m_multiplayerMaskObj.transform.parent = transform;
            MultiplayerMasks multiplayerMasks = m_multiplayerMaskObj.GetComponent<MultiplayerMasks>();
            multiplayerMasks.m_maskColors = m_colors;
        }
    }
    
    //handle changes to background image configuration
    private void UpdateBGImage()
    {
        if (m_bgImageObj != null)
        {
            Debug.Log("Destroying BG");
            Destroy(m_bgImageObj);
        }
        if (m_bgType != BGType.None)
        {
            if (m_renderType == RenderType.GUITexture || m_renderType == RenderType.Plane)
            {
                if (m_renderType == RenderType.GUITexture)
                {
                    m_bgImageObj = Instantiate(g_backgroundImageGUIPrefab) as GameObject;
                    GUITexture guiTexture = m_bgImageObj.GetComponent<GUITexture>();
                    ScreenPositionHelper.SetGUITexturePosition(guiTexture, m_rect, m_snapToPosition, 0);
                }
                else if (m_renderType == RenderType.Plane)
                {
                    m_bgImageObj = Instantiate(g_backgroundImagePlanePrefab) as GameObject;
                }

                if (m_bgType == BGType.Color)
                {
                    m_bgImageObj.GetComponent<DepthCamImage>().imageType = DepthCamImage.ImageType.IMAGE_TYPE_COLOR;
                }
                else if (m_bgType == BGType.Depth)
                {
                    m_bgImageObj.GetComponent<DepthCamImage>().imageType = DepthCamImage.ImageType.IMAGE_TYPE_DEPTH;
                }
            }
            else
            {
                m_bgImageObj = Instantiate(g_backgroundImageOnGUIPrefab) as GameObject;
                OnGUIDepthCamImage depthCamImage = m_bgImageObj.GetComponent<OnGUIDepthCamImage>();
                depthCamImage.m_rect = ScreenPositionHelper.SnapOnGUIRect(m_rect, m_snapToPosition);
                if (m_bgType == BGType.Color)
                {
                    depthCamImage.imageType = OnGUIDepthCamImage.ImageType.IMAGE_TYPE_COLOR;
                }
                else if (m_bgType == BGType.Depth)
                {
                    depthCamImage.imageType = OnGUIDepthCamImage.ImageType.IMAGE_TYPE_DEPTH;
                }
            }
            if (m_bgImageObj != null)
            {
                m_bgImageObj.transform.parent = transform;
            }
        }
    }

       
    // used when there is a change to the images positions
    private void FixChildGUITexturePositions()
    {
        foreach (GUITexture guiTexture in GetComponentsInChildren<GUITexture>())
        {
            if (guiTexture.GetComponent<PlayerMask>())
            {
                ScreenPositionHelper.SetGUITexturePosition(guiTexture, m_rect, m_snapToPosition, 1);
            }
            else
            {
                ScreenPositionHelper.SetGUITexturePosition(guiTexture, m_rect, m_snapToPosition, 0);
            }
        }
    }

    // used when there is a change to the images positions
    private void FixChildOnGUITexturePositions()
    {
        foreach (OnGUIPlayerMask playerMask in GetComponentsInChildren<OnGUIPlayerMask>())
        {
            playerMask.m_rect = ScreenPositionHelper.SnapOnGUIRect(m_rect,m_snapToPosition);
        }

        foreach (OnGUIDepthCamImage depthCamImage in GetComponentsInChildren<OnGUIDepthCamImage>())
        {
            depthCamImage.m_rect = ScreenPositionHelper.SnapOnGUIRect(m_rect,m_snapToPosition);
        }
    }
}
