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
            float tempz = playerTransform.position.z;
            float tempy = playerTransform.position.y;
            Vector3 temp2 = new Vector3 (0,tempy,tempz);
            transform.position = temp2+cameraOrientationVector;
        }
    }