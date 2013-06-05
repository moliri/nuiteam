using UnityEngine;
using System.Collections;

public class MovingObstacles : MonoBehaviour {

    public Vector3 pointB;
     
    IEnumerator Start () {
        Vector3 pointA = transform.position;
        while (true) {
            yield return MoveObject(pointA, pointB, 3);
            yield return MoveObject(pointB, pointA, 3);
        }
    }
     
    IEnumerator MoveObject (Vector3 startPos, Vector3 endPos, float time) {
        float i = 0.0f;
        float rate = 1.0f / time;
        while (i < 1.0f) {
            i += Time.deltaTime * rate;
            transform.position = Vector3.Lerp(startPos, endPos, i);
            yield break; 
        }
    }
}