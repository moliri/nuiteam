using UnityEngine;
using System.Collections;

public class Movement : MonoBehaviour {

    public int LRForce = 50;
	public int UDForce = 10;
	public int weakForce = 2;
	public int jumpForce = 20;
	//public float jumpHeight = 30f*Time.deltaTime;
	public bool IsGrounded;
	
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		
        if (Input.GetKey(KeyCode.LeftArrow) && IsGrounded) {
            rigidbody.AddForce(-Vector3.right*LRForce);
        }
        if (Input.GetKey(KeyCode.RightArrow) && IsGrounded) {
            rigidbody.AddForce(Vector3.right*LRForce);
        }
        if (Input.GetKey(KeyCode.DownArrow) && IsGrounded) {
            if (rigidbody.velocity.z > 0) {
                rigidbody.AddForce(-Vector3.forward*UDForce);
            } 
        }
        if (Input.GetKey(KeyCode.UpArrow)) {
            if (rigidbody.velocity.z > 0 && IsGrounded==false) {
                rigidbody.AddForce(Vector3.forward*weakForce);
				//rigidbody.AddForce(Vector3.forward*weakForce);
				//rigidbody.AddForce(-Vector3.forward*(weakForce-5));
            }
			else if (rigidbody.velocity.z > 0  && IsGrounded) {
				rigidbody.AddForce(Vector3.forward*UDForce);
			}
        }
		
		if (Input.GetKey(KeyCode.Space)) {
			if (IsGrounded){
				//rigidbody.AddForce(Vector3.up*jumpHeight,ForceMode.Impulse);
				rigidbody.AddForce(Vector3.up*jumpForce,ForceMode.Impulse);
			}
		}
	}
	
	void OnCollisionStay(Collision collisionInfo) {
		if(collisionInfo.gameObject.tag == "ground")  {
			IsGrounded = true;
		}
	}
	
	void OnCollisionExit(Collision collisionInfo) {
		IsGrounded = false;
	}
}
