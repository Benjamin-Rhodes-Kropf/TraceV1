using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwipeManager : MonoBehaviour
{
    //swipe
    [SerializeField]private float changeInX;
    [SerializeField]private float xMouseValue;
    [SerializeField] private bool _mouseDown;
    [SerializeField]private float mouseDownTime;
    [SerializeField] private int _currentScreenId;
    [SerializeField] private int _numberOfScreens;
    [SerializeField] private float swipeMultiplier;
    [SerializeField] private bool _isCurrentlySwitchingScreens;
    // Start is called before the first frame update
    void Start()
    {

    }
    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            _mouseDown = true;
        }
        if (Input.GetMouseButtonUp(0))
        {
            _mouseDown = false;
        }
        
        
        
        
        
        if (Input.GetMouseButtonDown(0) & !_isCurrentlySwitchingScreens)
        {
            xMouseValue = Input.mousePosition.x; // starting mouse pos for offset
            mouseDownTime = Time.time;//decide between click and drag
        }
        //friction
        changeInX *= 0.96f;

        if (Input.GetMouseButtonUp(0) && Time.time-mouseDownTime < 0.4f && Mathf.Abs(changeInX)<15)
        {
            Debug.Log("mouse clicked");
        }
        
        if (_mouseDown && Time.time - mouseDownTime > 0.4f)
        {
            Debug.Log("mouse is dragging");
        }

        if (Input.mousePosition.x > 0 && Input.GetMouseButton(0) && !_isCurrentlySwitchingScreens)
        {
            changeInX = Input.mousePosition.x - xMouseValue;
        }

        // if (changeInX > 120 && !_isCurrentlySwitchingScreens && Input.GetMouseButton(0) && _currentScreenId > 1)
        // {
        //     StartCoroutine(switchingFocusedScreen());
        //     _currentScreenId -= 1;
        // }
        // if (changeInX < -120 && !_isCurrentlySwitchingScreens && Input.GetMouseButton(0) && _currentScreenId < _numberOfScreens)
        // {
        //     StartCoroutine(switchingFocusedScreen());
        //     _currentScreenId += 1; 
        // }

        
        
        // transform.position = new Vector3(transform.position.x+changeInX/1000,transform.position.y,transform.position.z);
        // transform.position = Vector3.Lerp(transform.transform.position, focusedScreen, Time.deltaTime * 12);
    }
    
    IEnumerator switchingFocusedScreen()
    {
        
        if (_isCurrentlySwitchingScreens) { yield return null; }
        _isCurrentlySwitchingScreens = true;
        changeInX =0;
        yield return new WaitForSeconds(0.25f);
        changeInX = 0;
        mouseDownTime = 0;
        _isCurrentlySwitchingScreens = false;
    }
}
