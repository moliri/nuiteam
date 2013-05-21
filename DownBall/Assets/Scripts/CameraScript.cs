using UnityEngine;
using System.Collections;

    public class CameraScript : MonoBehaviour 
    {
        public Transform playerTransform;

        public Vector3 cameraOrientationVector = new Vector3 (0, 25, -20f);

 
        void Start () 
        {
            //playerTransform = GameObject.FindWithTag("Player").transform;
        }
 
        void LateUpdate () 
        { 
           transform.position = playerTransform.position + cameraOrientationVector;
        }
    }