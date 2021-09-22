using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static PlayerMovementScript;

public class CeilingHandlerScript : MonoBehaviour
{
    private PlayerMovementScript playerScript;
    [SerializeField] private LayerMask playerLayer;
    private bool areCeiling;
    // Start is called before the first frame update
    void Awake()
    {
        playerScript = GetComponentInParent<PlayerMovementScript>();
    }
    public bool GetAreCeiling()
    {
        return areCeiling;
    }
    // Update is called once per frame
    void Update()
    {
        if (playerScript.GetPlayerState().Equals(PlayerState.Crouching))
        {
            transform.localPosition = new Vector3(transform.localPosition.x, 6.7f, transform.localPosition.z);
        }
        else if(!playerScript.GetPlayerState().Equals(PlayerState.Crouching) && !areCeiling)
        {
            transform.localPosition = new Vector3(transform.localPosition.x, 9f, transform.localPosition.z);
        }
        Debug.LogWarning(areCeiling);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.tag.Equals("Player"))
        {
            areCeiling = true;
        }
        else { areCeiling = false; }
    }
    private void OnTriggerStay(Collider other)
    {
        if (!other.tag.Equals("Player"))
        {
            areCeiling = true;
        }
        else { areCeiling = false; }
    }
    private void OnTriggerExit(Collider other)
    {
        areCeiling = false;
    }
    private void OnCollisionExit(Collision collision)
    {
        areCeiling = false;
    }
}

