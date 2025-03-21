using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Linq;

public class HighscoreManager : MonoBehaviour
{
    public Transform highscoreContent;
    public GameObject highscoreEntryPrefab;
    public Transform HS_Scroll_View;
    public GameObject highscorePanel;
    private ScoreManager scoreManager;
    private UserManager userManager;

    [Header("Buttons")]
    public Button global_HS_Button;
    public Button friends_HS_Button;
    public Button highscoreToggleButton; // Add a reference to the highscore toggle button

    // Start is called before the first frame update
    void Start()
    {
        scoreManager = FindObjectOfType<ScoreManager>();
        userManager = FindObjectOfType<UserManager>();

        if (scoreManager == null)
        {
            Debug.LogError("ScoreManager not found in the scene!");
        }
        else
        {
            Debug.Log("ScoreManager found and initialized.");
        }

        if (userManager == null)
        {
            Debug.LogError("UserManager not found in the scene!");
        }
        else
        {
            Debug.Log("UserManager found and initialized.");
        }

        if (global_HS_Button != null)
        {
            global_HS_Button.onClick.AddListener(FetchAndDisplayHighscores);
        }
        else
        {
            Debug.LogError("Global Highscore Button is not assigned in the Inspector!");
        }

        if (friends_HS_Button != null)
        {
            friends_HS_Button.onClick.AddListener(FetchAndDisplayFriendHighscores);
        }
        else
        {
            Debug.LogError("Friends Highscore Button is not assigned in the Inspector!");
        }

        if (highscoreToggleButton != null)
        {
            highscoreToggleButton.onClick.AddListener(ToggleHighscorePanel);
        }
        else
        {
            Debug.LogError("Highscore Toggle Button is not assigned in the Inspector!");
        }
    }

    public void ToggleHighscorePanel()
    {
        if (highscorePanel == null)
        {
            Debug.LogError("Highscore panel is not assigned in the Inspector!");
            return;
        }

        bool isActive = !highscorePanel.activeSelf;
        highscorePanel.SetActive(isActive);

        if (isActive)
        {
            FetchAndDisplayHighscores(); // Fetch global scores when opening the panel
        }
    }

    public void FetchAndDisplayHighscores()
    {
        Debug.Log($"FetchAndDisplayHighscores called. This HighscoreManager instance: {GetInstanceID()}");

        if (scoreManager == null)
        {
            // Try re-fetching just in case it was lost somehow
            scoreManager = FindObjectOfType<ScoreManager>();
            Debug.Log(scoreManager != null
                ? "Re-fetched ScoreManager successfully."
                : "Failed to re-fetch ScoreManager.");
        }

        if (scoreManager != null)
        {
            Debug.Log("Fetching global highscores.");
            StartCoroutine(FetchScores());
        }
        else
        {
            Debug.LogError("scoreManager not found in the scene!");
        }
    }

    private IEnumerator FetchScores()
    {
        // Double-check scoreManager again
        if (scoreManager == null)
        {
            Debug.LogWarning("scoreManager was null — trying to re-fetch.");
            scoreManager = FindObjectOfType<ScoreManager>();

            if (scoreManager == null)
            {
                Debug.LogError("scoreManager still not found! Aborting.");
                yield break; // Stop coroutine if still null
            }
        }

        // Fetch and display scores
        scoreManager.FetchAllScores(scores =>
        {
            if (scores == null)
            {
                Debug.LogError("Failed to fetch scores.");
                return;
            }

            foreach (var score in scores)
            {
                string displayName = score.user.userName;

                Debug.Log($"Score: {score.scoreValue}, User: {displayName}");
            }

            DisplayHighscores(scores);
        });
    }

