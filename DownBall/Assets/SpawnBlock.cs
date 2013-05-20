using UnityEngine;
using System.Collections;

public class SpawnBlock : MonoBehaviour {
	
	public GameObject blox;
	
	
	
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	if (Input.GetKeyDown("u")){
			Instantiate (blox, transform.position, transform.rotation);
		}
	}
}
