using UnityEngine;
using System.Collections;

public class KinectMovement : MonoBehaviour {

    public int navForce;
	public int fwdForce;
	public int breakForce;
	public Vector3 jumpVelocity;
	public bool IsGrounded;
    public int decrement;
    public GameObject Left;
    public GameObject Right;
    public GameObject Head;    
    public GameObject HeadReference;
    public int tolerance1;
    public int tolerance2;
    public int tolerance3;
	
	// Use this for initialization
	void Start () {
	}
	
	// Update is called once per frame
	void Update () {
		
        if((Left.transform.position.y + tolerance1) >= Right.transform.position.y && (Left.transform.position.y - tolerance1) <= Right.transform.position.y){
            //No Direction
        }else if((Left.transform.position.y < Right.transform.position.y) && IsGrounded) {
            //leaning left
            for (int i=navForce; i>0; i -= decrement)	{
				rigidbody.AddForce(-Vector3.right*i*Time.deltaTime);
			}
        }else if((Left.transform.position.y > Right.transform.position.y) && IsGrounded) {
            //leaning right
            for (int i=navForce; i>0; i -= decrement)	{
            	rigidbody.AddForce(Vector3.right*i*Time.deltaTime);
			}
        }
        if((Head.transform.position.z + tolerance2) >= HeadReference.transform.position.z && (Head.transform.position.z - tolerance2) <= HeadReference.transform.position.z){
            //No Direction
        }
        else if ((Head.transform.position.z > HeadReference.transform.position.z) && IsGrounded)
        {
            //Leaning forward
            if (rigidbody.velocity.z > 0 && IsGrounded==false) {
                rigidbody.AddForce(Vector3.forward*0);
            }
			else if (rigidbody.velocity.z > 0  && IsGrounded) {
				rigidbody.AddForce(Vector3.forward*fwdForce);
			}
        }
        else if ((Head.transform.position.z < HeadReference.transform.position.z) && IsGrounded)
        {
            //Leaning back
            if (rigidbody.velocity.z > 0) {
				rigidbody.AddForce(-Vector3.forward*breakForce);
	        }
        }        
        if(((Left.transform.position.y + tolerance3) >= Head.transform.position.y && (Right.transform.position.y + tolerance3) >= Head.transform.position.y) && ((Left.transform.position.y - tolerance3) <= Head.transform.position.y && (Right.transform.position.y - tolerance3) <= Head.transform.position.y))
        {
            
        }
        else if (Left.transform.position.y > Head.transform.position.y && Right.transform.position.y > Head.transform.position.y)
        {
            if (IsGrounded)
            {
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