    private void DisplayHighscores(List<ScoreResponse> scores)
    {
        if (scores == null)
        {
            Debug.LogError("Failed to fetch highscores.");
            return;
        }

        // Clear old entries
        foreach (Transform child in highscoreContent)
        {
            Destroy(child.gameObject);
        }

        // Create new entries
        for (int i = 0; i < scores.Count; i++)
        {
            ScoreResponse score = scores[i];

            if (highscoreEntryPrefab == null)
            {
                Debug.LogError("Highscore Entry Prefab is missing! Assign it in the Inspector.");
                return;
            }

            GameObject entry = Instantiate(highscoreEntryPrefab, highscoreContent);
            TMP_Text rankText = entry.transform.Find("RankText").GetComponent<TMP_Text>();
            TMP_Text userText = entry.transform.Find("UserText").GetComponent<TMP_Text>();
            TMP_Text userIdText = entry.transform.Find("UserIdText").GetComponent<TMP_Text>();
            TMP_Text scoreText = entry.transform.Find("ScoreText").GetComponent<TMP_Text>();
            TMP_Text timeText = entry.transform.Find("TimeText").GetComponent<TMP_Text>();
            TMP_Text accuracyText = entry.transform.Find("AccuText").GetComponent<TMP_Text>();

            if (rankText == null) Debug.LogError("RankText not found in prefab!");
            if (userText == null) Debug.LogError("UserText not found in prefab!");
            if (userIdText == null) Debug.LogError("UserIdText not found in prefab!");
            if (scoreText == null) Debug.LogError("ScoreText not found in prefab!");
            if (timeText == null) Debug.LogError("TimeText not found in prefab!");
            if (accuracyText == null) Debug.LogError("AccuText not found in prefab!");

            rankText.text = (i + 1).ToString(); // Convert int to string
            scoreText.text = score.scoreValue.ToString(); // Convert int to string
            timeText.text = score.roundTime.ToString(); // Convert float to string
            accuracyText.text = score.averageAccuracy.ToString("F2") + "%"; // Convert float to string
            userText.text = score.user.userName;  // Set the username directly from the score response
            userIdText.text = score.user.userId.ToString(); // Convert int to string
        }
    }

    public void FetchAndDisplayFriendHighscores()
    {
        Debug.Log($"FetchAndDisplayFriendHighscores called. This HighscoreManager instance: {GetInstanceID()}");

        if (userManager == null)
        {
            // Try re-fetching just in case it was lost somehow
            userManager = FindObjectOfType<UserManager>();
            Debug.Log(userManager != null
                ? "Re-fetched UserManager successfully."
                : "Failed to re-fetch UserManager.");
        }

        if (userManager != null)
        {
            int userId = PlayerPrefs.GetInt("userId", -1);
            if (userId == -1)
            {
                Debug.LogError("No userId found in PlayerPrefs.");
                return;
            }

            Debug.Log("Fetching friend highscores for user " + userId);
            StartCoroutine(FetchFriendScores(userId));
        }
        else
        {
            Debug.LogError("userManager not found in the scene!");
        }
    }

    private IEnumerator FetchFriendScores(int userId)
    {
        // Double-check userManager again
        if (userManager == null)
        {
            Debug.LogWarning("userManager was null — trying to re-fetch.");
            userManager = FindObjectOfType<UserManager>();

            if (userManager == null)
            {
                Debug.LogError("userManager still not found! Aborting.");
                yield break; // Stop coroutine if still null
            }
        }

        // Fetch and display friend scores
        yield return userManager.GetUserById(userId, userResponse =>
        {
            if (userResponse == null)
            {
                Debug.LogError("Failed to fetch user profile.");
                return;
            }

            List<ScoreResponse> friendScores = new List<ScoreResponse>();

            foreach (var friend in userResponse.friends)
            {
                friendScores.AddRange(friend.scores.Select(score => new ScoreResponse
                {
                    scoreId = score.scoreValue, // Assuming scoreId is not available in ScoreSummary
                    userId = friend.userId,
                    scoreValue = score.scoreValue,
                    roundTime = score.roundTime,
                    averageAccuracy = score.averageAccuracy,
                    user = new UserSummary
                    {
                        userId = friend.userId,
                        userName = friend.userName,
                        playerTag = friend.playerTag
                    }
                }));
            }

            DisplayHighscores(friendScores);
        });
    }

    // Update is called once per frame
    void Update()
    {

    }
}