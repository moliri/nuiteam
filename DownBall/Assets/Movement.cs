using UnityEngine;
using System.Collections;

public class Movement : MonoBehaviour {

    public int force = 100;
	public float jumpHeight = 30f*Time.deltaTime;
    Vector3 jump = Vector3.zero;	
    public string left = "left";
    public string right = "right";
    public string down = "down";
    public string up = "up";
    public string space = "space";
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		float y = rigidbody.position.y - 1.5f;
		
        if (Input.GetKey(left)) {
            rigidbody.AddForce(-Vector3.right*force*5);
        }
        if (Input.GetKey(right)) {
            rigidbody.AddForce(Vector3.right*force*5);
        }
        if (Input.GetKey(down)) {
            if (rigidbody.velocity.z > 0) {
                rigidbody.AddForce(-Vector3.forward*force*4);
            } 
        }
        if (Input.GetKey(up)) {
            if (rigidbody.velocity.z > 0) {
                rigidbody.AddForce(Vector3.forward*force*2);
            } 
        }
		
		if (Input.GetKey(space) && (y <= 0))
		{
			jump = Vector3.up;
			rigidbody.AddForce(jump*jumpHeight,ForceMode.Impulse);
		}
	}
}
