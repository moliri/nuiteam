using UnityEngine;
using System.Collections;

public class p2move : MonoBehaviour {
	public GameObject target;
	public int force = 100;
	public float jumpHeight = 30f*Time.deltaTime;
	Vector3 jump = Vector3.zero;	
	public string left = "left";
	public string right = "right";
	public string down = "down";
	public string up = "up";
	public string space = "space";
	Vector3 newpos;
	Vector3 xmod;
// Use this for initialization
void Start () {
}

// Update is called once per frame
void Update () {
	newpos = target.transform.position;
	xmod.x = transform.position.x;//for movement limits

	if (transform.position.x <= newpos.x +100){
		if (transform.position.x >= newpos.x - 100){
			if (Input.GetKey("a")) {
			xmod.x -= 3;
			//rigidbody.AddForce(-Vector3.right*force*5);
			}
			if (Input.GetKey("d")) {
			xmod.x += 3;
			//rigidbody.AddForce(Vector3.right*force*5);
			}

		}
			else{
		xmod.x += 1;
		}
	}
	else{
		xmod.x -=1;
	}
	
	newpos.y += -22.63086f; //-24+2
	newpos.z += 31.590771f;
	newpos.x = xmod.x;
	
	transform.position = newpos;//follow down track
	}
}