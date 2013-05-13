using UnityEngine;
using System.Collections;

public class Timer : MonoBehaviour {
	public int myFontSize;
	void Start(){
		myFontSize = 60;
	}
	
	void OnGUI () {
		GUI.skin.label.fontSize = myFontSize;
		GUI.Label (new Rect (0,0,200,200), Time.time.ToString().Substring(0,4));
	}
	
    void Update(){ 
		OnGUI();
    }
}

