using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grappling : MonoBehaviour
{
    [Header("Grappling")]
    [SerializeField] private float maxGrappleDistance;
    [SerializeField] private float grappleDelayTime;
    [SerializeField] private float overshootYAxis;
    [SerializeField] private LayerMask whatIsGrappeable;
    private Vector3 grapplePoint;
    [HideInInspector] public bool grappling;

    [Header("Cooldown")]
    [SerializeField] private float cooldown;
    private float cooldownTimer;

    [Header("Input")]
    [SerializeField] private KeyCode grappleKey = KeyCode.Mouse0;

    [Header("References")]
    [SerializeField] private Transform camera;
    [SerializeField] private Transform gunTip;
    private PlayerMovementAdvanced pm;

    private void Start()
    {
        pm = GetComponent<PlayerMovementAdvanced>();
    }

    private void Update()
    {
        if (Input.GetKeyDown(grappleKey)) StartGrapple();

        if (cooldownTimer > 0) cooldownTimer -= Time.deltaTime;
    }

    /// <summary>
    /// Throws the grappling hook and checks if it has hit something, in that case the grappling will be executed after a short delay.
    /// </summary>
    private void StartGrapple()
    {
        if (cooldownTimer > 0) return;

        GetComponent<Swinging>().StopSwing();
        grappling = true;
        //pm.freeze = true;

        RaycastHit hit;
        if(Physics.Raycast(camera.position, camera.forward, out hit, maxGrappleDistance, whatIsGrappeable))
        {
            grapplePoint = hit.point;
            Invoke(nameof(ExecuteGrapple), grappleDelayTime);
        }
        else
        {
            grapplePoint = camera.position + camera.forward * maxGrappleDistance;
            Invoke(nameof(StopGrapple), grappleDelayTime);
        }
    }

    /// <summary>
    /// Pulls the player to the grapple point.
    /// </summary>
    private void ExecuteGrapple()
    {
        //pm.freeze = false;

        Vector3 lowestPoint = new Vector3(transform.position.x, transform.position.y - 1f, transform.position.z);
        float grapplePointRelativeYPos = grapplePoint.y - lowestPoint.y;
        float highestPointOnArc = grapplePointRelativeYPos + overshootYAxis;

        if (grapplePointRelativeYPos < 0) highestPointOnArc = overshootYAxis;

        pm.JumpToPosition(grapplePoint, highestPointOnArc);
        Invoke(nameof(StopGrapple), 1f);
    }

    /// <summary>
    /// Stops the grappling.
    /// </summary>
    public void StopGrapple()
    {
        pm.freeze = false;
        grappling = false;
        cooldownTimer = cooldown;
    }

    public Vector3 GetGrapplePoint() { return grapplePoint; }
}
