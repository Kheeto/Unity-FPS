using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndPortal : MonoBehaviour
{
    [SerializeField] private Transform player;

    private void OnTriggerEnter(Collider other)
    {
        if (other.transform.parent == player)
        {
            // end level
        }
    }
}
