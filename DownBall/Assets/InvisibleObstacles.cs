using UnityEngine;
using System.Collections;

public class InvisibleObstacles : MonoBehaviour {
    
    GameObject[] obstacles;
    public GameObject WatchTarget;
    
	// Use this for initialization
	void Start () {
        obstacles = GameObject.FindGameObjectsWithTag("Obstacles");
	}
	
	// Update is called once per frame
	void Update () {
        foreach (GameObject ob in obstacles)
        {
            if (ob.transform.position.z < WatchTarget.transform.position.z)
            {
                ob.GetComponent<MeshRenderer>().enabled = false;
            }
        }
	}
}
