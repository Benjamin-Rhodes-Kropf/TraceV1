using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class User {
    //they must be public for json utility conversion
    public string username;
    public string name;
    public string email;
    public string phone;
    public bool isOnline;
    public int friendCount;
    public int score;
    

    public User()
    {
        isOnline = false;
        friendCount = 0;
        score = 0;
    }

    public User(string username, string name, string email, string phone) {
        this.username = username;
        this.name = name;
        this.email = email;
        this.phone = phone;
    }
}
