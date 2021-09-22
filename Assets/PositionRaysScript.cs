using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PositionRaysScript : MonoBehaviour
{

    private PlayerMovementScript PlayerMovement;
    private void Awake()
    {
        PlayerMovement = GameObject.Find("Player").GetComponent<PlayerMovementScript>();
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = new Vector3(PlayerMovement.transform.position.x, PlayerMovement.transform.position.y + 1f, PlayerMovement.transform.position.z);
        var angle = PlayerMovement.GetYRotation();
        if(angle >= 0 && angle <= 90)
        {
            transform.rotation = new Quaternion(0, 90, 0, 0);
        }
        if (angle >= 91 && angle <= 180)
        {
            transform.rotation = new Quaternion(0, 90, 0, 0);
        }
        if (angle >= 181 && angle <= 270)
        {
            transform.rotation = new Quaternion(0, 180, 0, 0);
        }
        if (angle >= 271 && angle <= 360)
        {
            transform.rotation = new Quaternion(0, 270, 0, 0);
        }
    }
}
