using UnityEngine;
using System.Collections;
using OmekFramework.Beckon.Main;

public class RestPositionOnEnter : MonoBehaviour {

	
	// Update is called once per frame
	void Update () {
        JointPositionTransformer jpt = GetComponent<JointPositionTransformer>();
        if (jpt != null)
        {
            if (BeckonManager.BeckonInstance.Alerts.IsAlertActive("playerEnters"))
            {
                jpt.RecenterOnWorldPosition(false, true);
                jpt.WorldBox.CenterOffset.x = 0;
            }
        }
	}
}
