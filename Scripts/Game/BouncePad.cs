using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BouncePad : MonoBehaviour
{
    [Header("Bounce Pad Settings")]
    [SerializeField] private float overshootY = 1f;
    [SerializeField] private Transform targetPoint;

    private void OnCollisionEnter(Collision collision)
    {
        PlayerMovementAdvanced pm = collision.gameObject.GetComponentInParent<PlayerMovementAdvanced>();
        if (pm != null)
            Bounce(pm);
    }

    /// <summary>
    /// Pulls the player to a certain position.
    /// </summary>
    private void Bounce(PlayerMovementAdvanced pm)
    {
        Vector3 lowestPoint = new Vector3(transform.position.x, transform.position.y - 1f, transform.position.z);
        float bouncePointRelativeYPos = targetPoint.position.y - lowestPoint.y;
        float highestPointOnArc = bouncePointRelativeYPos + overshootY;

        if (bouncePointRelativeYPos < 0) highestPointOnArc = overshootY;

        pm.JumpToPosition(targetPoint.position, highestPointOnArc);
    }
}
