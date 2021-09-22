using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScalableWallScript : MonoBehaviour
{
    // Start is called before the first frame update
    private float topHeight;


    // Update is called once per frame
    void Update()
    {
        
    }

    public float GetTopLimit()
    {
        topHeight = this.gameObject.GetComponent<Collider>().bounds.max.y;
        return topHeight;
    }
}
