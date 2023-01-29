using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dashing : MonoBehaviour
{
    [SerializeField] private bool enableDashing;

    [Header("Dashing")]
    [SerializeField] private float forwardForce;
    [SerializeField] private float upwardForce;
    [SerializeField] private float dashDuration;
    [SerializeField] private float maxDashYSpeed;
    private Vector3 forceToApply;

    [Header("Settings")]
    [SerializeField] private bool useCameraForward = true;
    [SerializeField] private bool allowAllDirections = true;
    [SerializeField] private bool disableGravity = false;
    [SerializeField] private bool resetVel = true;

    [Header("Cooldown")]
    [SerializeField] private float cooldown;
    private float cooldownTimer;

    [Header("Input")]
    [SerializeField] private KeyCode dashKey = KeyCode.Tab;

    [Header("References")]
    [SerializeField] private Transform orientation;
    [SerializeField] private Transform camera;
    private Rigidbody rb;
    private PlayerMovementAdvanced pm;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        pm = GetComponent<PlayerMovementAdvanced>();
        GetComponent<Dashing>().enabled = enableDashing;
    }

    private void Update()
    {
        if (Input.GetKeyDown(dashKey))
            Dash();

        if (cooldownTimer > 0) cooldownTimer -= Time.deltaTime;
    }
    
    /// <summary>
    /// Pushes the player to make him Dash using the specified force.
    /// </summary>
    private void Dash()
    {
        if (pm.wallrunning) return;
        if (pm.climbing) return;
        if (pm.freeze) return;
        if (pm.unlimited) return;
        if (cooldownTimer > 0) return; // cooldown still active
        else cooldownTimer = cooldown; // resets cooldown

        pm.dashing = true; // no drag
        pm.maxYSpeed = maxDashYSpeed;

        camera.GetComponent<PlayerCamera>().HandleFov(85, .2f);

        // calculates the correct direction based on settings
        Transform forwardT;
        if (useCameraForward) forwardT = camera;
        else forwardT = orientation;
        Vector3 direction = GetDirection(forwardT);

        forceToApply = direction * forwardForce + orientation.up * upwardForce;
        if (disableGravity) rb.useGravity = false;
        
        Invoke(nameof(Push), .025f);
        Invoke(nameof(ResetDash), dashDuration);
    }

    /// <summary>
    /// Applies the dash force to the player.
    /// </summary>
    private void Push()
    {
        if (resetVel)
            rb.velocity = Vector3.zero;

        rb.AddForce(forceToApply, ForceMode.Impulse);
    }

    /// <summary>
    /// Resets the dash ability after the cooldown.
    /// </summary>
    private void ResetDash()
    {
        pm.dashing = false;
        pm.maxYSpeed = 0;

        camera.GetComponent<PlayerCamera>().HandleFov(80, .2f);

        if (disableGravity) rb.useGravity = true;
    }

    /// <summary>
    /// Calculates the correct direction of a dash.
    /// </summary>
    /// <param name="forwardT">The transform it will get the forward direction from.</param>
    /// <returns>The normalized direction the dash force should use.</returns>
    private Vector3 GetDirection(Transform forwardT)
    {
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");

        // sets the direction based on settings and input
        Vector3 direction = new Vector3();
        if (allowAllDirections)
            direction = forwardT.forward * vertical + forwardT.right * horizontal;
        else
            direction = forwardT.forward;
        if (vertical == 0 && horizontal == 0) direction = forwardT.forward;

        return direction.normalized;
    }
}
