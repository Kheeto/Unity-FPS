using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovementAdvanced : MonoBehaviour
{
    [Header("Movement")]
    private float moveSpeed;
    private float desiredMoveSpeed;
    private float lastDesiredMoveSpeed;
    [SerializeField] private float walkSpeed;
    [SerializeField] private float sprintSpeed;
    [SerializeField] private float slideSpeed;
    [SerializeField] private float wallrunSpeed;
    [SerializeField] private float climbSpeed;
    [SerializeField] private float dashSpeed;
    [SerializeField] private float dashSpeedChangeFactor;
    [SerializeField] private float swingSpeed;
    private float speedChangeFactor;

    [SerializeField] private float speedIncreaseMultiplier;
    [SerializeField] private float slopeIncreaseMultiplier;

    [SerializeField] private float groundDrag;
    [HideInInspector] public float maxYSpeed;

    [Header("Jumping")]
    [SerializeField] private float jumpForce;
    [SerializeField] private float jumpCooldown;
    [SerializeField] private float airMultiplier;
    private bool readyToJump;

    [Header("Crouching")]
    [SerializeField] private float crouchSpeed;
    [SerializeField] private float crouchYScale;
    private float startYScale;

    [Header("Keybinds")]
    [SerializeField] private KeyCode jumpKey = KeyCode.Space;
    [SerializeField] private KeyCode sprintKey = KeyCode.LeftShift;
    [SerializeField] private KeyCode crouchKey = KeyCode.LeftControl;

    [Header("Ground Check")]
    [SerializeField] private float playerHeight;
    [SerializeField] private LayerMask whatIsGround;
    public bool grounded;

    [Header("Slope Handling")]
    [SerializeField] private float maxSlopeAngle;
    private RaycastHit slopeHit;
    private bool exitingSlope;

    [Header("Camera Effects")]
    [SerializeField] private PlayerCamera playerCamera;
    [SerializeField] private float normalFov = 80f;
    [SerializeField] private float sprintFov = 85f;
    [SerializeField] private float slideFov = 85f;
    [SerializeField] private float grappleFov = 90f;
    [SerializeField] private float dashFov = 90f;

    [Header("References")]
    [SerializeField] private Transform orientation;
    private Vector3 moveDirection;
    private Rigidbody rb;
    private Climbing climbingScript;

    // input and movement state management
    private float horizontalInput, verticalInput;
    private bool keepMomentum;

    // movements states for other scripts
    [HideInInspector] public bool sliding;
    [HideInInspector] public bool wallrunning;
    [HideInInspector] public bool climbing;
    [HideInInspector] public bool freeze;
    [HideInInspector] public bool unlimited;
    [HideInInspector] public bool restricted;
    [HideInInspector] public bool dashing;
    [HideInInspector] public bool activeGrapple;
    [HideInInspector] public bool swinging;
    [HideInInspector] public MovementState state;
    [HideInInspector] public MovementState lastState;

    /// <summary>
    /// What type of movement is the player currently performing?
    /// </summary>
    public enum MovementState
    {
        freeze,
        unlimited,
        walking,
        sprinting,
        wallrunning,
        climbing,
        crouching,
        sliding,
        dashing,
        air,
        grappling,
        swinging
    }

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
        readyToJump = true;
        startYScale = transform.localScale.y;
        climbingScript = GetComponent<Climbing>();
        speedChangeFactor = speedIncreaseMultiplier;
    }

    private void Update()
    {
        // ground check
        grounded = Physics.Raycast(transform.position, Vector3.down, playerHeight * 0.5f + 0.2f, whatIsGround);

        MyInput();
        SpeedControl();
        StateHandler();

        // handles drag
        if (grounded && state != MovementState.dashing && !activeGrapple)
            rb.drag = groundDrag;
        else
            rb.drag = 0;
    }

    private void FixedUpdate()
    {
        MovePlayer();
    }

    /// <summary>
    /// Checks for the player input used for movement
    /// </summary>
    private void MyInput()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");

        // when to jump
        if(Input.GetKey(jumpKey) && readyToJump && grounded && !swinging)
        {
            readyToJump = false;
            Jump();

            // cooldown before being able to jump again
            Invoke(nameof(ResetJump), jumpCooldown);
        }

        // start crouch
        if (Input.GetKeyDown(crouchKey))
        {
            transform.localScale = new Vector3(transform.localScale.x, crouchYScale, transform.localScale.z);
            rb.AddForce(Vector3.down * 5f, ForceMode.Impulse);
        }

        // stop crouch
        if (Input.GetKeyUp(crouchKey))
        {
            transform.localScale = new Vector3(transform.localScale.x, startYScale, transform.localScale.z);
        }
    }

    /// <summary>
    /// Handles the different Movement States of the player in different ways
    /// </summary>
    private void StateHandler()
    {
        // swinging
        if(swinging)
        {
            state = MovementState.swinging;
            if(!grounded) desiredMoveSpeed = swingSpeed;
            else if (grounded && Input.GetKey(sprintKey)) desiredMoveSpeed = sprintSpeed;
            else desiredMoveSpeed = walkSpeed;
        }

        // grappling
        else if(activeGrapple)
        {
            state = MovementState.grappling;
            desiredMoveSpeed = sprintSpeed;
        }

        // dashing
        else if (dashing)
        {
            state = MovementState.dashing;
            desiredMoveSpeed = dashSpeed;
            speedChangeFactor = dashSpeedChangeFactor;
        }
        
        // freeze
        else if (freeze)
        {
            state = MovementState.freeze;
            rb.velocity = Vector3.zero;
            desiredMoveSpeed = 0f;
        }

        // unlimited
        else if (unlimited)
        {
            state = MovementState.unlimited;
            desiredMoveSpeed = 999f;
            return;
        }

        // climbing
        else if (climbing)
        {
            state = MovementState.climbing;
            desiredMoveSpeed = climbSpeed;
        }

        // wallrunning
        else if (wallrunning)
        {
            state = MovementState.wallrunning;
            desiredMoveSpeed = wallrunSpeed;
        }

        // sliding
        else if (sliding)
        {
            state = MovementState.sliding;

            // increases speed by 1 every second
            if (OnSlope()) // && rb.velocity.y < 0.1f
            {
                desiredMoveSpeed = slideSpeed;
                keepMomentum = true;
                playerCamera.HandleFov(slideFov, .2f);
            }
            else
                desiredMoveSpeed = sprintSpeed;
        }

        // crouching
        else if (Input.GetKey(crouchKey))
        {
            state = MovementState.crouching;
            desiredMoveSpeed = crouchSpeed;
        }

        // sprinting
        else if(grounded && Input.GetKey(sprintKey))
        {
            state = MovementState.sprinting;
            desiredMoveSpeed = sprintSpeed;
            playerCamera.HandleFov(sprintFov, .2f);
        }

        // walking
        else if (grounded)
        {
            state = MovementState.walking;
            desiredMoveSpeed = walkSpeed;
        }

        // air movement
        else
        {
            if(!wallrunning) state = MovementState.air;

            if (desiredMoveSpeed < sprintSpeed)
                desiredMoveSpeed = walkSpeed;
            else
                desiredMoveSpeed = sprintSpeed;
        }

        if (grounded && !dashing && !Input.GetKey(sprintKey) && !activeGrapple && !sliding)
            playerCamera.HandleFov(normalFov, .2f);

        bool desiredMoveSpeedHasChanged = desiredMoveSpeed != lastDesiredMoveSpeed;
        if (lastState == MovementState.dashing) keepMomentum = true;

        if(desiredMoveSpeedHasChanged)
        {
            if (keepMomentum)
            {
                StopAllCoroutines();
                StartCoroutine(SmoothlyLerpMoveSpeed());
            }
            else {
                StopAllCoroutines();
                moveSpeed = desiredMoveSpeed; 
            }
        }
        lastDesiredMoveSpeed = desiredMoveSpeed;
        lastState = state;

        if (Mathf.Abs(desiredMoveSpeed - moveSpeed) < .1f) keepMomentum = false;
    }

    /// <summary>
    /// Lerps the current movement speed to the desired one smoothly.
    /// </summary>
    /// <returns>Always returns null.</returns>
    private IEnumerator SmoothlyLerpMoveSpeed()
    {
        float time = 0;
        float difference = Mathf.Abs(desiredMoveSpeed - moveSpeed);
        float startValue = moveSpeed;

        float boostFactor = speedChangeFactor;

        while (time < difference)
        {
            moveSpeed = Mathf.Lerp(startValue, desiredMoveSpeed, time / difference);

            if (OnSlope())
            {
                float slopeAngle = Vector3.Angle(Vector3.up, slopeHit.normal);
                float slopeAngleIncrease = 1 + (slopeAngle / 90f);

                time += Time.deltaTime * boostFactor * slopeIncreaseMultiplier * slopeAngleIncrease;
            }
            else
                time += Time.deltaTime * boostFactor;

            yield return null;
        }

        moveSpeed = desiredMoveSpeed;
        speedChangeFactor = speedIncreaseMultiplier;
        keepMomentum = false;
    }

    /// <summary>
    /// Applies player movement and physics.
    /// Call this in LateUpdate() so the frame rate doesn't affect movement.
    /// </summary>
    private void MovePlayer()
    {
        if (restricted) return;
        if (climbingScript.exitingwall) return;
        if (activeGrapple) return;
        if (swinging) return;
        if (state == MovementState.dashing) return;
        
        // calculate movement direction
        moveDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;

        // on slope
        if (OnSlope() && !exitingSlope)
        {
            rb.AddForce(GetSlopeMoveDirection(moveDirection) * moveSpeed * 20f, ForceMode.Force);

            if (rb.velocity.y > 0)
                rb.AddForce(Vector3.down * 80f, ForceMode.Force);
        }

        // on ground
        else if(grounded)
            rb.AddForce(moveDirection.normalized * moveSpeed * 10f, ForceMode.Force);

        // in air
        else if(!grounded)
            rb.AddForce(moveDirection.normalized * moveSpeed * 10f * airMultiplier, ForceMode.Force);

        // turn gravity off while on slope
        if(!wallrunning) rb.useGravity = !OnSlope();
    }

    /// <summary>
    /// Makes sure the speed does not exceed the limit it should reach and changes it based on the movement state.
    /// </summary>
    private void SpeedControl()
    {
        if (activeGrapple) return;

        // limiting speed on slope
        if (OnSlope() && !exitingSlope)
        {
            if (rb.velocity.magnitude > moveSpeed)
                rb.velocity = rb.velocity.normalized * moveSpeed;
        }

        // limiting speed on ground or in air
        else
        {
            Vector3 flatVel = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

            // limit velocity if needed
            if (flatVel.magnitude > moveSpeed)
            {
                Vector3 limitedVel = flatVel.normalized * moveSpeed;
                rb.velocity = new Vector3(limitedVel.x, rb.velocity.y, limitedVel.z);
            }
        }

        // limit y vel
        if(maxYSpeed != 0 && rb.velocity.y > maxYSpeed)
                rb.velocity = new Vector3(rb.velocity.x, maxYSpeed, rb.velocity.z);
    }

    /// <summary>
    /// Resets y velocity and applies vertical force to the player to make him jump.
    /// </summary>
    private void Jump()
    {
        exitingSlope = true;

        // reset y velocity
        rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

        rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
    }

    /// <summary>
    /// Makes the player able to jump again. Should be invoked after cooldown ends.
    /// </summary>
    private void ResetJump()
    {
        readyToJump = true;
        exitingSlope = false;
    }

    #region Grappling

    private bool enableMovementOnNextTouch;
    /// <summary>
    /// Pushes the player to the hook point when grappling.
    /// </summary>
    public void JumpToPosition(Vector3 targetPosition, float trajectoryHeight)
    {
        activeGrapple = true;
        
        velocityToSet = CalculateJumpVelocity(transform.position, targetPosition, trajectoryHeight);
        Invoke(nameof(SetVelocity), .1f);
        Invoke(nameof(ResetRestrictions), 3f);
    }

    private Vector3 velocityToSet;
    private void SetVelocity()
    {
        enableMovementOnNextTouch = true;
        rb.velocity = velocityToSet;

        playerCamera.HandleFov(grappleFov, .2f);
    }

    public void ResetRestrictions() { 
        activeGrapple = false;
        playerCamera.HandleFov(normalFov, .5f);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if(enableMovementOnNextTouch)
        {
            enableMovementOnNextTouch = false;
            ResetRestrictions();
            GetComponent<Grappling>().StopGrapple();
        }
    }

    #endregion

    /// <returns>Is the player currently on a slope?</returns>
    public bool OnSlope()
    {
        if(Physics.Raycast(transform.position, Vector3.down, out slopeHit, playerHeight * 0.5f + 0.3f))
        {
            float angle = Vector3.Angle(Vector3.up, slopeHit.normal);
            return angle < maxSlopeAngle && angle != 0;
        }

        return false;
    }

    /// <summary>
    /// Calculates the direction the player should move to when moving on a slope.
    /// </summary>
    /// <param name="direction">The current movement direction.</param>
    /// <returns>The desired direction.</returns>
    public Vector3 GetSlopeMoveDirection(Vector3 direction)
    {
        return Vector3.ProjectOnPlane(direction, slopeHit.normal).normalized;
    }

    /// <summary>
    /// Calculates the force to push the player exactly to the hook point using Kinematic equations.
    /// </summary>
    /// <returns>The force to add to the player rigidbody.</returns>
    public Vector3 CalculateJumpVelocity(Vector3 startPoint, Vector3 endPoint, float trajectoryHeight)
    {
        float gravity = Physics.gravity.y;
        float displacementY = endPoint.y - startPoint.y;
        Vector3 displacementXZ = new Vector3(endPoint.x - startPoint.x, 0f, endPoint.z - startPoint.z);

        Vector3 velocityY = Vector3.up * Mathf.Sqrt(-2 * gravity * trajectoryHeight);
        Vector3 velocityXZ = displacementXZ / (Mathf.Sqrt(-2 * trajectoryHeight / gravity)
            + Mathf.Sqrt(2 * (displacementY - trajectoryHeight) / gravity));

        return velocityXZ + velocityY;
    }
}
