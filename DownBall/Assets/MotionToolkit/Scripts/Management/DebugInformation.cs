using UnityEngine;
using System.Collections;
using OmekFramework.Beckon.Main;
using System.Collections.Generic;
using OmekFramework.Beckon.Data;
using OmekFramework.Common.BasicTypes;
using OmekFramework.Common.Main;

/// <summary>
/// Used To render some useful debug information
/// </summary>
[AddComponentMenu("Omek/Management/Debug Information")]
public class DebugInformation : MonoBehaviour {

	[HideInInspector]
	public Texture m_cursorTexture;

    [HideInInspector]
    public Texture m_recordingTexture;

    private LinkedList<string> m_lastGestures = new LinkedList<string>();
    private LinkedList<string> m_lastAlerts = new LinkedList<string>();

    void Update()
    {
        Dictionary<string, List<CommonDefines.EventNotification>> currentEvents;
        currentEvents = FrameworkManager.GenericInstance.Gestures.GetCurrentGesturesCopy();
        foreach (KeyValuePair<string, List<CommonDefines.EventNotification>> pair in currentEvents)
        {
            foreach (CommonDefines.EventNotification notification in pair.Value)
            {
                m_lastGestures.AddFirst(string.Format("{0}: {1}(PersonID = {2})", notification.TimeStamp, notification.EventName, notification.TrackedObjectID));
            }
        }
        while (m_lastGestures.Count > MAX_HISTORY)
        {
            m_lastGestures.RemoveLast();
        }

		currentEvents = FrameworkManager.GenericInstance.Alerts.GetCurrentAlertsCopy();
        foreach (KeyValuePair<string, List<CommonDefines.EventNotification>> pair in currentEvents)
        {
            foreach (CommonDefines.EventNotification notification in pair.Value)
            {
                m_lastAlerts.AddFirst(string.Format("{0}: {1}(PersonID = {2})", notification.TimeStamp, notification.EventName, notification.TrackedObjectID));
            }
        }
        while (m_lastAlerts.Count > MAX_HISTORY)
        {
            m_lastAlerts.RemoveLast();
        }

       

    }

    public int getDepthValueOfPixel(int x, int y)
    {
        OmekFramework.Common.BasicTypes.CommonDefines.ImageData imageData;
        OmekFramework.Common.BasicTypes.CommonDefines.ImageFormat format;
        BeckonData.Image.Depth.GetData(out imageData);
        BeckonData.Image.Depth.GetImageFormat(out format);
        int bppTimeChannels = format.m_channels * format.m_bpc;
        int imagePos = y * format.m_widthStep + x * bppTimeChannels;
        // the depth values are 2 bytes long each
        int depthVal = (imageData.m_dataArr[imagePos + 1] << 8) + imageData.m_dataArr[imagePos];
        return depthVal;
    }

    void OnGUI()
    {
        ShowPlayersInfo();
		//ShowPointerDebugInfo();
        ShowRecordingInfo();

        ShowAlertsAndGestures();
    }

    private void ShowAlertsAndGestures()
    {
        GUI.Box(new Rect(40,20, 400, 200), "Last Alerts");
        Rect rect = new Rect(50, 40, 380, 25);
        float labelHeight = 20;
        foreach (string str in m_lastAlerts)
        {
            GUI.Label(rect, str);
            rect.y += labelHeight;
        }
        

        GUI.Box(new Rect(40, 230, 400, 200), "Last Gestures");
        rect = new Rect(50, 250, 380, 25);
        labelHeight = 20;
        foreach (string str in m_lastGestures)
        {
            GUI.Label(rect, str);
            rect.y += labelHeight;
        }
    }

    //show recording sign if recording is on
    private void ShowRecordingInfo()
    {
		if (FrameworkManager.GenericInstance.IsInRecording)
        {
            GUI.color = Color.red;
            GUI.Label(new Rect(Screen.width - m_recordingTexture.width ,0, m_recordingTexture.width, m_recordingTexture.height), m_cursorTexture);
        }
    }

    // display person information
    private void ShowPlayersInfo()
    {
        IPlayerSelection ps = FrameworkManager.GenericInstance.GenericPlayerSelection;
        PersonMonitor bm = BeckonManager.BeckonInstance.PersonMonitor;
		
        GUI.Box(new Rect(40, Screen.height - 250, 900, 200), "Beckon Debug Info. Frame: " + BeckonManager.BeckonInstance.LastFrame);
        Rect rect = new Rect(50, Screen.height - 250, 900, 25);
        float labelHeight = 25;
        GUI.color = Color.red;
        GUI.Label(rect, "expectedPlayersCount = " + ps.ExpectedPlayerCount);
        rect.y += labelHeight;
        List<int> pointerControllingTrackedObjects = new List<int>(ps.PointerControllingTrackedObjects);
		if (pointerControllingTrackedObjects.Count == 0)
        {
            GUI.Label(rect, "Mouse controling persons: None");
        }
        else
        {
            GUI.Label(rect, "Mouse controling persons: " + string.Join(", ", pointerControllingTrackedObjects.ConvertAll<string>((i) => { return i.ToString(); }).ToArray()));
        }
        rect.y += labelHeight;
        GUI.Label(rect, "All Persons in System: " + string.Join(", ", pointerControllingTrackedObjects.ConvertAll<string>((i) => { return i.ToString(); }).ToArray()));
        rect.y += labelHeight;
        int index = 0;
        foreach (int personID in bm.TrackedObjectsInSystem)
        {
            OmekFramework.Common.BasicTypes.SpaceTypes.Vector3 center2D,center3D;
            BeckonData.Persons[(uint)personID].CenterOfMass3D.Get(out center3D);
            BeckonData.Persons[(uint)personID].CenterOfMass2D.Get(out center2D);
            GUI.Label(rect, string.Format("{0} : Person {1}, GameID {2}, State: {6} ,Position {3}, Image Position {4}, PosType {5}",
                index++,
                personID,
                ps.PlayerIdOfTrackedObjectId(personID),
                center3D.ToString("0.00"),
                center2D.ToString("0.0"),
                bm.GetPositionType((uint)personID),
                bm.GetInferredState((uint)personID)));
            rect.y += labelHeight;           
        }
    }

	private Color[] m_pointerColors = new Color[] { Color.red, Color.green, Color.blue, Color.yellow };
	private static readonly float CURSOR_SIZE = 64;
    private static readonly int MAX_HISTORY = 8;

    // display pointer information 
	private void ShowPointerDebugInfo()
	{
		UnityPointerManager pointerManager = GetComponent<UnityPointerManager>();
		if (pointerManager == null)
		{
			return;
		}

		UnityPointerManager.PointerData[] pointers = pointerManager.m_currentPointers;
		for (int i = 0; i < pointers.Length && i < m_pointerColors.Length; ++i)
		{
			UnityPointerManager.PointerData curPointer = pointers[i];
			if (curPointer.m_pointer == null)
			{
				continue;
			}
			
			GUI.color = m_pointerColors[i];
			float posX, posY;

			posX = curPointer.m_position.x * Screen.width - CURSOR_SIZE / 2;
			posY = curPointer.m_position.y * Screen.height - CURSOR_SIZE / 2;

			GUI.Label(new Rect(posX, posY, CURSOR_SIZE, CURSOR_SIZE), m_cursorTexture);
		}
	}
}
