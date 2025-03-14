using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using TMPro;

public class HighscoreManager : MonoBehaviour
{
    public Transform highscoreContent;
    public GameObject highscoreEntryPrefab;
    public Transform HS_Scroll_View;
    public GameObject highscorePanel;
    private ScoreManager scoreManager;
    private UserManager userManager;
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

        if (userManager == null)
        {
            Debug.LogError("UserManager not found in the scene!");
        }
    }

    public void FetchAndDisplayHighscores()
    {
        if (scoreManager != null)
        {
           Debug.Log("Fetching users and highscores...");
            StartCoroutine(FetchUsersAndScores());
        }
        else
        {
            Debug.LogError("ApiManager not found in the scene!");
        }
    }
    private IEnumerator FetchUsersAndScores()
    {
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

        // Now fetch and display scores
        scoreManager.FetchAllScores(scores =>
        {
            foreach (var score in scores)
            {
                string displayName = userDictionary.ContainsKey(score.userId)
                    ? userDictionary[score.userId]
                    : $"User {score.userId}"; // Fallback to "User [ID]" if username is missing

                Debug.Log($"Score: {score.scoreValue}, User: {displayName}");
            }

            DisplayHighscores(scores);
        });
    }
    private void DisplayHighscores(List<ScoreResponse> scores)
    {
        highscorePanel.SetActive(true);
        if (scores == null)
        {
            Debug.LogError("Failed to fetch highscores.");
            return;
        }

        Debug.Log("Displaying " + scores.Count + " highscores.");
        foreach (var score in scores)
        {
            Debug.Log("Score: " + score.scoreValue + " by user: " + score.userId);
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

            if (rankText == null) Debug.LogError("RankText not found in prefab!");
            if (userText == null) Debug.LogError("UserText not found in prefab!");
            if (scoreText == null) Debug.LogError("ScoreText not found in prefab!");

            rankText.text = (i + 1).ToString(); // Convert int to string
            scoreText.text = score.scoreValue.ToString(); // Convert int to string
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

    // Update is called once per frame
    void Update()
    {
        
    }
}
