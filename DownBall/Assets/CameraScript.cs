using UnityEngine;
using System.Collections;

    public class CameraScript : MonoBehaviour 
    {
        Transform playerTransform;

        Vector3 cameraOrientationVector = new Vector3 (0, 15, -10f);

 
        void Start () 
        {
            playerTransform = GameObject.FindWithTag("Player").transform;
        }
 
        void LateUpdate () 
        { 
           transform.position = playerTransform.position + cameraOrientationVector;
        }
    }