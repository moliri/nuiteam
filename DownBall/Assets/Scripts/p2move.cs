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
	//xmod.y = transform.position.y;
	//xmod.z = transform.position.z;


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
	
	/*if (transform.position.z <= newpos.z +50){
			if (transform.position.z >= newpos.z -5){
				if (Input.GetKey("w")){
					xmod.y += -2.263086f;
					xmod.z += 3.1590771f;
				}
				if (Input.GetKey ("s")){
					xmod.y += 2.263086f;
					xmod.z += -3.1590771f;
				}
			}
			else{
				xmod.y	= newpos.y;
				xmod.z  = newpos.z -5;
			}
	}
	else{
		xmod.y	+= (2.263086f)/2;
		xmod.z  += -(3.1590771f)/3;		
	}*/
	
	newpos.y -= 22.63086f; //-24+2 ..= xmod.y;
	newpos.z += 31.590771f;  // xmod.z;
	newpos.x = xmod.x;
	
	transform.position = newpos;//follow down track
	}
}