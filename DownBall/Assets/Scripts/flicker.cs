using UnityEngine;
using System.Collections;

public class flicker : MonoBehaviour {

	 public float duration = 1.0F;
    public Color color0 = Color.red;
    public Color color1 = Color.blue;
    void Update() {
        float t = Mathf.PingPong(Time.time, duration) / duration;
        light.color = Color.Lerp(color0, color1, t);
    }
	/*int timer = 0;
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
		if (timer == 30){
			light.color = 	
		}
		if(timer == 180){
			
			timer = 0;
		}
		
		timer ++;
	}*/
}
