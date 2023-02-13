using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Missile : MonoBehaviour
{
    [Header("Homing Missile")]
    [SerializeField] private float thrust = 5f;
    [SerializeField] private float turnSpeed = 10f;
    [SerializeField] private float maxRange = 500f;

    [Header("Prediciton")]
    [SerializeField] private float minPredictionDistance = 5f;
    [SerializeField] private float maxPredictionTime = 5f;

    private Vector3 prediction;
    private Rigidbody rb;
    private Rigidbody target;
    private Transform player;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();

        player = PlayerMovementAdvanced.Instance.transform;
        target = PlayerMovementAdvanced.Instance.GetRigidbody();
    }

    private void FixedUpdate()
    {
        rb.AddForce(transform.forward * thrust * Time.fixedDeltaTime);

        // Predict player's movement
        float leadTimePercentage = Mathf.InverseLerp(minPredictionDistance, maxRange,
            Vector3.Distance(transform.position, player.position));
        float predictionTime = Mathf.Lerp(0f, maxPredictionTime, leadTimePercentage);
        prediction = target.position + target.velocity * predictionTime;

        // Rotate missile
        Quaternion targetRotation = Quaternion.LookRotation(player.position);
        rb.MoveRotation(Quaternion.RotateTowards(transform.rotation, targetRotation,
            turnSpeed * Time.fixedDeltaTime));
    }

    private void OnDrawGizmos()
    {
        if (!Application.isPlaying) return;

        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, player.position);

        Gizmos.color = Color.blue;
        Gizmos.DrawLine(player.position, prediction);

        Gizmos.color = Color.green;
        Gizmos.DrawLine(transform.position, prediction);
    }
}
