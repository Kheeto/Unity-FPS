using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PressurePad : MonoBehaviour
{
    [Header("Pressure pad settings")]
    [SerializeField] private List<Door> doors = new List<Door>();

    private List<Rigidbody> pressObjects;

    private void Start()
    {
        pressObjects = new List<Rigidbody>();
    }

    private void OnCollisionEnter(Collision collision)
    {
        Rigidbody rb = collision.gameObject.GetComponent<Rigidbody>();

        if (rb != null)
            pressObjects.Add(rb);

        UpdateDoors();
    }

    private void OnCollisionExit(Collision collision)
    {
        Rigidbody rb = collision.gameObject.GetComponent<Rigidbody>();

        if (rb != null)
            pressObjects.Remove(rb);

        UpdateDoors();
    }

    private void UpdateDoors()
    {
        if (IsPressed())
            foreach (Door d in doors)
                d.ToggleDoor(true);
        else
            foreach (Door d in doors)
                d.ToggleDoor(false);
    }

    public bool IsPressed() {
        return pressObjects.Count > 0;
    }
}
