using UnityEngine;
using System.Collections;

public class Movement : MonoBehaviour {

    public int LRForce;
	public int UDForce;
	public int weakForce;
	public int jumpForce;
	//public float jumpHeight = 30f*Time.deltaTime;
	public bool IsGrounded;
	
	// Use this for initialization
	void Start () {
		LRForce = 50;
		UDForce = 10;
		weakForce = 2;
		jumpForce = 20;
	}
	
	// Update is called once per frame
	void Update () {
		
        if (Input.GetKey(KeyCode.LeftArrow) && IsGrounded) {
            rigidbody.AddForce(-Vector3.right*LRForce);
			//rigidbody.velocity = new Vector3(-25,0,0);
        }
        if (Input.GetKey(KeyCode.RightArrow) && IsGrounded) {
            rigidbody.AddForce(Vector3.right*LRForce);
			//rigidbody.velocity = new Vector3(25,0,0);
        }
        if (Input.GetKey(KeyCode.DownArrow) && IsGrounded) {
            if (rigidbody.velocity.z > 0) {
                rigidbody.AddForce(-Vector3.forward*LRForce);
	            } 
        }
        if (Input.GetKey(KeyCode.UpArrow)) {
            if (rigidbody.velocity.z > 0 && IsGrounded==false) {
                rigidbody.AddForce(Vector3.forward*weakForce);
				//rigidbody.velocity = new Vector3(0,15,0);
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
