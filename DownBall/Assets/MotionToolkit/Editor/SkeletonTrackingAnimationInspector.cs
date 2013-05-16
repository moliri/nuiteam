using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System;
using OmekFramework.Beckon.BasicTypes;

[CustomEditor(typeof(SkeletonTrackingAnimation))]
public class SkeletonTrackingAnimationInspector : Editor
{
    public static bool[] foldouts= null;
    private SerializedObject m_serializedObject;
    private SerializedProperty m_usePlayerID;
    private SerializedProperty m_personIndex;
    private SerializedProperty m_defaultLayerOfTrackingAnimation;
    private SerializedProperty m_startAnimationAutomatically;
    private SerializedProperty m_useSmoothing;
    private SerializedProperty m_animationSmoothing;
    private SerializedProperty m_mirror;
    private SerializedProperty m_useRetargetSkeleton;
    private SerializedProperty m_confidenceChangeRate;
    private SerializedProperty m_confidenceChangeSafetyTime;
    private SerializedProperty m_hierarchyList;
    private SerializedProperty m_rollJointsList;

    public void OnEnable()
    {
        m_serializedObject = new SerializedObject(target);
        m_usePlayerID = m_serializedObject.FindProperty("m_usePlayerID");
        m_personIndex = m_serializedObject.FindProperty("m_personIndex");
        m_defaultLayerOfTrackingAnimation = m_serializedObject.FindProperty("m_defaultLayerOfTrackingAnimation");
        m_startAnimationAutomatically = m_serializedObject.FindProperty("m_startAnimationAutomatically");
		m_useSmoothing = m_serializedObject.FindProperty("m_useSmoothing");
		m_animationSmoothing = m_serializedObject.FindProperty("m_animationSmoothing");
		m_mirror = m_serializedObject.FindProperty("m_mirror");
        m_useRetargetSkeleton = m_serializedObject.FindProperty("m_useRetargetSkeleton");
        m_hierarchyList = m_serializedObject.FindProperty("m_hierarchyList");
        m_rollJointsList = m_serializedObject.FindProperty("m_rollJointsList");
        m_confidenceChangeRate = m_serializedObject.FindProperty("m_confidenceChangeRate");
        m_confidenceChangeSafetyTime = m_serializedObject.FindProperty("m_confidenceChangeSafetyTime");
    }
    public override void OnInspectorGUI()
    {
        GUI.skin = EditorGUIUtility.GetBuiltinSkin(EditorSkin.Inspector);
        EditorGUILayout.ObjectField("Script", MonoScript.FromMonoBehaviour((MonoBehaviour)target), typeof(MonoScript), false);
        m_serializedObject.Update();
        EditorGUILayout.PropertyField(m_usePlayerID, new GUIContent("Use Player ID", "Does the animation attached to a specific person ID or player ID (which may be assigned to different persons according to the player selection strategy)"));
        if (m_usePlayerID.boolValue)
        {
            EditorGUILayout.PropertyField(m_personIndex, new GUIContent("Player ID", "Which player to attach this tracking animation (A player ID may be assigned to different persons according to the player selection strategy)"));
        }
        else
        {
            EditorGUILayout.PropertyField(m_personIndex, new GUIContent("Person ID", "Which person to attach this tracking animation"));
        }
        EditorGUILayout.PropertyField(m_defaultLayerOfTrackingAnimation, new GUIContent("Default Layer Of Tracking Animation", "The layer that will be assign to 'trackingAnimation' by default"));
        EditorGUILayout.PropertyField(m_startAnimationAutomatically, new GUIContent("Start Animation Automatically", "should 'trackingAnimation' be started automatically"));
        EditorGUILayout.PropertyField(m_useSmoothing, new GUIContent("Use Smoothing", "Set to use animation smoothing"));
        EditorGUILayout.PropertyField(m_animationSmoothing, new GUIContent("Animation Smoothing", "Lower values will smooth the animation, but cause delays"));
        EditorGUILayout.PropertyField(m_mirror, new GUIContent("Mirror", "Should the tracking be reflected as if standing in front of a mirror"));
        EditorGUILayout.PropertyField(m_useRetargetSkeleton, new GUIContent("Use Retarget Skeleton", "Should we use retargeted skeleton or not"));
        EditorGUILayout.PropertyField(m_confidenceChangeRate, new GUIContent("Confidence Change Rate", "How fast to blend the animation in and out when we lose the confidence of a joint"));
        EditorGUILayout.PropertyField(m_confidenceChangeSafetyTime, new GUIContent("Confidence Change Safety Time", "How long is the time after losing a joint confidence and before starting the blend out"));
        
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.Separator();
        if (GUILayout.Button("Configure Automatically"))
        {
            AutoConfigureTransforms();
        }
        EditorGUILayout.Separator();
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.Separator();
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Active", GUILayout.Width(60),GUILayout.ExpandWidth(false));
        EditorGUILayout.LabelField("Joint Name", GUILayout.ExpandWidth(false));
        EditorGUILayout.Separator();
        EditorGUILayout.LabelField("Joint Transform",GUILayout.ExpandWidth(false));
        EditorGUILayout.Separator();
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.Separator();
        EditorGUI.indentLevel++;
        GUILayout.BeginVertical();
        for (int i = 0; i < m_hierarchyList.arraySize; i++)
        {
            SerializedProperty sp = m_hierarchyList.GetArrayElementAtIndex(i);            
            SerializedProperty jointIDProperty = sp.FindPropertyRelative("m_jointID");
            SerializedProperty transformProperty = sp.FindPropertyRelative("m_transform");
            SerializedProperty activeProperty = sp.FindPropertyRelative("m_active");
            GUILayout.BeginHorizontal();
            EditorGUILayout.PropertyField(activeProperty, new GUIContent(""), true, GUILayout.Width(50), GUILayout.ExpandWidth(false));
            GUIStyle style = new GUIStyle(GUI.skin.label);
            if (transformProperty.prefabOverride)
            {
                style.fontStyle = FontStyle.Bold;
            }
            EditorGUILayout.LabelField(jointIDProperty.enumNames[jointIDProperty.enumValueIndex], style);
            EditorGUILayout.PropertyField(transformProperty, new GUIContent(""));
            
            
            GUILayout.EndHorizontal();
        }
        GUILayout.EndVertical();
        EditorGUI.indentLevel--;

        
        m_serializedObject.ApplyModifiedProperties();

        EditorGUILayout.Separator();
        EditorGUILayout.LabelField("Roll Joints:");
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Active", GUILayout.Width(60), GUILayout.ExpandWidth(false));
        EditorGUILayout.LabelField("Joint Name", GUILayout.ExpandWidth(false));
        EditorGUILayout.Separator();
        EditorGUILayout.LabelField("Joint Transform", GUILayout.ExpandWidth(false));
        EditorGUILayout.Separator();
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.Separator();
        EditorGUI.indentLevel++;
        GUILayout.BeginVertical();
        for (int i = 0; i < m_rollJointsList.arraySize; i++)
        {
            SerializedProperty sp = m_rollJointsList.GetArrayElementAtIndex(i);
            SerializedProperty jointName = sp.FindPropertyRelative("m_name");
            //SerializedProperty transformProperty = sp.FindPropertyRelative("m_transform");
            SerializedProperty transformProperty = sp.FindPropertyRelative("m_transform");
            SerializedProperty activeProperty = sp.FindPropertyRelative("m_active");
            GUILayout.BeginHorizontal();
            EditorGUILayout.PropertyField(activeProperty, new GUIContent(""), true, GUILayout.Width(50), GUILayout.ExpandWidth(false));
            GUIStyle style = new GUIStyle(GUI.skin.label);
            if (transformProperty.prefabOverride)
            {
                style.fontStyle = FontStyle.Bold;
            }
            EditorGUILayout.LabelField(jointName.stringValue, style);
            EditorGUILayout.PropertyField(transformProperty, new GUIContent(""));
            
            GUILayout.EndHorizontal();
        }
        GUILayout.EndVertical();
        EditorGUI.indentLevel--;
       
        m_serializedObject.ApplyModifiedProperties();
    }


