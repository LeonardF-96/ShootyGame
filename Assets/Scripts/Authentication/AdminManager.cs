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
    public GameObject createPanel;

    [Header("Text fields")]
    public TMP_Text nameText;
    public TMP_InputField descInputField;
    public TMP_InputField createInputField;

    [Header("Buttons")]
    public Button adminButton;
    public Button backButton;

    public Button scoresButton;
    public Button usersButton;
    public Button weaponsButton;
    public Button weaponTypesButton;

    public Button createButton;
    public Button confirmCreateButton;
    public Button deleteButton;
    public Button updateButton;

    [Header("Scroll View Elements")]
    public Transform subjectScrollView;
    public GameObject subjectEntryPrefab;
    public Transform subjectScrollContent;

    private ScoreManager scoreManager;
    private UserManager userManager;
    private WeaponManager weaponManager;

    private string currentTableName;

    void Start()
    {
        scoreManager = FindObjectOfType<ScoreManager>();
        userManager = FindObjectOfType<UserManager>();
        weaponManager = FindObjectOfType<WeaponManager>();

        adminPanel.SetActive(false);
        createPanel.SetActive(false);

        adminButton.onClick.AddListener(OpenAdminPanel);
        backButton.onClick.AddListener(CloseAdminPanel);

        scoresButton.onClick.AddListener(() => {
            currentTableName = "Scores";
            PopulateEntries(currentTableName);
            SetCreateTemplate(currentTableName);
        });
        usersButton.onClick.AddListener(() => {
            currentTableName = "Users";
            PopulateEntries(currentTableName);
            SetCreateTemplate(currentTableName);
        });
        weaponsButton.onClick.AddListener(() => {
            currentTableName = "Weapons";
            PopulateEntries(currentTableName);
            SetCreateTemplate(currentTableName);
        });
        weaponTypesButton.onClick.AddListener(() => {
            currentTableName = "WeaponType";
            PopulateEntries(currentTableName);
            SetCreateTemplate(currentTableName);
        });

        createButton.onClick.AddListener(() => createPanel.SetActive(!createPanel.activeSelf));
        confirmCreateButton.onClick.AddListener(ProcessCreateInput);
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
    void SetCreateTemplate(string tableName)
    {
        if (createInputField == null) return;

        object template = tableName switch
        {
            "Scores" => new ScoreRequest { userId = 0, scoreValue = 0, roundTime = 0f, averageAccuracy = 0f, moneyEarned = 0},
            "Users" => new UserRequest { username = "NewUser", email = "email@example.com", password = "password", role = 0 },
            "Weapons" => new WeaponRequest { name = "NewWeapon", price = 0, reloadSpeed = 0f, magSize = 0, fireRate = 0, fireMode = 0, weaponTypeId = 0 },
            "WeaponType" => new WeaponTypeRequest { name = "NewWeaponType", equipmentSlot = 0 },
            _ => null
        };

        if (template != null)
        {
            createInputField.text = JsonConvert.SerializeObject(template, Formatting.Indented);
        }
    }
    void ProcessCreateInput()
    {
        Debug.Log("Processing create input...");
        string json = createInputField.text;
        Debug.Log("json: " + json);
        if (string.IsNullOrEmpty(json))
        {
            Debug.LogError("No JSON data found.");
            return;
        }
        if (string.IsNullOrEmpty(currentTableName))
        {
            Debug.LogError("No table name found.");
            return;
        }
        object data = currentTableName switch
        {
            "Scores" => JsonConvert.DeserializeObject<ScoreRequest>(json),
            "Users" => JsonConvert.DeserializeObject<UserRequest>(json),
            "Weapons" => JsonConvert.DeserializeObject<WeaponRequest>(json),
            "WeaponType" => JsonConvert.DeserializeObject<WeaponTypeRequest>(json),
            _ => null
        };
        if (data != null)
        {
            Debug.Log("data: " + data);
            switch (currentTableName)
            {
                case "Scores":
                    StartCoroutine(scoreManager.SendScoreToApi((ScoreRequest)data));
                    break;
                case "Users":
                    StartCoroutine(userManager.CreateUser((UserRequest)data, result => HandleCreateResult(result, currentTableName)));
                    break;
                case "Weapons":
                    StartCoroutine(weaponManager.CreateWeapon((WeaponRequest)data, result => HandleCreateResult(result, currentTableName)));
                    break;
                case "WeaponType":
                    StartCoroutine(weaponManager.CreateWeaponType((WeaponTypeRequest)data, result => HandleCreateResult(result, currentTableName)));
                    break;
                default:
                    break;
            }
        }
        else if (data == null)
        {
            Debug.LogError("Failed to parse JSON data.");
        }
    }
    void HandleCreateResult(bool result, string tableName)
    {
        if (result)
        {
            Debug.Log($"{tableName} created successfully.");
            PopulateEntries(currentTableName); // Refresh the entries to show the new data
        }
        else
        {
            Debug.LogError($"Failed to create {tableName}.");
        }
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