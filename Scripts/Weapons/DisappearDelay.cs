using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DisappearDelay : MonoBehaviour
{
    [SerializeField] private float duration;

    private void OnEnable()
    {
        Invoke(nameof(Disappear), duration);
    }

    /// <summary>
    /// Destroys the gameobject object after a certain duration.
    /// </summary>
    private void Disappear()
    {
        Destroy(gameObject);
    }
}
