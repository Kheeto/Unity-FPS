using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Climbing : MonoBehaviour
{
    [SerializeField] private bool enableClimbing = true;
    
    [Header("Climbing")]
    [SerializeField] private float climbSpeed;
    [SerializeField] private float maxClimbTime;
    [SerializeField] private bool climbTimeLimit;
    private float climbTimer;
    private bool climbing;

    [Header("Climb Jumping")]
    [SerializeField] private float climbJumpUpForce;
    [SerializeField] private float climbJumpBackForce;
    [SerializeField] private KeyCode jumpKey = KeyCode.Space;
    [SerializeField] private int climbJumps;
    private int climbJumpsLeft;
    private Transform lastWall;
    private Vector3 lastWallNormal;

    [Header("Exiting")]
    public bool exitingwall;
    [SerializeField] private float exitWallTime;
    private float exitWallTimer;

    [Header("Detection")]
    [SerializeField] private float detectionLength;
    [SerializeField] private float sphereCastRadius;
    [SerializeField] private float maxWallLookAngle = 30f;
    private float wallLookAngle;
    private RaycastHit frontWallHit;
    private bool wallFront;
    [SerializeField] private float minWallNormalAngleChange;

    [Header("References")]
    [SerializeField] private Transform orientation;
    [SerializeField] private LayerMask whatIsLadder;
    private Rigidbody rb;
    private PlayerMovementAdvanced pm;
    private LedgeGrabbing lg;


    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        pm = GetComponent<PlayerMovementAdvanced>();
        lg = GetComponent<LedgeGrabbing>();
        GetComponent<Climbing>().enabled = enableClimbing;
    }

    // Update is called once per frame
    private void Update()
    {
        WallCheck();
        StateMachine();

        if (climbing && !exitingwall) ClimbingMovement();
    }

    /// <summary>
    /// Checks for a ladder in front of the player with a maximum angle, using a sphere cast.
    /// Also checks if the player is in front of a different wall.
    /// </summary>
    private void WallCheck()
    {
        wallFront = Physics.SphereCast(transform.position,
            sphereCastRadius, orientation.forward,
            out frontWallHit, detectionLength, whatIsLadder);

        wallLookAngle = Vector3.Angle(orientation.forward, -frontWallHit.normal);

        // is it a different wall?
        bool newWall = frontWallHit.transform != lastWall ||
            Mathf.Abs(Vector3.Angle(lastWallNormal, frontWallHit.normal)) > minWallNormalAngleChange;

        if ((wallFront && newWall) || pm.grounded)
        {
            climbTimer = maxClimbTime;
            climbJumpsLeft = climbJumps;
        }
    }

    /// <summary>
    /// Handles the timers and the different Climbing states
    /// </summary>
    private void StateMachine()
    {
        // Ledge Grabbing
        if(lg.holding)
        {
            if (climbing) StopClimbing();
            // everything else gets handled by the SubStateMachine() in LedgeGrabbing.cs
        }

        // Climbing
        else if(wallFront && (Input.GetAxisRaw("Vertical") > 0) && wallLookAngle < maxWallLookAngle && !exitingwall)
        {
            if (!climbing && climbTimer > 0) StartClimbing();

            if (climbTimer > 0) climbTimer -= Time.deltaTime;
            if (climbTimer < 0 && climbTimeLimit) StopClimbing();
        }

        // Exiting
        else if(exitingwall)
        {
            if (climbing) StopClimbing();

            if (exitWallTimer > 0) exitWallTimer -= Time.deltaTime;
            if (exitWallTimer < 0) exitingwall = false;
        }

        else
        {
            if (climbing) StopClimbing();
        }

        // Climb jumping
        if (wallFront && Input.GetKeyDown(jumpKey) && climbJumpsLeft > 0) ClimbJump();
    }

    /// <summary>
    /// Starts climbing and checks if the player touched a new wall.
    /// </summary>
    private void StartClimbing()
    {
        climbing = true;
        pm.climbing = true;

        lastWall = frontWallHit.transform;
        lastWallNormal = frontWallHit.normal;
    }

    /// <summary>
    /// Handles the climbing movement by moving the player up while he presses the forward key and decreasing his horizontal speed.
    /// </summary>
    private void ClimbingMovement()
    {
        rb.velocity = new Vector3(rb.velocity.x, climbSpeed, rb.velocity.z);
        // horizontal speed while climbing is modified by the PlayerMovement script.
    }

    /// <summary>
    /// Stops climbing.
    /// </summary>
    private void StopClimbing()
    {
        climbing = false;
        pm.climbing = false;
    }

    /// <summary>
    /// Stops climbing and makes the player jump off the wall.
    /// </summary>
    private void ClimbJump()
    {
        if (pm.grounded) return;
        if (lg.holding || lg.exitingLedge) return;

        exitingwall = true;
        exitWallTimer = exitWallTime;
        
        Vector3 forceToApply = transform.up * climbJumpUpForce + frontWallHit.normal * climbJumpBackForce;

        // resets y velocity and applies jump force
        rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.y);
        rb.AddForce(forceToApply, ForceMode.Impulse);

        climbJumpsLeft--;
    }
}
