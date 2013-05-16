using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using System;
using OmekFramework.Beckon.Main;
using OmekFramework.Beckon.BasicTypes;
using OmekFramework.Common.BasicTypes;
using Omek;
using OmekFramework.Beckon.Data;

[AddComponentMenu("Omek/Tracking Controllers/Skeleton Tracking Animation")]
[RequireComponent(typeof(Animation))]
public class SkeletonTrackingAnimation : MonoBehaviour
{
    [Serializable]
    public class ConfidanceParams
    {
        
        internal float m_confidenceWeight = -1;
        internal float m_lastCurrentStateTime = 0;
        internal bool m_lastFrameConfidence = true;
        internal uint m_currentConfidence = 0;
    }

    [Serializable]
    public class TransformWrapper
    {
        public Omek.JointID m_jointID;
        public Transform m_transform;
        public Quaternion m_originalRotation;
        public Quaternion m_lastRotation;
        internal ConfidanceParams m_confidanceParams;
        public bool m_active = true;
        public bool m_mandatory = false;
        public Vector3 m_TPosePosition;
        public RollJointWrapper m_rollJoint = null;

        public TransformWrapper(Omek.JointID jointID, bool mandatory)
        {
            m_jointID = jointID;
            m_mandatory = mandatory;
        }
    }

    [Serializable]
    public class RollJointWrapper
    {
        public string m_name;        
        public Quaternion m_originalRotation;
        public Vector3 m_localTwistDir;
        public TransformWrapper m_parentWrapper;
		public Quaternion m_originalParentRotation;
        public Quaternion m_inverseOriginalParentRotation;
        public bool m_active = true;
        public Transform m_transform;

        public RollJointWrapper(string name)
        {
            m_name = name;
        }
    }    
    public bool m_usePlayerID = false;
    public int m_personIndex = -1;
    public int m_defaultLayerOfTrackingAnimation = 1;
    public bool m_startAnimationAutomatically = true;
    public bool m_useSmoothing = true;
    public float m_animationSmoothing = 540;
    public bool m_mirror;
    public bool m_useRetargetSkeleton;
    public float m_confidenceChangeRate = 1;
    public float m_confidenceChangeSafetyTime = 0.3f;

    // will contain all the above transforms ordered so a transform always appear before its children
    public List<TransformWrapper> m_hierarchyList = new List<TransformWrapper>();
    // will  hold all the roll joint transforms
    public List<RollJointWrapper> m_rollJointsList = new List<RollJointWrapper>();
    private float m_weight = 0; // current animation weight
    private Transform m_transform; // used to improve performance
    private string m_characterName; // the character name given as identifier to Beckon SDK
    private bool m_initalized = false;
    private int m_lastPersonIndex = -1; // what was the last person we got tracking of
    private ISkeleton m_currentSkeleton;

