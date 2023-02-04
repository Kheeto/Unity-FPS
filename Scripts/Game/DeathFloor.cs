using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeathFloor : MonoBehaviour
{
    [SerializeField] private Transform player;
    [SerializeField] private MenuManager menuManager;

    private void OnTriggerEnter(Collider other)
    {
        if (other.transform == player)
        {
            menuManager.ShowDeathMenu();
        }
    }
}
