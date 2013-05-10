using UnityEngine;
using System.Collections;

public class Timer : MonoBehaviour {
	
    public float maxTime = 6.0F;
	public float currentTime = 0;	
	
    void Update() {
		if(currentTime >= maxTime){
			//do stuff when time is reached
        }
    }
}

