using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class LoginUIManager : MonoBehaviour
{
    [Header("Login")]
    public TMP_InputField emailLoginField;
    public TMP_InputField passwordLoginField;
    public TMP_Text warningLoginText;
    public TMP_Text confirmLoginText;
    [SerializeField] private GameObject _screenManager;
    
    public void ClearLoginFeilds()
    {
        emailLoginField.text = "";
        passwordLoginField.text = "";
    }
    
    //Function for the login button
    public void LoginButton()
    {
        String test = "hello world";
        //Call the login coroutine passing the email and password with return values
        StartCoroutine(FirebaseManager.instance.TryLogin(emailLoginField.text, passwordLoginField.text, (myReturnValue) => {
            if (myReturnValue != null)
            {
                confirmLoginText.text = "";
                warningLoginText.text = myReturnValue;
            }
            else
            {
                warningLoginText.text = "";
                confirmLoginText.text = "Confirmed, Your In!";
                _screenManager.GetComponent<ScreenManager>().Login();
            }
        }));
    }
    
    public void SignOutButton()
    {
        FirebaseManager.instance.SignOut();
        // UIManager.instance.LoginScreen();
        ClearLoginFeilds();
    }
    
    // Start is called before the first frame update
    void Awake()
    {
        ClearLoginFeilds();
    }
}
