using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class MainMenuController : MonoBehaviour
{
    public TMP_Text loginStatusText;
    public TMP_Text moneyText;

    [Header("Auth Dependent Buttons")]
    public Button friendsButton;
    public Button storeButton;
    public Button highscoresButton;
    public Button playButton;
    public Button adminButton; // Add a reference to the admin button

    [Header("Auth Dependent Panels")]
    public GameObject friendsPanel;
    public GameObject storePanel;
    public GameObject highscorePanel;
    public GameObject profilePanel;
    public GameObject adminPanel;
    public GameObject wpnChoicePanel;

    private AuthenticationManager authManager;

    void Start()
    {
        Debug.Log("MainMenuController Start called.");
        authManager = FindObjectOfType<AuthenticationManager>();

        if (EventSystem.current == null)
        {
            Debug.LogError("No EventSystem found. UI interactions will not work.");
        }

        Camera mainCamera = Camera.main;
        if (mainCamera == null)
        {
            Debug.LogError("No main camera found in the scene.");
        }
        else
        {
            Debug.Log("Main camera found: " + mainCamera.name);
        }

        Canvas[] canvases = FindObjectsOfType<Canvas>();
        foreach (Canvas canvas in canvases)
        {
            Debug.Log("Canvas found: " + canvas.name + ", Render Mode: " + canvas.renderMode);
        }

        Button[] buttons = FindObjectsOfType<Button>();
        foreach (Button button in buttons)
        {
            Debug.Log("Button found: " + button.name + ", Interactable: " + button.interactable);
            button.onClick.AddListener(() => Debug.Log("Button clicked: " + button.name));

            if (!button.gameObject.activeInHierarchy)
            {
                Debug.LogError("Button " + button.name + " is not active in hierarchy.");
            }
        }
        HidePanels();
        UpdateUI();
    }

    void OnEnable()
    {
        Debug.Log("MainMenuController OnEnable called.");
        authManager = FindObjectOfType<AuthenticationManager>();
        AssignUIElements();
        HidePanels();
        UpdateUI();
    }
    void Awake()
    {
        if (FindObjectsOfType<MainMenuController>().Length > 1)
        {
            Destroy(gameObject);
            Debug.Log("Duplicate MainMenuController destroyed.");
            return;
        }
        //debug log
        Debug.Log("MainMenuController Awake called.");
        DontDestroyOnLoad(gameObject);
    }

    void AssignUIElements()
    {
        loginStatusText = GameObject.Find("UsernameText")?.GetComponent<TMP_Text>();
        moneyText = GameObject.Find("MoneyText")?.GetComponent<TMP_Text>();

        friendsButton = GameObject.Find("FriendsButton")?.GetComponent<Button>();
        storeButton = GameObject.Find("StoreButton")?.GetComponent<Button>();
        highscoresButton = GameObject.Find("HighscoresButton")?.GetComponent<Button>();
        playButton = GameObject.Find("PlayButton")?.GetComponent<Button>();
        adminButton = GameObject.Find("AdminButton")?.GetComponent<Button>(); // Assign the admin button

        storePanel = GameObject.Find("StorePanel");
        highscorePanel = GameObject.Find("HighscorePanel");
        wpnChoicePanel = GameObject.Find("WpnChoicePanel");

        if (loginStatusText == null) Debug.LogError("UsernameText not found!");
        if (moneyText == null) Debug.LogError("MoneyText not found!");
        if (friendsButton == null) Debug.LogError("FriendsButton not found!");
        if (storeButton == null) Debug.LogError("StoreButton not found!");
        if (highscoresButton == null) Debug.LogError("HighscoresButton not found!");
        if (playButton == null) Debug.LogError("PlayButton not found!");
        if (adminButton == null) Debug.LogError("AdminButton not found!"); // Check if the admin button is found

        if (storePanel == null) Debug.LogError("StorePanel not found!");
        if (highscorePanel == null) Debug.LogError("HighscorePanel not found!");
        if (wpnChoicePanel == null) Debug.LogError("WpnChoicePanel not found!");
    }

    void HidePanels()
    {
        if (storePanel != null) storePanel.SetActive(false);
        if (highscorePanel != null) highscorePanel.SetActive(false);
        if (wpnChoicePanel != null) wpnChoicePanel.SetActive(false);
        if (adminPanel != null) adminPanel.SetActive(false);
    }

    public void UpdateUI()
    {
        Debug.Log("UpdateUI called.");

        string token = PlayerPrefs.GetString("authToken", null);
        if (!string.IsNullOrEmpty(token))
        {
            Debug.Log("Token found: " + token);

            string userDataJson = PlayerPrefs.GetString("userData");
            if (!string.IsNullOrEmpty(userDataJson))
            {
                SignInResponse user = JsonUtility.FromJson<SignInResponse>(userDataJson);
                UpdateLoggedInText(user.userName, user.money);
                SetAuthDependentButtonsActive(true);
                SetAdminButtonActive(user.role == "Admin"); // Show admin button if user role is "Admin"
            }
            else
            {
                Debug.LogWarning("No user data found in PlayerPrefs. Validating token...");
                StartCoroutine(authManager.ValidateToken(token));
            }
        }
        else
        {
            Debug.LogWarning("No token found in PlayerPrefs.");
            UpdateLoggedInText("Logged out", 0);
            SetAuthDependentButtonsActive(false);
            SetAdminButtonActive(false); // Hide admin button
        }
    }

    public void UpdateLoggedInText(string username, int userMoney)
    {
        Debug.Log("UpdateLoggedInText called with username: " + username + " Money: " + userMoney);

        if (loginStatusText != null)
        {
            if (username == "Logged out")
            {
                loginStatusText.text = "Logged out";
                Debug.Log("loginStatusText updated to 'Logged out'");
                Debug.Log("moneyText set to empty string.");
                moneyText.text = "";
            }
            else
            {
                loginStatusText.text = $"Logged in as: {username}";
                Debug.Log($"loginStatusText updated to 'Logged in as: {username}'");
                moneyText.text = "Money: " + userMoney;
                Debug.Log("moneyText updated with user's money.");
            }
        }
        else
        {
            Debug.LogError("loginStatusText is not assigned in MainMenuController.");
        }
    }

    public void SetAuthDependentButtonsActive(bool isActive)
    {
        Debug.Log("SetAuthDependentButtonsActive called with isActive: " + isActive);

        if (friendsButton != null) friendsButton.gameObject.SetActive(isActive);
        if (storeButton != null) storeButton.gameObject.SetActive(isActive);
        if (highscoresButton != null) highscoresButton.gameObject.SetActive(isActive);
        if (playButton != null) playButton.gameObject.SetActive(isActive);
    }

    public void SetAdminButtonActive(bool isActive)
    {
        Debug.Log("SetAdminButtonActive called with isActive: " + isActive);
        if (adminButton != null)
        {
            adminButton.gameObject.SetActive(isActive);
            adminButton.interactable = isActive; // Set the button's interactable state
        }
    }
}