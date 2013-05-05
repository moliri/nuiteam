using UnityEngine;
using System.Collections;

public class Movement : MonoBehaviour {

    public int force = 25;
    
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
        if (Input.GetKey("left")) {
            rigidbody.AddForce(-Vector3.right*force);
        }
        if (Input.GetKey("right")) {
            rigidbody.AddForce(Vector3.right*force);
        }
        if (Input.GetKey("down")) {
            if (rigidbody.velocity.z > 0) {
                rigidbody.AddForce(-Vector3.forward*force);
            }
        }   
	}
}
