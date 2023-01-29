using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Swinging : MonoBehaviour
{
    [Header("Swinging")]
    [SerializeField] private float maxSwingDistance = 25f;
    [SerializeField] private float jointSpring = 4.5f;
    [SerializeField] private float jointDamper = 7f;
    [SerializeField] private float jointMassScale = 4.5f;
    private Vector3 swingPoint;
    //private Vector3 currentGrapplePosition;
    [HideInInspector] public SpringJoint joint;
    [HideInInspector] public bool swinging;

    [Header("Odm Gear")]
    [SerializeField] private float horizontalThrustForce;
    [SerializeField] private float forwardThrustForce;
    [SerializeField] private float extendCableSpeed;

    [Header("Prediction")]
    [SerializeField] private float predictionSphereCastRadius;
    [SerializeField] private Transform predictionPoint;
    private RaycastHit predictionHit;

    [Header("Input")]
    [SerializeField] private KeyCode swingKey = KeyCode.Mouse1;

    [Header("References")]
    private Rigidbody rb;
    [SerializeField] private Transform orientation, gunTip, camera, player;
    [SerializeField] private LayerMask whatIsGrappeable;
    private PlayerMovementAdvanced pm;

    private void Start()
    {
        pm = GetComponent<PlayerMovementAdvanced>();
        rb = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        if (Input.GetKeyDown(swingKey)) StartSwing();
        if (Input.GetKeyUp(swingKey)) StopSwing();

        CheckForSwingPoints();

        if (joint != null) OdmGearMovement();
    }

    /// <summary>
    /// Starts a swing and sets up the joint.
    /// </summary>
    private void StartSwing()
    {
        if (predictionHit.point == Vector3.zero) return;

        // deactivate active grapple
        if(GetComponent<Grappling>() != null)
            GetComponent<Grappling>().StopGrapple();
        pm.ResetRestrictions();

        swinging = true;
        pm.swinging = true;

        // sets up the joint
        swingPoint = predictionHit.point;
        joint = player.gameObject.AddComponent<SpringJoint>();
        joint.autoConfigureConnectedAnchor = false;
        joint.connectedAnchor = swingPoint;

        // the distance the grapple will try to keep
        float distanceFromPoint = Vector3.Distance(player.position, swingPoint);
        joint.maxDistance = distanceFromPoint * .8f;
        joint.minDistance = distanceFromPoint * .25f;

        joint.spring = jointSpring;
        joint.damper = jointDamper;
        joint.massScale = jointMassScale;
    }
    
    /// <summary>
    /// Lets the player move the cable and shorten or extend it.
    /// </summary>
    private void OdmGearMovement()
    {
        // go right
        if (Input.GetKey(KeyCode.D)) rb.AddForce(orientation.right * horizontalThrustForce * Time.deltaTime);
        // go left
        if (Input.GetKey(KeyCode.A)) rb.AddForce(-orientation.right * horizontalThrustForce * Time.deltaTime);
        // go forward
        if (Input.GetKey(KeyCode.W)) rb.AddForce(orientation.forward * forwardThrustForce * Time.deltaTime);

        // shorten cable
        if (Input.GetKey(KeyCode.Space))
        {
            Vector3 directionToPoint = swingPoint - transform.position;
            rb.AddForce(directionToPoint.normalized * forwardThrustForce * Time.deltaTime);

            float distanceFromPoint = Vector3.Distance(transform.position, swingPoint);

            joint.maxDistance = distanceFromPoint * .8f;
            joint.minDistance = distanceFromPoint * .25f;
        }

        // shorten cable
        if(Input.GetKey(KeyCode.S))
        {
            float extendedDistanceFromPoint = Vector3.Distance(transform.position, swingPoint) + extendCableSpeed;
            joint.maxDistance = extendedDistanceFromPoint * .8f;
            joint.minDistance = extendedDistanceFromPoint * .25f;
        }
    }

    /// <summary>
    /// Checks for the closest point the player can use to swing for aim prediction.
    /// </summary>
    private void CheckForSwingPoints()
    {
        if (joint != null) return; // if you are already swinging don't check for new points

        RaycastHit sphereCastHit;
        Physics.SphereCast(camera.position, predictionSphereCastRadius, camera.forward,
            out sphereCastHit, maxSwingDistance, whatIsGrappeable);

        RaycastHit raycastHit;
        Physics.Raycast(camera.position, camera.forward,
            out raycastHit, maxSwingDistance, whatIsGrappeable);

        Vector3 realHitPoint;
        // Option 1 - Direct hit
        if (raycastHit.point != Vector3.zero)
            realHitPoint = raycastHit.point;
        // Option 2 - Indirect (predicted) hit
        else if (sphereCastHit.point != Vector3.zero)
            realHitPoint = sphereCastHit.point;
        // Option 3 - Miss
        else
            realHitPoint = Vector3.zero;

        // prediction point found
        if(realHitPoint != Vector3.zero)
        {
            predictionPoint.gameObject.SetActive(true);
            predictionPoint.position = realHitPoint;
        } else { // not found
            predictionPoint.gameObject.SetActive(false);
        }

        predictionHit = raycastHit.point == Vector3.zero ? sphereCastHit : raycastHit;
    }

    /// <summary>
    /// Stops swinging and disables the joint and the rope.
    /// </summary>
    public void StopSwing()
    {
        swinging = false;
        pm.swinging = false;

        Destroy(joint);
    }

    public Vector3 GetSwingPoint() { return swingPoint; }
}
