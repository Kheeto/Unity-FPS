using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeathFloor : MonoBehaviour
{
    [Header("Rising Floor")]
    [SerializeField] private float speed;

    [Header("References")]
    [SerializeField] private Transform player;
    [SerializeField] private MenuManager menuManager;

    private void Update() {
        transform.position = new Vector3(
            transform.position.x,
            transform.position.y + speed * Time.deltaTime,
            transform.position.z);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.transform == player)
        {
            menuManager.ShowDeathMenu();
        }
    }
}
