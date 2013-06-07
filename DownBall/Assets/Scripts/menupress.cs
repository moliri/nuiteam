using UnityEngine;
using System.Collections;

public class menupress : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		if (Input.GetKey("x")){
			
			Application.LoadLevel("Demo");	
		}
		if (Input.GetKey("i")){
			
			Application.LoadLevel("Instructions");	
		}
	}
}
