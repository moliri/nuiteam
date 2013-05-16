using System.Collections;
using System.Collections.Generic;
using System.Text;
using OmekFramework.Beckon.Main;
using UnityEngine;
using OmekFramework.Common.BasicTypes;

[AddComponentMenu("Omek/Management/Unity Player Management")]
public class UnityPlayerManagement : MonoBehaviour
{
	static UnityPlayerManagement g_instance;

	/// <summary>
	/// Singleton Instance for easy access
	/// </summary>
	public static UnityPlayerManagement Instance
	{
		get
		{
			return g_instance;
		}
	}

	#region Static Members

	/// <summary>
	/// Describe Predefined player selection strategy
	/// </summary>
	public enum PlayerSelectionStrategies
	{
		None,
		ReconsiderClosestToCamera,
		PreDeterminedClosestToCamera,
		PointerControlersFirst,
		GestureEnabled,
        NoChange,
	}

	/// <summary>
	/// Describe Predefined pointer selection selection strategy
	/// </summary>
	public enum PointerSelectionStrategies
	{
		None,
		Normal,
		SameAsPlayers,
		GestureEnabled,
	}

	/// <summary>
	/// Hold instances of players selection strategy
	/// </summary>
	public static Dictionary<PlayerSelectionStrategies, OmekFramework.Beckon.Main.PlayerSelectionStrategy.StrategyWithExpectedPlayers> g_playerSelectionStrategiesInstances;

	/// <summary>
	/// Hold instances of pointer selection strategy
	/// </summary>
	public static Dictionary<PointerSelectionStrategies, OmekFramework.Beckon.Main.PlayerSelectionStrategy.StrategyWithExpectedPlayers> g_pointerSelectionStrategiesInstances;

	/// <summary>
	/// Static Constructor initialize static members
	/// </summary>
	static UnityPlayerManagement()
	{
		g_playerSelectionStrategiesInstances = new Dictionary<PlayerSelectionStrategies, OmekFramework.Beckon.Main.PlayerSelectionStrategy.StrategyWithExpectedPlayers>();
		g_playerSelectionStrategiesInstances.Add(PlayerSelectionStrategies.None, null);
		g_playerSelectionStrategiesInstances.Add(PlayerSelectionStrategies.PointerControlersFirst, new OmekFramework.Beckon.Main.PlayerSelectionStrategy.PointerControllersFirst());
		g_playerSelectionStrategiesInstances.Add(PlayerSelectionStrategies.ReconsiderClosestToCamera, new OmekFramework.Beckon.Main.PlayerSelectionStrategy.ReconsiderClosestToCamera());
		g_playerSelectionStrategiesInstances.Add(PlayerSelectionStrategies.PreDeterminedClosestToCamera, new OmekFramework.Beckon.Main.PlayerSelectionStrategy.PreDeterminedClosestToCamera());
		g_playerSelectionStrategiesInstances.Add(PlayerSelectionStrategies.GestureEnabled, new OmekFramework.Beckon.Main.PlayerSelectionStrategy.GestureEnabled());
        g_playerSelectionStrategiesInstances.Add(PlayerSelectionStrategies.NoChange, new OmekFramework.Beckon.Main.PlayerSelectionStrategy.NoChange());

		g_pointerSelectionStrategiesInstances = new Dictionary<PointerSelectionStrategies, OmekFramework.Beckon.Main.PlayerSelectionStrategy.StrategyWithExpectedPlayers>();
		g_pointerSelectionStrategiesInstances.Add(PointerSelectionStrategies.None, null);
		g_pointerSelectionStrategiesInstances.Add(PointerSelectionStrategies.Normal, new OmekFramework.Beckon.Main.PlayerSelectionStrategy.StandardPointerSelectionStrategy());
		g_pointerSelectionStrategiesInstances.Add(PointerSelectionStrategies.SameAsPlayers, new OmekFramework.Beckon.Main.PlayerSelectionStrategy.PointerControlledByPlayers());
		g_pointerSelectionStrategiesInstances.Add(PointerSelectionStrategies.GestureEnabled, g_playerSelectionStrategiesInstances[PlayerSelectionStrategies.GestureEnabled]);
	}

	#endregion Static Members

	#region Types

