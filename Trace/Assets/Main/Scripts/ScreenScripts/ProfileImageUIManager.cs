using System;
using System.Collections;
using System.Collections.Generic;
using Firebase.Extensions;
using Firebase.Storage;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ProfileImageUIManager : MonoBehaviour
{
    [Header("ProfileImage")]
    [SerializeField]private Image _profileImage;
    [SerializeField]private RawImage _profileImageRaw;
    [SerializeField] private String baseImageUrl;
    public Image imageToUpload;
    public TMP_InputField profileName;


    [Header("Test")] [SerializeField] private Sprite profileImageOne;
    [SerializeField] private Sprite profileImageTwo;
    [SerializeField] private Sprite profileImageThree;
    
    
    private void Awake()
    {
        _profileImage.sprite = profileImageOne;
    }
    
    public void Update()
    {
        if (FbManager.instance.userImageTexture != null)
        {
            _profileImageRaw.texture = FbManager.instance.userImageTexture;
        }
    }


    public void SelectPhotoButton()
    {
        if (_profileImage.sprite == profileImageOne)
        {
            _profileImage.sprite = profileImageTwo;
        }
        else if (_profileImage.sprite == profileImageTwo)
        {
            _profileImage.sprite = profileImageThree;
        }
        else if (_profileImage.sprite == profileImageThree)
        {
            _profileImage.sprite = profileImageOne;
        }
    }
    public void SetProfileNickName()
    {
        StartCoroutine(FbManager.instance.SetUserNickName(profileName.text, (myReturnValue) =>
        {
            if (myReturnValue != null)
            {
                Debug.Log(myReturnValue);
            }
            else
            {
                Debug.Log("db changed user nickName");
            }
        }));
    }
    
    public void ChangeProfileImage()
    {
        
    }
    
    public void Login()
    {
        StartCoroutine(FbManager.instance.AutoLogin());
    }
}
