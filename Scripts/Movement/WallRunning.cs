using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallRunning : MonoBehaviour
{
    [Header("Wallrunning")]
    [SerializeField] private LayerMask whatIsWall;
    [SerializeField] private LayerMask whatIsGround;
    [SerializeField] private float wallRunForce;
    [SerializeField] private float wallJumpUpForce;
    [SerializeField] private float wallJumpSideForce;
    [SerializeField] private float wallClimbSpeed;
    [SerializeField] private float maxWallRunTime;
    [SerializeField] private bool wallRunLimit;
    private float wallRunTimer;

    [Header("Input")]
    [SerializeField] private KeyCode wallJumpKey = KeyCode.Space;
    [SerializeField] private KeyCode upwardsRunKey = KeyCode.LeftShift;
    [SerializeField] private KeyCode downwardsRunKey = KeyCode.LeftControl;
    private bool upwardsRunning;
    private bool downwardsRunning;
    private float horizontalInput;
    private float verticalInput;

    [Header("Detection")]
    [SerializeField] private float wallCheckDistance;
    [SerializeField] private float minJumpHeight;
    private RaycastHit leftWallhit;
    private RaycastHit rightWallhit;
    private bool wallLeft;
    private bool wallRight;

    [Header("Exiting")]
    private bool exitingWall;
    [SerializeField] private float exitWallTime;
    private float exitWallTimer;

    [Header("Gravity")]
    [SerializeField] private bool useGravity;
    [Range(0, 9.81f)]
    [SerializeField] private float gravityCounterForce;

    [Header("Camera effects")]
    private float defaultFov;
    [SerializeField] private float wallRunFov = 90f;
    [SerializeField] private float cameraTilt = 15f;
    [SerializeField] private float transitionTime = .25f;

    [Header("References")]
    [SerializeField] private Transform orientation;
    [SerializeField] private PlayerCamera camera;
    private Rigidbody rb;
    private PlayerMovementAdvanced pm;
    private LedgeGrabbing lg;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        pm = GetComponent<PlayerMovementAdvanced>();
        lg = GetComponent<LedgeGrabbing>();
        wallRunTimer = maxWallRunTime;
        defaultFov = camera.GetComponent<Camera>().fieldOfView;
    }

    private void Update()
    {
        CheckForWall();
        StateMachine();
    }

    private void FixedUpdate()
    {
        if (pm.wallrunning)
            WallRunningMovement();
    }

    /// <summary>
    /// Manages the inputs to start and stop a Wallrun.
    /// </summary>
    private void StateMachine()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");

        upwardsRunning = Input.GetKey(upwardsRunKey);
        downwardsRunning = Input.GetKey(downwardsRunKey);

        // Wallrunning state
        if((wallLeft || wallRight) && verticalInput > 0 && AboveGround() && !exitingWall)
        {
            if (!pm.wallrunning)
                StartWallRun();

            // wallrun timer
            if (wallRunLimit) {
                if (wallRunTimer > 0)
                    wallRunTimer -= Time.deltaTime;

                if (wallRunTimer <= 0 && pm.wallrunning);
                {
                    exitingWall = true;
                    exitWallTimer = exitWallTime;
                }
            }
            
            // wall jumping
            if (Input.GetKeyDown(wallJumpKey)) WallJump();
        }

        // Exiting state
        else if(exitingWall)
        {
            if (pm.wallrunning)
                StopWallRun();

            if (exitWallTimer > 0)
                exitWallTimer -= Time.deltaTime;

            if (exitWallTimer <= 0)
                exitingWall = false;
        }

        // Default state
        else
        {
            if (pm.wallrunning)
                StopWallRun();
        }
    }

    /// <summary>
    /// Starts the wallrun and applies camera effects.
    /// </summary>
    private void StartWallRun()
    {
        pm.wallrunning = true;
        wallRunTimer = maxWallRunTime;
        
        rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

        // cool camera effects
        camera.HandleFov(wallRunFov, transitionTime);
        if (wallLeft) camera.HandleTilt(-cameraTilt, transitionTime);
        if (wallRight) camera.HandleTilt(cameraTilt, transitionTime);
    }

    /// <summary>
    /// Manages the wallrunning movement and physics.
    /// </summary>
    private void WallRunningMovement()
    {
        rb.useGravity = useGravity;

        Vector3 wallNormal = wallRight ? rightWallhit.normal : leftWallhit.normal;

        Vector3 wallForward = Vector3.Cross(wallNormal, transform.up);

        if ((orientation.forward - wallForward).magnitude > (orientation.forward - -wallForward).magnitude)
            wallForward = -wallForward;

        // forward force
        rb.AddForce(wallForward * wallRunForce, ForceMode.Force);

        // up/down force
        if (upwardsRunning)
            rb.velocity = new Vector3(rb.velocity.x, wallClimbSpeed, rb.velocity.z);
        if (downwardsRunning)
            rb.velocity = new Vector3(rb.velocity.x, -wallClimbSpeed, rb.velocity.z);

        // push the player to the wall so he doesn't fall
        if (!(wallLeft && horizontalInput > 0) && !(wallRight && horizontalInput < 0))
            rb.AddForce(-wallNormal * 100, ForceMode.Force);

        // lowers gravity (if it's enabled)
        if (useGravity)
            rb.AddForce(transform.up * gravityCounterForce, ForceMode.Force);
    }

    /// <summary>
    /// Stops a wallrun, including wallrun movement and physics. Also resets camera effects.
    /// </summary>
    private void StopWallRun()
    {
        pm.wallrunning = false;

        // reset camera effects
        camera.HandleFov(defaultFov, transitionTime);
        camera.HandleTilt(0f, transitionTime);
    }

    /// <summary>
    /// Disables the wallrun and jumps off the wall. Can be used to jump to another wall or to the ground.
    /// </summary>
    private void WallJump()
    {
        if (lg.holding || lg.exitingLedge) return;
        
        // makes sure the player can exit the wall
        exitingWall = true;
        exitWallTimer = exitWallTime;

        Vector3 wallNormal = wallRight ? rightWallhit.normal : leftWallhit.normal;
        Vector3 forceToApply = transform.up * wallJumpUpForce + wallNormal * wallJumpSideForce;

        // jump force
        rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.y);
        rb.AddForce(forceToApply, ForceMode.Impulse);
    }

    /// <summary>
    /// Checks if a wall is close enough to the player so he can start a Wallrun.
    /// </summary>
    private void CheckForWall()
    {
        wallRight = Physics.Raycast(transform.position, orientation.right, out rightWallhit, wallCheckDistance, whatIsWall);
        wallLeft = Physics.Raycast(transform.position, -orientation.right, out leftWallhit, wallCheckDistance, whatIsWall);
    }

    /// <returns>Is the player above the ground?</returns>
    private bool AboveGround()
    {
        return !Physics.Raycast(transform.position, Vector3.down, minJumpHeight, whatIsGround);
    }
}
