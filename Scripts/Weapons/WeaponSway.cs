using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponSway : MonoBehaviour
{
    [Header("Weapon Sway")]
    [SerializeField] private float lookSwayMultiplier = .1f;
    [SerializeField] private float lookSwayMax = .2f;
    [SerializeField] private float movementSwayMultiplier = .01f;
    [SerializeField] private float movementSwayMax = .3f;
    [SerializeField] private float movementSwayLerp = 2f;
    [SerializeField] private float smoothSpeed = 6f;
    private Vector3 initialPosition;

    [Header("Weapon Tilt")]
    [SerializeField] private float lookTiltAmount = 4f;
    [SerializeField] private float movementTiltAmount = .5f;
    [SerializeField] private float tiltMaxAmount = 6f;
    [SerializeField] private float tiltSpeed = 12f;
    private Quaternion initialRotation;
    [Space]
    [SerializeField] private bool tiltX;
    [SerializeField] private bool tiltY;
    [SerializeField] private bool tiltZ;

    [Header("References")]
    [SerializeField] private Rigidbody rb;

    private void Awake()
    {
        initialPosition = transform.localPosition;
        initialRotation = transform.localRotation;
    }

    private void Update()
    {
        Vector2 input = GetInput();
        HandleSway(input.x, input.y);
        HandleTilt(input.x, input.y);
    }

    private Vector3 movementSway;
    /// <summary>
    /// Moves the weapon towards the opposite direction of where the player is looking and moving
    /// </summary>
    private void HandleSway(float xInput, float yInput)
    {
        // Look sway
        float xLook = Mathf.Clamp(xInput * lookSwayMultiplier, -lookSwayMax, lookSwayMax);
        float yLook = Mathf.Clamp(yInput * lookSwayMultiplier, -lookSwayMax, lookSwayMax);

        // Movement sway
        Vector3 localVelocity = transform.InverseTransformDirection(rb.velocity);
        movementSway.x = Mathf.Lerp(movementSway.x, Mathf.Clamp(localVelocity.x * movementSwayMultiplier,
            -movementSwayMax, movementSwayMax), movementSwayLerp * Time.deltaTime);
        movementSway.y = Mathf.Lerp(movementSway.y, Mathf.Clamp(localVelocity.y * movementSwayMultiplier,
            -movementSwayMax, movementSwayMax), movementSwayLerp * Time.deltaTime);
        movementSway.z = Mathf.Lerp(movementSway.z, Mathf.Clamp(localVelocity.z * movementSwayMultiplier,
            -movementSwayMax, movementSwayMax), movementSwayLerp * Time.deltaTime);

        Vector3 finalPosition = new Vector3(
            movementSway.x + xLook,
            movementSway.y + yLook,
            movementSway.z);
        transform.localPosition = Vector3.Lerp(transform.localPosition, finalPosition + initialPosition, smoothSpeed * Time.deltaTime);
    }

    /// <summary>
    /// Tilts the gun depending on the player input;
    /// </summary>
    private void HandleTilt(float xInput, float yInput)
    {
        // Look tilt
        float yLookTilt = Mathf.Clamp(xInput * lookTiltAmount, -tiltMaxAmount, tiltMaxAmount);
        float xLookTilt = -Mathf.Clamp(yInput * lookTiltAmount, -tiltMaxAmount, tiltMaxAmount);

        // Movement tilt
        Vector3 localVelocity = transform.InverseTransformDirection(rb.velocity);
        float xMovement = Mathf.Clamp(localVelocity.y * movementTiltAmount,
            -tiltMaxAmount, tiltMaxAmount) + Mathf.Clamp(localVelocity.z * movementTiltAmount,
            -tiltMaxAmount, tiltMaxAmount);
        float zMovement = -Mathf.Clamp(localVelocity.x * movementTiltAmount,
            -tiltMaxAmount, tiltMaxAmount);

        Vector3 tilt = new Vector3(
            xMovement + xLookTilt,
            yLookTilt,
            zMovement);

        Quaternion finalRotation = Quaternion.Euler(new Vector3(
            tiltX ? tilt.x : 0f,
            tiltY ? tilt.y : 0f,
            tiltZ ? tilt.z : 0f
        ));
        transform.localRotation = Quaternion.Slerp(transform.localRotation, finalRotation * initialRotation, tiltSpeed * Time.deltaTime);
    }

    /// <summary>
    /// Returns the opposite of the player's X and Y mouse input.
    /// </summary>
    /// <returns>A Vector2 containing the X and Y values.</returns>
    private Vector2 GetInput()
    {
        float xInput = -Input.GetAxis("Mouse X");
        float yInput = -Input.GetAxis("Mouse Y");

        return new Vector2(xInput, yInput);
    }

}
