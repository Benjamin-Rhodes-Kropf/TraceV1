using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class RegisterUIManager : MonoBehaviour
{
    //Register variables
    [Header("Register")]
    [SerializeField] private TMP_InputField usernameRegisterField;
    [SerializeField] private TMP_InputField emailRegisterField;
    [SerializeField] private TMP_InputField phoneNumber; //need to do proper validation
    [SerializeField] private TMP_InputField passwordRegisterField;
    [SerializeField] private TMP_InputField passwordRegisterVerifyField;
    [SerializeField] private TMP_Text warningRegisterText;
    [SerializeField] private TMP_Text confirmRegisterText;
    [SerializeField] private GameObject _canvas;
    [SerializeField] private String selectImageScreenID;
    public void ClearRegisterFeilds()
    {
        usernameRegisterField.text = "";
        emailRegisterField.text = "";
        passwordRegisterField.text = "";
        passwordRegisterVerifyField.text = "";
    }
    
    //Function for the register button
    public void RegisterButton()
    {
        if (passwordRegisterField.text != passwordRegisterVerifyField.text)
        {
            confirmRegisterText.text = "";
            warningRegisterText.text = "Passwords do not match!";
            return;
        }
        
        //Call the register coroutine passing the email, password, and username
        StartCoroutine(FirebaseManager.instance.TryRegister(emailRegisterField.text, passwordRegisterField.text, usernameRegisterField.text,  (myReturnValue) => {
            if (myReturnValue != null)
            {
                confirmRegisterText.text = "";
                warningRegisterText.text = myReturnValue;
            }
            else
            {
                warningRegisterText.text = "";
                confirmRegisterText.text = "Confirmed, Account Created!";
                Debug.Log("accountCreated");
                _canvas.GetComponent<ScreenManager>().ChangeScreen(selectImageScreenID);
            }
        }));
    }
}
