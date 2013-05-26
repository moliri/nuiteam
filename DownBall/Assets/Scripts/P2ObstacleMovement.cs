using UnityEngine;
using System.Collections;

public class P2ObstacleMovement : MonoBehaviour 
{    
    public Transform followObject;
    public Transform cameraObject;
    public Vector3 locationVector = new Vector3 (0,0,0);
    
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
        float tempz = followObject.position.z;
        float tempy = followObject.position.y;
        Vector3 temp = new Vector3 (0,tempy,tempz);
        transform.position = temp+locationVector;
        float dist = Vector3.Distance(cameraObject.position, transform.position);
        
	}
}
