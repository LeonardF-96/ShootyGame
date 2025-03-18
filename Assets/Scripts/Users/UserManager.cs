using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;
using System;

public class UserManager : MonoBehaviour
{
    private string baseUrl = "https://localhost:7154/api/";

    public void FetchAllUsers(Action<List<UserResponse>> callback)
    {
        StartCoroutine(GetAllUsers(callback));
        Debug.Log("Fetching all users...");
    }

    private IEnumerator GetAllUsers(Action<List<UserResponse>> callback)
    {
        using (UnityWebRequest request = UnityWebRequest.Get(baseUrl + "User"))
        {
            string token = PlayerPrefs.GetString("authToken");
            if (!string.IsNullOrEmpty(token))
            {
                request.SetRequestHeader("Authorization", "Bearer " + token);
            }

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError(request.error);
                callback?.Invoke(null);
            }
            else
            {
                Debug.Log(request.downloadHandler.text);
                string json = "{\"items\":" + request.downloadHandler.text + "}";
                Wrapper<UserResponse> wrapper = JsonUtility.FromJson<Wrapper<UserResponse>>(json);
                callback?.Invoke(wrapper.items);
            }
        }
    }

    private class Wrapper<T>
    {
        public List<T> items;
    }

    public IEnumerator GetUserById(int userId, Action<UserResponse> callback)
    {
        using (UnityWebRequest request = UnityWebRequest.Get($"{baseUrl}/User/{userId}"))
        {
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError(request.error);
                callback?.Invoke(null);
            }
            else
            {
                Debug.Log(request.downloadHandler.text);
                UserResponse user = JsonUtility.FromJson<UserResponse>(request.downloadHandler.text);
                callback?.Invoke(user);
            }
        }
    }
    public IEnumerator FetchAndUpdateUserData(Action<bool> callback)
    {
        int userId = PlayerPrefs.GetInt("userId", -1);
        if (userId == -1)
        {
            Debug.LogError("No userId found in PlayerPrefs.");
            callback?.Invoke(false);
            yield break;
        }

        string url = $"{baseUrl}User/{userId}";

        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            string token = PlayerPrefs.GetString("authToken", null);
            if (!string.IsNullOrEmpty(token))
            {
                request.SetRequestHeader("Authorization", "Bearer " + token);
            }

            request.downloadHandler = new DownloadHandlerBuffer();

            Debug.Log($"Fetching fresh user money data from: {url}");
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError($"Error fetching user data: {request.error}");
                callback?.Invoke(false);
                yield break;
            }

            if (string.IsNullOrEmpty(request.downloadHandler.text))
            {
                Debug.LogWarning("Empty response received. Aborting update.");
                callback?.Invoke(false);
                yield break;
            }

            try
            {
                // Parse new user data
                UserResponse newUserData = JsonUtility.FromJson<UserResponse>(request.downloadHandler.text);

                // Retrieve existing user data without modifying other fields
                string existingUserDataJson = PlayerPrefs.GetString("userData", "{}");
                UserResponse existingUserData = JsonUtility.FromJson<UserResponse>(existingUserDataJson);

                // Only update money if valid
                if (newUserData != null)
                {
                    existingUserData.money = newUserData.money;

                    // Save updated user data
                    string updatedUserDataJson = JsonUtility.ToJson(existingUserData);
                    PlayerPrefs.SetString("userData", updatedUserDataJson);
                    PlayerPrefs.Save();

                    Debug.Log("User money updated successfully in PlayerPrefs.");
                }
                else
                {
                    Debug.LogError("Parsed newUserData is null! Aborting update.");
                    callback?.Invoke(false);
                    yield break;
                }

                // Update UI
                MainMenuController mainMenu = FindObjectOfType<MainMenuController>();
                if (mainMenu != null)
                {
                    mainMenu.UpdateUI();
                }
                else
                {
                    Debug.LogWarning("MainMenuController not found!");
                }

                callback?.Invoke(true);
            }
            catch (Exception e)
            {
                Debug.LogError($"Exception while updating money: {e.Message}");
                callback?.Invoke(false);
            }
        }
    }
}