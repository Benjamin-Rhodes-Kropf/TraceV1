using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class SlideToAnimScript : MonoBehaviour
{
    [SerializeField] private Transform _start;
    [SerializeField] private Transform _finish;
    [SerializeField] private float _duration = 1;
    private void Awake()
    {
        transform.position = _start.position;
    }

    public void SlideToFinish()
    {
        transform.DOMoveY(_finish.position.y, _duration);
    }

    public void SlideToStart()
    {
        transform.DOMoveY(_start.position.y, _duration);
    }
}
