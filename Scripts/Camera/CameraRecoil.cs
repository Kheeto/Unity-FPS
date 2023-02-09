using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraRecoil : MonoBehaviour
{
    [Header("Recoil Settings")]
    [SerializeField] private Vector3 recoil = new Vector3(-2f, 1f, 2f);
    [SerializeField] private Vector3 returnSpeed = new Vector3(1f, 3f, 5f);
    [SerializeField] private float roughness = 13f;

    private Vector3 currentRot;
    private Vector3 targetRot;

    private void Update()
    {
        targetRot = new Vector3(
            Mathf.Lerp(targetRot.x, 0f, returnSpeed.x * Time.deltaTime),
            Mathf.Lerp(targetRot.y, 0f, returnSpeed.y * Time.deltaTime),
            Mathf.Lerp(targetRot.z, 0f, returnSpeed.z * Time.deltaTime)
        );
        currentRot = Vector3.Slerp(currentRot, targetRot, roughness * Time.deltaTime);

        transform.localRotation = Quaternion.Euler(currentRot);
    }

    public void ApplyRecoil()
    {
        targetRot += new Vector3(recoil.x, Random.Range(-recoil.y, recoil.y), Random.Range(-recoil.z, recoil.z));
    }
}
