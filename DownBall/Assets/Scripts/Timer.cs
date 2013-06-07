using UnityEngine;
using System.Collections;

public class Timer : MonoBehaviour {
	public bool atCheckpoint;
	public int myFontSize;
	public float maxTime;
	float loltime;
	void Start(){
		myFontSize = 60;
		maxTime = 99F;
		loltime = Time.timeScale;
	}
	Vector3 newpos;
	
	void OnGUI () {
		GUI.skin.label.fontSize = myFontSize;
		if (Time.timeSinceLevelLoad > maxTime){
			GUI.Label (new Rect (0,0,Screen.width,200), "You didn't finish in time!");
		}
		else if (atCheckpoint) {
			Time.timeScale = 0;
			GUI.Label (new Rect (0,0,200,200), Time.timeSinceLevelLoad.ToString().Substring(0,4));
			rigidbody.AddForce(-Vector3.forward*50);
			
			if (Input.GetKey("r")){//reset key once level ends to position based on level
				if (Application.loadedLevelName == "Demo")//reset for tutorial level
				{
					Time.timeScale = loltime;//starts time again
					Application.LoadLevel("Demo");//reloads level
				}
				if (Application.loadedLevelName == "Mouth")//reset for mouth
				{
					Time.timeScale = loltime;
					Application.LoadLevel("Mouth");
				}
				if (Application.loadedLevelName == "Stomach")//reset for stomach
				{
					Time.timeScale = loltime;
					Application.LoadLevel("Stomach");
				}
				if (Application.loadedLevelName == "Intestines")//reset for intestinces
				{
					Time.timeScale = loltime;
					Application.LoadLevel("Intestines");
				}
				if (Application.loadedLevelName == "Blood")//reset for blood
				{
					Time.timeScale = loltime;
					Application.LoadLevel("Blood");
				}
			}
			
			if (Input.GetKey("t")){//advance to next level
				if (Application.loadedLevelName == "Demo")//advance for tutorial level
				{
					Time.timeScale = loltime;//starts time again
					Application.LoadLevel("Mouth");//loads mouth
				}
				if (Application.loadedLevelName == "Mouth")//advance for mouth
				{
					Time.timeScale = loltime;
					Application.LoadLevel("Stomach");//loads stomach
				}
				if (Application.loadedLevelName == "Stomach")//advance for stomach
				{
					Time.timeScale = loltime;
					Application.LoadLevel("Intestines");//loads intestines
				}
				if (Application.loadedLevelName == "Intestines")//advance for intestinces
				{
					Time.timeScale = loltime;
					Application.LoadLevel("Blood");// loads blood
				}
				if (Application.loadedLevelName == "Blood")//advance for blood
				{
					Time.timeScale = loltime;
					//Application.LoadLevel("GameOver"); //if exists
				}
			}
			
		}
		else if (Time.time <= maxTime) {
			GUI.Label (new Rect (0,0,200,200), Time.timeSinceLevelLoad.ToString().Substring(0,4));
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

