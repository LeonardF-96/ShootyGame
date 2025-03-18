using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Text;
using TMPro;
using UnityEngine.UI;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class AuthenticationManager : MonoBehaviour
{
    private string baseUrl = "https://localhost:7154/api/";
    private MainMenuController mainMenuController;
    private WeaponManager weaponManager;
    private ScoreManager scoreManager;

    [Header("Login Fields")]
    [SerializeField] private TMP_InputField loginEmailInput;
    [SerializeField] private TMP_InputField loginPasswordInput;
    [SerializeField] private Button loginButton;

    [Header("Sign Up Fields")]
    [SerializeField] private TMP_InputField signUpUsernameInput;
    [SerializeField] private TMP_InputField signUpEmailInput;
    [SerializeField] private TMP_InputField signUpPasswordInput;
    [SerializeField] private Button signUpButton;

    [Header("UI Elements")]
    [SerializeField] private GameObject loginPanel;
    [SerializeField] private Button showLoginButton;
    [SerializeField] private Button exitLoginButton;

    public static AuthenticationManager instance;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            Debug.Log("AuthenticationManager instance created.");
        }
        else
        {
            Destroy(gameObject);
            Debug.Log("Duplicate AuthenticationManager destroyed.");
        }
    }

    void Start()
    {
        mainMenuController = FindObjectOfType<MainMenuController>();
        weaponManager = FindObjectOfType<WeaponManager>();
        scoreManager = FindObjectOfType<ScoreManager>();

        loginButton.onClick.AddListener(OnLoginButtonClicked);
        signUpButton.onClick.AddListener(OnSignUpButtonClicked);
        showLoginButton.onClick.AddListener(OnShowLoginButtonClicked);
        exitLoginButton.onClick.AddListener(OnExitLoginButtonClicked);

        if (mainMenuController == null)
        {
            Debug.LogWarning("MainMenuController not found.");
        }

        string token = PlayerPrefs.GetString("authToken", null);
        if (!string.IsNullOrEmpty(token))
        {
            Debug.Log("Token found: " + token);
            StartCoroutine(ValidateToken(token));
        }

        loginPanel.SetActive(false);
    }

    public void Logout()
    {
        PlayerPrefs.DeleteKey("authToken");
        PlayerPrefs.DeleteKey("userData");
        PlayerPrefs.DeleteKey("userId");
        PlayerPrefs.Save();

        ResetAuthenticationManager();

        mainMenuController.UpdateLoggedInText("Logged out", 0);
        mainMenuController.SetAuthDependentButtonsActive(false);
    }

    void ResetAuthenticationManager()
    {
        loginEmailInput.text = "";
        loginPasswordInput.text = "";
        signUpUsernameInput.text = "";
        signUpEmailInput.text = "";
        signUpPasswordInput.text = "";
        loginPanel.SetActive(false);
    }

    void OnLoginButtonClicked()
    {
        string email = loginEmailInput.text;
        string password = loginPasswordInput.text;
        Debug.Log("Email: " + email + " Password: " + password);

        if (!IsValidEmail(email))
        {
            Debug.LogError("Invalid email format for login.");
            loginEmailInput.text = "";
            loginPasswordInput.text = "";
            return;
        }

        StartCoroutine(AuthenticateUser(new UserRequest { email = email, password = password }));
        loginEmailInput.text = "";
        loginPasswordInput.text = "";
    }

    void OnSignUpButtonClicked()
    {
        string username = signUpUsernameInput.text;
        string email = signUpEmailInput.text;
        string password = signUpPasswordInput.text;
        int role = 1; // Set default role to 1

        if (!IsValidEmail(email))
        {
            Debug.LogError("Invalid email format for sign up.");
            signUpUsernameInput.text = "";
            signUpEmailInput.text = "";
            signUpPasswordInput.text = "";
            return;
        }

        StartCoroutine(CreateUser(new UserRequest { username = username, email = email, password = password, role = role }));
        signUpUsernameInput.text = "";
        signUpEmailInput.text = "";
        signUpPasswordInput.text = "";
    }

    void OnShowLoginButtonClicked()
    {
        loginPanel.SetActive(true);
    }

    void OnExitLoginButtonClicked()
    {
        loginPanel.SetActive(false);
    }

    IEnumerator AuthenticateUser(UserRequest signInRequest)
    {
        // Define the endpoint for user authentication
        string endpoint = "User/authenticate";

        // Convert the sign-in request object to JSON format
        string jsonData = JsonUtility.ToJson(signInRequest);
        Debug.Log("AuthenticateUser Request Body: " + jsonData);

        // Create a new UnityWebRequest for a POST request to the authentication endpoint
        using (UnityWebRequest request = new UnityWebRequest(baseUrl + endpoint, "POST"))
        {
            // Set the request body with the JSON data
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);

            // Set the download handler to handle the response
            request.downloadHandler = new DownloadHandlerBuffer();

            // Set the request header to indicate the content type is JSON
            request.SetRequestHeader("Content-Type", "application/json");

            // Send the request and wait for the response
            yield return request.SendWebRequest();

            // Check if there was a connection error or protocol error
            if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
            {
                // Log the error message
                Debug.LogError(request.error);
            }
            else
            {
                // Log the raw response from the server
                Debug.Log("Raw response: " + request.downloadHandler.text);

                // Deserialize the response JSON into a SignInResponse object
                SignInResponse user = JsonUtility.FromJson<SignInResponse>(request.downloadHandler.text);

                // Log the user's money
                Debug.Log("Money from server: " + user.money);

                // Call the OnUserAuthenticated method to handle the authenticated user
                OnUserAuthenticated(user);
            }
        }
    }

    IEnumerator CreateUser(UserRequest newUser)
    {
        string endpoint = "User";
        string jsonData = JsonUtility.ToJson(newUser);
        Debug.Log("CreateUser Request Body: " + jsonData);

        using (UnityWebRequest request = new UnityWebRequest(baseUrl + endpoint, "POST"))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError(request.error);
            }
            else
            {
                Debug.Log(request.downloadHandler.text);
                SignInResponse user = JsonUtility.FromJson<SignInResponse>(request.downloadHandler.text);

                string token = user.token;
                if (!string.IsNullOrEmpty(token))
                {
                    PlayerPrefs.SetString("authToken", token);
                    PlayerPrefs.Save();
                    Debug.Log("Token saved: " + token);
                }

                OnUserAuthenticated(user);
            }
        }
    }

    void OnUserAuthenticated(SignInResponse user)
    {
        Debug.Log("User authenticated: " + user.userName);

        if (mainMenuController == null)
        {
            mainMenuController = FindObjectOfType<MainMenuController>();
            if (mainMenuController == null)
            {
                Debug.LogError("MainMenuController not found in scene.");
                return;
            }
        }

        Debug.Log("UpdateLoggedInText called with user: " + user.userName + " And money: " + user.money);
        mainMenuController.UpdateLoggedInText(user.userName, user.money);

        string userDataJson = JsonUtility.ToJson(user);
        PlayerPrefs.SetString("userData", userDataJson);
        PlayerPrefs.SetString("authToken", user.token);
        PlayerPrefs.SetInt("userId", user.userId);
        PlayerPrefs.Save();

        // Fetch user weapons and scores after authentication
        if (weaponManager != null)
        {
            weaponManager.FetchUserWeapons(user.userId, (weapons) =>
            {
                Debug.Log("Fetched user weapons: " + weapons.Count);
            });
        }
        else
        {
            Debug.LogError("WeaponManager not found in scene.");
        }

        if (scoreManager != null)
        {
            scoreManager.FetchUserScores(user.userId);
        }
        else
        {
            Debug.LogError("ScoreManager not found in scene.");
        }

        mainMenuController.SetAuthDependentButtonsActive(true);
    }
    IEnumerator ValidateToken(string token)
    {
        int userId = PlayerPrefs.GetInt("userId", -1);
        if (userId == -1)
        {
            Debug.LogError("No userId found in PlayerPrefs.");
            yield break;
        }

        string validationUrl = $"{baseUrl}User/{userId}";

        using (UnityWebRequest request = UnityWebRequest.Get(validationUrl))
        {
            request.SetRequestHeader("Authorization", "Bearer " + token);
            request.downloadHandler = new DownloadHandlerBuffer();

            Debug.Log($"Sending GET request to: {validationUrl}");
            Debug.Log($"Authorization Header: Bearer {token}");

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError($"Error: {request.error}");
                Debug.LogError($"Response Code: {request.responseCode}");
                Debug.LogError($"Response: {request.downloadHandler.text}");
            }
            else if (request.responseCode == 400)
            {
                Debug.LogError($"Bad Request: {request.downloadHandler.text}");
            }
            else
            {
                Debug.Log($"Response: {request.downloadHandler.text}");
                // Deserialize the response data
                //SignInResponse user = JsonUtility.FromJson<SignInResponse>(request.downloadHandler.text);
                string userDataJson = PlayerPrefs.GetString("userData");
                if (!string.IsNullOrEmpty(userDataJson))
                {
                    SignInResponse user = JsonUtility.FromJson<SignInResponse>(userDataJson);
                    // Store updated user data in PlayerPrefs
                    PlayerPrefs.SetString("userData", request.downloadHandler.text);
                    PlayerPrefs.Save();

                    // Authenticate the user with fresh data
                    OnUserAuthenticated(user);
                }
                else
                {
                    Debug.LogError("Failed to parse user data from server response.");
                }
            }
        }
    }

    private bool IsValidEmail(string email)
    {
        if (string.IsNullOrEmpty(email))
        {
            return false;
        }
        string emailPattern = @"^[a-zA-Z0-9_.+-]+@[a-zA-Z0-9-]+\.[a-zA-Z0-9-.]+$";
        return Regex.IsMatch(email, emailPattern);
    }
}