    private List<Dictionary<Omek.JointID, string>> m_knowenConfigurations =
        new List<Dictionary<Omek.JointID, string>>() 
        {
            new Dictionary<Omek.JointID,string>()
            {                
                 {Omek.JointID.head, "Head"          },
                 {Omek.JointID.neck,"Neck"           },
                 {Omek.JointID.rightFingerTip, "RightHand"},
                 {Omek.JointID.leftFingerTip, "LeftHand" },
                 {Omek.JointID.rightShoulder,"RightArm" },
                 {Omek.JointID.leftShoulder, "LeftArm"  },
                 {Omek.JointID.rightElbow, "RightForeArm"    },
                 {Omek.JointID.leftElbow, "LeftForeArm"     },
                 {Omek.JointID.rightCollar, "RightShoulder"   },
                 {Omek.JointID.leftCollar, "LeftShoulder"    },
                 {Omek.JointID.hips, "Hips"          },
                 {Omek.JointID.rightKnee, "RightLeg"     },
                 {Omek.JointID.leftKnee, "LeftLeg"      },
                 {Omek.JointID.rightFoot, "RightFoot"     },
                 {Omek.JointID.leftFoot, "LeftFoot"      },
                 {Omek.JointID.rightHip, "RightUpLeg"      },
                 {Omek.JointID.leftHip, "LeftUpLeg"       },
                 {Omek.JointID.spine1, "Spine1"        },
                 {Omek.JointID.spine2, "Spine2"        },
                 {Omek.JointID.spine3, "Spine3"        },
                 {Omek.JointID.spine4, "Spine4"        },
                 {Omek.JointID.waist, "Spine"        }
            },
            new Dictionary<Omek.JointID,string>()
            {                
                 {Omek.JointID.head, "Head"          },
                 {Omek.JointID.neck,"Neck"           },
                 {Omek.JointID.rightFingerTip, "R Hand"},
                 {Omek.JointID.leftFingerTip, "L Hand" },
                 {Omek.JointID.rightShoulder,"R UpperArm" },
                 {Omek.JointID.leftShoulder, "L UpperArm"  },
                 {Omek.JointID.rightElbow, "R Forearm"    },
                 {Omek.JointID.leftElbow, "L Forearm"     },
                 {Omek.JointID.rightCollar, "R Clavicle"   },
                 {Omek.JointID.leftCollar, "L Clavicle"    },
                 {Omek.JointID.hips, "Hips"          },
                 {Omek.JointID.rightKnee, "R Calf"     },
                 {Omek.JointID.leftKnee, "L Calf"      },
                 {Omek.JointID.rightFoot, "R Foot"     },
                 {Omek.JointID.leftFoot, "L Foot"      },
                 {Omek.JointID.rightHip, "R Thigh"      },
                 {Omek.JointID.leftHip, "L Thigh"       },
                 {Omek.JointID.spine1, "Spine1"        },
                 {Omek.JointID.spine2, "Spine2"        },
                 {Omek.JointID.spine3, "Spine3"        },
                 {Omek.JointID.spine4, "Spine4"        },
                 {Omek.JointID.waist, "Spine"        }
            },
            new Dictionary<Omek.JointID,string>()
            {                
                 {Omek.JointID.head, "Head"          },
                 {Omek.JointID.neck,"Neck"           },
                 {Omek.JointID.rightFingerTip, "Hand_R"},
                 {Omek.JointID.leftFingerTip, "Hand_L" },
                 {Omek.JointID.rightShoulder,"UpArm_R" },
                 {Omek.JointID.leftShoulder, "UpArm_L"  },
                 {Omek.JointID.rightElbow, "LoArm_R"    },
                 {Omek.JointID.leftElbow, "LoArm_L"     },
                 {Omek.JointID.rightCollar, "Shoulder_R"   },
                 {Omek.JointID.leftCollar, "Shoulder_L"    },
                 {Omek.JointID.hips, "Hips"          },
                 {Omek.JointID.rightKnee, "LoLeg_R"     },
                 {Omek.JointID.leftKnee, "LoLeg_L"      },
                 {Omek.JointID.rightFoot, "Foot_R"     },
                 {Omek.JointID.leftFoot, "Foot_L"      },
                 {Omek.JointID.rightHip, "UpLeg_R"      },
                 {Omek.JointID.leftHip, "UpLeg_L"       },
                 {Omek.JointID.spine1, "Spine1"        },
                 {Omek.JointID.spine2, "Spine2"        },
                 {Omek.JointID.spine3, "Spine3"        },
                 {Omek.JointID.spine4, "Spine4"        },
                 {Omek.JointID.waist, "Root"        }
            },
            new Dictionary<Omek.JointID,string>()
            {                
                 {Omek.JointID.head, "Head"          },
                 {Omek.JointID.neck,"Neck"           },
                 {Omek.JointID.rightFingerTip, "RightWrist"},
                 {Omek.JointID.leftFingerTip, "LeftWrist" },
                 {Omek.JointID.rightShoulder,"RightShoulder" },
                 {Omek.JointID.leftShoulder, "LeftShoulder"  },
                 {Omek.JointID.rightElbow, "RightElbow"    },
                 {Omek.JointID.leftElbow, "LeftElbow"     },
                 {Omek.JointID.rightCollar, "RightCollar"   },
                 {Omek.JointID.leftCollar, "LeftCollar"    },
                 {Omek.JointID.hips, "Hips"          },
                 {Omek.JointID.rightKnee, "RightKnee"     },
                 {Omek.JointID.leftKnee, "LeftKnee"      },
                 {Omek.JointID.rightFoot, "RightAnkle"     },
                 {Omek.JointID.leftFoot, "LeftAnkle"      },
                 {Omek.JointID.rightHip, "RightHip"      },
                 {Omek.JointID.leftHip, "LeftHip"       },
                 {Omek.JointID.spine1, "Chest"        },
            },
            new Dictionary<Omek.JointID,string>()
            {                
                 {Omek.JointID.head, "head"          },
                 {Omek.JointID.neck,"neck"           },
                 {Omek.JointID.rightFingerTip, "rHand"},
                 {Omek.JointID.leftFingerTip, "lHand" },
                 {Omek.JointID.rightShoulder,"rShldr" },
                 {Omek.JointID.leftShoulder, "lShldr"  },
                 {Omek.JointID.rightElbow, "rForeArm"    },
                 {Omek.JointID.leftElbow, "lForeArm"     },
                 {Omek.JointID.rightCollar, "rCollar"   },
                 {Omek.JointID.leftCollar, "lCollar"    },
                 {Omek.JointID.hips, "hip"          },
                 {Omek.JointID.rightKnee, "rShin"     },
                 {Omek.JointID.leftKnee, "lShin"      },
                 {Omek.JointID.rightFoot, "rFoot"     },
                 {Omek.JointID.leftFoot, "lFoot"      },
                 {Omek.JointID.rightHip, "rThigh"      },
                 {Omek.JointID.leftHip, "lThigh"       },
                 {Omek.JointID.spine1, "chest"        },
                 {Omek.JointID.waist, "abdomen"        }
            },
            new Dictionary<Omek.JointID,string>()
            {                
                 {Omek.JointID.head, "Head"          },
                 {Omek.JointID.neck,"Neck"           },
                 {Omek.JointID.rightFingerTip, "RHand"},
                 {Omek.JointID.leftFingerTip, "LHand" },
                 {Omek.JointID.rightShoulder,"RShoulder" },
                 {Omek.JointID.leftShoulder, "LShoulder"  },
                 {Omek.JointID.rightElbow, "RForearm"    },
                 {Omek.JointID.leftElbow, "LForearm"     },
                 {Omek.JointID.rightCollar, "RClavicle"   },
                 {Omek.JointID.leftCollar, "LClavicle"    },
                 {Omek.JointID.hips, "Hip"          },
                 {Omek.JointID.rightKnee, "RShin"     },
                 {Omek.JointID.leftKnee, "LShin"      },
                 {Omek.JointID.rightFoot, "RFoot"     },
                 {Omek.JointID.leftFoot, "LFoot"      },
                 {Omek.JointID.rightHip, "RThigh"      },
                 {Omek.JointID.leftHip, "LThigh"       },
                 {Omek.JointID.spine1, "MiddleSpine"        },
                 {Omek.JointID.spine2, "Chest"        },
                 {Omek.JointID.waist, "LowerSpine"        }
            },
            new Dictionary<Omek.JointID,string>()
            {                
                 {Omek.JointID.head, "Head"          },
                 {Omek.JointID.neck,"Neck"           },
                 {Omek.JointID.rightFingerTip, "R_Hand"},
                 {Omek.JointID.leftFingerTip, "L_Hand" },
                 {Omek.JointID.rightShoulder,"R_UpperArm" },
                 {Omek.JointID.leftShoulder, "L_UpperArm"  },
                 {Omek.JointID.rightElbow, "R_Forearm"    },
                 {Omek.JointID.leftElbow, "L_Forearm"     },
                 {Omek.JointID.rightCollar, "R_Clavicle"   },
                 {Omek.JointID.leftCollar, "L_Clavicle"    },
                 {Omek.JointID.hips, "Hips"          },
                 {Omek.JointID.rightKnee, "R_Calf"     },
                 {Omek.JointID.leftKnee, "L_Calf"      },
                 {Omek.JointID.rightFoot, "R_Foot"     },
                 {Omek.JointID.leftFoot, "L_Foot"      },
                 {Omek.JointID.rightHip, "R_Thigh"      },
                 {Omek.JointID.leftHip, "L_Thigh"       },
                 {Omek.JointID.spine1, "Spine1"        },
                 {Omek.JointID.spine2, "Spine2"        },
                 {Omek.JointID.spine3, "Spine3"        },
                 {Omek.JointID.spine4, "Spine4"        },
                 {Omek.JointID.waist, "Spine"        }
            }

        };
    private List<Dictionary<string, string>> m_knowenRollJointsConfigurations =
        new List<Dictionary<string, string>>() 
        {
            new Dictionary<string,string>()
            {
                {"LeftArmRoll", "LeftArmRoll"},
                {"RightArmRoll", "RightArmRoll"},
                {"LeftForeArmRoll", "LeftForeArmRoll"},
                {"RightForeArmRoll", "RightForeArmRoll"},
                {"LeftUpLegRoll", "LeftUpLegRoll"},
                {"RightUpLegRoll", "RightUpLegRoll"},
                {"LeftLegRoll", "LeftLegRoll"},
                {"RightLegRoll", "RightLegRoll"}
            },
            new Dictionary<string,string>()
            {
                {"LeftArmRoll", "LUpArmTwist"},
                {"RightArmRoll", "RUpArmTwist"},
                {"LeftForeArmRoll", "R ForeTwist"},
                {"RightForeArmRoll", "L ForeTwist"},
                {"LeftUpLegRoll", "LThighTwist"},
                {"RightUpLegRoll", "RThighTwist"},
                {"LeftLegRoll", "LCalfTwist"},
                {"RightLegRoll", "RCalfTwist"}
            },
            new Dictionary<string,string>(),
            new Dictionary<string,string>(),
            new Dictionary<string,string>(),
            new Dictionary<string,string>(),
            new Dictionary<string,string>()
            {
                {"LeftArmRoll", "LUpArmTwist"},
                {"RightArmRoll", "RUpArmTwist"},
                {"LeftForeArmRoll", "R_ForeTwist"},
                {"RightForeArmRoll", "L_ForeTwist"},
                {"LeftUpLegRoll", "LThighTwist"},
                {"RightUpLegRoll", "RThighTwist"},
                {"LeftLegRoll", "LCalfTwist"},
                {"RightLegRoll", "RCalfTwist"}
            }
        };
    
