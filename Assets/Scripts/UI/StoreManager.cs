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
    private WeaponManager weaponManager;

    [Header("TMP fields")]
    [SerializeField] public TMP_Text weaponNameText;
    [SerializeField] public TMP_Text weaponTypeText;
    [SerializeField] public TMP_Text weaponPriceText;
    [SerializeField] public TMP_Text weaponReloadSpeedText;
    [SerializeField] public TMP_Text weaponMagSizeText;
    [SerializeField] public TMP_Text weaponFireRateText;


    // Start is called before the first frame update
    void Start()
    {
        weaponManager = FindObjectOfType<WeaponManager>();
    }

    public void FetchAndDisplayWeapons()
    {
        if (weaponManager != null)
        {
            Debug.Log("Fetching weapons...");
            StartCoroutine(FetchAllWeapons());
        }
        else
        {
            Debug.LogError("ApiManager not found in the scene!");
        }
    }

    private IEnumerator FetchAllWeapons()
    {
        bool weaponsFetched = false;

        // Log the start of the fetch process
        Debug.Log("Fetching all weapons...");

        // Fetch all weapons
        weaponManager.FetchAllWeapons(weapons =>
        {
            if (weapons != null && weapons.Count > 0)
            {
                Debug.Log("Weapons fetched successfully.");

                // Instantiate weapon entries
                foreach (var weapon in weapons)
                {
                    GameObject entry = Instantiate(storeEntryPrefab, storeContent);
                    TMP_Text wpnNameText = entry.transform.Find("WpnNameText").GetComponent<TMP_Text>();
                    Button button = entry.GetComponent<Button>();

                    if (wpnNameText == null || button == null)
                    {
                        Debug.LogError("Missing components in weapon prefab");
                    }
                    else
                    {
                        wpnNameText.text = weapon.Name;
                        Debug.Log("Weapon name set: " + weapon.Name);

                        button.onClick.AddListener(() =>
                        {
                            weaponNameText.text = weapon.Name;
                            weaponTypeText.text = "Type: " + weapon.Name;
                            weaponPriceText.text = "Price: " + weapon.Price.ToString();
                            weaponReloadSpeedText.text = "Reload Time: " + weapon.ReloadSpeed.ToString();
                            weaponMagSizeText.text = "Magazine Size: " + weapon.MagSize.ToString();
                            weaponFireRateText.text = "Firerate: " + weapon.FireRate.ToString();
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
}