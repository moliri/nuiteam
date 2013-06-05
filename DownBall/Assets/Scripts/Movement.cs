using UnityEngine;
using System.Collections;

public class Movement : MonoBehaviour {

    public int navForce;
	public int fwdForce=10;
	public int breakForce=100;
	public Vector3 jumpVelocity = new Vector3(0,15,5);
	public bool IsGrounded;
	
	// Use this for initialization
	void Start () {
	}
	
	// Update is called once per frame
	void Update () {
		
		//Left Key
        if (Input.GetKey(KeyCode.LeftArrow) && IsGrounded) {
			for (navForce=500; navForce>0; navForce -= 10)	{
				rigidbody.AddForce(-Vector3.right*navForce*Time.deltaTime);
			}
		}
	
		//Right Key
        if (Input.GetKey(KeyCode.RightArrow) && IsGrounded) {
			for (navForce=500; navForce>0; navForce -= 10)	{
            	rigidbody.AddForce(Vector3.right*navForce*Time.deltaTime);
			}
        }
		
		//Down Key
        if (Input.GetKey(KeyCode.DownArrow) && IsGrounded) {
            if (rigidbody.velocity.z > 0) {
				rigidbody.AddForce(-Vector3.forward*breakForce);
	        }
		}
        
		//Up Key
        if (Input.GetKey(KeyCode.UpArrow)) {
            if (rigidbody.velocity.z > 0 && IsGrounded==false) {
                rigidbody.AddForce(Vector3.forward*0);
				//rigidbody.velocity = new Vector3(0,15,0);
				//rigidbody.AddForce(Vector3.forward*weakForce);
				//rigidbody.AddForce(-Vector3.forward*(weakForce-5));
            }
			else if (rigidbody.velocity.z > 0  && IsGrounded) {
				rigidbody.AddForce(Vector3.forward*fwdForce);
			}
        }
		
		if (Input.GetKey(KeyCode.Space)) {
			if (IsGrounded){
				//rigidbody.AddForce(Vector3.up*jumpHeight,ForceMode.Impulse);
				//rigidbody.AddForce(Vector3.up*jumpForce,ForceMode.Impulse);
				rigidbody.AddForce(jumpVelocity,ForceMode.VelocityChange);
			}
		}
	}
	
	void OnCollisionStay(Collision collisionInfo) {
		if(collisionInfo.gameObject.tag == "Ground")  {
			IsGrounded = true;
		}
	}
	
	void OnCollisionExit(Collision collisionInfo) {
		IsGrounded = false;
	}
}
