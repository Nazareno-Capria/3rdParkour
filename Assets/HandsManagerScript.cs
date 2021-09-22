using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandsManagerScript : MonoBehaviour
{
    public GameObject LeftHandChecker;
    public GameObject RightHandChecker;
    

    // Update is called once per frame
    void Update()
    {
        
    }

    public bool GetLeftHand()
    {
        return LeftHandChecker.GetComponent<HandCheckerScript>().GetIsTouching();
    }
    public bool GetRightHand()
    {
        return RightHandChecker.GetComponent<HandCheckerScript>().GetIsTouching();
    }
    public Vector3 GetLeftHandPoint()
    {
        return LeftHandChecker.GetComponent<HandCheckerScript>().GetHandPoint();
    }
    public Vector3 GetRightHandPoint()
    {
        return RightHandChecker.GetComponent<HandCheckerScript>().GetHandPoint();
    }
    public ScalableWallScript GetWallScript()
    {
        if (GameObject.ReferenceEquals(LeftHandChecker.GetComponent<HandCheckerScript>().GetWallScript(), RightHandChecker.GetComponent<HandCheckerScript>().GetWallScript())){
            return LeftHandChecker.GetComponent<HandCheckerScript>().GetWallScript();
        }
        else return null;

    }
}
