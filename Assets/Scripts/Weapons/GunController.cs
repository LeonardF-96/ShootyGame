using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.VFX;
using Unity.VisualScripting;
using System;

public class GunController : MonoBehaviour
{
    public Camera fpsCam;
    public VisualEffect muzzleFlash;
    public GameObject impactEffect;
    private Animator animator;

    //public float fireRate = 10f; // Rate of fire
    public float recoilDistance = 0.1f; // The maximum distance the gun can be pushed back
    public float recoilSpeed = 10f; // The speed at which the gun returns to its original position after recoil

    private float nextTimeToFire = 0f;
    private Vector3 originalPosition;

    public int weaponId;
    public string weaponType;
    public int fireRate;
    public float reloadSpeed;
    private bool isReloading = false;
    public int magSize;
    public int currentAmmo;
    public FireMode fireMode;

    void Start()
    {
        Debug.Log($"GunController initialized - weaponId: {weaponId}, FireRate: {fireRate}, ReloadSpeed: {reloadSpeed}, MagSize: {magSize}");
        originalPosition = transform.localPosition;
        currentAmmo = magSize;
        animator = GetComponent<Animator>();
        if (animator != null)
        {
            animator.applyRootMotion = false;
            animator.enabled = false;
        }
    }
    void OnEnable()
    {
        isReloading = false;
    }
    public int GetCurrentAmmo()
    {
        return currentAmmo;
    }

    void Update()
    {
        if (isReloading)
        {
            return;
        }
        if (fireMode == FireMode.Auto && Input.GetButton("Fire1") && Time.time >= nextTimeToFire)
        {
            nextTimeToFire = Time.time + 60f / fireRate;
            Shoot();
        }
        else if (fireMode == FireMode.Single && Input.GetButtonDown("Fire1") && Time.time >= nextTimeToFire)
        {
            nextTimeToFire = Time.time + 60f / fireRate;
            Shoot();
        }
        if (Input.GetKeyDown(KeyCode.R))
        {
            StartCoroutine(Reload());
        }

        // Gradually move the gun back to its original position
        transform.localPosition = Vector3.Lerp(transform.localPosition, originalPosition, Time.deltaTime * recoilSpeed);
    }

    void Shoot()
    {
        Debug.Log($"Shoot method called. Current weapon ID: {weaponId}");

        if (currentAmmo <= 0)
        {
            Debug.Log("Out of ammo");
            return;
        }

        currentAmmo--;
        Debug.Log($"Shooting... Current ammo: {currentAmmo}");

        //// Temporarily disable animator during recoil
        //if (animator != null)
        //{
        //    animator.enabled = false; // Disable animator
        //}

        // Play muzzle flash effect
        if (muzzleFlash != null)
        {
            muzzleFlash.Play();
        }

        // Recoil effect: move the gun slightly backward
        transform.localPosition -= transform.forward * recoilDistance;
        Debug.Log($"New position after recoil: {transform.localPosition}");

        // Create a ray from the camera's center
        RaycastHit hit;
        if (Physics.Raycast(fpsCam.transform.position, fpsCam.transform.forward, out hit))
        {
            Debug.Log("Hit: " + hit.transform.name);

            // Draw a debug line to visualize the raycast (visible in the Scene view)
            Debug.DrawRay(fpsCam.transform.position, fpsCam.transform.forward * hit.distance, Color.red, 2f);

            // Check if the hit object has a Target component and call OnHit
            bool isHeadShot = hit.transform.name.Contains("_head");
            Target target = hit.transform.GetComponent<Target>();
            if (target != null)
            {
                Debug.Log("Target detected, calling OnHit"); // Log to confirm target detection
                target.OnHit(isHeadShot);
            }
            else
            {
                // If no Target component on the hit object, check the parent
                target = hit.transform.GetComponentInParent<Target>();
                if (target != null)
                {
                    Debug.Log("Target detected on parent object, calling OnHit"); // Log to confirm target detection on parent
                    target.OnHit(isHeadShot);
                }
                else
                {
                    Debug.Log("No Target component found on hit object or its parent."); // Log if no target is found
                    GameManager.instance.DeductScore(1);
                }
            }

            // Instantiate impact effect at the hit point
            if (impactEffect != null)
            {
                // Ensure the impact effect is oriented correctly
                Quaternion impactRotation = Quaternion.LookRotation(hit.normal);
                GameObject impactGO = Instantiate(impactEffect, hit.point, impactRotation);
                Destroy(impactGO, 2f); // Destroy impact effect after 2 seconds
            }
        }
        else
        {
            // Deduct points if no hit is detected
            Debug.Log("No hit detected. Deducting 1 point.");
            GameManager.instance.DeductScore(1);
        }
        //// Re-enable animator after recoil (next frame or after some delay)
        //if (animator != null)
        //{
        //    animator.enabled = true; // Re-enable animator
        //}
    }

