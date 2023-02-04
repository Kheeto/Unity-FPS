using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sliding : MonoBehaviour
{
    [Header("Sliding movement")]
    [SerializeField] private float slideSpeedDrain = .5f;
    [SerializeField] private float slideSpeedGain = 1f;
    [SerializeField] private float slideMoveForce = 10f;
    [SerializeField] private float slideDownForce = 3000f;
    [SerializeField] private float slideYScale = .4f;
    private float startYScale;

    [Header("Time Limit")]
    [SerializeField] private bool slideTimeLimit = false;
    [SerializeField] private float maxSlideTime = 5f;
    private float slideTimer;

    [Header("References")]
    [SerializeField] private Transform orientation;
    [SerializeField] private Transform playerObj;
    private Rigidbody rb;
    private PlayerMovementAdvanced pm;

    [Header("Input")]
    [SerializeField] private KeyCode slideKey = KeyCode.LeftControl;
    private float horizontalInput;
    private float verticalInput;

    private bool wasOnSlope;


    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        pm = GetComponent<PlayerMovementAdvanced>();

        startYScale = playerObj.localScale.y;
    }

    private void Update()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");

        if (pm.swinging || pm.activeGrapple) return;

        if (Input.GetKeyDown(slideKey) && (horizontalInput != 0 || verticalInput != 0))
            StartSlide();

        if (Input.GetKeyUp(slideKey) && pm.sliding)
            StopSlide();
    }

    private void FixedUpdate()
    {
        if (pm.sliding)
            SlidingMovement();
    }

    /// <summary>
    /// Starts the slide and scales down the player.
    /// </summary>
    private void StartSlide()
    {
        pm.sliding = true;

        playerObj.localScale = new Vector3(playerObj.localScale.x, slideYScale, playerObj.localScale.z);
        rb.AddForce(Vector3.down * 5f, ForceMode.Impulse);

        slideTimer = maxSlideTime;

        pm.currentSlideSpeed = pm.slideGroundSpeed;
    }

    /// <summary>
    /// Handles the movement while sliding and builds speed on slopes.
    /// </summary>
    private void SlidingMovement()
    {
        Vector3 inputDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;

        // sliding on a flat surface
        if (!pm.OnSlope())
        {
            rb.AddForce(inputDirection.normalized * slideMoveForce, ForceMode.Force);

            pm.currentSlideSpeed = Mathf.Lerp(pm.currentSlideSpeed, 0f, slideSpeedDrain * Time.deltaTime);

            if (slideTimeLimit) slideTimer -= Time.deltaTime;
        }

        // sliding down a slope
        else
        {
            rb.AddForce(pm.GetSlopeMoveDirection(inputDirection) * slideMoveForce, ForceMode.Force);
            rb.AddForce(Vector3.down * Time.deltaTime * slideDownForce);

            pm.currentSlideSpeed = Mathf.Lerp(pm.currentSlideSpeed, pm.slideSlopeSpeed, slideSpeedGain * Time.deltaTime);
        }

        if (slideTimer <= 0)
            StopSlide();

        Debug.Log(rb.velocity.magnitude);
    }

    /// <summary>
    /// Stops the slide and scales the player back.
    /// </summary>
    private void StopSlide()
    {
        pm.sliding = false;

        playerObj.localScale = new Vector3(playerObj.localScale.x, startYScale, playerObj.localScale.z);
    }
}
