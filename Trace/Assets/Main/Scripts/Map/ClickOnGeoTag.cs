using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClickOnGeoTag : MonoBehaviour
{
    [SerializeField] private Camera _camera;


    // Update is called once per frame
    void Update()
    {
        if ( Input.GetMouseButtonDown (0)){ 
            //Todo:re work clicking on geo pins
            
            // Debug.Log("mouse clicked");
            // RaycastHit hit; 
            // Ray ray = _camera.ScreenPointToRay(Input.mousePosition); 
            // if ( Physics.Raycast (ray,out hit,250.0f)) {
            //     Debug.Log("You selected the " + hit.transform.name); // ensure you picked right object
            // }
        }
        
        
    }
}
