using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


public class AddFriendsManager : MonoBehaviour
{
    [SerializeField] private TMP_InputField friendSearchBar;
    [SerializeField] private string searchedUserID;
    [SerializeField] private Texture searchedUserProfilePhoto;
    [SerializeField] private RawImage displayUserPhoto;
    // List<FriendRequest> FriendRequests = new List<FriendRequest>();
    Dictionary<string, FriendRequest> requests = new Dictionary<string, FriendRequest>();
    void OnDisable()
    {
        searchedUserID = "";
        Debug.Log("AddFriendsUIManager: script was disabled");
    }

    void OnEnable()
    {
        searchedUserID = "";
        Debug.Log("AddFriendsUIManager: script was enabled");
        //error calls before enabled
        friendSearchBar.text= "";
        displayUserPhoto.texture = null;
    }
    
    public void SubscribeToFriendRequests()
    {
       FbManager.instance.SubscribeToFriendShipRequests();
    }

    public void PrintAllFriendRequests()
    {
        var friendShipRequests= FbManager.instance.GetFriendShipRequests();
        foreach (var friendID in friendShipRequests)
        {
            //prints values of all friend requests
            Debug.Log("Friend Request From:" + friendID);
            
            //Todo: get friend request values to display to user using fb manager
            requests.Add(friendID,new FriendRequest());
        }
    }
    
    public void AcceptFriend(string friendID)
    {
        //write script that accepts friend and moves them from one collection to another
    }
    
    // Update is called once per frame
    void Update()
    {
        
    }
}
