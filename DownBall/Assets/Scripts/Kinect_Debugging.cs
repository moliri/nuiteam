using UnityEngine;
using System.Collections;

public class Kinect_Debugging : MonoBehaviour {
	
	public int myFontSize;
	
	public GameObject bodyPart1;
	public GameObject bodyPart2;
	public GameObject bodyPart3;
	public GameObject bodyPart4;
	
	public bool showBody1;
	public bool showBody2;
	public bool showBody3;
	public bool showBody4;
	
	private string body1;
	private string body2;
	private string body3;
	private string body4; 

	// Use this for initialization
	void Start () {
		body1 = bodyPart1.transform.position.ToString();
		body2 = bodyPart2.transform.position.ToString();
		body3 = bodyPart3.transform.position.ToString();
		body4 = bodyPart4.transform.position.ToString();
		myFontSize = 30;
	}
	
	void OnGUI () {
		GUI.skin.label.fontSize = myFontSize;
		if (showBody1){
			GUI.Label (new Rect (0,Screen.height - 200 ,Screen.width,100),
						body1+bodyPart1.gameObject.ToString());
		}		
		if (showBody2){
			GUI.Label (new Rect (0,Screen.height - 300 ,Screen.width,100),
						body2+bodyPart2.gameObject.ToString());
		}
		if (showBody3){
			GUI.Label (new Rect (0,Screen.height - 400 ,Screen.width,100),
						body3+bodyPart3.gameObject.ToString());
		}
		if (showBody4){
			GUI.Label (new Rect (0,Screen.height - 500 ,Screen.width,100),
						body4+bodyPart4.gameObject.ToString());
		}
	}
	
    void Update(){
		body1 = bodyPart1.transform.position.ToString();
		body2 = bodyPart2.transform.position.ToString();
		body3 = bodyPart3.transform.position.ToString();
		body4 = bodyPart4.transform.position.ToString();
		OnGUI();
    }
}
