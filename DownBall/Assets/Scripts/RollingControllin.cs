using UnityEngine;
using System.Collections;

public class RollingControllin : MonoBehaviour {

	// Use this for initialization
	public float speed = 40f;
	public float jumpHeight = 30f*Time.deltaTime;
	public float velocity = 0;
	Vector3 torque = Vector3.zero;
	Vector3 jump = Vector3.zero;	
	
	// Update is called once per frame
	void Update () {
		velocity = rigidbody.velocity.magnitude;
		float y = rigidbody.position.y - 1.5f;
		if (Input.GetKey(KeyCode.UpArrow))
		{
			torque = Camera.main.transform.right;
	    	torque.y = 0;
	    	rigidbody.AddTorque(torque.normalized*speed);
			rigidbody.AddForce (Camera.main.transform.up.normalized*speed);
		}
		if (Input.GetKey(KeyCode.DownArrow))
		{
			torque = Camera.main.transform.right;
	    	torque.y = 0;
	    	rigidbody.AddTorque(-torque.normalized*speed);
			rigidbody.AddForce (-Camera.main.transform.up.normalized*speed);
		}
		if (Input.GetKey(KeyCode.LeftArrow))
		{
			torque = Camera.main.transform.forward;
	    	torque.y = 0;
	    	rigidbody.AddTorque(torque.normalized*speed);
			rigidbody.AddForce (-Camera.main.transform.right.normalized*speed);
		}
		if (Input.GetKey(KeyCode.RightArrow))
		{
			torque = Camera.main.transform.forward;
	    	torque.y = 0;
	    	rigidbody.AddTorque(-torque.normalized*speed);
			rigidbody.AddForce (Camera.main.transform.right.normalized*speed);
		}
		if (Input.GetKey(KeyCode.Space) && (y <= 0))
		{
			jump = Vector3.up;
			rigidbody.AddForce(jump*jumpHeight,ForceMode.Impulse);
		}
	}
}
