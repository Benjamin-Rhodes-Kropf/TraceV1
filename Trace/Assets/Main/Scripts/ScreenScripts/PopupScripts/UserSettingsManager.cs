using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UserSettingsManager : MonoBehaviour
{
    [SerializeField] private RawImage _profileImage;


    private void OnEnable()
    {
        Debug.Log("profile settings enabled");
        _profileImage.texture = FirebaseManager.instance.userImageTexture;
    }


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
