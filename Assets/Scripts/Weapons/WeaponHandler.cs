using System.Collections.Generic;
using UnityEngine;

public class WeaponHandler : MonoBehaviour
{
    public Transform weaponHolder; // Where weapons are instantiated
    public List<GameObject> weaponPrefabs; // Assign your gun prefabs in the inspector
    public Camera fpsCam;

    private GameObject primaryWeapon;
    private GameObject secondaryWeapon;
    private GunController activeGun;

    void Start()
    {
        EquipWeapons(PlayerPrefs.GetInt("PrimaryWeaponId", -1), PlayerPrefs.GetInt("SecondaryWeaponId", -1));
    }

    public void EquipWeapons(int primaryWeaponId, int secondaryWeaponId)
    {
        Debug.Log($"Equipping weapons: Primary - {primaryWeaponId}, Secondary - {secondaryWeaponId}");

        if (primaryWeapon != null) Destroy(primaryWeapon);
        if (secondaryWeapon != null) Destroy(secondaryWeapon);

        primaryWeapon = InstantiateWeapon(primaryWeaponId);
        secondaryWeapon = InstantiateWeapon(secondaryWeaponId);

        if (primaryWeapon != null)
        {
            Debug.Log($"Primary weapon instantiated: {primaryWeapon.name}");
            EquipWeapon(primaryWeapon);
            ApplyWeaponStats(primaryWeapon.GetComponent<GunController>(), "PrimaryWeapon");
        }
        else
        {
            Debug.LogError("Failed to instantiate primary weapon!");
        }
        if (secondaryWeapon != null)
        {
            Debug.Log($"Secondary weapon instantiated: {secondaryWeapon.name}");
            ApplyWeaponStats(secondaryWeapon.GetComponent<GunController>(), "SecondaryWeapon");
        }
        else
        {
            Debug.LogError("Failed to instantiate secondary weapon!");
        }
    }

    private void ApplyWeaponStats(GunController gun, string weaponPrefix)
    {
        if (gun == null) return;

        gun.fireRate = PlayerPrefs.GetInt($"{weaponPrefix}_FireRate", gun.fireRate);
        gun.reloadSpeed = PlayerPrefs.GetFloat($"{weaponPrefix}_ReloadSpeed", gun.reloadSpeed);
        gun.magSize = PlayerPrefs.GetInt($"{weaponPrefix}_MagSize", gun.magSize);
        gun.weaponType = PlayerPrefs.GetString($"{weaponPrefix}_WeaponType", gun.weaponType);
        gun.weaponId = PlayerPrefs.GetInt($"{weaponPrefix}Id", gun.weaponId);

        // Set the current ammo to the saved value or the mag size if not saved
        gun.currentAmmo = PlayerPrefs.GetInt($"{weaponPrefix}_CurrentAmmo", gun.magSize);

        Debug.Log($"Applied stats to {weaponPrefix}: FireRate - {gun.fireRate}, ReloadSpeed - {gun.reloadSpeed}, MagSize - {gun.magSize}, WeaponType - {gun.weaponType}, WeaponId - {gun.weaponId}");
    }

    private GameObject InstantiateWeapon(int weaponId)
    {
        foreach (GameObject weaponPrefab in weaponPrefabs)
        {
            GunController gunController = weaponPrefab.GetComponent<GunController>();
            if (gunController != null && gunController.weaponId == weaponId)
            {
                GameObject newWeapon = Instantiate(weaponPrefab, weaponHolder);
                newWeapon.SetActive(false);
                GunController newGunController = newWeapon.GetComponent<GunController>();
                if (newGunController != null)
                {
                    newGunController.fpsCam = fpsCam; // Assign the fpsCam
                    Debug.Log($"Instantiated weapon: {newWeapon.name} with type {newGunController.weaponType}");
                }
                else
                {
                    Debug.LogError("Failed to get GunController component from the instantiated weapon!");
                }
                return newWeapon;
            }
        }

        Debug.LogWarning($"Weapon with ID {weaponId} not found in prefabs!");
        return null;
    }

    public void EquipWeapon(GameObject weapon)
    {
        if (activeGun != null) activeGun.gameObject.SetActive(false);

        weapon.SetActive(true);
        activeGun = weapon.GetComponent<GunController>();

        if (activeGun != null)
        {
            Debug.Log($"Equipped weapon: {activeGun.weaponType}");
        }
        else
        {
            Debug.LogError("Failed to get GunController component from the weapon!");
        }
    }

    public void SwapWeapons()
    {
        if (activeGun == primaryWeapon.GetComponent<GunController>())
        {
            EquipWeapon(secondaryWeapon);
        }
        else
        {
            EquipWeapon(primaryWeapon);
        }
    }

    public GunController GetActiveGun()
    {
        return activeGun;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            SwapWeapons();
        }
    }
}
