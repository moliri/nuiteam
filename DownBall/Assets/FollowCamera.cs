using UnityEngine;
using System.Collections;

public class FollowCamera : MonoBehaviour {
	
	public GameObject target;
	
	Vector3 offset;
	// Use this for initialization
	void Start () {
		target = GameObject.FindWithTag("Player");
		offset = target.transform.position - transform.position;
	}
	
	// Update is called once per frame
	void LateUpdate () {
		float desiredAngle = target.transform.eulerAngles.y;
    	Quaternion rotation = Quaternion.Euler(0, desiredAngle, 0);
		transform.position = target.transform.position - (rotation * offset);
		transform.LookAt(target.transform);
	}
}
