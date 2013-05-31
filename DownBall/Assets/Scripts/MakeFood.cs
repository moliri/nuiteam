using UnityEngine;
using System.Collections;

public class MakeFood : MonoBehaviour {

	public GameObject foodType;
	private Vector3 current;
	private float timer;

	// Use this for initialization
	void Start () {
		timer = -300;
		current=Vector3.zero;
	
	}
	
	// Update is called once per frame
	void Update () {
		timer++;
		if(timer%100==0){
			current.x = transform.position.x + (rigidbody.velocity.x);
			current.y = transform.position.y + (rigidbody.velocity.y);
			current.z = transform.position.z + (rigidbody.velocity.z);
			Instantiate(foodType,transform.position,transform.rotation);
		}
	}
}
