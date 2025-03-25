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
    private GameObject selectedEntry;
    private object selectedData;

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
        deleteButton.onClick.AddListener(DeleteSelectedEntry);
        updateButton.onClick.AddListener(UpdateSelectedEntry);
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
            string filteredJsonData = FilterJsonData(data, tableName);
            descInputField.text = filteredJsonData; // Fill input field with filtered JSON data
            selectedEntry = entry; // Store reference to selected entry
            selectedData = data; // Store reference to selected data
        });
    }

    string FilterJsonData(object data, string tableName)
    {
        switch (tableName)
        {
            case "Scores":
                var score = (ScoreResponse)data;
                var filteredScore = new
                {
                    score.scoreValue,
                    score.averageAccuracy,
                    score.roundTime,
                    moneyEarned = 0,
                    score.userId
                };
                return JsonConvert.SerializeObject(filteredScore, Formatting.Indented);
            case "Users":
                var user = (UserResponse)data;
                var filteredUser = new
                {
                    user.userName,
                    user.email,
                    password = "password",
                    user.role
                };
                return JsonConvert.SerializeObject(filteredUser, Formatting.Indented);
            case "Weapons":
                var weapon = (WeaponResponse)data;
                var filteredWeapon = new
                {
                    weapon.name,
                    weapon.price,
                    weapon.reloadSpeed,
                    weapon.magSize,
                    weapon.fireRate,
                    weapon.fireMode,
                    weaponTypeId = 0
                };
                return JsonConvert.SerializeObject(filteredWeapon, Formatting.Indented);
            case "WeaponType":
                var weaponType = (Weapon_WeaponTypeResponse)data;
                var filteredWeaponType = new
                {
                    weaponType.name,
                    weaponType.equipmentSlot
                };
                return JsonConvert.SerializeObject(filteredWeaponType, Formatting.Indented);
            default:
                return JsonConvert.SerializeObject(data, Formatting.Indented);
        }
    }
    void SetCreateTemplate(string tableName)
    {
        if (createInputField == null) return;

        object template = tableName switch
        {
            "Scores" => new ScoreRequest { userId = 0, scoreValue = 0, roundTime = 0f, averageAccuracy = 0f, moneyEarned = 0},
            "Users" => new UserRequest { userName = "NewUser", email = "email@example.com", password = "password", role = 0 },
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
    void DeleteSelectedEntry()
    {
        if (selectedEntry == null || selectedData == null)
        {
            Debug.LogError("No entry selected for deletion.");
            return;
        }

        if (string.IsNullOrEmpty(currentTableName))
        {
            Debug.LogError("No table name found.");
            return;
        }

        switch (currentTableName)
        {
            case "Scores":
                int scoreId = ((ScoreResponse)selectedData).scoreId;
                StartCoroutine(scoreManager.DeleteScoreById(scoreId, result => HandleDeleteResult(result, selectedEntry)));
                break;
            case "Users":
                int userId = ((UserResponse)selectedData).userId;
                StartCoroutine(userManager.DeleteUserById(userId, result => HandleDeleteResult(result, selectedEntry)));
                break;
            case "Weapons":
                int weaponId = ((WeaponResponse)selectedData).weaponId;
                StartCoroutine(weaponManager.DeleteWeaponById(weaponId, result => HandleDeleteResult(result, selectedEntry)));
                break;
            case "WeaponType":
                int weaponTypeId = ((Weapon_WeaponTypeResponse)selectedData).weaponTypeId;
                StartCoroutine(weaponManager.DeleteWeaponTypeById(weaponTypeId, result => HandleDeleteResult(result, selectedEntry)));
                break;
            default:
                Debug.LogError("Invalid table name.");
                break;
        }
    }
    void HandleDeleteResult(bool result, GameObject entry)
    {
        if (result)
        {
            Debug.Log("Entry deleted successfully.");
            Destroy(entry); // Remove the entry from the UI
        }
        else
        {
            Debug.LogError("Failed to delete entry.");
        }
    }
    void UpdateSelectedEntry()
    {
        if (selectedEntry == null || selectedData == null)
        {
            Debug.LogError("No entry selected for update.");
            return;
        }

        if (string.IsNullOrEmpty(currentTableName))
        {
            Debug.LogError("No table name found.");
            return;
        }

        string json = descInputField.text;
        if (string.IsNullOrEmpty(json))
        {
            Debug.LogError("No JSON data found in descInputField.");
            return;
        }

        switch (currentTableName)
        {
            case "Scores":
                ScoreRequest updatedScore = JsonConvert.DeserializeObject<ScoreRequest>(json);
                int scoreId = ((ScoreResponse)selectedData).scoreId;
                StartCoroutine(scoreManager.UpdateScoreById(scoreId, updatedScore, result => HandleUpdateResult(result, selectedEntry, updatedScore)));
                break;
            case "Users":
                UserRequest updatedUser = JsonConvert.DeserializeObject<UserRequest>(json);
                int userId = ((UserResponse)selectedData).userId;
                StartCoroutine(userManager.UpdateUserById(userId, updatedUser, result => HandleUpdateResult(result, selectedEntry, updatedUser)));
                break;
            case "Weapons":
                WeaponRequest updatedWeapon = JsonConvert.DeserializeObject<WeaponRequest>(json);
                int weaponId = ((WeaponResponse)selectedData).weaponId;
                StartCoroutine(weaponManager.UpdateWeaponById(weaponId, updatedWeapon, result => HandleUpdateResult(result, selectedEntry, updatedWeapon)));
                break;
            case "WeaponType":
                WeaponTypeRequest updatedWeaponType = JsonConvert.DeserializeObject<WeaponTypeRequest>(json);
                int weaponTypeId = ((Weapon_WeaponTypeResponse)selectedData).weaponTypeId;
                StartCoroutine(weaponManager.UpdateWeaponTypeById(weaponTypeId, updatedWeaponType, result => HandleUpdateResult(result, selectedEntry, updatedWeaponType)));
                break;
            default:
                Debug.LogError("Invalid table name.");
                break;
        }
    }
    void HandleUpdateResult(bool result, GameObject entry, object updatedData)
    {
        if (result)
        {
            Debug.Log("Entry updated successfully.");
            TMP_Text entryText = entry.transform.Find("SubjectButton/SubjectNameText").GetComponent<TMP_Text>();
            string displayValue = ExtractDisplayValue(updatedData, currentTableName);
            entryText.text = displayValue; // Update the UI with the new data
            selectedData = updatedData; // Update the selected data reference
            PopulateEntries(currentTableName); // Refresh the entries to show the updated data
        }
        else
        {
            Debug.LogError("Failed to update entry.");
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