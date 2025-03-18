using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using TMPro;
using UnityEngine.UI;
using System;

public class StoreManager : MonoBehaviour
{
    public Transform storeContent;
    public GameObject storeEntryPrefab;
    public Transform store_Scroll_View;
    [SerializeField]public GameObject storePanel;
    private WeaponManager weaponManager;

    [Header("TMP fields")]
    [SerializeField] public TMP_Text weaponNameText;
    [SerializeField] public TMP_Text weaponTypeText;
    [SerializeField] public TMP_Text weaponPriceText;
    [SerializeField] public TMP_Text weaponReloadSpeedText;
    [SerializeField] public TMP_Text weaponMagSizeText;
    [SerializeField] public TMP_Text weaponFireRateText;
    [SerializeField] public TMP_Text weaponFireModeText;

    [Header("Buttons")]
    [SerializeField] public Button buyButton;
    [SerializeField] public Button backButton;

    private static WeaponResponse selectedWeapon;
    public static List<WeaponResponse> userWeapons = new List<WeaponResponse>();

    // Start is called before the first frame update
    void Start()
    {
        Debug.Log("StoreManager Start called. Instance ID: " + GetInstanceID());
        weaponManager = FindObjectOfType<WeaponManager>();

        if (weaponManager == null)
        {
            Debug.LogError("WeaponManager not found in the scene!");
        }

        if (storePanel == null)
        {
            Debug.LogError("StorePanel is not assigned in the Inspector!");
        }

        if (storeContent == null)
        {
            Debug.LogError("StoreContent is not assigned in the Inspector!");
        }

        if (storeEntryPrefab == null)
        {
            Debug.LogError("StoreEntryPrefab is not assigned in the Inspector!");
        }
        if (buyButton == null)
        {
            Debug.LogError("BuyButton is not assigned in the Inspector!");
        }

        if (backButton == null)
        {
            Debug.LogError("BackButton is not assigned in the Inspector!");
        }

        Debug.Log("Current userData: " + PlayerPrefs.GetString("userData"));

        buyButton.onClick.AddListener(OnBuyButtonClicked);
        backButton.onClick.AddListener(OnBackButtonClicked);

        StartCoroutine(InitializeStore());
    }
    private IEnumerator InitializeStore()
    {
        bool userWeaponsFetched = false;

        FetchUserWeapons(() =>
        {
            userWeaponsFetched = true;
            foreach (var weapon in userWeapons)
            {
                Debug.Log($"Weapon: {weapon.name}, Slot: {weapon.weaponType.equipmentSlot}");
            }
        });

        yield return new WaitUntil(() => userWeaponsFetched);
    }
    public void FetchUserWeapons(Action callback)
    {
        int userId = PlayerPrefs.GetInt("userId"); // Assuming userId is stored in PlayerPrefs
        Debug.Log("Fetching user weapons...");
        weaponManager.FetchUserWeapons(userId, userWeaponResponses =>
        {
            SetUserWeapons(userWeaponResponses);
            callback?.Invoke();
        });
    }
    public void SetUserWeapons(List<User_WeaponResponse> userWeaponResponses)
    {
        if (userWeaponResponses == null)
        {
            Debug.LogWarning("userWeaponResponses is null.");
            userWeapons = new List<WeaponResponse>();
            return;
        }
        userWeapons = ConvertToWeaponResponses(userWeaponResponses);
        Debug.Log("User weapons set. Total weapons owned: " + userWeapons.Count);

        foreach (var weapon in userWeapons)
        {
            Debug.Log($"Owned weapon - ID: {weapon.weaponId}, Name: {weapon.name}, Slot: {weapon.weaponType.equipmentSlot}");
        }
    }
    private List<WeaponResponse> ConvertToWeaponResponses(List<User_WeaponResponse> userWeaponResponses)
    {
        List<WeaponResponse> weaponResponses = new List<WeaponResponse>();
        foreach (var userWeapon in userWeaponResponses)
        {
            Debug.Log("userWeapon: " + userWeapon);
            WeaponResponse weaponResponse = new WeaponResponse
            {
                weaponId = userWeapon.weaponId,
                name = userWeapon.name,
                price = userWeapon.price,
                reloadSpeed = userWeapon.reloadSpeed,
                magSize = userWeapon.magSize,
                fireRate = userWeapon.fireRate,
                fireMode = WeaponManager.ParseFireMode(userWeapon.fireMode), // Use class name for static method
                weaponType = new Weapon_WeaponTypeResponse
                {
                    weaponTypeId = userWeapon.weaponType.weaponTypeId,
                    name = userWeapon.weaponType.name,
                    equipmentSlot = userWeapon.weaponType.equipmentSlot // Parse equipmentSlot
                    //equipmentSlot = WeaponManager.ParseEquipmentSlot(userWeapon.weaponType.equipmentSlot) // Use class name for static method
                }
            };
            weaponResponses.Add(weaponResponse);
        }
        return weaponResponses;
    }