    /// <summary>
    /// this should be in constructor as the editor relay on this structure to exist
    /// </summary>
    public SkeletonTrackingAnimation()
    {
        m_hierarchyList.Add(new TransformWrapper(Omek.JointID.hips,  true));
        m_hierarchyList.Add(new TransformWrapper(Omek.JointID.waist,  true));
        m_hierarchyList.Add(new TransformWrapper(Omek.JointID.spine1,  false));
        m_hierarchyList.Add(new TransformWrapper(Omek.JointID.spine2,  false));
        m_hierarchyList.Add(new TransformWrapper(Omek.JointID.spine3,  false));
        m_hierarchyList.Add(new TransformWrapper(Omek.JointID.spine4,  false));

        m_hierarchyList.Add(new TransformWrapper(Omek.JointID.leftCollar,  false));
        m_hierarchyList.Add(new TransformWrapper(Omek.JointID.rightCollar,  false));

        m_hierarchyList.Add(new TransformWrapper(Omek.JointID.leftShoulder,  true));
        m_hierarchyList.Add(new TransformWrapper(Omek.JointID.rightShoulder,  true));
        m_hierarchyList.Add(new TransformWrapper(Omek.JointID.leftElbow,  true));

        m_hierarchyList.Add(new TransformWrapper(Omek.JointID.rightElbow,  true));
        m_hierarchyList.Add(new TransformWrapper(Omek.JointID.leftFingerTip,  true));
        m_hierarchyList.Add(new TransformWrapper(Omek.JointID.rightFingerTip,  true));

        m_hierarchyList.Add(new TransformWrapper(Omek.JointID.leftHip,  true));
        m_hierarchyList.Add(new TransformWrapper(Omek.JointID.rightHip,  true));
        m_hierarchyList.Add(new TransformWrapper(Omek.JointID.leftKnee,  true));
        m_hierarchyList.Add(new TransformWrapper(Omek.JointID.rightKnee,  true));

        m_hierarchyList.Add(new TransformWrapper(Omek.JointID.leftFoot,  true));
        m_hierarchyList.Add(new TransformWrapper(Omek.JointID.rightFoot,  true));
                
        m_hierarchyList.Add(new TransformWrapper(Omek.JointID.neck,   false));
        m_hierarchyList.Add(new TransformWrapper(Omek.JointID.head,  true));

        m_rollJointsList.Add(new RollJointWrapper("LeftArmRoll"));
        m_rollJointsList.Add(new RollJointWrapper("RightArmRoll"));
        m_rollJointsList.Add(new RollJointWrapper("LeftForeArmRoll"));
        m_rollJointsList.Add(new RollJointWrapper("RightForeArmRoll"));
        m_rollJointsList.Add(new RollJointWrapper("LeftUpLegRoll"));
        m_rollJointsList.Add(new RollJointWrapper("RightUpLegRoll"));
        m_rollJointsList.Add(new RollJointWrapper("LeftLegRoll"));
        m_rollJointsList.Add(new RollJointWrapper("RightLegRoll"));
    }
    // Use this for initialization    
    void Awake()
    {
        m_transform = transform;

        bool beckonAlive = BeckonManager.BeckonInstance.IsInit();
        if (beckonAlive)
        {
            OMKStatus rc;
            m_currentSkeleton = Factory.createSkeleton(out rc);
            if (rc == OMKStatus.OMK_SUCCESS)
            {
                InitializeHeirarchy();
                CheckForPersonChange();
            }
            else
            {
                Debug.LogError("Couldn't create Skeleton: " + rc,this);
            }
        }
        else
        {
            Debug.LogError("Beckon is not initialized");
        }
    }

    /// <summary>
    /// We start animation on Start and now Awake so it won't be overridden by auto played animation
    /// </summary>
    void Start()
    {
        CreateAnimation();       
    }

    void OnDestroy()
    {
        if (m_currentSkeleton != null)
        {
            Factory.releaseSkeleton(m_currentSkeleton);
        }
    }
    
    /// <summary>
    /// Check if the person this animation should follow - changed
    /// </summary>
    private void CheckForPersonChange()
    {
        int currentPersonIndex = m_personIndex;
        if (m_usePlayerID)
        {
            currentPersonIndex = BeckonManager.BeckonInstance.PlayerSelection.TrackedObjectIdOfPlayerId(m_personIndex);
        }
        
        if (m_lastPersonIndex != currentPersonIndex)
        {
            RetargetMotionForPlayer(currentPersonIndex);
        }
    }

    /// <summary>
    /// Initialize the Joints Hierarchy used to perform the animation
    /// </summary>
    private void InitializeHeirarchy()
    {
        //fill m_hierarchyList with all the transforms given by the user
        // ordered so a transform always appear before its children
        // foreach Transform we store also the relevant joint from Beckon
        Quaternion baseRotation = m_transform.rotation;
        m_transform.rotation = Quaternion.identity;
        foreach (TransformWrapper transformWrapper in m_hierarchyList)
        {
            if (transformWrapper.m_transform != null)
            {
                transformWrapper.m_originalRotation = transformWrapper.m_transform.rotation;
                transformWrapper.m_lastRotation = transformWrapper.m_transform.rotation;
                transformWrapper.m_TPosePosition = m_transform.InverseTransformPoint(transformWrapper.m_transform.position);
                transformWrapper.m_confidanceParams = new ConfidanceParams();
            }
            else if (transformWrapper.m_mandatory)
            {
                Debug.LogError("Mandatory transform is missing for joint: " + transformWrapper.m_jointID);
            }
        }

        foreach (RollJointWrapper rollJointWrapper in m_rollJointsList)
        {
            if (rollJointWrapper.m_transform != null)
            {
                rollJointWrapper.m_originalRotation = rollJointWrapper.m_transform.localRotation;
                rollJointWrapper.m_localTwistDir = rollJointWrapper.m_transform.GetChild(0).position - rollJointWrapper.m_transform.parent.position;  
            }            
        }

        JoinRollJoint("LeftArmRoll", Omek.JointID.leftShoulder);
        JoinRollJoint("RightArmRoll", Omek.JointID.rightShoulder);
        JoinRollJoint("LeftForeArmRoll", Omek.JointID.leftElbow);
        JoinRollJoint("RightForeArmRoll", Omek.JointID.rightElbow);
        JoinRollJoint("LeftUpLegRoll", Omek.JointID.leftHip);
        JoinRollJoint("RightUpLegRoll", Omek.JointID.rightHip);
        JoinRollJoint("LeftLegRoll", Omek.JointID.leftKnee);
        JoinRollJoint("RightLegRoll", Omek.JointID.rightKnee);
        m_transform.rotation = baseRotation;
    }

