using UnityEngine;
using System.Collections;
using OmekFramework.Beckon;
using OmekFramework.Beckon.Main;
using OmekFramework.Common.BasicTypes;
using OmekFramework.Beckon.Data;
using OmekFramework.Common.GeneralUtils;
using System.Collections.Generic;
using System.Runtime.InteropServices;



[AddComponentMenu("Omek/Management/Unity Beckon Manager")]
public class UnityBeckonManager : MonoBehaviour 
{   
    // The maximum number of person allowed in the system
    public int MaxPersons = 3;

    // Should a sequence be used (otherwise a depth camera will be used)
    public bool UseSequence;

    // the sequence path (folder name, without the actual filename)
    public string SequencePath;

    // configuration parameters for the 3D camera
    public BeckonSessionConfiguration.CameraParam[] CameraParams;

    // Should processing be started in a separate thread, this may improve performance on a multi core machine.
    // When true, pausing the game, does not pause the background processing
    public bool UseSeparateThread;

    // The maximum length of a sequence recorded.
    // Sequence recording may be toggled by pressing F12.
    public int RecordingLength = 1000;
    
    //list of gesture Beckon SDK will try to enable and recognize
    public List<string> GestureList;

    // list of alerts Beckon SDK will report.
    public List<Omek.AlertEventType> AlertList;
    
    //"should Beckon debug messages should be displayed, (otherwise only warning and errors would be displayed)"
    public bool LogSDKDebug = false;

    private bool m_isInitilzed = false;

    public bool IsInitilzed
    {
        get { return m_isInitilzed; }
        set { m_isInitilzed = value; }
    }

    public bool HasNewImage
    {
        get { return m_hasNewImage; }
    }

    private bool m_sequenceEnded = false;
    private BeckonManager m_beckonManager;
    private BeckonSessionConfiguration m_currentConf;

	/// <summary>
	/// Indicates if there is a new image in the current frame.
	/// </summary>
	private bool m_hasNewImage = false;

    // used to keep the state of the inspector
    [HideInInspector]
    public List<bool> m_foldoutState;

    // used to let unity find native DLLs needed by Beckon SDK
    [DllImport("kernel32")]
    private static extern bool SetDllDirectory(string dir);


