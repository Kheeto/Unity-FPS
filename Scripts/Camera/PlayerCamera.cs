using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class PlayerCamera : MonoBehaviour
{
    [SerializeField] private float sensX;
    [SerializeField] private float sensY;
    [SerializeField] private Transform orientation;
    [SerializeField] private Transform cameraHolder;

    private float xRotation, yRotation;

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        float mouseX = Input.GetAxis("Mouse X") * sensX * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * sensY * Time.deltaTime;

        yRotation += mouseX;
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        // applies the new rotation
        cameraHolder.rotation = Quaternion.Euler(xRotation, yRotation, 0);
        orientation.rotation = Quaternion.Euler(0, yRotation, 0);
    }

    /// <summary>
    /// Smoothly highers or lowers the camera fov depending on the input values.
    /// </summary>
    /// <param name="endValue">The desired FOV</param>
    /// <param name="transitionTime">Time in seconds the transition will take</param>
    public void HandleFov(float endValue, float transitionTime = 0.25f)
    {
        GetComponent<Camera>().DOFieldOfView(endValue, transitionTime);
    }

    /// <summary>
    /// Smoothly tilts the camera on the z axis depending on the input values.
    /// </summary>
    /// <param name="zTilt">The desired camera Tilt</param>
    /// <param name="transitionTime">Time in seconds the transition will take</param>
    public void HandleTilt(float zTilt, float transitionTime = 0.25f)
    {
        transform.DOLocalRotate(new Vector3(0, 0, zTilt), transitionTime);
    }
}
