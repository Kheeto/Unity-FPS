using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Button : MonoBehaviour
{
    [SerializeField] private List<Door> doors = new List<Door>();

    [Header("Button animation")]
    [SerializeField] private Vector3 defaultPosition;
    [SerializeField] private Vector3 pressedPosition;
    [SerializeField] private float clickSpeed = 1f;

    private bool pressed = false;

    private void Start()
    {
        pressed = false;
        transform.localPosition = defaultPosition;
    }

    private void OnMouseDown()
    {
        pressed = true;

        foreach (Door door in doors)
        {
            door.ToggleDoor(true);
        }
    }

    private void Update()
    {
        if (pressed)
            transform.localPosition = Vector3.Lerp(transform.localPosition, pressedPosition,
                clickSpeed * Time.deltaTime);
    }
}
