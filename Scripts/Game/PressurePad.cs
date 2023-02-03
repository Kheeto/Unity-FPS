using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PressurePad : MonoBehaviour
{
    [Header("Pressure pad settings")]
    [SerializeField] private bool pressed;
    [SerializeField] private List<Door> doors = new List<Door>();

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.GetComponent<Rigidbody>() != null)
            pressed = true;

        foreach (Door d in doors)
            d.ToggleDoor(true);
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.GetComponent<Rigidbody>() != null)
            pressed = false;

        foreach (Door d in doors)
            d.ToggleDoor(false);
    }

    public bool IsPressed() { return pressed; }
}