    IEnumerator Reload()
    {
        //if (animator != null)
        //{
        //    if (!animator.enabled) animator.enabled = true; // Ensure animator is active
        //    animator.Play("Reload", 0, 0f); // Start the reload animation from the beginning
        //    animator.SetFloat("ReloadSpeed", 1f / reloadSpeed);
        //    animator.SetBool("ReloadComplete", false); // Ensure the reload animation doesn't exit early
        //}

        isReloading = true;
        Debug.Log($"Reloading... (Duration: {reloadSpeed} seconds)");

        // Get reference to the reload bar Image from GameManager
        Image reloadBarImage = GameManager.instance.ReloadIndicatorImage;
        RectTransform barRectTransform = reloadBarImage.GetComponent<RectTransform>();

        reloadBarImage.enabled = true;
        float elapsedTime = 0f;

        // Gradually update the reload bar while reloading
        while (elapsedTime < reloadSpeed)
        {
            elapsedTime += Time.deltaTime;

            // Calculate the fill amount based on the elapsed time
            float fillAmount = Mathf.Lerp(1, 0, elapsedTime / reloadSpeed);

            // Update the scale of the image (shrink it vertically)
            barRectTransform.localScale = new Vector3(1, fillAmount, 1);

            yield return null;
        }

        // Reset the reload bar to full once reload is complete
        barRectTransform.localScale = new Vector3(1, 1, 1);

        reloadBarImage.enabled = false;
        currentAmmo = magSize;
        isReloading = false;
        Debug.Log("Reloaded");
        // Signal animation to finish
        // Let animation finish, then disable the animator again
        //if (animator != null)
        //{
        //    animator.SetBool("ReloadComplete", true);
        //    yield return new WaitForSeconds(0.1f); // small buffer to let the animation transition
        //    animator.enabled = false; // turn off the animator until next reload
        //}
    }

    // Method to update the weapon stats dynamically
    public void UpdateWeaponStats(int newFireRate, int newReloadSpeed, int newMagSize)
    {
        fireRate = newFireRate;
        reloadSpeed = newReloadSpeed;
        magSize = newMagSize;

        Debug.Log($"Weapon stats updated - FireRate: {fireRate}, ReloadSpeed: {reloadSpeed}, MagSize: {magSize}");
    }
    public void ApplyWeaponData(WeaponResponse weapon)
    {
        weaponId = weapon.weaponId;
        weaponType = weapon.name;
        fireRate = weapon.fireRate > 0 ? weapon.fireRate : 1; // Prevent divide by zero
        reloadSpeed = weapon.reloadSpeed;
        magSize = weapon.magSize;
        fireMode = weapon.fireMode;

        Debug.Log($"Applied weapon data: ID={weaponId}, Type={weaponType}, FireRate={fireRate}, ReloadSpeed={reloadSpeed}, MagSize={magSize}, FireMode={fireMode}");
    }
}