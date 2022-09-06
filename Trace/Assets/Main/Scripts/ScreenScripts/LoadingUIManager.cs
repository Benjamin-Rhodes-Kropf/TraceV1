using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoadingUIManager : MonoBehaviour
{
    [Header("animatedObjects")] 
    [SerializeField] private SlideToAnimScript _loginButton;
    [SerializeField] private SlideToAnimScript _signUpButton;
    [SerializeField] private SlideToAnimScript _traceText;


    public void PullButtonsUp()
    {
        _loginButton.SlideToFinish();
        _signUpButton.SlideToFinish();
        _traceText.SlideToFinish();
    }
    public void PullButtonsDown()
    {
        _traceText.SlideToStart();
        _loginButton.SlideToStart();
        _signUpButton.SlideToStart();
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
