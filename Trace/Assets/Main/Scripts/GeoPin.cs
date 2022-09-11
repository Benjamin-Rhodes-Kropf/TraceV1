using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GeoPin : MonoBehaviour
{
    private Transform _camera;
    private Vector3 target;
    // Start is called before the first frame update
    private void Awake()
    {
        _camera = Camera.main.transform;
    }

    // Update is called once per frame
    void Update()
    {
        target = _camera.transform.position;
        Vector3 relativePos = new Vector3(target.x,target.y, target.z) - transform.position;
        transform.rotation = Quaternion.LookRotation(relativePos);
    }
}