	/// <summary>
	/// Describe a trapezoid
	/// </summary>
	[System.Serializable]
	public class TrapezoidDefinition
	{
		/// <summary>
		/// Width of the edge near the camera
		/// </summary>
		public float nearWidth;

		/// <summary>
		/// Width of the edge far the camera
		/// </summary>
		public float farWidth;

		/// <summary>
		/// Distance of the near base from the camera
		/// </summary>
		public float nearPlane;

		/// <summary>
		/// distance of the far edge from the camera
		/// </summary>
		public float farPlane;

		/// <summary>
		/// constructor
		/// </summary>
		public TrapezoidDefinition() { }

		public TrapezoidDefinition(float nearWidth, float farWidth, float nearPlane, float farPlane)
		{
			this.nearWidth = nearWidth;
			this.farWidth = farWidth;
			this.nearPlane = nearPlane;
			this.farPlane = farPlane;
		}
	}

	/// <summary>
	/// hold 2 trapezoids.
	/// </summary>
	[System.Serializable,Tooltip("Describe two trapezoids")]
	public class TrapezoidsPair
	{
        [Tooltip("The inner trapezoid")]
		public TrapezoidDefinition innerTrapezoid;
        [Tooltip("The outer trapezoid")]
		public TrapezoidDefinition outerTrapezoid;

		public TrapezoidsPair()
		{
			this.innerTrapezoid = new TrapezoidDefinition();
			this.outerTrapezoid = new TrapezoidDefinition();
		}

		public TrapezoidsPair(TrapezoidDefinition innerTrapezoid, TrapezoidDefinition outerTrapezoid)
		{
			this.innerTrapezoid = innerTrapezoid;
			this.outerTrapezoid = outerTrapezoid;
		}
	}

	#endregion Types

	#region Serialized Members

	/// <summary>
	/// hold sets of trapezoids used for player selection
	/// </summary>
	[SerializeField]
	private List<TrapezoidsPair> m_trapezoidConfigurations = new List<TrapezoidsPair>(new TrapezoidsPair[]{
        new TrapezoidsPair(new TrapezoidDefinition(80,160,150,280), new TrapezoidDefinition(120,310,120,310)),
        new TrapezoidsPair(new TrapezoidDefinition(80,160,150,280), new TrapezoidDefinition(120,310,120,310)) });

	/// <summary>
	/// which set of trapezoids is currently in use
	/// </summary>
	[SerializeField]
	private int m_currentTrapezoidConfigurationIndex;

	/// <summary>
	/// save the selected player selection strategy
	/// </summary>
	[SerializeField]
	private PlayerSelectionStrategies m_lastPlayerSelectionStrategy = PlayerSelectionStrategies.ReconsiderClosestToCamera;

	/// <summary>
	/// save the selected pointer selection strategy
	/// </summary>
	[SerializeField]
	private PointerSelectionStrategies m_lastPointerSelectionStrategy = PointerSelectionStrategies.Normal;

	/// <summary>
	/// save the player enable gesture when using GestureEnabled strategy
	/// </summary>
	[SerializeField]
	private string m_selectPlayerGesture = "rightClick";

	/// <summary>
	/// save the player disable gesture when using GestureEnabled strategy
	/// </summary>
	[SerializeField]
	private string m_unselectPlayerGesture = "leftClick";

	/// <summary>
	/// save the indexing scheme used to choose color for each person
	/// </summary>
	[SerializeField]
	private PersonProperties.IndexingScheme m_indexingScheme = PersonProperties.IndexingScheme.UseConsistentIDs;

	/// <summary>
	/// number of wanted players
	/// </summary>
	[SerializeField]
	private int m_expectedPlayersCount = 1;

	/// <summary>
	/// number of wanted pointer controlling persons
	/// </summary>
	[SerializeField]
	private int m_expectedPointerControllersCount = 1;

	/// <summary>
	/// how much time after we have no information about a player we remove it from being an active player
	/// </summary>
	[SerializeField]
	private float m_removeNoPositionDuration = 2;

	/// <summary>
	/// how much time after a player moves outside the outer trapezoid we remove it from being an active player
	/// </summary>
	[SerializeField]
	private float m_removeBadPositionDuration = 5;

	#endregion Serialized Members

	#region Public Properties

