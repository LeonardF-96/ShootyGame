using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using TMPro;
using System.Linq;
using System.Collections.Generic;

public class PlayButton : MonoBehaviour
{
    public Transform PrimaryChoiceContent;
    public Transform SecondaryChoiceContent;
    public GameObject WpnEntryPrefab;
    public Transform Primary_Choice_Scroll_View;
    public Transform Secondary_Choice_Scroll_View;
    private WeaponManager weaponManager;

    [SerializeField] private GameObject weaponSelectionPanel;
    [SerializeField] private Button confirmButton;

    private int selectedPrimaryWeaponId = -1;
    private int selectedSecondaryWeaponId = -1;

    private Button selectedPrimaryButton;
    private Button selectedSecondaryButton;

    private Color defaultButtonColor = Color.cyan;
    private Color selectedButtonColor = Color.green;

    private void Start()
    {
        weaponManager = FindObjectOfType<WeaponManager>();
        weaponSelectionPanel.SetActive(false);
        confirmButton.onClick.AddListener(OnConfirmWeaponSelection);
    }

    public void OnPlayButtonClicked()
    {
        // Show weapon selection panel instead of loading the scene immediately
        weaponSelectionPanel.SetActive(true);
        StartCoroutine(PopulateWeaponChoices());
    }

    private IEnumerator PopulateWeaponChoices()
    {
        // Ensure userWeapons is populated
        if (StoreManager.userWeapons == null || StoreManager.userWeapons.Count == 0)
        {
            Debug.LogError("User weapons list is empty. Cannot populate weapon choices.");
            yield break;
        }

        // Clear existing entries
        foreach (Transform child in PrimaryChoiceContent) Destroy(child.gameObject);
        foreach (Transform child in SecondaryChoiceContent) Destroy(child.gameObject);

        // Populate Primary and Secondary choices based on EquipmentSlot
        foreach (var weapon in StoreManager.userWeapons)
        {
            GameObject entry = Instantiate(WpnEntryPrefab);
            TMP_Text weaponText = entry.transform.Find("WeaponNameText").GetComponent<TMP_Text>();
            Button weaponButton = entry.GetComponent<Button>();

            if (weaponText != null)
            {
                weaponText.text = weapon.name;
            }

            // Assign the OnWeaponClicked listener
            weaponButton.onClick.AddListener(() => OnWeaponClicked(weapon, weaponButton));

            // Place weapon in the appropriate slot based on EquipmentSlot
            if (weapon.weaponType.equipmentSlot == EquipmentSlot.Secondary)
            {
                entry.transform.SetParent(SecondaryChoiceContent, false);
            }
            else if (weapon.weaponType.equipmentSlot == EquipmentSlot.Primary)
            {
                entry.transform.SetParent(PrimaryChoiceContent, false);
            }

            // Highlight preselected weaponId 1 for secondary and weaponId 3 for primary
            if (weapon.weaponId == 1)
            {
                selectedSecondaryWeaponId = weapon.weaponId;
                selectedSecondaryButton = weaponButton;
                HighlightButton(weaponButton, true);
            }
            else if (weapon.weaponId == 3)
            {
                selectedPrimaryWeaponId = weapon.weaponId;
                selectedPrimaryButton = weaponButton;
                HighlightButton(weaponButton, true);
            }
        }

        yield return null;
    }

    private void OnWeaponClicked(WeaponResponse weapon, Button weaponButton)
    {
        if (weapon == null)
        {
            Debug.LogError("Weapon is null.");
            return;
        }
        // Assign the weapon selection
        if (weapon.weaponType.equipmentSlot == EquipmentSlot.Secondary)
        {
            selectedSecondaryWeaponId = weapon.weaponId;
            Debug.Log($"Selected Secondary Weapon ID: {selectedSecondaryWeaponId}");

            // Highlight the selected button and reset the previous selection
            if (selectedSecondaryButton != null)
            {
                HighlightButton(selectedSecondaryButton, false);
            }
            selectedSecondaryButton = weaponButton;
            HighlightButton(weaponButton, true);
        }
        else if (weapon.weaponType.equipmentSlot == EquipmentSlot.Primary)
        {
            selectedPrimaryWeaponId = weapon.weaponId;
            Debug.Log($"Selected Primary Weapon ID: {selectedPrimaryWeaponId}");

            // Highlight the selected button and reset the previous selection
            if (selectedPrimaryButton != null)
            {
                HighlightButton(selectedPrimaryButton, false);
            }
            selectedPrimaryButton = weaponButton;
            HighlightButton(weaponButton, true);
        }
    }

    private void HighlightButton(Button button, bool highlight)
    {
        ColorBlock colors = button.colors;
        colors.normalColor = highlight ? selectedButtonColor : defaultButtonColor;
        button.colors = colors;
    }

    private void OnConfirmWeaponSelection()
    {
        // Save primary and secondary weapon choices to PlayerPrefs
        PlayerPrefs.SetInt("PrimaryWeaponId", selectedPrimaryWeaponId);
        PlayerPrefs.SetInt("SecondaryWeaponId", selectedSecondaryWeaponId);

        // Save weapon stats for primary weapon
        WeaponResponse primaryWeapon = weaponManager.GetWeaponById(selectedPrimaryWeaponId);
        if (primaryWeapon != null)
        {
            PlayerPrefs.SetInt("PrimaryWeapon_FireRate", primaryWeapon.fireRate);
            PlayerPrefs.SetFloat("PrimaryWeapon_ReloadSpeed", primaryWeapon.reloadSpeed);
            PlayerPrefs.SetInt("PrimaryWeapon_MagSize", primaryWeapon.magSize);
            PlayerPrefs.SetString("PrimaryWeapon_WeaponType", primaryWeapon.weaponType.name);
            PlayerPrefs.SetString("PrimaryWeapon_WeaponName", primaryWeapon.name);
        }

        // Save weapon stats for secondary weapon
        WeaponResponse secondaryWeapon = weaponManager.GetWeaponById(selectedSecondaryWeaponId);
        if (secondaryWeapon != null)
        {
            PlayerPrefs.SetInt("SecondaryWeapon_FireRate", secondaryWeapon.fireRate);
            PlayerPrefs.SetFloat("SecondaryWeapon_ReloadSpeed", secondaryWeapon.reloadSpeed);
            PlayerPrefs.SetInt("SecondaryWeapon_MagSize", secondaryWeapon.magSize);
            PlayerPrefs.SetString("SecondaryWeapon_WeaponType", secondaryWeapon.weaponType.name);
            PlayerPrefs.SetString("SecondaryWeapon_WeaponName", secondaryWeapon.name);
        }

        PlayerPrefs.Save();
        Debug.Log("Saved PrimaryWeaponId: " + selectedPrimaryWeaponId);
        Debug.Log("Saved SecondaryWeaponId: " + selectedSecondaryWeaponId);
        Debug.Log("Weapon confirmed! Loading GameScene...");

        // Load the game scene
        SceneManager.LoadScene("GameScene");
    }
}