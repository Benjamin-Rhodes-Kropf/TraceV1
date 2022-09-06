using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomDebugManager : MonoBehaviour
{
    public static CustomDebugManager instance;
    [SerializeField]private List<GameObject> _buttons = new List<GameObject>();
    [SerializeField]private List<GameObject> _texts = new List<GameObject>();

    [Header("Debug")]
    public bool showDebugText;
    public bool showDebugButtons;
    

    private void Awake()
    {
        foreach (var gameObject in _buttons)
        {
            gameObject.SetActive(showDebugButtons);
        }
        foreach (var gameObject in _texts)
        {
            gameObject.SetActive(showDebugText);
        }
    }
}
