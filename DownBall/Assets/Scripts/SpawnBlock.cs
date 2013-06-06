using UnityEngine;
using System.Collections;
public class SpawnBlock : MonoBehaviour {

    public GameObject blox;
    public GameObject blox1;
    float timer;


    // Use this for initialization
    void Start () {
        timer = 99;
    }

    // Update is called once per frame
    void Update () {
        timer += 1;
        if (timer == 101){
            timer = 100;
        }

        if (Input.GetKeyDown("c")){
            if (timer == 100){
                Destroy(Instantiate (blox, transform.position, transform.rotation),3);
                timer -= 100;
            }
        }
        if (Input.GetKeyDown("x")){
            if (timer == 100){
                Destroy(Instantiate (blox1, transform.position, transform.rotation),3);
                timer -= 100;
            }
        }
    }
}