    // initialize a roll joint data for the given joint as parent
    private void JoinRollJoint(string rollJointName, Omek.JointID parentJointID)
    {
        TransformWrapper tw = m_hierarchyList.Find((a) => { return a.m_jointID == parentJointID; });
        RollJointWrapper rjw = m_rollJointsList.Find((a) => { return a.m_name == rollJointName; });
        tw.m_rollJoint = rjw;
        rjw.m_parentWrapper = tw;
		if (tw.m_transform != null)
		{
			rjw.m_originalParentRotation = tw.m_transform.localRotation;
            rjw.m_inverseOriginalParentRotation = Quaternion.Inverse(rjw.m_originalParentRotation);
		}
    }    

    /// <summary>
    /// retarget the movement of specific person to this avatar
    /// </summary>
    /// <param name="personIndex">the person to retarget the movement of</param>
    public void RetargetMotionForPlayer(int personIndex)
    {
        m_initalized = false;

        if (!BeckonManager.BeckonInstance.IsInit())
            return;

        m_characterName = gameObject.name + GetInstanceID();
        BeckonManager.BeckonInstance.PersonAnalyzer.removeTarget(m_characterName);
        m_lastPersonIndex = personIndex;        
        if (m_lastPersonIndex >= 0)
        {
            // create a skeleton according to avatar proportions
            OMKStatus rc;
            IEditableSkeleton targetSkeleton = Factory.createEditableSkeleton(out rc);
            float[] worldPos = new float[3];
            float[] imagePos = new float[] {0,0,0};
            if (rc == OMKStatus.OMK_SUCCESS)
            {
                foreach (TransformWrapper transformWrapper in m_hierarchyList)
                {
                    if (transformWrapper.m_transform != null)
                    {
                        worldPos[0] = -transformWrapper.m_TPosePosition.x;
                        worldPos[1] = transformWrapper.m_TPosePosition.y;
                        worldPos[2] = transformWrapper.m_TPosePosition.z;
                        targetSkeleton.setJointPosition((JointID)transformWrapper.m_jointID,worldPos,imagePos);
                    }
                    else if (transformWrapper.m_mandatory)
                    {
                        Debug.LogError("A mandatory joint " + transformWrapper.m_jointID + " has no transform assigned. Couldn't retarget Character. ", this);
                        return;
                    }
                }
                // retarget
                rc = BeckonManager.BeckonInstance.PersonAnalyzer.retargetMotionFromSkeleton(m_characterName, targetSkeleton, (uint)m_lastPersonIndex);
                Factory.releaseSkeleton(targetSkeleton);
                if (rc != OMKStatus.OMK_SUCCESS)
                {
                    Debug.LogError("Couldn't retarget Character. " + rc, this);
                }
                else
                {
                    m_initalized = true;
                }
            }
        }
    }

    /// <summary>
    /// Create an empty AnimationClip named "trackingAnimation".
    /// We used this clip as an interface to blend tracking animation using the standard Unity Animation System
    /// </summary>
    private void CreateAnimation()
    {
        AnimationClip clip = new AnimationClip();
        animation.AddClip(clip, "trackingAnimation");
        animation["trackingAnimation"].wrapMode = WrapMode.Loop;
        animation["trackingAnimation"].layer = m_defaultLayerOfTrackingAnimation;
        if (m_startAnimationAutomatically == true)
        {

            animation.Play("trackingAnimation", AnimationPlayMode.Mix);
            animation.Blend("trackingAnimation", 0.99f);
        }
    } 


