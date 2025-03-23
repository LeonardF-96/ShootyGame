using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Newtonsoft.Json; // Ensure you have Newtonsoft.Json installed for JSON serialization

public class AdminManager : MonoBehaviour
{
    [Header("Panels")]
    public GameObject adminPanel;
    public GameObject descPanel;

    [Header("Text fields")]
    public TMP_Text nameText;
    public TMP_InputField descInputField; // Add this!

    [Header("Buttons")]
    public Button adminButton;
    public Button backButton;

    public Button scoresButton;
    public Button usersButton;
    public Button weaponsButton;
    public Button weaponTypesButton;

    public Button createButton;
    public Button deleteButton;
    public Button updateButton;

    [Header("Scroll View Elements")]
    public Transform subjectScrollView;
    public GameObject subjectEntryPrefab;
    public Transform subjectScrollContent;

    private ScoreManager scoreManager;
    private UserManager userManager;
    private WeaponManager weaponManager;

    private Dictionary<string, List<string>> tableSchemas = new Dictionary<string, List<string>>()
    {
        {"Scores", new List<string> { "ScoreId", "UserId", "ScoreValue", "AverageAccuracy", "RoundTime"} },
        {"Users", new List<string> { "UserId", "UserName", "Email", "PlayerTag", "Money", "Role"} },
        {"Weapons", new List<string> { "WeaponId", "WeaponTypeId", "Name", "Price", "ReloadSpeed", "MagSize", "FireRate", "FireMode"} },
        {"WeaponType", new List<string> { "WeaponTypeId", "Name", "EquipmentSlot" } }
    };

    void Start()
    {
        scoreManager = FindObjectOfType<ScoreManager>();
        userManager = FindObjectOfType<UserManager>();
        weaponManager = FindObjectOfType<WeaponManager>();

        adminPanel.SetActive(false);

        adminButton.onClick.AddListener(OpenAdminPanel);
        backButton.onClick.AddListener(CloseAdminPanel);

        scoresButton.onClick.AddListener(() => PopulateEntries("Scores"));
        usersButton.onClick.AddListener(() => PopulateEntries("Users"));
        weaponsButton.onClick.AddListener(() => PopulateEntries("Weapons"));
        weaponTypesButton.onClick.AddListener(() => PopulateEntries("WeaponType"));
    }

    void PopulateEntries(string tableName)
    {
        // Clear existing entries
        foreach (Transform child in subjectScrollContent)
        {
            Destroy(child.gameObject);
        }

        if (tableName == "Scores")
        {
            scoreManager.FetchAllScores((scores) =>
            {
                foreach (var score in scores)
                {
                    CreateEntry(score, tableName);
                }
            });
        }
        else if (tableName == "Users")
        {
            userManager.FetchAllUsers((users) =>
            {
                foreach (var user in users)
                {
                    CreateEntry(user, tableName);
                }
            });
        }
        else if (tableName == "Weapons")
        {
            weaponManager.FetchAllWeapons((weapons) =>
            {
                foreach (var weapon in weapons)
                {
                    CreateEntry(weapon, tableName);
                }
            });
        }
        else if (tableName == "WeaponType")
        {
            weaponManager.FetchAllWeaponTypes((weaponTypes) =>
            {
                foreach (var weaponType in weaponTypes)
                {
                    CreateEntry(weaponType, tableName);
                }
            });
        }
    }
    void CreateEntry(object data, string tableName)
    {
        GameObject entry = Instantiate(subjectEntryPrefab, subjectScrollContent);

        TMP_Text entryText = entry.transform.Find("SubjectButton/SubjectNameText").GetComponent<TMP_Text>();
        Button entryButton = entry.transform.Find("SubjectButton").GetComponent<Button>();

        string jsonData = JsonConvert.SerializeObject(data, Formatting.Indented); // Convert entire object to JSON
        string displayValue = ExtractDisplayValue(data, tableName); // Extract a single relevant value

        entryText.text = displayValue; // Set text to relevant value

        entryButton.onClick.AddListener(() =>
        {
            descInputField.text = jsonData; // Fill input field with JSON data
        });
    }

    void OpenAdminPanel()
    {
        adminPanel.SetActive(true);
    }

    void CloseAdminPanel()
    {
        adminPanel.SetActive(false);
    }
    string ExtractDisplayValue(object data, string tableName)
    {
        switch (tableName)
        {
            case "Scores":
                return ((ScoreResponse)data).scoreValue.ToString();
            case "Users":
                return ((UserResponse)data).playerTag;
            case "Weapons":
                return ((WeaponResponse)data).name;
            case "WeaponType":
                return ((Weapon_WeaponTypeResponse)data).name;
            default:
                return "Unknown";
        }
    }
}