using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandCheckerScript : MonoBehaviour
{
    private bool isTouching;
    private Vector3 handPoint;
    private ScalableWallScript WallScript;
    // Start is called before the first frame update
    private void Awake()
    {
        isTouching = false;

    }

    public bool GetIsTouching()
    {
        return isTouching;
    }
    public Vector3 GetHandPoint()
    {
        return handPoint;
    }
    public ScalableWallScript GetWallScript()
    {
        return WallScript;
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("ScalableWall"))
        {
            isTouching = true;
            WallScript = other.GetComponent<ScalableWallScript>();
            handPoint = other.ClosestPoint(new Vector3(transform.position.x, other.bounds.max.y, transform.position.z));
        }
    }
    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("ScalableWall"))
        {
            isTouching = true;
            WallScript = other.GetComponent<ScalableWallScript>();
        }
    }
    private void OnTriggerExit(Collider other)
    {
        isTouching = false;
        
    }
}
