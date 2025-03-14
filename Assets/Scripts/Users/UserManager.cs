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
}