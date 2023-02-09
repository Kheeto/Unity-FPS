using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpeedEffect : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float minimumSpeed = 12f;

    [Header("References")]
    [SerializeField] private ParticleSystem speedEffect;
    [SerializeField] private Rigidbody rb;

    private void Start()
    {
        if (speedEffect.isPlaying) speedEffect.Stop();
    }

    private void FixedUpdate()
    {
        if (rb.velocity.magnitude >= minimumSpeed)
        {
            if (!speedEffect.isPlaying) speedEffect.Play();
        }
        else { if (speedEffect.isPlaying) speedEffect.Stop(); }
    }
}
