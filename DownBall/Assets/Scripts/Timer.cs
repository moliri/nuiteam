using UnityEngine;
using System.Collections;

public class Timer : MonoBehaviour {
	public bool atCheckpoint;
	public int myFontSize;
	public float maxTime;
	void Start(){
		myFontSize = 60;
		maxTime = 60F;
	}
	
	void OnGUI () {
		GUI.skin.label.fontSize = myFontSize;
		if (Time.time > maxTime){
			GUI.Label (new Rect (0,0,Screen.width,200), "You didn't finish in time!");
		}
		else if (atCheckpoint) {
			Time.timeScale = 0;
			GUI.Label (new Rect (0,0,200,200), Time.time.ToString().Substring(0,4));
			rigidbody.AddForce(-Vector3.forward*50);
		}
		else if (Time.time <= maxTime) {
			GUI.Label (new Rect (0,0,200,200), Time.time.ToString().Substring(0,4));
		}
	}
	
    void Update(){ 
		OnGUI();
    }
		
	void OnCollisionEnter(Collision collisionInfo) {
		if(collisionInfo.gameObject.tag == "Checkpoint")  {
			atCheckpoint = true;
		}
	}
}