    // We use LateUpdate so the transforms will be updated by all other animation.
    // We'll compute the transform of the tracking animation and blend them together
    void LateUpdate()
    {
        CheckForPersonChange();
		if (m_initalized)
        {
            
            m_weight = calcualteCurrentWeight();
            if (m_weight > 0)
            {               
                OMKStatus rc = OMKStatus.OMK_ERROR_SKELETON_NOT_FOUND;
                if (m_useRetargetSkeleton)
                {
                    rc = BeckonManager.BeckonInstance.PersonAnalyzer.getRetargetedSkeleton(m_characterName, ref m_currentSkeleton);
                }
                else
                {
                    Omek.IPerson person;
                    rc = BeckonManager.BeckonInstance.PersonAnalyzer.getPerson((uint)m_lastPersonIndex,out person);
                    if (rc == OMKStatus.OMK_SUCCESS)
                    {
                        rc = person.copySkeleton(m_currentSkeleton);
                    }                 
                }
                if (rc != OMKStatus.OMK_SUCCESS)
                {
                    return;
                }
                Quaternion baseRotation = m_transform.rotation;
                m_transform.rotation = Quaternion.identity;
                float[] beckonQuat = new float[4];
                foreach (TransformWrapper transformWrapper in m_hierarchyList)
                {
                    if (!transformWrapper.m_active || transformWrapper.m_transform== null)
                    {
                        continue;
                    }
                    Omek.JointID jointID = transformWrapper.m_jointID;
                    if (m_mirror)
                    {
                        jointID = GetReflectedJointID(jointID);
                    }

                    rc = m_currentSkeleton.getJointOrientation((JointID)jointID, true, beckonQuat);
                    
                    if (rc== OMKStatus.OMK_SUCCESS)
                    {
                        // fixing quaternion handness                        
                        Quaternion quat = UnityConverter.ToUnityQuaternion(beckonQuat);
                        if (m_mirror)
                        {
                            quat.x = -quat.x;
                            quat.w = -quat.w;                       
                        }                                    
     
                        // add the original base T-pose rotation to the current rotation.
                        quat = quat * transformWrapper.m_originalRotation;

                        //Smoothly rotate towards the new quaternion based on the confidence
                       
                        UpdateConfidence(transformWrapper.m_confidanceParams,jointID);
                        if (transformWrapper.m_confidanceParams.m_currentConfidence >= 70)
                        {
                            if (m_useSmoothing == true)
                            {
                                quat = RotateTowards(transformWrapper.m_lastRotation, quat, Time.deltaTime * m_animationSmoothing);
                            }
                        }
                        else
                        {
                            quat = transformWrapper.m_lastRotation;
                        }

                        // save the current rotation for next frame
                        transformWrapper.m_lastRotation = quat;
                        
                        // smoothly blend in, the orientation from tracking with the transform orientation from all other animations base on animation weight
                        transformWrapper.m_transform.rotation = Quaternion.Slerp(transformWrapper.m_transform.rotation, quat, m_weight * transformWrapper.m_confidanceParams.m_confidenceWeight);

                        if (transformWrapper.m_rollJoint != null && transformWrapper.m_rollJoint.m_transform != null && transformWrapper.m_rollJoint.m_active)
                        {
                            rollRotation(transformWrapper.m_rollJoint);
                        }
                        
                    }

                }
                m_transform.rotation = baseRotation;

            }
        }

    }

