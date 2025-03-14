using System.Collections;
using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    public TMP_Text targetsLeftText;
    public TMP_Text stopwatchText;
    public TMP_Text AmmoLeftText;
    public Image ReloadIndicatorImage;

    public GameObject statsPanel; // Reference to the stats panel
    public TMP_Text accuracyText;
    public TMP_Text totalScoreText;
    public TMP_Text timeUsedText;
    public TMP_Text finalScoreText;
    public TMP_Text MoneyText;

    private int score = 0;
    private int hitScore = 0;
    private int totalTargets;
    private int totalFirstHits = 0;
    private int totalMisses;
    private float averageAccuracy;
    private float startTime;
    private float elapsedTime;
    private int MoneyEarned;

    private bool gameStarted = false;

    private const float maxTime = 90f; // Maximum time allowed for the game completion while still getting a bonus
    private const float fastestTime = 13f; // Fastest time to complete the game
    private const int maxBonusPoints = 1000; // Maximum time score achievable

    private ScoreManager scoreManager;


    void Awake()
    {
        // Singleton pattern
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            Debug.Log("GameManager instance set");
        }
        else
        {
            Destroy(gameObject);
            Debug.Log("Duplicate GameManager destroyed");
        }
    }
    void Start()
    {
        scoreManager = FindAnyObjectByType<ScoreManager>();
        // Check if instance exists
        if (instance == null)
        {
            Debug.LogError("ScoreManager not found in scene.");
        }


        statsPanel.SetActive(false); // Hide the stats panel

        totalTargets = FindObjectsOfType<Target>().Length;
        startTime = Time.time;
        UpdateUI();
    }
    void Update()
    {
        if(gameStarted)
        {
            UpdateStopwatch();
            UpdateAmmoLeftUI();
            // Check if all targets are hit
            if (totalFirstHits == totalTargets)
            {
                gameStarted = false;
                CalculateFinalScore();
                StartCoroutine(ShowStatsPanel());
            }
        }
    }
    // Update the stopwatch UI with the elapsed time since startTime
    private void UpdateStopwatch()
    {
        float StopWatchTime = Time.time - startTime;
        TimeSpan timeSpan = TimeSpan.FromSeconds(StopWatchTime);
        stopwatchText.text = timeSpan.ToString(@"mm\:ss\.fff"); // Show seconds with milliseconds
    }
    // Calculate the final score, including time-based bonus
    private void CalculateFinalScore()
    {
        elapsedTime = Time.time - startTime;
        int timeBonus = CalculateTimeBonus(elapsedTime);
        score += timeBonus;
        Debug.Log("Time Bonus: " + timeBonus);
        Debug.Log("Final Score: " + score);

        if (finalScoreText != null)
        {
            finalScoreText.text = "Final Score: " + score;
        }

        CalculateAccuracy();

        CalculateMoneyEarned(elapsedTime, timeBonus);
    }

    // Calculate a bonus score based on the elapsed time
    private int CalculateTimeBonus(float elapsedTime)
    {
        if (elapsedTime > maxTime)
        {
            return 0; // No bonus if the elapsed time exceeds the maximum time
        }

        // Calculate the bonus based on how close the player is to the fastest time
        float timeBonusFactor = Mathf.Clamp01((fastestTime / Mathf.Max(elapsedTime, 1f))); // Avoid division by zero
        int timeBonus = Mathf.RoundToInt(maxBonusPoints * timeBonusFactor);
        return timeBonus;
    }
    //Calculate accuracy
    public void CalculateAccuracy()
    {
        // Ensure there are no division by zero errors
        if (totalFirstHits + totalMisses > 0)
        {
            averageAccuracy = ((float)totalFirstHits / (totalFirstHits + totalMisses)) * 100f;
            Debug.Log("Accuracy: " + averageAccuracy.ToString("F2") + "%");
        }
        else
        {
            averageAccuracy = 0f; // Set accuracy to 0 if no shots have been fired
            Debug.Log("Accuracy: 0%");
        }
    }

    private void CalculateMoneyEarned(float elapsedTime, int timeBonus)
    {
        float accuracyBonus = Mathf.Clamp(averageAccuracy / 100f, 0f, 1f); // Assume accuracy is a percentage

        // Calculate money earned based on accuracy and time bonus
        int moneyEarned = Mathf.RoundToInt((accuracyBonus + timeBonus / (float)maxBonusPoints) * 100);
        MoneyEarned = Mathf.Clamp(moneyEarned, 0, 100); // Ensure money does not exceed 100
        Debug.Log("Money Earned: " + MoneyEarned);
    }

    public void AddScore(int points)
    {
        score += points;
        hitScore += points;
        totalFirstHits++;
        UpdateTargetsLeftUI();
        Debug.Log("Score added: " + points + ". Current score: " + score);
        Debug.Log("Total first hits: " + totalFirstHits);

        //if (totalFirstHits == totalTargets)
        //{
        //    CalculateAccuracy();
        //}
    }
    public void DeductScore(int points)
    {
        // Ensure score doesn't drop below 0
        score = Mathf.Max(score - points, 0);
        hitScore = Mathf.Max(hitScore - points, 0);
        totalMisses++;
        Debug.Log("Score deducted: " + points + ". Current score: " + score);
    }
    public int GetScore()
    {
        return score;
    }
    public int GetTotalFirstHits()
    {
        return totalFirstHits;
    }
    public int GetTotalMisses()
    {
        return totalMisses;
    }
    private void UpdateUI()
    {
        UpdateTargetsLeftUI();
        UpdateAmmoLeftUI();
        UpdateStopwatch();
    }
    private void UpdateAmmoLeftUI()
    {
        GunController gunController = FindObjectOfType<GunController>();
        if (gunController != null && AmmoLeftText != null)
        {
            AmmoLeftText.text = gunController.GetCurrentAmmo() + "/" + gunController.magSize;
        }
    }
    private void UpdateTargetsLeftUI()
    {
        if (targetsLeftText != null)
        {
            targetsLeftText.text = "Targets Left: " + (totalTargets - totalFirstHits);
        }
    }
    public void StartGame()
    {
        startTime = Time.time;
        gameStarted = true;
        Debug.Log("Game started at: " + startTime);
    }
    private IEnumerator ShowStatsPanel()
    {
        // Calculate and display the stats
        elapsedTime = Time.time - startTime;
        TimeSpan timeSpan = TimeSpan.FromSeconds(elapsedTime);

        if (statsPanel != null)
        {
            accuracyText.text = "Accuracy: " + averageAccuracy.ToString("F2") + "%";
            totalScoreText.text = "Total Score: " + hitScore;
            timeUsedText.text = "Time Used: " + timeSpan.ToString(@"mm\:ss\.fff");
            finalScoreText.text = "Final Score: " + score;
            MoneyText.text = "Money Earned: " + MoneyEarned;

            statsPanel.SetActive(true); // Show the stats panel
        }

        yield return new WaitForSeconds(5f);

        Debug.Log("Sending score to API...");
        // Send score to the API
        yield return StartCoroutine(SendScoreToApi());

        Debug.Log("Loading MainMenu scene...");
        // Ensure score was sent to API before loading the MainMenu scene
        Cursor.lockState = CursorLockMode.None; // Unlocks the cursor
        Cursor.visible = true; // Makes the cursor visible
        SceneManager.LoadScene("MainMenu"); // Load the MainMenu scene
        // Destroy GameManager instance after completing its job
        Destroy(gameObject);
    }
    private IEnumerator SendScoreToApi()
    {
        int userId = PlayerPrefs.GetInt("userId", -1);
        if (userId == -1)
        {
            Debug.LogError("No userId found in PlayerPrefs.");
            yield break;
        }

        string token = PlayerPrefs.GetString("authToken", null);
        if (string.IsNullOrEmpty(token))
        {
            Debug.LogError("No authToken found in PlayerPrefs.");
            yield break;
        }
        if (scoreManager == null)
        {
            Debug.LogError("scoreManager is null.");
            yield break;
        }
        float roundedElapsedTime = Mathf.Round(elapsedTime * 100f) / 100f; // Round to 2 decimal places
        float roundedAccuracy = Mathf.Round(averageAccuracy * 100f) / 100f; // Round to 2 decimal places

        ScoreRequest scoreRequest = new ScoreRequest
        {
            userId = userId,
            scoreValue = score,
            roundTime = roundedElapsedTime,
            averageAccuracy = roundedAccuracy
        };
        Debug.Log($"Score request: {scoreRequest}");
        Debug.Log("Sending score to API...");
        yield return StartCoroutine(scoreManager.SendScoreToApi(scoreRequest));
        Debug.Log("Score sent to API.");
    }
}

