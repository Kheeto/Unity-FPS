using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateGun : MonoBehaviour
{
    [SerializeField] private PlayerMovementAdvanced playerScript;
    [SerializeField] private Grappling grapplingScript;
    [SerializeField] private Swinging swingScript;
    private Quaternion desiredRotation;
    private float rotationSpeed = 5f;

    private void Update()
    {
        if(playerScript.swinging)
            desiredRotation = Quaternion.LookRotation(swingScript.GetSwingPoint() - transform.position);

        else if(playerScript.activeGrapple)
            desiredRotation = Quaternion.LookRotation(grapplingScript.GetGrapplePoint() - transform.position);

        else
            desiredRotation = transform.parent.rotation;

        transform.rotation = Quaternion.Lerp(transform.rotation, desiredRotation, Time.deltaTime * rotationSpeed);
    }

}
