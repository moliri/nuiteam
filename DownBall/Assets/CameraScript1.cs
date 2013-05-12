using UnityEngine;
using System.Collections;

    public class CameraScript1 : MonoBehaviour 
    {
        public Transform playerTransform;

        public Vector3 cameraOrientationVector = new Vector3 (0, 25, -20f);

 
        void Start () 
        {
            //playerTransform = GameObject.FindWithTag("Player").transform;
        }
 
        void LateUpdate () 
        { 
            float temp = playerTransform.position.z;
            Vector3 temp2 = new Vector3 (0,0,temp);
            transform.position = temp2+cameraOrientationVector;
        }
    }