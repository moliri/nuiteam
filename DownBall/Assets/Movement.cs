using UnityEngine;
using System.Collections;

public class Movement : MonoBehaviour {

    public int navForce = 25;
	public int weakForce = 3;
	public int jumpForce = 20;
	//public float jumpHeight = 30f*Time.deltaTime;
	public bool IsGrounded;
	
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		
        if (Input.GetKey(KeyCode.LeftArrow) && IsGrounded) {
            rigidbody.AddForce(-Vector3.right*navForce);
			rigidbody.velocity = -Vector3(10, 0, 0);
        }
        if (Input.GetKey(KeyCode.RightArrow) && IsGrounded) {
            rigidbody.AddForce(Vector3.right*navForce);
			rigidbody.velocity = Vector3(10, 0, 0);
        }
        if (Input.GetKey(KeyCode.DownArrow) && IsGrounded) {
            if (rigidbody.velocity.z > 0) {
                rigidbody.AddForce(-Vector3.forward*navForce);
            } 
        }
        if (Input.GetKey(KeyCode.UpArrow)) {
            if (rigidbody.velocity.z > 0 && IsGrounded==false) {
                rigidbody.AddForce(Vector3.forward*weakForce);
				rigidbody.velocity = Vector3(0, 0, 10);
				//rigidbody.AddForce(-Vector3.forward*(weakForce-5));
            }
			else if (rigidbody.velocity.z > 0  && IsGrounded) {
				rigidbody.AddForce(Vector3.forward*navForce);
				rigidbody.velocity = Vector3(0, 0, 10);
			}
        }
		
		if (Input.GetKey(KeyCode.Space)) {
			if (IsGrounded){
				//rigidbody.AddForce(Vector3.up*jumpHeight,ForceMode.Impulse);
				rigidbody.AddForce(Vector3.up*jumpForce,ForceMode.Impulse);
				rigidbody.velocity = Vector3(0, 10, 0);
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
