using UnityEngine;
using TMPro;
using EZCameraShake;
using System.Collections.Generic;

public class Gun : MonoBehaviour
{
    [Header("Shooting")]
    [SerializeField] private int bulletDamage;
    [SerializeField] private float bulletSpread;
    [SerializeField] private float gunRange;
    [SerializeField] private float reloadTime;
    [SerializeField] private float fireRate;
    [Tooltip("Time between each bullet is shot if bulletsPerTap > 1")]
    [SerializeField] private float timeBetweenBullets;
    [SerializeField] private LayerMask whatIsEnemy;
    private RaycastHit rayHit;
    private bool shooting, readyToShoot, reloading, recoilAnim;

    [Header("Ammo")]
    [SerializeField] private bool infiniteAmmo;
    [SerializeField] private int magazineSize;
    [SerializeField] private int bulletsPerTap;
    [SerializeField] private bool automatic;
    [SerializeField] TextMeshProUGUI ammoText;
    private int bulletsLeft, bulletsShot;

    [Header("Effects")]
    [SerializeField] private bool cameraShake;
    [SerializeField] private float shakeMagnitude;
    [SerializeField] private float shakeRoughness;
    [SerializeField] private float fadeInTime;
    [SerializeField] private float fadeOutTime;

    [Header("References")]
    [SerializeField] private Camera camera;
    [SerializeField] private CameraRecoil recoil;
    [SerializeField] private Transform gunMuzzle;
    [SerializeField] private GameObject muzzleFlash, bulletHoleGraphic;
    [SerializeField] private Animator animator;

    [Header("Audio")]
    [SerializeField] private List<AudioSource> shootSounds = new List<AudioSource>();

    private void Awake()
    {
        bulletsLeft = magazineSize;
        readyToShoot = true;
    }
    private void Update()
    {
        MyInput();

        if (!infiniteAmmo) ammoText.SetText(bulletsLeft.ToString());

        if (animator != null)
        {
            animator.SetBool("shooting", recoilAnim);
            animator.SetBool("reloading", reloading);
        }
    }

    /// <summary>
    /// Handles the shoot and reload input of the player.
    /// </summary>
    private void MyInput()
    {
        if (automatic) shooting = Input.GetKey(KeyCode.Mouse0);
        else shooting = Input.GetKeyDown(KeyCode.Mouse0);

        if (Input.GetKeyDown(KeyCode.R) && bulletsLeft < magazineSize && !reloading) Reload();

        //Shoot
        if (readyToShoot && shooting && !reloading)
        {
            if (!infiniteAmmo && bulletsLeft <= 0) return;

            bulletsShot = bulletsPerTap;
            Shoot();
        }
    }

    /// <summary>
    /// Shoots both a physical bullet and a raycast which calculates the correct path and where the bullet should land.
    /// Also checks if an enemy has been hit and applies camera effects.
    /// </summary>
    private void Shoot()
    {
        readyToShoot = false;

        // calculates spread direction
        float x = Random.Range(-bulletSpread, bulletSpread);
        float y = Random.Range(-bulletSpread, bulletSpread);
        Vector3 direction = camera.transform.forward + new Vector3(x, y, 0);

        // checks if the bullet hit something
        if (Physics.Raycast(camera.transform.position, direction, out rayHit, gunRange, whatIsEnemy))
        {
            if (rayHit.collider.GetComponent<Enemy>())
                rayHit.collider.GetComponent<Enemy>().TakeDamage(bulletDamage);
        }

        // visual effects
        if (cameraShake) 
            CameraShaker.Instance.ShakeOnce(shakeMagnitude, shakeRoughness, fadeInTime, fadeOutTime);
        if (bulletHoleGraphic != null)
        {
            Instantiate(bulletHoleGraphic,
                rayHit.point + (rayHit.normal * .01f),
                Quaternion.FromToRotation(Vector3.up, rayHit.normal));
        }
        if (muzzleFlash != null)
            Instantiate(muzzleFlash, gunMuzzle.position, camera.transform.rotation, gunMuzzle);

        bulletsLeft--;
        bulletsShot--;
        Invoke(nameof(ResetShot), fireRate);

        if (bulletsShot > 0 && bulletsLeft > 0)
            Invoke(nameof(Shoot), timeBetweenBullets);

        recoilAnim = true;
        Invoke(nameof(ResetRecoilAnimation), .1f);

        shootSounds[Random.Range(0, shootSounds.Count)].Play();

        recoil.ApplyRecoil();
    }
    private void ResetShot()
    {
        readyToShoot = true;
    }

    private void Reload()
    {
        reloading = true;
        Invoke("ReloadFinished", reloadTime);
    }

    private void ResetRecoilAnimation()
    {
        recoilAnim = false;
    }

    private void ReloadFinished()
    {
        bulletsLeft = magazineSize;
        reloading = false;
    }
}