    // update the confidence  for a joint
    private void UpdateConfidence(ConfidanceParams confidanceParams, Omek.JointID jointID)
    {
        if (m_lastPersonIndex < 0)
            return;
        BeckonData.Persons[(uint)m_lastPersonIndex].Skeleton[(Omek.JointID)jointID].Confidence.Get(out confidanceParams.m_currentConfidence);       
        // do we have good confidence for this frame
        bool isConfidant = (confidanceParams.m_currentConfidence >= 70);

        // initialize if this the first time
        if (confidanceParams.m_confidenceWeight == -1)
        {
            confidanceParams.m_lastFrameConfidence = isConfidant;
            confidanceParams.m_lastCurrentStateTime = Time.realtimeSinceStartup;
            confidanceParams.m_confidenceWeight = isConfidant ? 1 : 0;
            return;
        }

        // if there is a difference from last value and enough time passed since the last change
        if (isConfidant != confidanceParams.m_lastFrameConfidence && Time.realtimeSinceStartup >= confidanceParams.m_lastCurrentStateTime + m_confidenceChangeSafetyTime)
        {
            confidanceParams.m_lastFrameConfidence = isConfidant;
            confidanceParams.m_lastCurrentStateTime = Time.realtimeSinceStartup;
        }
        else if (isConfidant == confidanceParams.m_lastFrameConfidence) // we have the same value as before - update last known time
        {
            confidanceParams.m_lastCurrentStateTime = Time.realtimeSinceStartup;
        }
        // move the real confidenceWeight towards 1 or 0 according to the current value
        if (confidanceParams.m_lastFrameConfidence)
        {
            confidanceParams.m_confidenceWeight = Mathf.MoveTowards(confidanceParams.m_confidenceWeight, 1, Time.deltaTime * m_confidenceChangeRate);
        }
        else
        {
            confidanceParams.m_confidenceWeight = Mathf.MoveTowards(confidanceParams.m_confidenceWeight, 0, Time.deltaTime * m_confidenceChangeRate);
        }
    }    

    /// <summary>
    /// Transfer half the rotation from a parent transform to a roll joint.
    /// The rotation is transferred only along the rotation axis of the roll joint
    /// </summary>
    /// <param name="rollJointWrapper">Roll joint information</param>
    private void rollRotation(RollJointWrapper rollJointWrapper)
    {
        Transform parent = rollJointWrapper.m_transform.parent;		
        // get the current twist direction
        Vector3 twistDir = (Quaternion.Inverse(parent.localRotation) * rollJointWrapper.m_inverseOriginalParentRotation) * rollJointWrapper.m_localTwistDir;
        // get the swing direction
        Quaternion dirRotation = Quaternion.FromToRotation(twistDir, rollJointWrapper.m_localTwistDir);
        // the twist rotation is the full rotation minus the swing rotation
        Quaternion twistQuat = rollJointWrapper.m_originalParentRotation * parent.localRotation * Quaternion.Inverse(dirRotation);
        
        // get the twist angle
        float angle;
        Vector3 rotation_axis;
        twistQuat.ToAngleAxis(out angle, out rotation_axis);
                
        if (!Mathf.Approximately(angle, 0) && !Mathf.Approximately(angle, 360))
        {
            // apply half the angle to the parent and half to the roll joint
            float halfAngle = 0;
            if (angle > 180)
            {
                halfAngle = (angle + 360) / 2;
            }
            else
            {
                halfAngle = angle / 2;
            }
            twistQuat = Quaternion.AngleAxis(halfAngle, rotation_axis);
            parent.localRotation = Quaternion.Slerp(parent.localRotation, parent.localRotation * Quaternion.Inverse(twistQuat), m_weight);
            rollJointWrapper.m_transform.localRotation = Quaternion.Slerp(rollJointWrapper.m_transform.localRotation, twistQuat * rollJointWrapper.m_originalRotation, m_weight);
        }
    }
 
    /// <summary>
    /// calculate the weight of the tracking animation
    /// </summary>
    /// <returns>The weight of the tracking animation</returns>
    private float calcualteCurrentWeight()
    {   
        AnimationState trackingAnim = animation["trackingAnimation"];
        float higherLayersAnimationsWeights = 0;
        float myLayerAnimationsWeights = 0;
        // try to do the same calculation the animation system does
        IEnumerator iter = animation.GetEnumerator();
        iter.Reset();

        while (iter.MoveNext())
        {
            AnimationState state = iter.Current as AnimationState;
            if (state.enabled == true && state.blendMode != AnimationBlendMode.Additive)
            {
                if (state.layer > trackingAnim.layer)
                    higherLayersAnimationsWeights += state.weight;
                if (state.layer == trackingAnim.layer)
                    myLayerAnimationsWeights += state.weight;
            }
        }

        if (higherLayersAnimationsWeights >= 1)
        {
            return 0;
        }
        else if (higherLayersAnimationsWeights + myLayerAnimationsWeights <= 1)
            return trackingAnim.weight;
        else
            return trackingAnim.weight / myLayerAnimationsWeights * (1 - higherLayersAnimationsWeights);
    }

