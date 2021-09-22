using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FloorCheckerScript : MonoBehaviour
{
    // Start is called before the first frame update
    private float floorHeight;
    private bool isGrounded;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void UpdateFloorHeight(float y) 
    {
        floorHeight = y;
    }
    public void UpdateIsGrounded(bool grounded)
    {
        isGrounded = grounded;
    }
    public float GetFloorHeight()
    {
        return floorHeight;
    }
    public bool GetGrounded()
    {
        return isGrounded;
    }
    private void OnTriggerEnter(Collider other)
    {
        UpdateIsGrounded(true);
    }
    private void OnTriggerStay(Collider other)
    {
        UpdateIsGrounded(true);
    }
    private void OnTriggerExit(Collider other)
    {
        UpdateIsGrounded(false);
    }
}