    /// <summary>
    /// Initialize Omek's Framework
    /// </summary>
    void Awake()
    {        
        Object[] ubms = GameObject.FindObjectsOfType(typeof(UnityBeckonManager));        
        if (ubms.Length > 1)
        {
            Debug.LogError("There is more then one UnityBeckonManagers in the scene");
            return;
        }
        SetFrameworkLogger();

        m_currentConf = new BeckonSessionConfiguration();
        m_currentConf.TrackingMode = Omek.TrackingMode.FullBody;
        m_currentConf.MaxPersons = MaxPersons;
        m_currentConf.MaxPersonsWithAutomaticSkeletons = 0;
        m_currentConf.UseSDKPlayerSelection = false;
        m_currentConf.RecordingFrameLength = RecordingLength;
        m_currentConf.UseRunSensor = UseSeparateThread;
        m_currentConf.UseSequence = UseSequence;
        m_currentConf.SequencePath = System.IO.Path.GetFullPath(SequencePath);
        m_currentConf.CameraParams = CameraParams;
        m_currentConf.GestureList = GestureList.ToArray();
            if (!AlertList.Contains(Omek.AlertEventType.Alert_CandidateEnters))
                AlertList.Add(Omek.AlertEventType.Alert_CandidateEnters);
            if (!AlertList.Contains(Omek.AlertEventType.Alert_CandidateLeaves))
                AlertList.Add(Omek.AlertEventType.Alert_CandidateLeaves);
            if (!AlertList.Contains(Omek.AlertEventType.Alert_PlayerEnters))
                AlertList.Add(Omek.AlertEventType.Alert_PlayerEnters);
            if (!AlertList.Contains(Omek.AlertEventType.Alert_PlayerLeaves))
                AlertList.Add(Omek.AlertEventType.Alert_PlayerLeaves);
        m_currentConf.AlertList = AlertList.ToArray();

        SetDllDirectory(Application.dataPath + @"\MotionToolkit\Plugins\");
        m_isInitilzed = CreateSensor();




    }

    private bool CreateSensor()
    {		
        //Debug.Log("m_currentConf " + m_currentConf.SequencePath);
        ReturnCode rc = BeckonManager.BeckonInstance.CreateSensor(m_currentConf);
        if (rc.IsError())
        {
            Debug.LogError("CreateSensor failed: " + rc);
            return false;
        }

        if (m_currentConf.UseRunSensor)
        {
            rc = new ReturnCode(BeckonManager.BeckonInstance.Analyzer.runTrackingThread());
            if (rc.IsError())
            {
                Debug.LogError(rc.ToString());
                return false;
            }
        }
        return true;
    }

    // Update is called once per frame - call Framework's BeckonManager update method
	void Update () {
        if (m_isInitilzed)
        {
            HandleInput();
			if (!m_sequenceEnded)
			{
				ReturnCode rc = BeckonManager.BeckonInstance.UpdateStates(out m_hasNewImage);
				if (rc.IsError())
				{                    
					if (rc.SDKReturnCode == Omek.OMKStatus.OMK_ERROR_SEQUENCE_EOF_REACHED || rc.SDKReturnCode == Omek.OMKStatus.OMK_ERROR_ASSERTION_FAILURE)
					{
                        m_sequenceEnded = true;
                        m_hasNewImage = false;
						Debug.LogWarning(new ReturnCode(Omek.OMKStatus.OMK_ERROR_SEQUENCE_EOF_REACHED).ToString());
					}
					else
					{
						Debug.LogError(rc.ToString());
						if (rc.FrameworkReturnCode == ReturnCode.FrameworkReturnCodes.Sensor_Not_Initialized)
						{
							m_isInitilzed = false;
						}
						return;
					}
				}
			}
        }
	}

    void OnDestroy()
    {
        BeckonManager.BeckonInstance.DestroySensor();
    }

    /// <summary>
    /// handle input.
    /// f10 restart playing a sequence
    /// F12 start/stop sequence recording
    /// </summary>
    private void HandleInput()
    {
        if (Input.GetKeyDown(KeyCode.F10))
        {
            if (m_currentConf.UseSequence)
            {
                RestartSequence();
            }
        }
        if (Input.GetKeyDown(KeyCode.F12))
        {
            if (!m_currentConf.UseSequence)
            {
                ToggleRecording();
            }
        }
    }

    // restart playing of a sequence
    public void RestartSequence()
    {
        BeckonManager.BeckonInstance.Sensor.restartSequence();
        BeckonManager.BeckonInstance.ResetState();
        m_sequenceEnded = false;
    }

    // start/stop recording a sequence
    public void ToggleRecording()
    {
        BeckonManager.BeckonInstance.ToggleRecording();
    }

    // register a logger handler to output unity log's to the framework log
    private void SetFrameworkLogger()
    {
        Logger.LogDebug(this, "Init the logger");
        if (CheckForUnityAppender() == true)
        {
            Debug.Log("Won't enable logging of Unity log to Omek framework log as it will loop back to Unity log");
        }
        else
        {
            Application.RegisterLogCallback(HandleFrameworkLog);
        }
    }
    
    // write unity logs to framework log
    void HandleFrameworkLog(string logString, string stackTrace, LogType type)
    {
        switch(type)
        {
            case LogType.Log:
                Logger.LogDebug(this,logString);
                break;
            case LogType.Warning:
                Logger.LogWarn(this,logString);
                break;
            case LogType.Error:
                Logger.LogError(this, logString);
                break;
            case LogType.Assert:
            case LogType.Exception:
                Logger.LogFatal(this, logString);
                break;
        }        
    }

    // check if the logging system uses UnityLog4NetAppender
    private bool CheckForUnityAppender()
    {
        log4net.Repository.Hierarchy.Hierarchy hierarchy = log4net.LogManager.GetRepository() as log4net.Repository.Hierarchy.Hierarchy;
        if (hierarchy.Root.Appenders.Count > 0)
        {
            foreach (log4net.Appender.IAppender appender in hierarchy.Root.Appenders)
            {
                if (appender is UnityLog4NetAppender)
                {
                    if (!LogSDKDebug)
                    {
                        (appender as UnityLog4NetAppender).Threshold = log4net.Core.Level.Info;
                    }
                    return true;

                }

            }
            return false;
        }
        else
        {
            // if we got here the is no configuration for the logger
            hierarchy.Root.RemoveAllAppenders(); /*Remove any other appenders*/

            UnityLog4NetAppender unityAppender = new UnityLog4NetAppender();
            log4net.Layout.PatternLayout pl = new log4net.Layout.PatternLayout();
            pl.ConversionPattern = "%m";
            pl.ActivateOptions();
            unityAppender.Layout = pl;
            if (!LogSDKDebug)
            {
                (unityAppender as UnityLog4NetAppender).Threshold = log4net.Core.Level.Info;
            }
            unityAppender.ActivateOptions();

            log4net.Config.BasicConfigurator.Configure(unityAppender); 
            return true;
        }
    }

    public bool ReinitSensor()
    {
        if (m_isInitilzed == false)
        {
            ReturnCode rc = BeckonManager.BeckonInstance.DestroySensor();
            if (rc.IsError())
            {
                Debug.LogError(rc.ToString());
            }
            m_isInitilzed = CreateSensor();
        }
        return m_isInitilzed;
    }
}
