using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraScript : MonoBehaviour
{
    private Camera _camera;
    // Start is called before the first frame update
    void Start()
    {
        _camera = Camera.main;
    }

    // Update is called once per frame
    void Update()
    {
        if ( Input.GetMouseButtonDown (0)){ 
            Debug.Log("mouse hit");
            RaycastHit hit; 
            Ray ray = _camera.ScreenPointToRay(Input.mousePosition); 
            if ( Physics.Raycast (ray,out hit,250.0f)) {
                Debug.Log("You selected the " + hit.transform.name); // ensure you picked right object
            }
        }
    }
}
