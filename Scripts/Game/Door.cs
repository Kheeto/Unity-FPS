using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Door : MonoBehaviour
{
    [Header("Door settings")]
    [SerializeField] private Transform movingPart;
    [SerializeField] private Vector3 openPosition;
    [SerializeField] private Vector3 closedPosition;
    [SerializeField] private float doorSpeed = 1f;
    [SerializeField] private bool open = false;

    private void Update()
    {
        if(open)
            movingPart.transform.localPosition = Vector3.Lerp(
                movingPart.transform.localPosition, openPosition, doorSpeed * Time.deltaTime);
        else
            movingPart.transform.localPosition = Vector3.Lerp(
                movingPart.transform.localPosition, closedPosition, doorSpeed * Time.deltaTime);
    }

    public void ToggleDoor(bool state) { open = state; }
}