    private Transform SearchHierarchyForBone(Transform current, string name)
    {
        // check if the current bone is the bone we're looking for, if so return it
        if (current.name.Contains(name))
            return current;

        // search through child bones for the bone we're looking for
        foreach (Transform child in current)
        {
            // the recursive step; repeat the search one step deeper in the hierarchy
            Transform found = SearchHierarchyForBone(child, name);

            // a transform was returned by the search above that is not null,
            // it must be the bone we're looking for
            if (found != null)
                return found;
        }

        // bone with name was not found
        return null;
    }

    private Transform SearchSkinedMeshRendererForBone(SkinnedMeshRenderer skinnedMeshRenderer, string name)
    {
        return Array.Find(skinnedMeshRenderer.bones, (bone) => { return bone.name.Contains(name); });
    }

    private Transform SearchRendererOrHeirarchy(Transform root, string name)
    {
        SkinnedMeshRenderer[] renderers = root.GetComponentsInChildren<SkinnedMeshRenderer>();
        if (renderers != null && renderers.Length > 0)
        {
            foreach (SkinnedMeshRenderer smr in root.GetComponentsInChildren<SkinnedMeshRenderer>())
            {
                Transform result = SearchSkinedMeshRendererForBone(smr, name);
                if (result != null)
                {
                    return result;
                }
            }
        }
        else
        {
            return SearchHierarchyForBone(root, name);
        }
        return null;

    }
    private int CountBones(Transform root, IEnumerable<string> boneNames)
    {
        int count = 0;
        foreach (string boneName in boneNames)
        {
            if (SearchRendererOrHeirarchy(root, boneName) != null)
            {
                count++;
            }
        }
        return count;
    }