    public void FetchAndDisplayWeapons()
    {
        weaponManager = FindObjectOfType<WeaponManager>();
        storePanel.SetActive(!storePanel.activeSelf);
        if (weaponManager != null)
        {
            Debug.Log("Fetching weapons...");
            StartCoroutine(FetchAllWeapons());
        }
        else
        {
            Debug.LogError("WeaponManager not found in the scene!");
        }
    }

    private IEnumerator FetchAllWeapons()
    {
        bool weaponsFetched = false;
        Debug.Log("Fetching all weapons...");

        // Fetch all weapons
        weaponManager.FetchAllWeapons(weapons =>
        {
            if (weapons != null && weapons.Count > 0)
            {
                Debug.Log("Weapons fetched successfully.");

                // Clear old entries
                foreach (Transform child in storeContent)
                {
                    Destroy(child.gameObject);
                }
                // Instantiate weapon entries
                foreach (var weapon in weapons)
                {
                    GameObject entry = Instantiate(storeEntryPrefab, storeContent);
                    entry.SetActive(true);
                    TMP_Text wpnNameText = entry.transform.Find("WpnNameText").GetComponentInChildren<TMP_Text>();
                    Button button = entry.GetComponent<Button>();

                    if (wpnNameText == null || button == null)
                    {
                        Debug.LogError("Missing components in weapon prefab");
                    }
                    wpnNameText.text = weapon.name;

                    button.onClick.AddListener(() =>
                    {
                        selectedWeapon = weapon;
                        Debug.Log("Selected weapon set: " + selectedWeapon.name);
                        weaponNameText.text = weapon.name;
                        weaponTypeText.text = "Type: " + weapon.weaponType.name;
                        weaponPriceText.text = "Price: " + weapon.price.ToString();
                        weaponReloadSpeedText.text = "Reload Time: " + weapon.reloadSpeed.ToString();
                        weaponMagSizeText.text = "Magazine Size: " + weapon.magSize.ToString();
                        weaponFireRateText.text = "Firerate: " + weapon.fireRate.ToString();
                        weaponFireModeText.text = "Firemode: " + weapon.fireMode.ToString();
                        CheckBuyButton(weapon);
                    });
                }
                weaponsFetched = true;
            }
            else
            {
                Debug.LogError("Failed to fetch weapons or no weapons available.");
                weaponsFetched = true; // Proceed even if weapon fetching fails
            }
        });

        // Wait until weapons are fetched or error is handled
        yield return new WaitUntil(() => weaponsFetched);

        if (weaponsFetched)
        {
            Debug.Log("All weapons processed.");
        }
        else
        {
            Debug.LogError("Weapons were not fetched correctly.");
        }
    }
    private void OnBuyButtonClicked()
    {
        Debug.Log("StoreManager Instance ID: " + GetInstanceID());
        Debug.Log("Buy button clicked. SelectedWeapon: " + (selectedWeapon != null ? selectedWeapon.name : "null"));
        if (selectedWeapon != null)
        {
            int userId = PlayerPrefs.GetInt("userId");
            Debug.Log("Buying weapon: " + selectedWeapon.name);
            weaponManager.BuyUserWeapon(userId, selectedWeapon.weaponId, success =>
            {
                if (success)
                {
                    Debug.Log("Weapon purchase successful.");
                    StartCoroutine(FindObjectOfType<UserManager>().FetchAndUpdateUserData(updated =>
                    {
                        if (updated)
                        {
                            FindObjectOfType<MainMenuController>().UpdateUI();
                            FetchUserWeapons(() =>
                            {
                                CheckBuyButton(selectedWeapon);
                                Debug.Log("Current userData: " + PlayerPrefs.GetString("userData"));
                            });
                        }
                    }));  // Fetch fresh user data
                }
                else
                {
                    Debug.LogWarning("Weapon purchase failed.");
                }
            });
        }
        else
        {
            Debug.LogWarning("No weapon selected to buy.");
        }
    }
    private void CheckBuyButton(WeaponResponse weapon)
    {
        Debug.Log($"Checking ownership for weapon '{weapon.name}' (ID: {weapon.weaponId})");

        bool ownsWeapon = userWeapons.Exists(userWeapon => userWeapon.weaponId == weapon.weaponId);
        buyButton.interactable = !ownsWeapon;

        Debug.Log($"Final ownership status for '{weapon.name}': {ownsWeapon}");
        buyButton.interactable = !ownsWeapon;
    }

    private void OnBackButtonClicked()
    {
        Debug.Log("Back button clicked.");
        storePanel.SetActive(false);
    }
}