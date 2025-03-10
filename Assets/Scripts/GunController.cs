using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.VFX;
using Unity.VisualScripting;
using static UnityEngine.GraphicsBuffer;

public class GunController : MonoBehaviour
{
    public Camera fpsCam;
    public VisualEffect muzzleFlash;
    public GameObject impactEffect;

    public float fireRate = 10f; // Rate of fire
    public float recoilDistance = 0.1f; // The maximum distance the gun can be pushed back
    public float recoilSpeed = 10f; // The speed at which the gun returns to its original position after recoil

    private float nextTimeToFire = 0f;
    private Vector3 originalPosition;

    void Start()
    {
        originalPosition = transform.localPosition;
    }

    void Update()
    {
        if (Input.GetButton("Fire1") && Time.time >= nextTimeToFire)
        {
            nextTimeToFire = Time.time + 1f / fireRate;
            Shoot();
        }
        // Gradually move the gun back to its original position
        transform.localPosition = Vector3.Lerp(transform.localPosition, originalPosition, Time.deltaTime * recoilSpeed);
    }

    void Shoot()
    {
        // Play muzzle flash effect
        if (muzzleFlash != null)
        {
            muzzleFlash.Play();
        }

        // Recoil effect: move the gun slightly backward
        transform.localPosition -= transform.forward * recoilDistance;

        // Create a ray from the camera's center
        RaycastHit hit;
        if (Physics.Raycast(fpsCam.transform.position, fpsCam.transform.forward, out hit))
        {
            Debug.Log("Hit: " + hit.transform.name);

            // Draw a debug line to visualize the raycast (visible in the Scene view)
            Debug.DrawRay(fpsCam.transform.position, fpsCam.transform.forward * hit.distance, Color.red, 2f);

            // Check if the hit object has a Target component and call OnHit
            Target target = hit.transform.GetComponent<Target>();
            if (target != null)
            {
               Debug.Log("Target hit: " + target.name);
                target.OnHit();
            }
            else
            {
                // If no Target component on the hit object, check the parent
                target = hit.transform.GetComponentInParent<Target>();
                if (target != null)
                {
                    Debug.Log("Target detected on parent object, calling OnHit"); // Log to confirm target detection on parent
                    target.OnHit();
                }
                else
                {
                    Debug.Log("No Target component found on hit object or its parent."); // Log if no target is found
                }
            }

            // Call CheckRaycastHit method of the target
            //target.CheckRaycastHit(); // Add this line

            // Instantiate impact effect at the hit point
            if (impactEffect != null)
            {
                // Ensure the impact effect is oriented correctly
                Quaternion impactRotation = Quaternion.LookRotation(hit.normal);
                GameObject impactGO = Instantiate(impactEffect, hit.point, impactRotation);
                Destroy(impactGO, 2f); // Destroy impact effect after 2 seconds
            }
        }
    }
}