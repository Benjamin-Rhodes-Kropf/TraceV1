using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;

public class ScreenManager : MonoBehaviour
{
    // animator that controls transitions, requires next and prev triggers
    [SerializeField] private Animator ScreenAnimator;

    // containers for currently displayed screen and hidden screens
    [SerializeField] private Transform PopUpParent;
    [SerializeField] private Transform activeParent;
    [SerializeField] private Transform inactiveParent;
    
    // containers for animating screens
    [SerializeField] private Transform startParent;
    [SerializeField] private Transform endParent;
    
    // screens to be dislayed
    [SerializeField] private UIScreen[] Screens;
    [SerializeField] private UIScreen[] PopUpScreens;

    [SerializeField] private List<UIScreen> history;
    [SerializeField] private UIScreen current;
    [SerializeField] private UIScreen currentPopUp;
    
    //screen ref for logout
    [SerializeField] private LoadingUIManager _loadingPageUIManager;
    
    //setup screens
    void Start()
    {
        // re-parent all screen transforms to hidden object
        foreach(var s in Screens)
        {
            s.ScreenObject.gameObject.SetActive(true);
            s.ScreenObject.transform.SetParent(inactiveParent, false);
        }
        foreach(var s in PopUpScreens)
        {
            s.ScreenObject.gameObject.SetActive(true);
            s.ScreenObject.transform.SetParent(inactiveParent, false);
        }
        
        ResetMenu();
        activeParent.gameObject.SetActive(true);
        inactiveParent.gameObject.SetActive(false);
    }
    public void Login()
    {
        ScreenAnimator.SetTrigger("Login"); // trigger animation //Next
        current.ScreenObject.SetParent(endParent, false); // set current screen parent for animation
        UIScreen screen = ScreenFromID("MapScreen");
        screen.ScreenObject.SetParent(startParent, false); // set new screen parent for animation
        current = screen;
    }

    public void EnterPhoto()
    {
        ScreenAnimator.SetTrigger("TakePhoto"); // trigger animation
    }
    public void ExitPhoto()
    {
        ScreenAnimator.SetTrigger("ExitPhoto"); // trigger animation
    }

    public void PullUpOnboardingOptions()
    {
        _loadingPageUIManager.PullButtonsUp();
    }

    //menu animation controls
    public void PopupScreen(string PopUpID)
    {
        currentPopUp = PopupFromID(PopUpID);
        currentPopUp.ScreenObject.SetParent(PopUpParent);
        ScreenAnimator.SetTrigger("PopIn"); // trigger animation
    }
    public void ClosePopup()
    {
        ScreenAnimator.SetTrigger("PopOut"); // trigger animation
    }
    
    public void ChangeScreen(string ScreenID)
    {
        UIScreen screen = ScreenFromID(ScreenID);
        if ( screen != null)
        {
            history.Clear();
            ScreenAnimator.SetTrigger("Next"); // trigger animation
            current.ScreenObject.SetParent(startParent, false); // set current screen parent for animation
            history.Add(current); // add current screen to history
            current = screen; // assign new as current
            screen.ScreenObject.SetParent(endParent, false); // set new screen parent for animation
        }
    }
    public void GoBackScreen()
    {
        //Todo: Make work for more than one screen
        if (history.Count < 1) { 
            Debug.LogWarning("historyLessThanOne");
            return; // if first screen, ignore
        }
        UIScreen screen = history[history.Count - 1]; // get previous screen
        history.Remove(history[history.Count - 1]); // remove current screen from history
        ScreenAnimator.SetTrigger("Prev"); // trigger animation //Next
        current.ScreenObject.SetParent(endParent, false); // set current screen parent for animation
        current = screen; // assign new as current
        screen.ScreenObject.SetParent(startParent, false); // set new screen parent for animation
    }

    
    
    
    //get IDs
    UIScreen ScreenFromID(string ScreenID)
    {
        foreach (UIScreen screen in Screens)
        {
            if (screen.Name == ScreenID) return screen;
        }

        return null;
    }
    UIScreen PopupFromID(string ScreenID)
    {
        foreach (UIScreen screen in PopUpScreens)
        {
            if (screen.Name == ScreenID) return screen;
        }

        return null;
    }
    
    //functions called from animation
    public void SetActiveParent()
    {
        foreach (var s in Screens)
        {
            if (s != current) s.ScreenObject.SetParent(inactiveParent, false);
        }

        // show active screen
        current.ScreenObject.SetParent(activeParent, false);
    }
    public void ClearAllPopups()
    {
        foreach (var p in PopUpScreens)
        {
            p.ScreenObject.SetParent(inactiveParent, false);
        }
    }
    public void ResetMenu()
    {
        // clear history
        history = new List<UIScreen>();
        
        //set loading screen
        UIScreen screen = ScreenFromID("LoadingScreen");
        current = screen; // set start screen
        current.ScreenObject.SetParent(startParent, false); // set current screen parent for animation

        //old code
        //set screen to load in
        // ScreenAnimator.SetTrigger("Next");  // trigger animation
        // current = Screens[startScreenIndex]; // set start screen
        // current.ScreenObject.SetParent(endParent, false); // set start screen parent for animation
    }

    
    //Todo: remove in production
    void Update()
    {
        //backForTestMode
        if (Input.GetKeyDown("space"))
        {
            if (history.Count > 0)
            {
                //Todo: get history working (look at how rqts did it)
                GoBackScreen();
            }
        }
    }
}

[System.Serializable]
public class UIScreen
{
    public string Name;
    public Transform ScreenObject;
}