	/// <summary>
	/// Get/Sets the active player selection strategy
	/// </summary>
	public PlayerSelectionStrategies PlayerSelectionStrategy
	{
		get
		{
			return m_lastPlayerSelectionStrategy;
		}
		set
		{
			if (value != m_lastPlayerSelectionStrategy)
			{
				if (g_playerSelectionStrategiesInstances[value] != null)
				{
					g_playerSelectionStrategiesInstances[value].ExpectedPlayersCount = m_expectedPlayersCount;
				}
				BeckonManager.BeckonInstance.PlayerSelection.SetPlayerSelectionStrategy(g_playerSelectionStrategiesInstances[value]);
				m_lastPlayerSelectionStrategy = value;
			}
		}
	}

	/// <summary>
	/// Get/Sets the active pointer controlling persons selection strategy
	/// </summary>
	public PointerSelectionStrategies PointerSelectionStrategy
	{
		get
		{
			return m_lastPointerSelectionStrategy;
		}
		set
		{
			if (value != m_lastPointerSelectionStrategy)
			{
				if (g_pointerSelectionStrategiesInstances[value] != null)
				{
					g_pointerSelectionStrategiesInstances[value].ExpectedPlayersCount = m_expectedPointerControllersCount;
				}
				BeckonManager.BeckonInstance.PlayerSelection.SetPointerSelectionStrategy(g_pointerSelectionStrategiesInstances[value]);
				m_lastPointerSelectionStrategy = value;
			}
		}
	}

	/// <summary>
	/// Set which gesture will enable a person as an active player, when using GestureEnabled strategy
	/// </summary>
	public string PlayerSelectGesture
	{
		get
		{
			return m_selectPlayerGesture;
		}
		set
		{
			m_selectPlayerGesture = value;
			(g_playerSelectionStrategiesInstances[PlayerSelectionStrategies.GestureEnabled] as OmekFramework.Beckon.Main.PlayerSelectionStrategy.GestureEnabled).SelectPlayerGesture = m_selectPlayerGesture;
		}
	}

	/// <summary>
	/// Set which gesture will disable a person as an active player, when using GestureEnabled strategy
	/// </summary>
	public string PlayerUnselectGesture
	{
		get
		{
			return m_unselectPlayerGesture;
		}
		set
		{
			m_unselectPlayerGesture = value;
			(g_playerSelectionStrategiesInstances[PlayerSelectionStrategies.GestureEnabled] as OmekFramework.Beckon.Main.PlayerSelectionStrategy.GestureEnabled).UnselectPlayerGesture = m_unselectPlayerGesture;
		}
	}

	/// <summary>
	/// The wanted number of players
	/// </summary>
	public int ExpectedPlayersCount
	{
		get
		{
			return m_expectedPlayersCount;
		}
		set
		{
			m_expectedPlayersCount = value;
			OmekFramework.Beckon.Main.PlayerSelectionStrategy.StrategyWithExpectedPlayers strategy;
			if (g_playerSelectionStrategiesInstances.TryGetValue(m_lastPlayerSelectionStrategy, out strategy) && strategy != null)
			{
				strategy.ExpectedPlayersCount = m_expectedPlayersCount;
			}
		}
	}

	/// <summary>
	/// the wanted number of person controlling a pointer
	/// </summary>
	public int ExpectedPointerControllersCount
	{
		get
		{
			return m_expectedPointerControllersCount;
		}
		set
		{
			m_expectedPointerControllersCount = value;
			OmekFramework.Beckon.Main.PlayerSelectionStrategy.StrategyWithExpectedPlayers strategy;
			if (g_pointerSelectionStrategiesInstances.TryGetValue(m_lastPointerSelectionStrategy, out strategy) && strategy != null)
			{
				strategy.ExpectedPlayersCount = m_expectedPointerControllersCount;
			}
		}
	}

	/// <summary>
	/// hold sets of trapezoids used for player selection
	/// </summary>
	public List<TrapezoidsPair> TrapezoidConfigurations
	{
		get { return m_trapezoidConfigurations; }
	}

	/// <summary>
	/// which set of trapezoids is currently in use
	/// </summary>
	public int CurrentTrapezoidConfigurationIndex
	{
		get { return m_currentTrapezoidConfigurationIndex; }
		set { m_currentTrapezoidConfigurationIndex = correctTrapezoidConfigurationIndex(value); }
	}

