using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class CameraController : MonoBehaviour
{
    [SerializeField] private CinemachineFreeLook cinemaCamera;
    [SerializeField] private float zoomVelocity;
    [SerializeField] private float minFov;
    [SerializeField] private float maxFov;

    private void Awake()
    {
        cinemaCamera.m_CommonLens = true;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
    // Update is called once per frame
    void Update()
    {
        if(Input.GetAxis("Mouse ScrollWheel") > 0 && cinemaCamera.m_Lens.FieldOfView > minFov)
        {

            cinemaCamera.m_Lens.FieldOfView -= Input.GetAxis("Mouse ScrollWheel") * zoomVelocity;
        }
        if (Input.GetAxis("Mouse ScrollWheel") < 0 && cinemaCamera.m_Lens.FieldOfView < maxFov)
        {
            
            cinemaCamera.m_Lens.FieldOfView -= Input.GetAxis("Mouse ScrollWheel") * zoomVelocity;
        }
    }
}
