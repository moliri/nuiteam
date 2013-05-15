using UnityEngine;
using System.Collections;

public class Movement : MonoBehaviour {

    public int strongForce;
	public int weakForce;
	public int jumpForce;
	//public float jumpHeight = 30f*Time.deltaTime;
	public bool IsGrounded;
	
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		
        if (Input.GetKey(KeyCode.LeftArrow)) {
            rigidbody.AddForce(-Vector3.right*strongForce);
        }
        if (Input.GetKey(KeyCode.RightArrow)) {
            rigidbody.AddForce(Vector3.right*strongForce);
        }
        if (Input.GetKey(KeyCode.DownArrow)) {
            if (rigidbody.velocity.z > 0) {
                rigidbody.AddForce(-Vector3.forward*strongForce);
            } 
        }
        if (Input.GetKey(KeyCode.UpArrow)) {
            if (rigidbody.velocity.z > 0 && IsGrounded==false) {
                rigidbody.AddForce(Vector3.forward*0);
				//rigidbody.AddForce(Vector3.forward*weakForce);
				//rigidbody.AddForce(-Vector3.forward*(weakForce-5));
            }
			else if (rigidbody.velocity.z > 0) {
				rigidbody.AddForce(Vector3.forward*strongForce);
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
