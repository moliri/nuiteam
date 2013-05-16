using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using OmekFramework.Beckon.Main;
using OmekFramework.Common.BasicTypes;
using OmekFramework.Common.Main; 
#if UNITY_EDITOR
using UnityEditor;
#endif

[AddComponentMenu("Omek/Management/Escalation Manager")]
public class EscalationManager: MonoBehaviour
{
    static EscalationManager g_instance;
    public static EscalationManager Instance
    {
        get { return g_instance; }
    }

    [System.Serializable]
    public class EscalationCondition
    {
        public string name;
        public TrackedObjectState.PositionType positionType;
        public float time;
    }
    public List<EscalationCondition> escalationConditions;

    public string notExistState;
    public string defaultState;
    public bool resetEscalationOnlyOnGoodState = true;

    List<EscalationCondition>[] m_escalationsOfState;

    class CachedPersonState
    {
        public string state;
        public int frameChecked;
        public TrackedObjectState.PositionType positionType;
        public float changeTime;
    }
    List<CachedPersonState> m_personStates;

    public void SortEscalationConditionsByTime()
    {
        escalationConditions.Sort(delegate(EscalationCondition condition1, EscalationCondition condition2)
        {
            int typeDiff = condition1.time.CompareTo(condition2.time);
            if (typeDiff != 0)
            {
                return typeDiff;
            }
            else
            {
                return condition2.positionType.CompareTo(condition1.positionType);
            }
        });
    }

    public void SortEscalationConditionsByType()
    {
        escalationConditions.Sort(delegate(EscalationCondition condition1, EscalationCondition condition2)
        {
            int typeDiff = condition2.positionType.CompareTo(condition1.positionType);
            if (typeDiff != 0)
            {
                return typeDiff;
            }
            else
            {
                return condition1.time.CompareTo(condition2.time);
            }
        });
    }

    void prepareEscalationsOfStates()
    {
        m_escalationsOfState = new List<EscalationCondition>[System.Enum.GetValues(typeof(TrackedObjectState.PositionType)).Length];
        for (int enumIndex = 0; enumIndex < m_escalationsOfState.Length; ++enumIndex)
        {
            m_escalationsOfState[enumIndex] = new List<EscalationCondition>();
        }

        SortEscalationConditionsByType();
        foreach (EscalationCondition condition in escalationConditions)
        {
            m_escalationsOfState[(int)condition.positionType].Add(condition);
        }
    }

    void preparePersonStatesArray()
    {
        m_personStates = new List<CachedPersonState>(OmekFramework.Beckon.BasicTypes.BeckonDefines.MAX_SKELETONS);
    }

    void updatePersonStatesArray(int personID)
    {
        CachedPersonState newState;
        while (m_personStates.Count <= personID)
        {
            newState = new CachedPersonState();
            newState.state = defaultState;
            newState.frameChecked = 0;
            m_personStates.Add(newState);
        }
    }

    void Awake()
    {
        g_instance = this;
        prepareEscalationsOfStates();
        preparePersonStatesArray();
    }

    public string GetEscalationOfPerson(int personID)
    {
        updatePersonStatesArray(personID);
        if (m_personStates[personID].frameChecked == Time.frameCount)
        {
            return m_personStates[personID].state;
        }
        else
        {
            m_personStates[personID].frameChecked = Time.frameCount;

            TrackedObjectState.PositionType positionType = BeckonManager.BeckonInstance.PersonMonitor.GetPositionType((uint)personID);
            float changeTime = BeckonManager.BeckonInstance.PersonMonitor.GetPositionTypeChangeTime((uint)personID);
            if (m_personStates[personID].positionType != positionType)
            {
                // If required we don't allow to go from BAD to WARNING (only to good)
                if (resetEscalationOnlyOnGoodState == true && positionType != TrackedObjectState.PositionType.GOOD && positionType > m_personStates[personID].positionType)
                {
                    positionType = m_personStates[personID].positionType;
                    changeTime = m_personStates[personID].changeTime;
                }
                else
                {
                    m_personStates[personID].positionType = positionType;
                    m_personStates[personID].changeTime = changeTime;
                }
            }

            float timeFromChange = OmekFramework.Common.BasicTypes.Time.Time.realtimeSinceStartup - changeTime;
            List<EscalationCondition> escalationTimes = m_escalationsOfState[(int)positionType];

            //Debug.Log("positionType = " + positionType + " timeFromChange = " + timeFromChange);

            if (escalationTimes.Count == 0)
            {
                //m_personStates[personID].state = defaultState;
                //return defaultState;
                return m_personStates[personID].state;
            }
            int currentIndex = 0;
            while (escalationTimes.Count > currentIndex + 1 && escalationTimes[currentIndex + 1].time < timeFromChange)
            {
                currentIndex += 1;
            }
            if (escalationTimes[currentIndex].time <= timeFromChange)
            {
                m_personStates[personID].state = escalationTimes[currentIndex].name;
                return escalationTimes[currentIndex].name;
            }
            else
            {
                //m_personStates[personID].state = defaultState;
                //return defaultState;
                return m_personStates[personID].state;
            }
        }
    }
    public string GetEscalationOfPlayer(int playerID)
    {
        int personID = BeckonManager.BeckonInstance.PlayerSelection.TrackedObjectIdOfPlayerId(playerID);
        if (personID == PlayerSelection.NON_ASSIGNED_PLAYER)
        {
            return notExistState;
        }

        return GetEscalationOfPerson(personID);
    }

#if UNITY_EDITOR
    [MenuItem("CONTEXT/EscalationManager/Sort By Time", false, 1000)]
    public static void MenuSortByTime(MenuCommand command)
    {
        EscalationManager manager = (EscalationManager)command.context;
        manager.SortEscalationConditionsByTime();
    }

    [MenuItem("CONTEXT/EscalationManager/Sort By Type", false, 1001)]
    public static void MenuSortByType(MenuCommand command)
    {
        EscalationManager manager = (EscalationManager)command.context;
        manager.SortEscalationConditionsByType();
    }

#endif
}