using System;
using System.Collections;
using System.Collections.Generic;
using Mapbox.Json.Bson;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


public class AddFriendsUIManager : MonoBehaviour
{
    [SerializeField] private TMP_InputField friendSearchBar;
    [SerializeField] private string searchedUserID;
    [SerializeField] private Texture searchedUserProfilePhoto;
    [SerializeField] private RawImage displayUserPhoto;
    void OnDisable()
    {
        searchedUserID = "";
        Debug.Log("AddFriendsUIManager: script was disabled");
    }

    void OnEnable()
    {
        searchedUserID = "";
        Debug.Log("AddFriendsUIManager: script was enabled");
        friendSearchBar.text = "";
    }
    
    public void SearchForUser()
    {
        StartCoroutine(FirebaseManager.instance.SearchForUserByUsername(friendSearchBar.text, (myReturnValue) =>
        {
            if (myReturnValue.IsSuccessful)
            {
                Debug.Log("search returned user");
                Debug.Log(myReturnValue.ReturnValue);
                searchedUserID = myReturnValue.message.ToString();
                searchedUserProfilePhoto = (Texture2D)myReturnValue.ReturnValue;
                displayUserPhoto.texture = searchedUserProfilePhoto;
            }
            else
            {
                Debug.LogWarning("failed to find user");
            }
        }));
    }

    public void RequestFriendWithSearchedUser()
    {
        //if there is no user
        //return with somthin along the lines of "you have to search a valid user first"
        //else
        //call firebase manager request friendship
    }
    


    // Update is called once per frame
    void Update()
    {
        
    }
}
