using UnityEngine;
using System.Collections;

public class Movement1 : MonoBehaviour {

    public int navForce;
	public int fwdForce;
	public int breakForce;
	public Vector3 jumpVelocity;
	public bool IsGrounded;
    public GameObject Left;
    public GameObject Right;
    public GameObject ForwardBreak;    
    public GameObject FBReference;
    public int tolerance1;
    public int tolerance2;
	
	// Use this for initialization
	void Start () {
		fwdForce = 10;
		breakForce = 100;
		jumpVelocity = new Vector3(0, 15, 5);
	}
	
	// Update is called once per frame
	void Update () {
		
        if((Left.transform.position.y + tolerance1) >= Right.transform.position.y && (Left.transform.position.y - tolerance1) <= Right.transform.position.y){
            //No Direction
        }else if(Left.transform.position.y < Right.transform.position.y){
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
        if((ForwardBreak.transform.position.z + tolerance2) >= FBReference.transform.position.z && (ForwardBreak.transform.position.z - tolerance2) <= FBReference.transform.position.z){
            //No Direction
        }
        else if (ForwardBreak.transform.position.z > FBReference.transform.position.z)
        {
            //Leaning forward
            if (rigidbody.velocity.z > 0 && IsGrounded==false) {
                rigidbody.AddForce(Vector3.forward*0);
            }
			else if (rigidbody.velocity.z > 0  && IsGrounded) {
				rigidbody.AddForce(Vector3.forward*fwdForce);
			}
        }
        else
        {
            //Leaning back
            if (rigidbody.velocity.z > 0) {
				rigidbody.AddForce(-Vector3.forward*breakForce);
	        }
        }
        
        /*
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