	/// <summary>
	/// the set of trapezoids that is currently in use
	/// </summary>
	public TrapezoidsPair CurrentTrapezoidsConfiguration
	{
		get { return m_trapezoidConfigurations[m_currentTrapezoidConfigurationIndex]; }
	}

	/// <summary>
	/// how much time after we have no information about a player we remove it from being an active player
	/// </summary>
	public float RemoveNoPositionDuration
	{
		get { return m_removeNoPositionDuration; }
		set
		{
			m_removeNoPositionDuration = value;
			BeckonManager.BeckonInstance.PlayerSelection.RemoveNoPositionDuration = value;
		}
	}

	/// <summary>
	/// how much time after a player moves outside the outer trapezoid we remove it from being an active player
	/// </summary>
	public float RemoveBadPositionDuration
	{
		get { return m_removeBadPositionDuration; }
		set
		{
			m_removeBadPositionDuration = value;
			BeckonManager.BeckonInstance.PlayerSelection.RemoveBadPositionDuration = value;
		}
	}

	/// <summary>
	/// The indexing scheme used to choose color for each person
	/// </summary>
	public static PersonProperties.IndexingScheme IndexingScheme
	{
		get
		{
			if (Instance != null)
			{
				return Instance.m_indexingScheme;
			}
			else
			{
				return PersonProperties.IndexingScheme.UseConsistentIDs;
			}
		}
		set
		{
			if (Instance != null)
			{
				Instance.m_indexingScheme = value;
			}
		}
	}

	#endregion Public Properties

	#region Private Members

	private TrapezoidCondition m_innerTrapezoid;
	private TrapezoidCondition m_outerTrapezoid;
	private OccludedCondition m_occlusionCondition;

	private const string m_innerTrapezoidConditionName = "innerTrapezoid";
	private const string m_outerTrapezoidConditionName = "outerTrapezoid";
	private const string m_occlusionConditionName = "occlusionCondition";

	#endregion Private Members

	#region Methods

	private int correctTrapezoidConfigurationIndex(int index)
	{
		return Mathf.Clamp(index, 0, m_trapezoidConfigurations.Count - 1);
	}

	// Use this for initialization
	private void Awake()
	{
		g_instance = this;

		PlayerSelectGesture = m_selectPlayerGesture;
		PlayerUnselectGesture = m_unselectPlayerGesture;
		if (g_playerSelectionStrategiesInstances[m_lastPlayerSelectionStrategy] != null)
		{
			g_playerSelectionStrategiesInstances[m_lastPlayerSelectionStrategy].ExpectedPlayersCount = m_expectedPlayersCount;
		}
		if (g_pointerSelectionStrategiesInstances[m_lastPointerSelectionStrategy] != null)
		{
			g_pointerSelectionStrategiesInstances[m_lastPointerSelectionStrategy].ExpectedPlayersCount = m_expectedPointerControllersCount;
		}
		BeckonManager.BeckonInstance.PlayerSelection.SetPointerSelectionStrategy(g_pointerSelectionStrategiesInstances[m_lastPointerSelectionStrategy]);
		BeckonManager.BeckonInstance.PlayerSelection.SetPlayerSelectionStrategy(g_playerSelectionStrategiesInstances[m_lastPlayerSelectionStrategy]);
		BeckonManager.BeckonInstance.PlayerSelection.RemoveBadPositionDuration = m_removeBadPositionDuration;
		BeckonManager.BeckonInstance.PlayerSelection.RemoveNoPositionDuration = m_removeNoPositionDuration;		
		m_innerTrapezoid = new WarningTrapezoidCondition(CurrentTrapezoidsConfiguration.innerTrapezoid.nearWidth,
												CurrentTrapezoidsConfiguration.innerTrapezoid.farWidth,
												CurrentTrapezoidsConfiguration.innerTrapezoid.nearPlane,
												CurrentTrapezoidsConfiguration.innerTrapezoid.farPlane);
		m_outerTrapezoid = new BadTrapezoidCondition(CurrentTrapezoidsConfiguration.outerTrapezoid.nearWidth,
												CurrentTrapezoidsConfiguration.outerTrapezoid.farWidth,
												CurrentTrapezoidsConfiguration.outerTrapezoid.nearPlane,
												CurrentTrapezoidsConfiguration.outerTrapezoid.farPlane);
		m_occlusionCondition = new OccludedCondition();
		BeckonManager.BeckonInstance.PersonMonitor.SetPositionCondition(m_innerTrapezoidConditionName, m_innerTrapezoid);
        BeckonManager.BeckonInstance.PersonMonitor.SetPositionCondition(m_outerTrapezoidConditionName, m_outerTrapezoid);
        BeckonManager.BeckonInstance.PersonMonitor.SetPositionCondition(m_occlusionConditionName, m_occlusionCondition);
	}

