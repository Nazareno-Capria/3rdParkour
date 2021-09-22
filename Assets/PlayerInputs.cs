using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInputs : MonoBehaviour
{
    // Start is called before the first frame update
    public Vector2 moveInput
    {
        get
        {
            Vector2 moveInput = Vector2.zero;
            moveInput.x = Input.GetAxis("Horizontal");
            moveInput.y = Input.GetAxis("Vertical");
            return Vector2.ClampMagnitude(moveInput, 1f);
        }
    }

    public bool running { get { return Input.GetKey(KeyCode.LeftShift); } }

    public bool crouching { get { return Input.GetKeyDown(KeyCode.LeftControl); } }

    public bool jump { get { return Input.GetKeyDown(KeyCode.Space); } }
}
