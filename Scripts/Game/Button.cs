using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Button : MonoBehaviour
{
    [Header("Button settings")]
    [SerializeField] private List<Door> doors = new List<Door>();

    private void OnMouseDown()
    {
        foreach (Door door in doors)
        {
            door.ToggleDoor(true);
        }
    }
}