	/// <summary>
	/// transfer the trapezoid configuration to the framework
	/// </summary>
	public void ApplyCurrentTrapezoidsConfiguration()
	{
		if (m_innerTrapezoid == null || m_outerTrapezoid == null)
			return;

		TrapezoidCondition.TrapezoidDimensions innerTrapezoidDim = m_innerTrapezoid.TrapezoidDimensionsRef;
		TrapezoidCondition.TrapezoidDimensions outerTrapezoidDim = m_outerTrapezoid.TrapezoidDimensionsRef;
		TrapezoidDefinition innerTrapezoidDef = CurrentTrapezoidsConfiguration.innerTrapezoid;
		TrapezoidDefinition outerTrapezoidDef = CurrentTrapezoidsConfiguration.outerTrapezoid;
		innerTrapezoidDim.NearWidth = innerTrapezoidDef.nearWidth;
		innerTrapezoidDim.FarWidth = innerTrapezoidDef.farWidth;
		innerTrapezoidDim.NearPlane = innerTrapezoidDef.nearPlane;
		innerTrapezoidDim.FarPlane = innerTrapezoidDef.farPlane;
		outerTrapezoidDim.NearWidth = outerTrapezoidDef.nearWidth;
		outerTrapezoidDim.FarWidth = outerTrapezoidDef.farWidth;
		outerTrapezoidDim.NearPlane = outerTrapezoidDef.nearPlane;
		outerTrapezoidDim.FarPlane = outerTrapezoidDef.farPlane;
	}

	/// <summary>
	/// get the currently used trapezoid configuration to the framework
	/// </summary>
	/// <returns></returns>
	public static TrapezoidsPair GetAppliedTrapezoidsConfiguration()
	{
		TrapezoidCondition innerTrapezoid = (TrapezoidCondition)BeckonManager.BeckonInstance.PersonMonitor.GetPositionCondition(m_innerTrapezoidConditionName);
        TrapezoidCondition outerTrapezoid = (TrapezoidCondition)BeckonManager.BeckonInstance.PersonMonitor.GetPositionCondition(m_outerTrapezoidConditionName);
		if (innerTrapezoid == null || outerTrapezoid == null)
			return null;

		TrapezoidsPair tp = new TrapezoidsPair();
		TrapezoidCondition.TrapezoidDimensions innerTrapezoidDim = innerTrapezoid.TrapezoidDimensionsRef;
		TrapezoidCondition.TrapezoidDimensions outerTrapezoidDim = outerTrapezoid.TrapezoidDimensionsRef;
		TrapezoidDefinition innerTrapezoidDef = tp.innerTrapezoid;
		TrapezoidDefinition outerTrapezoidDef = tp.outerTrapezoid;

		innerTrapezoidDef.nearWidth = innerTrapezoidDim.NearWidth;
		innerTrapezoidDef.farWidth = innerTrapezoidDim.FarWidth;
		innerTrapezoidDef.nearPlane = innerTrapezoidDim.NearPlane;
		innerTrapezoidDef.farPlane = innerTrapezoidDim.FarPlane;
		outerTrapezoidDef.nearWidth = outerTrapezoidDim.NearWidth;
		outerTrapezoidDef.farWidth = outerTrapezoidDim.FarWidth;
		outerTrapezoidDef.nearPlane = outerTrapezoidDim.NearPlane;
		outerTrapezoidDef.farPlane = outerTrapezoidDim.FarPlane;

		return tp;
	}

    /// <summary>
    /// sets the system join time for players to now. players that entered the FOV before the system join time will be ignored in some player selection strategies.
    /// </summary>
    public void SetSystemJoinTimeToNow()
    {
        ((OmekFramework.Beckon.Main.PlayerSelectionStrategy.PreDeterminedClosestToCamera)g_playerSelectionStrategiesInstances[PlayerSelectionStrategies.PreDeterminedClosestToCamera]).SetSystemJoinTimeToNow();
    }

	#endregion Methods
}