using UnityEngine;
using System.Collections;

    public class Player2Camera : MonoBehaviour 
    {
        public Transform playerTransform;        

        public Vector3 cameraOrientationVector = new Vector3 (0, 25, -20f);

 
        void Start () 
        {
            //playerTransform = GameObject.FindWithTag("Player").transform;
        }
 
        void Update () 
        { 
            camera.transparencySortMode = TransparencySortMode.Orthographic;
            float tempz = playerTransform.position.z;
            float tempy = playerTransform.position.y;
            Vector3 temp2 = new Vector3 (0,tempy,tempz);
            transform.position = temp2+cameraOrientationVector;
            RaycastHit[] hits;
            hits = Physics.RaycastAll(transform.position, temp2, 50F);
            int i = 0;
            /*
            while(i < hits.Length) {
                RaycastHit hit = hits[i];
                Renderer renderer = hit.collider.renderer;
                if (renderer) 
                {
                    renderer.material.shader = Shader.Find("Transparent/Diffuse");
                    Color color = renderer.material.color;
                    color.a = 0.3F;
                    renderer.material.color = color;
                }
                i++;
            }
            */
        }
    }