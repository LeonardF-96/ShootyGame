using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using TMPro;
using UnityEngine.UI;

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

    private WeaponResponse selectedWeapon;
    private List<WeaponResponse> userWeapons = new List<WeaponResponse>();

    // Start is called before the first frame update
    void Start()
    {
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
        buyButton.onClick.AddListener(OnBuyButtonClicked);
        backButton.onClick.AddListener(OnBackButtonClicked);

        FetchUserWeapons();
    }
    private void FetchUserWeapons()
    {
        int userId = PlayerPrefs.GetInt("userId"); // Assuming userId is stored in PlayerPrefs
        weaponManager.FetchUserWeapons(userId, SetUserWeapons);
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
            Debug.Log($"Owned weapon - ID: {weapon.weaponId}, Name: {weapon.name}");
        }
    }
    private List<WeaponResponse> ConvertToWeaponResponses(List<User_WeaponResponse> userWeaponResponses)
    {
        List<WeaponResponse> weaponResponses = new List<WeaponResponse>();
        foreach (var userWeapon in userWeaponResponses)
        {
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
                    equipmentSlot = userWeapon.weaponType.equipmentSlot
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
                // Ensure userWeapons is not null
                if (userWeapons == null)
                {
                    Debug.LogWarning("userWeapons is null, initializing an empty list.");
                    userWeapons = new List<WeaponResponse>();
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
                    else
                    {
                        wpnNameText.text = weapon.name;
                        Debug.Log("Weapon name set: " + weapon.name);

                        button.onClick.AddListener(() =>
                        {
                            selectedWeapon = weapon;
                            weaponNameText.text = weapon.name;
                            weaponTypeText.text = "Type: " + weapon.weaponType.name;
                            weaponPriceText.text = "Price: " + weapon.price.ToString();
                            weaponReloadSpeedText.text = "Reload Time: " + weapon.reloadSpeed.ToString();
                            weaponMagSizeText.text = "Magazine Size: " + weapon.magSize.ToString();
                            weaponFireRateText.text = "Firerate: " + weapon.fireRate.ToString();
                            weaponFireModeText.text = "Firemode: " + weapon.fireMode.ToString();

                            // Check if the user already owns the weapon
                            bool ownsWeapon = userWeapons.Exists(w => w.weaponId == weapon.weaponId);
                            Debug.Log($"Checking ownership for weapon ID {weapon.weaponId}: {(ownsWeapon ? "Owned" : "Not owned")}");
                            buyButton.interactable = !ownsWeapon;
                        });
                    }
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
        if (selectedWeapon != null)
        {
            Debug.Log("Buying weapon: " + selectedWeapon.name);
            // Implement the buy functionality here
            // For example, send a request to the server to purchase the weapon
        }
        else
        {
            Debug.LogWarning("No weapon selected to buy.");
        }
    }

    private void OnBackButtonClicked()
    {
        Debug.Log("Back button clicked.");
        storePanel.SetActive(false);
    }
}