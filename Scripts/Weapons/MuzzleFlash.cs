using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MuzzleFlash : MonoBehaviour
{
    [SerializeField] private float duration;

    private void OnEnable()
    {
        Invoke(nameof(EndMuzzleFlash), duration);
    }

    /// <summary>
    /// Destroys the muzzleflash object after a certain duration.
    /// </summary>
    private void EndMuzzleFlash()
    {
        Destroy(gameObject);
    }
}
