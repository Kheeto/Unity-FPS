using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DraggableObject : MonoBehaviour
{
    [Header("Object Dragging")]
    [SerializeField] private float dragSpeed = 800f;

    private Rigidbody rb;
    private Vector3 mouseOffset;
    private float mZcoord;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void OnMouseDown()
    {
        mZcoord = Camera.main.WorldToScreenPoint(gameObject.transform.position).z;
        mouseOffset = gameObject.transform.position - GetMouseWorldPos();

        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.useGravity = false;
    }

    private void OnMouseDrag()
    {
        Vector3 dragPosition = GetMouseWorldPos() + mouseOffset;
        Vector3 moveDirection = dragPosition - transform.position;

        rb.velocity = moveDirection * dragSpeed * Time.deltaTime;
        rb.angularVelocity = Vector3.zero;
    }

    private void OnMouseUp()
    {
        rb.useGravity = true;
    }

    private Vector3 GetMouseWorldPos()
    {
        Vector3 mousePos = Input.mousePosition;
        mousePos.z = mZcoord;
        return Camera.main.ScreenToWorldPoint(mousePos);
    }
}
