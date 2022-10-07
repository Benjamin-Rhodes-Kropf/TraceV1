using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


public class AddFriendsUIManager : MonoBehaviour
{
    [SerializeField] private TMP_InputField friendSearchBar;
    void OnDisable()
    {
        Debug.Log("AddFriendsUIManager: script was disabled");
    }

    void OnEnable()
    {
        Debug.Log("AddFriendsUIManager: script was enabled");
        friendSearchBar.text = "";
    }
    
    public void SearchForUser()
    {
        StartCoroutine(FirebaseManager.instance.SearchForUserIDByUsername(friendSearchBar.text, (myReturnValue) =>
        {
            if (myReturnValue.IsSuccessful)
            {
                Debug.Log(myReturnValue.ReturnValue);
            }
            else
            {
                Debug.Log("db changed user nickName");
            }
        }));

    }


    // Update is called once per frame
    void Update()
    {
        
    }
}