    /// <summary>
    /// Set specific joint be active so it will play the animation
    /// </summary>
    /// <param name="jointID">the joint id to effect</param>
    /// <param name="isActive">should the joint be activate or deactivate</param>
    /// <param name="applyForAllChildren">should the joint children state be changed as well</param>
    public void SetJointActive(Omek.JointID jointID,bool isActive,bool applyForAllChildren = false)
    {
        foreach (TransformWrapper tw in m_hierarchyList)
        {
            if (tw.m_jointID == jointID)
            {

                if (applyForAllChildren)
                {
                    SetJointActiveForChildren(tw.m_transform,isActive);
                }
                else
                {
                    tw.m_active = isActive;
                }
                break;
            }
        }
        
    }

    /// <summary>
    /// Set specific joint be active so it will play the animation
    /// </summary>
    /// <param name="jointTransform">the joint id to effect</param>
    /// <param name="isActive">should the joint be activate or deactivate</param>
    /// <param name="applyForAllChildren">should the joint children state be changed as well</param>
    public void SetJointActive(Transform jointTransform, bool isActive, bool applyForAllChildren = false)
    {
        foreach (TransformWrapper tw in m_hierarchyList)
        {
            if (tw.m_transform == jointTransform)
            {

                if (applyForAllChildren)
                {
                    SetJointActiveForChildren(tw.m_transform, isActive);
                }
                else
                {
                    tw.m_active = isActive;
                }
                break;
            }
        }

        foreach (RollJointWrapper rjw in m_rollJointsList)
        {
            if (rjw.m_transform == jointTransform)
            {

                if (applyForAllChildren)
                {
                    SetJointActiveForChildren(rjw.m_transform, isActive);
                }
                else
                {
                    rjw.m_active = isActive;
                }
                break;
            }
        }
    }

    private void SetJointActiveForChildren(Transform trans,bool isActive)
    {
        TransformWrapper tw = m_hierarchyList.Find((a) => { return a.m_transform == trans; });
        if (tw != null)
        {
            tw.m_active = isActive;
        }        
        RollJointWrapper rjw = m_rollJointsList.Find((a) => { return a.m_transform == trans; });
        if (rjw != null)
        {
            rjw.m_active = isActive;
        }
        foreach (Transform child in trans)
        {
            SetJointActiveForChildren(child, isActive);
        }
    }

    // a better version of RotateTowards with a minimum threshold
    private static Quaternion RotateTowards(Quaternion from, Quaternion to, float maxDegreesDelta)
    {
        if (maxDegreesDelta < Mathf.Epsilon)
        {
            return from;
        }
        return Quaternion.RotateTowards(from, to, maxDegreesDelta);
    }
    
    // return the reflected joint of a given joint
    private static Omek.JointID GetReflectedJointID(Omek.JointID jointID)
    {
        switch (jointID)
        {
            case Omek.JointID.leftCollar:
                return Omek.JointID.rightCollar;
            case Omek.JointID.rightCollar:
                return Omek.JointID.leftCollar;
            case Omek.JointID.leftShoulder:
                return Omek.JointID.rightShoulder;
            case Omek.JointID.rightShoulder:
                return Omek.JointID.leftShoulder;
            case Omek.JointID.leftElbow:
                return Omek.JointID.rightElbow;
            case Omek.JointID.rightElbow:
                return Omek.JointID.leftElbow;
            case Omek.JointID.leftFingerTip:
                return Omek.JointID.rightFingerTip;
            case Omek.JointID.rightFingerTip:
                return Omek.JointID.leftFingerTip;
            case Omek.JointID.leftHip:
                return Omek.JointID.rightHip;
            case Omek.JointID.rightHip:
                return Omek.JointID.leftHip;
            case Omek.JointID.leftKnee:
                return Omek.JointID.rightKnee;
            case Omek.JointID.rightKnee:
                return Omek.JointID.leftKnee;
            case Omek.JointID.rightFoot:
                return Omek.JointID.leftFoot;
            case Omek.JointID.leftFoot:
                return Omek.JointID.rightFoot;

            default:
                return jointID;
        }
    }

    
    
}
