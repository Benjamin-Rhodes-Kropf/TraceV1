using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FriendRequest : MonoBehaviour
{
    public string username;
    public string name;
    public string userPhotoUrl;
    public Texture userPhoto;
    public int score;

    
    public FriendRequest() 
    {
    }

    public FriendRequest(string username, string name, string userPhotoUrl, Texture userPhoto, int score) {
        this.username = username;
        this.name = name;
        this.userPhotoUrl = userPhotoUrl;
        this.userPhoto = userPhoto;
        this.score = score;
    }
}
