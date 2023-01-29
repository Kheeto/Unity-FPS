using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LedgeGrabbing : MonoBehaviour
{
    [SerializeField] private bool enableLedgeGrabbing = true;

    [Header("Ledge Grabbing")]
    [SerializeField] private float moveToLedgeSpeed;
    [SerializeField] private float maxLedgeGrabDistance;
    [SerializeField] private float minTimeOnLedge;
    private float timeOnLedge;
    [HideInInspector] public bool holding;

    [Header("Ledge Jumping")]
    [SerializeField] private KeyCode jumpKey = KeyCode.Space;
    [SerializeField] private float ledgeJumpForwardForce;
    [SerializeField] private float ledgeJumpUpwardForce;

    [Header("Exiting")]
    [HideInInspector] public bool exitingLedge;
    [SerializeField] private float exitLedgeTime;
    private float exitLedgeTimer;

    [Header("Detection")]
    [SerializeField] private float ledgeDetectionLength;
    [SerializeField] private float ledgeSphereCastRadius;
    [SerializeField] private LayerMask whatIsLedge;
    private Transform lastLedge, currLedge;
    private RaycastHit ledgeHit;

    [Header("References")]
    private PlayerMovementAdvanced pm;
    [SerializeField] private Transform orientation;
    [SerializeField] private Transform camera;
    private Rigidbody rb;

    private void Start()
    {
        pm = GetComponent<PlayerMovementAdvanced>();
        rb = GetComponent<Rigidbody>();
        GetComponent<LedgeGrabbing>().enabled = enableLedgeGrabbing;
    }

    private void Update()
    {
        CheckForLedge();
        SubStateMachine();
    }

    /// <summary>
    /// Handles all the different states of the ledge grabbing.
    /// </summary>
    private void SubStateMachine() {
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");
        bool anyInputKeyPressed = horizontal != 0 || vertical != 0;

        // Holding state
        if(holding)
        {
            FreezeRigidbodyOnLedge();

            timeOnLedge += Time.deltaTime;
            if (timeOnLedge > minTimeOnLedge && anyInputKeyPressed) ExitLedgeHold();

            if (Input.GetKeyDown(jumpKey)) LedgeJump();
        }

        // Exiting state
        else if (exitingLedge)
        {
            if (exitLedgeTimer > 0) exitLedgeTimer -= Time.deltaTime;
            else exitingLedge = false;
        }
    }

    /// <summary>
    /// Continuously checks if the player can use a ledge.
    /// </summary>
    private void CheckForLedge()
    {
        bool ledgeDetected = Physics.SphereCast(transform.position, ledgeSphereCastRadius, camera.forward,
            out ledgeHit, ledgeDetectionLength, whatIsLedge);

        if (!ledgeDetected) return;

        float distanceToLedge = Vector3.Distance(transform.position, ledgeHit.transform.position);

        if (ledgeHit.transform == lastLedge) return;

        if (distanceToLedge < maxLedgeGrabDistance && !holding) EnterLedgeHold();
    }

    /// <summary>
    /// Jumps off a ledge hold.
    /// </summary>
    private void LedgeJump()
    {
        ExitLedgeHold();

        Invoke(nameof(ApplyJumpForce), .05f);
    }

    /// <summary>
    /// Jumps off a ledge hold.
    /// </summary>
    private void ApplyJumpForce()
    {
        Vector3 forceToAdd = camera.forward * ledgeJumpForwardForce + orientation.up * ledgeJumpUpwardForce;
        rb.velocity = Vector3.zero;
        rb.AddForce(forceToAdd, ForceMode.Impulse);
    }

    /// <summary>
    /// Starts holding a ledge, disables gravity and stops all momentum.
    /// </summary>
    private void EnterLedgeHold()
    {
        holding = true;
        pm.unlimited = true;
        pm.restricted = true;

        currLedge = ledgeHit.transform;
        lastLedge = ledgeHit.transform;

        rb.useGravity = false;
        rb.velocity = Vector3.zero;
    }

    /// <summary>
    /// Handles the player physics while on a ledge hold.
    /// </summary>
    private void FreezeRigidbodyOnLedge()
    {
        rb.useGravity = false;

        Vector3 directionToLedge = currLedge.position - transform.position;
        float distanceToLedge = Vector3.Distance(transform.position, currLedge.position);

        // Moves player towards ledge
        if(distanceToLedge > 1f)
        {
            if (rb.velocity.magnitude < moveToLedgeSpeed)
                rb.AddForce(directionToLedge.normalized * moveToLedgeSpeed * 1000f * Time.deltaTime);
        }

        // Hold onto ledge
        else
        {
            if (!pm.freeze) pm.freeze = true;
            if (pm.unlimited) pm.unlimited = false;
        }

        // Exits ledge hold if player gets far away
        if (distanceToLedge > maxLedgeGrabDistance) ExitLedgeHold();
    }

    /// <summary>
    /// Exits a ledge hold and resets the player movement.
    /// </summary>
    private void ExitLedgeHold()
    {
        // starts the timer
        holding = false;
        timeOnLedge = 0f;
        exitingLedge = true;
        exitLedgeTimer = exitLedgeTime;

        pm.restricted = false;
        pm.freeze = false;
        rb.useGravity = true;

        StopAllCoroutines();
        Invoke(nameof(ResetLastLedge), 1f);
    }

    private void ResetLastLedge() {
        lastLedge = null;
    }
}
