using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using OmekFramework.Beckon.Main;

/// <summary>
/// This class create an instance of a playerMask prefab for each person.
/// and assign a material and color to according to the person type
/// </summary>
[AddComponentMenu("Omek/Sensor Image/Multiplayer Masks")]
public class MultiplayerMasks : MonoBehaviour 
{
    [System.Serializable]
    public class MaskColors : PersonProperties.StatePropertiesLists<Color32> { }

    /// <summary>
    /// The prefab of a single player mask, will be created per-player
    /// </summary> 
    public GameObject m_playerMaskPrefab;
    /// <summary>
    /// Materials for each person type
    /// </summary>
    public Material m_cursorControlingPlayerMaterial;
    public Material m_playerMaterial;
    public Material m_nonPlayerMaterial;
    /// <summary>
    /// should the colors be apply to the material or only to the PlayerMask script
    /// </summary>
    public bool m_TintPlayerMasksColors;
    
    // the colors to apply
    public MaskColors m_maskColors = new MaskColors();

    // used to hold all data needed per PlayerMask instance
    private class PlayerMaskParams
    {
        public uint personIndex;
        public PlayerMask playerMask;
        public OnGUIPlayerMask OnGUIPlayerMask;
        public GameObject playerMaskObj;
        public PlayerSelection.PersonIDState state;
    }

    private Dictionary<uint, PlayerMaskParams> m_playerMasksParams = new Dictionary<uint, PlayerMaskParams>();
    private List<uint> m_forDeletion = new List<uint>();
 
	// Use this for initialization
	void Start () 
    {
        Update();
	}
	
	// Update is called once per frame
	void Update () 
    {
        List<uint> foundIndices = new List<uint>(BeckonManager.BeckonInstance.PersonMonitor.TrackedObjectsInSystem);        

        foreach (uint personIndex in foundIndices)
        {
			PlayerMaskParams playerMaskParam = null;
            if (!m_playerMasksParams.ContainsKey(personIndex)) // new person - create a mask for it
            {
                GameObject newMask = GameObject.Instantiate(m_playerMaskPrefab) as GameObject;
                newMask.name = m_playerMaskPrefab.name + personIndex.ToString();
                newMask.layer = gameObject.layer;
                Vector3 maskLocalPosition = newMask.transform.localPosition;
                Vector3 maskLocalScale = newMask.transform.localScale;
                Quaternion maskLocalRotation = newMask.transform.localRotation;
                newMask.transform.parent = this.transform;
                newMask.transform.localPosition = maskLocalPosition;
                newMask.transform.localRotation = maskLocalRotation;
                newMask.transform.localScale = maskLocalScale;
                playerMaskParam = new PlayerMaskParams();
                playerMaskParam.playerMaskObj = newMask;
                playerMaskParam.playerMask = newMask.GetComponentInChildren<PlayerMask>();
                if (playerMaskParam.playerMask == null) // handle OnGUIPlayerMask prefabs too
                {
                    playerMaskParam.OnGUIPlayerMask = newMask.GetComponentInChildren<OnGUIPlayerMask>();
                }
                playerMaskParam.personIndex = personIndex;
                playerMaskParam.state = PlayerSelection.PersonIDState.NotExist;
                m_playerMasksParams.Add(personIndex, playerMaskParam);
            }
            playerMaskParam = m_playerMasksParams[personIndex];
            if (playerMaskParam.playerMask != null)
            {
                playerMaskParam.playerMask.personID = (int)personIndex;
            }
            else
            {
                playerMaskParam.OnGUIPlayerMask.personLabel = (int)personIndex;
            }
            UpdatePlayerMaskState(playerMaskParam);
            
        }

        //Cleanup unused masks
        foreach (KeyValuePair<uint, PlayerMaskParams> pair in m_playerMasksParams)
        {
            if (!foundIndices.Contains(pair.Key))
            {
                GameObject.Destroy(pair.Value.playerMaskObj);
                m_forDeletion.Add(pair.Key);
                break;
            }
        }

        foreach (uint key in m_forDeletion)
        {
            m_playerMasksParams.Remove(key);
        }

        m_forDeletion.Clear();
    }

    /// <summary>
    /// set Material and color according to person type
    /// </summary>
    /// <param name="playerMaskParam">contianig the playermask data</param>
    private void UpdatePlayerMaskState(PlayerMaskParams playerMaskParam)
    {
        PlayerSelection.PersonIDState state = BeckonManager.BeckonInstance.PlayerSelection.GetPersonIDState((int)playerMaskParam.personIndex);
        //State state = GetPersonState(playerMaskParam.personIndex);
        if (state != playerMaskParam.state)
        {
            playerMaskParam.state = state;
            
            if (playerMaskParam.playerMaskObj.renderer != null)
            {
                switch (state)
                {
                    case PlayerSelection.PersonIDState.NonPlayer:
                        if (m_nonPlayerMaterial != null)
                        {
                            playerMaskParam.playerMaskObj.renderer.material = m_nonPlayerMaterial;
                        }

                        break;
                    case PlayerSelection.PersonIDState.Player:
                        if (m_playerMaterial != null)
                        {
                            playerMaskParam.playerMaskObj.renderer.material = m_playerMaterial;
                        }
                        break;
                    case PlayerSelection.PersonIDState.PointerControllingPlayer:
                        if (m_cursorControlingPlayerMaterial != null)
                        {
                            playerMaskParam.playerMaskObj.renderer.material = m_cursorControlingPlayerMaterial;
                        }
                        break;                        
                }  
            }
        }
        Color32 colorAssignment;
        if (UnityPlayerColorThemes.Instance != null)
        {
            colorAssignment = UnityPlayerColorThemes.Instance.GetPlayerColorTheme((int)playerMaskParam.personIndex).m_mask;
        }
        else
        {
            colorAssignment = BeckonManager.BeckonInstance.PersonProperties.GetPropertyOfPerson<Color32>(playerMaskParam.personIndex, m_maskColors, UnityPlayerManagement.IndexingScheme);
        }
        
            
        if (playerMaskParam.playerMask != null)
        {
            playerMaskParam.playerMask.maskColor = colorAssignment;
        }
        else
        {
            playerMaskParam.OnGUIPlayerMask.maskColor = colorAssignment;
        }
        if (playerMaskParam.playerMaskObj.renderer != null && m_TintPlayerMasksColors)
        {
            playerMaskParam.playerMaskObj.renderer.material.color = colorAssignment;
        }
        

    }
               
    public void Reset()
    {
        //Cleanup unused masks
        foreach (KeyValuePair<uint, PlayerMaskParams> pair in m_playerMasksParams)
        {        
            GameObject.Destroy(pair.Value.playerMaskObj);
            m_forDeletion.Add(pair.Key);
            break;
        }

        foreach (uint key in m_forDeletion)
        {
            m_playerMasksParams.Remove(key);
        }

        m_forDeletion.Clear();
    }
}
