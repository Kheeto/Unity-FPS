using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponSway : MonoBehaviour
{
    [Header("Weapon Sway")]
    [SerializeField] private float swayAmount = .1f;
    [SerializeField] private float swayMaxAmount = .2f;
    [SerializeField] private float smoothSpeed = 6f;
    private Vector3 initialPosition;

    [Header("Weapon Tilt")]
    [SerializeField] private float tiltAmount = 4f;
    [SerializeField] private float tiltMaxAmount = 6f;
    [SerializeField] private float tiltSpeed = 12f;
    private Quaternion initialRotation;

    [Space]
    [SerializeField] private bool tiltX;
    [SerializeField] private bool tiltY;
    [SerializeField] private bool tiltZ;

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

    /// <summary>
    /// Moves the weapon towards the opposite direction of where the player is looking.
    /// </summary>
    private void HandleSway(float xInput, float yInput)
    {
        float xMovement = Mathf.Clamp(xInput * swayAmount, -swayMaxAmount, swayMaxAmount);
        float yMovement = Mathf.Clamp(yInput * swayAmount, -swayMaxAmount, swayMaxAmount);

        Vector3 finalPosition = new Vector3(xMovement, yMovement, 0f);
        transform.localPosition = Vector3.Lerp(transform.localPosition, finalPosition + initialPosition, smoothSpeed * Time.deltaTime);
    }

    /// <summary>
    /// Tilts the gun depending on the player input;
    /// </summary>
    private void HandleTilt(float xInput, float yInput)
    {
        float yTilt = Mathf.Clamp(xInput * tiltAmount, -tiltMaxAmount, tiltMaxAmount);
        float xTilt = Mathf.Clamp(yInput * tiltAmount, -tiltMaxAmount, tiltMaxAmount);

        Quaternion finalRotation = Quaternion.Euler(new Vector3(
            tiltX ? -xTilt : 0f,
            tiltY ? yTilt : 0f,
            tiltZ ? yTilt : 0f
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
