using UnityEngine;
using System.Collections;

public class Movement1 : MonoBehaviour {

    public int navForce;
	public int fwdForce;
	public int breakForce;
	public Vector3 jumpVelocity;
	public bool IsGrounded;
    public GameObject Shoulder_Left;
    public GameObject Shoulder_Right;
    public int tolerance;
	
	// Use this for initialization
	void Start () {
		fwdForce = 10;
		breakForce = 100;
		jumpVelocity = new Vector3(0, 15, 5);
	}
	
	// Update is called once per frame
	void Update () {
		
        if((Shoulder_Left.transform.position.y + tolerance) >= Shoulder_Right.transform.position.y && (Shoulder_Left.transform.position.y - tolerance) <= Shoulder_Right.transform.position.y){
            //No Direction
        }else if(Shoulder_Left.transform.position.y < Shoulder_Right.transform.position.y){
            //leaning left
            for (int i=navForce; i>0; i -= 10)	{
				rigidbody.AddForce(-Vector3.right*i*Time.deltaTime);
			}
        }else{
            //leaning right
            for (int i=navForce; i>0; i -= 10)	{
            	rigidbody.AddForce(Vector3.right*i*Time.deltaTime);
			}
        }
        /*
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
        */
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
