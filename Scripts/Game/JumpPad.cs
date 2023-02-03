using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JumpPad : MonoBehaviour
{
    [Header("Jump Pad Settings")]
    [SerializeField] private float jumpForce = 1000f;

    private void OnCollisionEnter(Collision collision)
    {
        Rigidbody rb = collision.gameObject.GetComponentInParent<Rigidbody>();
        if (rb != null)
            rb.AddForce(Vector3.up * jumpForce);
    }
}
