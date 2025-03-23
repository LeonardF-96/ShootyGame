using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;
using System;
using Newtonsoft.Json;

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
                string responseText = request.downloadHandler.text;
                Debug.Log("API Response: " + responseText);
                try
                {
                    List<UserResponse> users = JsonConvert.DeserializeObject<List<UserResponse>>(responseText);
                    callback?.Invoke(users);
                }
                catch (Exception e)
                {
                    Debug.LogError($"JSON parse error: {e.Message}");
                    callback?.Invoke(null);
                }
            }
        }
    }

    private class Wrapper<T>
    {
        public List<T> items;
    }

    public IEnumerator GetUserById(int userId, Action<UserResponse> callback)
    {
        Debug.Log($"Fetching user with id: {userId}, auth token: " + PlayerPrefs.GetString("authToken"));
        using (UnityWebRequest request = UnityWebRequest.Get($"{baseUrl}User/{userId}"))
        {
            string token = PlayerPrefs.GetString("authToken");
            Debug.Log("Using token: " + token); // Add debug log for token
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
                UserResponse user = JsonConvert.DeserializeObject<UserResponse>(request.downloadHandler.text);
                callback?.Invoke(user);
            }
        }
    }
    public IEnumerator UpdateUserById(int userId, UserRequest user, Action<bool> callback)
    {
        string json = JsonConvert.SerializeObject(user);
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(json);
        using (UnityWebRequest request = new UnityWebRequest($"{baseUrl}User/{userId}", "PUT"))
        {
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            string token = PlayerPrefs.GetString("authToken");
            if (!string.IsNullOrEmpty(token))
            {
                request.SetRequestHeader("Authorization", "Bearer " + token);
            }

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError($"Error updating user profile: {request.error}");
                callback?.Invoke(false);
            }
            else
            {
                Debug.Log("User profile updated successfully.");
                callback?.Invoke(true);
            }
        }
    }
    public IEnumerator DeleteUserById(int userId, Action<bool> callback)
    {
        using (UnityWebRequest request = UnityWebRequest.Delete($"{baseUrl}User/{userId}"))
        {
            string token = PlayerPrefs.GetString("authToken");
            if (!string.IsNullOrEmpty(token))
            {
                request.SetRequestHeader("Authorization", "Bearer " + token);
            }

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError($"Error deleting user: {request.error}");
                callback?.Invoke(false);
            }
            else
            {
                Debug.Log("User deleted successfully.");
                callback?.Invoke(true);
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
            Debug.Log("Using token: " + token); // Add debug log for token
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
    //_Friendships//////////////////////////////////////////////////////////
    public IEnumerator DeleteFriendship(int userId, int friendId, Action<bool> callback)
    {
        using (UnityWebRequest request = UnityWebRequest.Delete($"{baseUrl}User/{userId}/friends/{friendId}"))
        {
            string token = PlayerPrefs.GetString("authToken");
            if (!string.IsNullOrEmpty(token))
            {
                request.SetRequestHeader("Authorization", "Bearer " + token);
            }
            yield return request.SendWebRequest();
            if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError($"Error deleting friendship: {request.error}");
                callback?.Invoke(false);
            }
            else
            {
                Debug.Log("Friendship deleted successfully.");
                callback?.Invoke(true);
            }
        }
    }
    public IEnumerator GetAllIncomingFriendRequestsById(int userId, Action<List<FriendRequestResponse>> callback)
    {
        using (UnityWebRequest request = UnityWebRequest.Get($"{baseUrl}FriendReq/receiver/{userId}"))
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
                try
                {
                    List<FriendRequestResponse> friendRequests = JsonConvert.DeserializeObject<List<FriendRequestResponse>>(request.downloadHandler.text);
                    callback?.Invoke(friendRequests);
                }
                catch (Exception e)
                {
                    Debug.LogError($"JSON parse error: {e.Message}");
                    callback?.Invoke(null);
                }
            }
        }
    }
    public IEnumerator GetAllOutgoingFriendRequestsById(int userId, Action<List<FriendRequestResponse>> callback)
    {
        using (UnityWebRequest request = UnityWebRequest.Get($"{baseUrl}FriendReq/requester/{userId}"))
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
                try
                {
                    List<FriendRequestResponse> friendRequests = JsonConvert.DeserializeObject<List<FriendRequestResponse>>(request.downloadHandler.text);
                    callback?.Invoke(friendRequests);
                }
                catch (Exception e)
                {
                    Debug.LogError($"JSON parse error: {e.Message}");
                    callback?.Invoke(null);
                }
            }
        }
    }
    public IEnumerator SendFriendRequest(int userId, int friendId, Action<bool> callback)
    {
        string url = $"{baseUrl}FriendReq";
        var requestData = new
        {
            requesterId = userId,
            receiverId = friendId
        };
        string json = JsonConvert.SerializeObject(requestData);
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(json);

        using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
        {
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            string token = PlayerPrefs.GetString("authToken");
            if (!string.IsNullOrEmpty(token))
            {
                request.SetRequestHeader("Authorization", "Bearer " + token);
            }

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError($"Error sending friend request: {request.error}");
                callback?.Invoke(false);
            }
            else
            {
                Debug.Log("Friend request sent successfully.");
                callback?.Invoke(true);
            }
        }
    }
    public IEnumerator UpdateFriendRequestStatus(int friendRequestId, FriendRequestStatus status, Action<bool> callback)
    {
        string url = $"{baseUrl}FriendReq/{friendRequestId}";
        var requestData = new
        {
            status = status.ToString()
        };
        string json = JsonConvert.SerializeObject(requestData);
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(json);

        using (UnityWebRequest request = new UnityWebRequest(url, "PUT"))
        {
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            string token = PlayerPrefs.GetString("authToken");
            if (!string.IsNullOrEmpty(token))
            {
                request.SetRequestHeader("Authorization", "Bearer " + token);
            }

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError($"Error updating friend request status: {request.error}");
                callback?.Invoke(false);
            }
            else
            {
                Debug.Log("Friend request status updated successfully.");
                callback?.Invoke(true);
            }
        }
    }
}

public enum FriendRequestStatus
{
    Pending,
    Accepted,
    Declined,
    Canceled
}