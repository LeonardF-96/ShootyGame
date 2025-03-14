using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;

public class WeaponManager : MonoBehaviour
{
    private string baseUrl = "https://localhost:7154/api/";
    private List<WeaponResponse> fetchedWeapons;
    void Start()
    {
        // Initialize fetchedWeapons list
        fetchedWeapons = new List<WeaponResponse>();
    }
    public void FetchAllWeapons(Action<List<WeaponResponse>> callback)
    {
        StartCoroutine(GetAllWeapons(callback));
        Debug.Log("Fetching all weapons...");
    }

    private IEnumerator GetAllWeapons(Action<List<WeaponResponse>> callback)
    {
        Debug.Log("GetAllWeapons called");
        // Create a new UnityWebRequest for a GET request to fetch all weapons
        using (UnityWebRequest request = UnityWebRequest.Get($"{baseUrl}weapon"))
        {
            // Retrieve the authentication token from PlayerPrefs
            string token = PlayerPrefs.GetString("authToken");
            if (!string.IsNullOrEmpty(token))
            {
                // Set the Authorization header with the token
                request.SetRequestHeader("Authorization", "Bearer " + token);
            }

            // Send the request and wait for the response
            yield return request.SendWebRequest();

            // Check if the request was successful
            if (request.result != UnityWebRequest.Result.Success)
            {
                // Log the error message and invoke the callback with null
                Debug.LogError($"Error fetching weapons: {request.error}");
                callback?.Invoke(null);
                yield break;
            }

            // Retrieve the JSON response from the server
            string json = request.downloadHandler.text;
            Debug.Log($"Raw JSON received: {json}");

            try
            {
                fetchedWeapons = JsonHelper.FromJsonArray<WeaponResponse>(json);
                Debug.Log($"Fetched {fetchedWeapons.Count} weapons.");
                callback?.Invoke(fetchedWeapons);
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to parse weapon data: {ex.Message}");
                callback?.Invoke(null);
            }
        }
    }
    // Method to convert FireMode string to enum
    public static FireMode ParseFireMode(string fireModeString)
    {
        // Try to parse the string value to the FireMode enum
        if (Enum.TryParse(fireModeString, true, out FireMode fireMode))
        {
            return fireMode;
        }
        else
        {
            return FireMode.Single; // Default to Single if parsing fails
        }
    }
    public void FetchUserWeapons(int userId, Action<List<User_WeaponResponse>> callback)
    {
        StartCoroutine(GetUserWeapons(userId, callback));
    }

    private IEnumerator GetUserWeapons(int userId, Action<List<User_WeaponResponse>> callback)
    {
        // Define the endpoint for fetching user-specific weapons
        string endpoint = $"User/{userId}";

        // Create a new UnityWebRequest for a GET request to fetch user weapons
        using (UnityWebRequest request = UnityWebRequest.Get(baseUrl + endpoint))
        {
            // Retrieve the authentication token from PlayerPrefs
            string token = PlayerPrefs.GetString("authToken");
            if (!string.IsNullOrEmpty(token))
            {
                // Set the Authorization header with the token
                request.SetRequestHeader("Authorization", "Bearer " + token);
            }

            // Send the request and wait for the response
            yield return request.SendWebRequest();

            // Check if the request was successful
            if (request.result != UnityWebRequest.Result.Success)
            {
                // Log the error message
                Debug.LogError($"Error fetching user weapons: {request.error}");
                callback?.Invoke(null);
                yield break;
            }

            // Retrieve the JSON response from the server
            string json = request.downloadHandler.text;
            Debug.Log($"User weapons JSON received: {json}");

            try
            {
                // Parse the JSON response into the UserResponse class
                UserResponse userResponse = JsonUtility.FromJson<UserResponse>(json);
                List<User_WeaponResponse> userWeapons = userResponse.weapons;
                callback?.Invoke(userWeapons);
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to parse user weapon data: {ex.Message}");
                callback?.Invoke(null);
            }
        }
    }
    public WeaponResponse GetWeaponById(int weaponId)
    {
        if (fetchedWeapons == null || fetchedWeapons.Count == 0)
        {
            Debug.LogWarning("Weapon list is empty or not yet fetched.");
            return null;
        }

        WeaponResponse weapon = fetchedWeapons.FirstOrDefault(w => w.weaponId == weaponId);
        if (weapon == null)
        {
            Debug.LogWarning($"Weapon with ID {weaponId} not found.");
        }
        return weapon;
    }
}