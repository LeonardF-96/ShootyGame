using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ProfileManager : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] public GameObject profilePanel;
    [SerializeField] public GameObject deleteProfilePanel;
    [Header("Textfields")]
    [SerializeField] public GameObject usernameText;
    [SerializeField] public GameObject bestScoreText;
    [SerializeField] public GameObject avgAccuText;
    [SerializeField] public GameObject avgTimeText;
    [Header("Buttons")]
    [SerializeField] public Button updateProfileButton;
    [SerializeField] public Button deleteProfileButton;
    [SerializeField] public Button backButton;
    [SerializeField] public Button confirmDeleteButton;
    [SerializeField] public Button cancelDeleteButton;
    [Header("Inputfields")]
    [SerializeField] public TMP_InputField usernameInput;
    [SerializeField] public TMP_InputField passwordInput;
    [SerializeField] public TMP_InputField emailInput;
    [Header("Score elements")]
    [SerializeField] public GameObject scoreEntryPrefab;
    [SerializeField] public Transform scoreContent;
    [SerializeField] public GameObject scoreScroll;

    private UserManager userManager;
    private int userRole;
    private LogoutButton logoutButton;
    private ImageAnimator imageAnimator;

    // Start is called before the first frame update
    void Start()
    {
        profilePanel.SetActive(false);
        deleteProfilePanel.SetActive(false);
        backButton.onClick.AddListener(OnBackButtonClicked);
        updateProfileButton.onClick.AddListener(OnUpdateButtonClicked);
        deleteProfileButton.onClick.AddListener(OnDeleteButtonClicked);
        confirmDeleteButton.onClick.AddListener(OnConfirmDeleteButtonClicked);
        cancelDeleteButton.onClick.AddListener(OnCancelDeleteButtonClicked);
        userManager = FindObjectOfType<UserManager>();
        logoutButton = FindObjectOfType<LogoutButton>();
        imageAnimator = deleteProfilePanel.GetComponentInChildren<ImageAnimator>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void OnProfileButtonClicked()
    {
        if (profilePanel != null)
        {
            profilePanel.SetActive(!profilePanel.activeSelf);
            if (profilePanel.activeSelf)
            {
                if (profilePanel.activeSelf)
                {
                    FetchAndDisplayUserProfile();
                }
            }
        }
    }
    private void FetchAndDisplayUserProfile()
    {
        int userId = PlayerPrefs.GetInt("userId", -1);
        if (userId == -1)
        {
            Debug.LogError("No userId found in PlayerPrefs.");
            return;
        }

        if (userManager == null)
        {
            userManager = FindObjectOfType<UserManager>();
        }

        if (userManager != null)
        {
            StartCoroutine(userManager.GetUserById(userId, OnUserResponseReceived));
        }
        else
        {
            Debug.LogError("UserManager not found in the scene!");
        }
    }

    private void OnUserResponseReceived(UserResponse userResponse)
    {
        if (userResponse != null)
        {
            usernameText.GetComponent<TMP_Text>().text = userResponse.userName;
            usernameInput.text = userResponse.userName;
            emailInput.text = userResponse.email;
            // Parse the user role
            userRole = userResponse.role.ToLower() == "admin" ? 1 : 0;

            // Clear existing score entries
            foreach (Transform child in scoreContent)
            {
                Destroy(child.gameObject);
            }

            if (userResponse.scores != null && userResponse.scores.Any())
            {
                bestScoreText.GetComponent<TMP_Text>().text = ("Best Score: " + userResponse.scores.Max(s => s.scoreValue).ToString());
                avgAccuText.GetComponent<TMP_Text>().text = ("Average Precision: " + userResponse.scores.Average(s => s.averageAccuracy).ToString("F2") + "%");
                avgTimeText.GetComponent<TMP_Text>().text = ("Average Time: " + userResponse.scores.Average(s => s.roundTime).ToString("F2") + "s");

                // Clear existing score entries
                foreach (Transform child in scoreContent)
                {
                    Destroy(child.gameObject);
                }

                // Create new score entries
                foreach (var score in userResponse.scores)
                {
                    GameObject entry = Instantiate(scoreEntryPrefab, scoreContent);
                    TMP_Text scoreText = entry.transform.Find("ScoreText").GetComponent<TMP_Text>();
                    TMP_Text accuracyText = entry.transform.Find("AccuText").GetComponent<TMP_Text>();
                    TMP_Text timeText = entry.transform.Find("TimeText").GetComponent<TMP_Text>();

                    if (scoreText != null)
                    {
                        scoreText.text = score.scoreValue.ToString();
                    }
                    if (accuracyText != null)
                    {
                        accuracyText.text = score.averageAccuracy.ToString("F2") + "%";
                    }
                    if (timeText != null)
                    {
                        timeText.text = score.roundTime.ToString("F2") + "s";
                    }
                }
            }
            else
            {
                bestScoreText.GetComponent<TMP_Text>().text = "Best Score: ";
                avgAccuText.GetComponent<TMP_Text>().text = "Average Accuracy: ";
                avgTimeText.GetComponent<TMP_Text>().text = "Average Time: ";
            }
        }
        else
        {
            Debug.LogError("Failed to fetch user profile.");
        }
    }
    private void OnUpdateButtonClicked()
    {
        Debug.Log("Update button clicked.");
        if (userManager == null)
        {
            userManager = FindObjectOfType<UserManager>();
        }
        if (userManager != null)
        {
            string username = usernameInput.text;
            string password = passwordInput.text;
            string email = emailInput.text;
            int userId = PlayerPrefs.GetInt("userId", -1);

            UserRequest userRequest = new UserRequest
            {
                username = username,
                password = password,
                email = email,
                role = userRole
            };

            StartCoroutine(userManager.UpdateUserById(userId, userRequest, success =>
            {
                if (success)
                {
                    Debug.Log("User profile updated successfully.");
                    FetchAndDisplayUserProfile();
                }
                else
                {
                    Debug.LogError("Failed to update user profile.");
                }
            }));
        }
        else
        {
            Debug.LogError("UserManager not found in the scene!");
        }
    }
    private void OnDeleteButtonClicked()
    {
        Debug.Log("Delete button clicked.");
        deleteProfilePanel.SetActive(true);
        if (imageAnimator != null)
        {
            imageAnimator.StartAnimation();
        }
    }
    private void OnConfirmDeleteButtonClicked()
    {
        Debug.Log("Confirm delete button clicked.");
        if (userManager == null)
        {
            userManager = FindObjectOfType<UserManager>();
        }
        if (userManager != null)
        {
            int userId = PlayerPrefs.GetInt("userId", -1);
            StartCoroutine(userManager.DeleteUserById(userId, success =>
            {
                if (success)
                {
                    Debug.Log("User deleted successfully.");
                    profilePanel.SetActive(false);
                    deleteProfilePanel.SetActive(false);
                    if (logoutButton != null)
                    {
                        logoutButton.OnLogoutButtonClick();
                    }
                    else
                    {
                        Debug.LogError("LogoutButton not found in the scene!");
                    }
                }
                else
                {
                    Debug.LogError("Failed to delete user.");
                }
            }));
        }
        else
        {
            Debug.LogError("UserManager not found in the scene!");
        }
    }

    private void OnCancelDeleteButtonClicked()
    {
        Debug.Log("Cancel delete button clicked.");
        deleteProfilePanel.SetActive(false);
    }
    private void OnBackButtonClicked()
    {
        Debug.Log("Back button clicked.");
        if (profilePanel != null)
        {
            profilePanel.SetActive(false);
        }
    }
}
