using UnityEngine;
using System.Collections;
using System.Collections.Generic;
 
public class HideObjects : MonoBehaviour {
 
    public Transform WatchTarget;
    public LayerMask OccluderMask;
    //This is the material with the Transparent/Diffuse With Shadow shader
    public Material HiderMaterial;
    public Shader HiderShader = Shader.Find("Transparent/Diffuse");
 
    //private Dictionary<Transform, Material> _LastTransforms;
    private Dictionary<Transform, Shader> _LastTransforms;
 
    void Start () {
        //_LastTransforms = new Dictionary<Transform, Material>();
        _LastTransforms = new Dictionary<Transform, Shader>();
    }
 
    void Update () {
        //reset and clear all the previous objects
        if(_LastTransforms.Count > 0){
            foreach(Transform t in _LastTransforms.Keys){
                t.renderer.material.shader = _LastTransforms[t];
            }
            _LastTransforms.Clear();
        }
 
        //Cast a ray from this object's transform the the watch target's transform.
        RaycastHit[] hits = Physics.RaycastAll(
            transform.position,
            WatchTarget.transform.position - transform.position,
            Vector3.Distance(WatchTarget.transform.position, transform.position),
            OccluderMask
            );
 
        //Loop through all overlapping objects and disable their mesh renderer
        if(hits.Length > 0){
            foreach(RaycastHit hit in hits){
                if(hit.collider.gameObject.transform != WatchTarget && hit.collider.transform.root != WatchTarget && hit.collider.gameObject.tag != "Wall") 
                {
                    _LastTransforms.Add(hit.collider.gameObject.transform, hit.collider.gameObject.renderer.material.shader);
                    hit.collider.gameObject.renderer.material.shader = HiderShader;
                }
            }
        }
    }
}