    private int FindBestConfiguration()
    {
        int bestIndex = -1;
        int maxCount = 0;
        Transform rootTransform = (target as SkeletonTrackingAnimation).transform;
        for (int i = 0; i < m_knowenConfigurations.Count; i++)
        {
            int currentCount = CountBones(rootTransform, m_knowenConfigurations[i].Values);
            currentCount += CountBones(rootTransform, m_knowenRollJointsConfigurations[i].Values);
            if (currentCount > maxCount)
            {
                bestIndex = i;
                maxCount = currentCount;
            }
        }

        return bestIndex;
    }

    private void ApplyConfiguration(int index)
    {
        SkeletonTrackingAnimation sta = target as SkeletonTrackingAnimation;
        foreach (SkeletonTrackingAnimation.TransformWrapper tw in sta.m_hierarchyList)
        {
            tw.m_transform = SearchRendererOrHeirarchy(sta.transform, m_knowenConfigurations[index][(Omek.JointID)tw.m_jointID]);
        }

        foreach (SkeletonTrackingAnimation.RollJointWrapper rjw in sta.m_rollJointsList)
        {
            rjw.m_transform = SearchRendererOrHeirarchy(sta.transform, m_knowenRollJointsConfigurations[index][rjw.m_name]);
        }

        FixHips(sta);
    }

    private void FixHips(SkeletonTrackingAnimation sta)
    {

        SkeletonTrackingAnimation.TransformWrapper hipsWrapper = sta.m_hierarchyList.Find((a) => { return a.m_jointID == Omek.JointID.hips; });
        if (hipsWrapper != null && hipsWrapper.m_transform == null)
        {
            SkeletonTrackingAnimation.TransformWrapper headWrapper = sta.m_hierarchyList.Find((a) => { return a.m_jointID == Omek.JointID.head; });
            if (headWrapper != null && headWrapper.m_transform != null)
            {
                string hipsName;
                if (headWrapper.m_transform.name.Contains(" "))
                {
                    hipsName = headWrapper.m_transform.name.Split(' ')[0];
                }
                else
                {
                    hipsName = headWrapper.m_transform.name.Split('_')[0];
                }
                if (!string.IsNullOrEmpty(hipsName))
                {
                    hipsWrapper.m_transform = SearchRendererOrHeirarchy(sta.transform, hipsName);
                }
            }
        }
    }

    private void AutoConfigureTransforms()
    {
        int bestConfig = FindBestConfiguration();
        if (bestConfig > -1)
        {
            ApplyConfiguration(bestConfig);
        }
    }
}