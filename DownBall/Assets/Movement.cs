using UnityEngine;
using System.Collections;

public class Movement : MonoBehaviour {

    public int force = 100;
	public float jumpHeight = 30f*Time.deltaTime;
    Vector3 jump = Vector3.zero;	
	
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		float y = rigidbody.position.y - 1.5f;
		
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
		if (Input.GetKey(KeyCode.Space) && (y <= 0))
		{
			jump = Vector3.up;
			rigidbody.AddForce(jump*jumpHeight,ForceMode.Impulse);
		}
	}
}
