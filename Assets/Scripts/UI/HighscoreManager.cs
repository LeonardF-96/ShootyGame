using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using TMPro;
using UnityEngine.UI;

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

    private string currentState = "Global";
    //A dictionary to store userId and their corresponding usernames
    private Dictionary<int, string> userDictionary = new Dictionary<int, string>();

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
            Debug.Log("Fetching users and highscores from " + currentState);
            StartCoroutine(FetchUsersAndScores());
        }
        else
        {
            Debug.LogError("scoreManager not found in the scene!");
        }
    }
    private IEnumerator FetchUsersAndScores()
    {
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
        bool usersFetched = false;

        // Fetch all users first
        userManager.FetchAllUsers(users =>
        {
            if (users != null)
            {
                userDictionary.Clear();
                foreach (var user in users)
                {
                    userDictionary[user.userId] = user.userName; // Store userId -> username
                }
                usersFetched = true;
            }
            else
            {
                Debug.LogError("Failed to fetch users. Falling back to User ID.");
                usersFetched = true; // Proceed even if user fetching fails
            }
        });

        // Wait until users are fetched before proceeding
        yield return new WaitUntil(() => usersFetched);

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
                string displayName = userDictionary.ContainsKey(score.userId)
                    ? userDictionary[score.userId]
                    : $"User {score.userId}";

                Debug.Log($"Score: {score.scoreValue}, User: {displayName}");
            }

            DisplayHighscores(scores);
        });
    }
    private void DisplayHighscores(List<ScoreResponse> scores)
    {
        highscorePanel.SetActive(!highscorePanel.activeSelf);
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
            TMP_Text scoreText = entry.transform.Find("ScoreText").GetComponent<TMP_Text>();
            TMP_Text timeText = entry.transform.Find("TimeText").GetComponent<TMP_Text>();
            TMP_Text accuracyText = entry.transform.Find("AccuText").GetComponent<TMP_Text>();

            if (rankText == null) Debug.LogError("RankText not found in prefab!");
            if (userText == null) Debug.LogError("UserText not found in prefab!");
            if (scoreText == null) Debug.LogError("ScoreText not found in prefab!");
            if (timeText == null) Debug.LogError("TimeText not found in prefab!");
            if (accuracyText == null) Debug.LogError("AccuText not found in prefab!");

            rankText.text = (i + 1).ToString(); // Convert int to string
            scoreText.text = score.scoreValue.ToString(); // Convert int to string
            timeText.text = score.roundTime.ToString(); // Convert float to string
            accuracyText.text = score.averageAccuracy.ToString() + "%"; // Convert float to string
            // Look up the username from userDictionary using userId
            if (userDictionary.TryGetValue(score.userId, out string username))
            {
                userText.text = username;  // Set the username instead of userId
            }
            else
            {
                userText.text = "Unknown (" + score.userId + ")";  // Fallback in case the user isn't found
            }
        }
    }
    public void ToggleGlobalHighscores()
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
            FetchAndDisplayHighscores(); // Always fetch global scores when opening
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
