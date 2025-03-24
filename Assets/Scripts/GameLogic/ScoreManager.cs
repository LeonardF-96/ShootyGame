using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System;

public class ScoreManager : MonoBehaviour
{
    private string baseUrl = "https://localhost:7154/api/";

    public IEnumerator SendScoreToApi(ScoreRequest scoreRequest)
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

        string json = JsonUtility.ToJson(scoreRequest);
        byte[] bodyRaw = Encoding.UTF8.GetBytes(json);

        using (UnityWebRequest request = new UnityWebRequest(baseUrl + "score", "POST"))
        {
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("Authorization", "Bearer " + token);

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError($"Error: {request.error}");
            }
            else if (request.responseCode == 500)
            {
                Debug.LogError($"Server Error: {request.downloadHandler.text}");
            }
            else
            {
                Debug.Log("Score successfully sent to the API.");
                Debug.Log(request.downloadHandler.text);
            }
        }
    }

    public void FetchAllScores(Action<List<ScoreResponse>> callback)
    {
        StartCoroutine(GetAllScores(callback));
        Debug.Log("Fetching all scores...");
    }

    private IEnumerator GetAllScores(Action<List<ScoreResponse>> callback)
    {
        Debug.Log("Requesting URL: " + baseUrl + "Score");
        using (UnityWebRequest request = UnityWebRequest.Get(baseUrl + "Score"))
        {
            string token = PlayerPrefs.GetString("authToken");
            Debug.Log("Auth Token : " + token);
            if (!string.IsNullOrEmpty(token))
            {
                request.SetRequestHeader("Authorization", "Bearer " + token);
            }
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError("API Request Failed: " + request.error);
                callback?.Invoke(null);
            }
            else
            {
                Debug.Log("API Response: " + request.downloadHandler.text);
                try
                {
                    string json = "{\"items\":" + request.downloadHandler.text + "}";
                    Debug.Log("Wrapped JSON: " + json);
                    Wrapper<ScoreResponse> wrapper = JsonUtility.FromJson<Wrapper<ScoreResponse>>(json);
                    callback?.Invoke(wrapper.items);
                }
                catch (Exception ex)
                {
                    Debug.LogError("JSON parse error: " + ex.Message);
                    callback?.Invoke(null);
                }
            }
        }
    }

    public void FetchUserScores(int userId)
    {
        StartCoroutine(GetUserScores(userId));
    }

    private IEnumerator GetUserScores(int userId)
    {
        string endpoint = $"User/";
        using (UnityWebRequest request = UnityWebRequest.Get(baseUrl + endpoint + userId))
        {
            string token = PlayerPrefs.GetString("authToken");
            if (!string.IsNullOrEmpty(token))
            {
                request.SetRequestHeader("Authorization", "Bearer " + token);
            }

            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"Error fetching user scores: {request.error}");
                yield break;
            }

            string json = request.downloadHandler.text;
            Debug.Log($"User scores JSON received: {json}");

            try
            {
                List<ScoreResponse> userScores = JsonUtility.FromJson<List<ScoreResponse>>(json);
                Debug.Log($"Fetched {userScores.Count} user scores.");

                // Handle the user scores as needed
                // For example, you can pass them to a callback or store them in a list
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to parse user score data: {ex.Message}");
            }
        }
    }

    private class Wrapper<T>
    {
        public List<T> items;
    }
}