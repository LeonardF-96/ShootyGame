using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Linq;
using System;
using System.Text;

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
    public IEnumerator CreateWeapon(WeaponRequest weaponRequest, Action<bool> callback)
    {
        string json = JsonConvert.SerializeObject(weaponRequest);
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(json);

        using (UnityWebRequest request = new UnityWebRequest($"{baseUrl}Weapon", "POST"))
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
                Debug.LogError($"Error creating weapon: {request.error}");
                callback?.Invoke(false);
            }
            else
            {
                Debug.Log("Weapon created successfully.");
                callback?.Invoke(true);
            }
        }
    }
    public IEnumerator UpdateWeaponById(int weaponId, WeaponRequest updatedWeaponRequest, Action<bool> callback)
    {
        string token = PlayerPrefs.GetString("authToken", null);
        if (string.IsNullOrEmpty(token))
        {
            Debug.LogError("No authToken found in PlayerPrefs.");
            callback?.Invoke(false);
            yield break;
        }

        string json = JsonConvert.SerializeObject(updatedWeaponRequest);
        byte[] bodyRaw = Encoding.UTF8.GetBytes(json);

        using (UnityWebRequest request = new UnityWebRequest(baseUrl + "Weapon/" + weaponId, "PUT"))
        {
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("Authorization", "Bearer " + token);

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError($"Error: {request.error}");
                callback?.Invoke(false);
            }
            else if (request.responseCode == 500)
            {
                Debug.LogError($"Server Error: {request.downloadHandler.text}");
                callback?.Invoke(false);
            }
            else
            {
                Debug.Log("Weapon successfully updated.");
                callback?.Invoke(true);
            }
        }
    }
    public IEnumerator DeleteWeaponById(int weaponId, Action<bool> callback)
    {
        string token = PlayerPrefs.GetString("authToken", null);
        if (string.IsNullOrEmpty(token))
        {
            Debug.LogError("No authToken found in PlayerPrefs.");
            callback?.Invoke(false);
            yield break;
        }

        using (UnityWebRequest request = UnityWebRequest.Delete(baseUrl + "Weapon/" + weaponId))
        {
            request.SetRequestHeader("Authorization", "Bearer " + token);

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError($"Error: {request.error}");
                callback?.Invoke(false);
            }
            else if (request.responseCode == 500)
            {
                Debug.LogError($"Server Error: {request.downloadHandler.text}");
                callback?.Invoke(false);
            }
            else
            {
                Debug.Log("Weapon successfully deleted.");
                callback?.Invoke(true);
            }
        }
    }
    private IEnumerator GetAllWeapons(Action<List<WeaponResponse>> callback)
    {
        Debug.Log("GetAllWeapons called");
        // Create a new UnityWebRequest for a GET request to fetch all weapons
        using (UnityWebRequest request = UnityWebRequest.Get($"{baseUrl}Weapon"))
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
                fetchedWeapons = JsonConvert.DeserializeObject<List<WeaponResponse>>(json);
                // Ensure fireMode strings are converted to the correct enum
                if (fetchedWeapons != null)
                {
                    Debug.Log($"Fetched {fetchedWeapons.Count} weapons.");
                    callback?.Invoke(fetchedWeapons);
                }
                else
                {
                    Debug.LogError("Deserialized weapon list is null.");
                    callback?.Invoke(null);
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to parse weapon data: {ex.Message}");
                callback?.Invoke(null);
            }
        }
    }
    //Method to convert FireMode string to enum
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
    public static EquipmentSlot ParseEquipmentSlot(string equipmentSlotString)
    {
        // Try to parse the string value to the EquipmentSlot enum
        if (Enum.TryParse(equipmentSlotString, true, out EquipmentSlot equipmentSlot))
        {
            return equipmentSlot;
        }
        else
        {
            return EquipmentSlot.Primary; // Default to Primary if parsing fails
        }
    }
    public void FetchUserWeapons(int userId, Action<List<User_WeaponResponse>> callback)
    {
        StartCoroutine(GetUserWeapons(userId, callback));
    }
    public void BuyUserWeapon(int userId, int weaponId, Action<bool> callback)
    {
        StartCoroutine(PostBuyUserWeapon(userId, weaponId, callback));
    }
    private IEnumerator PostBuyUserWeapon(int userId, int weaponId, Action<bool> callback)
    {
        string endpoint = "User/weapons";
        string url = $"{baseUrl}{endpoint}";

        // Create a new UnityWebRequest for a POST request
        using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
        {
            // Create the JSON payload
            var payload = new
            {
                userId = userId,
                weaponId = weaponId
            };
            string jsonPayload = JsonConvert.SerializeObject(payload);

            // Set the request body
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonPayload);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

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
                // Log the error message and invoke the callback with false
                Debug.LogError($"Error buying weapon: {request.error}");
                callback?.Invoke(false);
            }
            else
            {
                // Log success and invoke the callback with true
                Debug.Log("Weapon bought successfully.");
                callback?.Invoke(true);
            }
        }
    }
    private IEnumerator GetUserWeapons(int userId, Action<List<User_WeaponResponse>> callback)
    {
        string endpoint = $"User/{userId}";
        using (UnityWebRequest request = UnityWebRequest.Get(baseUrl + endpoint))
        {
            string token = PlayerPrefs.GetString("authToken");
            if (!string.IsNullOrEmpty(token))
            {
                request.SetRequestHeader("Authorization", "Bearer " + token);
            }

            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"Error fetching user weapons: {request.error}");
                callback?.Invoke(null);
                yield break;
            }

            string json = request.downloadHandler.text;
            Debug.Log($"User weapons JSON received: {json}");

            try
            {
                UserResponse userResponse = JsonConvert.DeserializeObject<UserResponse>(json);
                List<User_WeaponResponse> userWeapons = userResponse?.weapons ?? new List<User_WeaponResponse>();
                callback?.Invoke(userWeapons);
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to parse user weapon data: {ex.Message}");
                callback?.Invoke(new List<User_WeaponResponse>());
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
    //WeaponType///////////////////////////////////
    public void FetchAllWeaponTypes(Action<List<Weapon_WeaponTypeResponse>> callback)
    {
        StartCoroutine(GetAllWeaponTypes(callback));
        Debug.Log("Fetching all weapon types...");
    }
    private IEnumerator GetAllWeaponTypes(Action<List<Weapon_WeaponTypeResponse>> callback)
    {
        Debug.Log("GetAllWeaponTypes called");
        // Create a new UnityWebRequest for a GET request to fetch all weapon types
        using (UnityWebRequest request = UnityWebRequest.Get($"{baseUrl}WeaponType"))
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
                Debug.LogError($"Error fetching weapon types: {request.error}");
                callback?.Invoke(null);
                yield break;
            }
            // Retrieve the JSON response from the server
            string json = request.downloadHandler.text;
            Debug.Log($"Raw JSON received: {json}");
            try
            {
                List<Weapon_WeaponTypeResponse> weaponTypes = JsonConvert.DeserializeObject<List<Weapon_WeaponTypeResponse>>(json);
                if (weaponTypes != null)
                {
                    Debug.Log($"Fetched {weaponTypes.Count} weapon types.");
                    callback?.Invoke(weaponTypes);
                }
                else
                {
                    Debug.LogError("Deserialized weapon type list is null.");
                    callback?.Invoke(null);
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to parse weapon type data: {ex.Message}");
                callback?.Invoke(null);
            }
        }
    }
    //method to create weapon type
    public IEnumerator CreateWeaponType(WeaponTypeRequest weaponTypeRequest, Action<bool> callback)
    {
        string json = JsonConvert.SerializeObject(weaponTypeRequest);
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(json);
        using (UnityWebRequest request = new UnityWebRequest($"{baseUrl}WeaponType", "POST"))
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
                Debug.LogError($"Error creating weapon type: {request.error}");
                callback?.Invoke(false);
            }
            else
            {
                Debug.Log("Weapon type created successfully.");
                callback?.Invoke(true);
            }
        }
    }
    public IEnumerator UpdateWeaponTypeById(int weaponTypeId, WeaponTypeRequest updatedWeaponTypeRequest, Action<bool> callback)
    {
        string token = PlayerPrefs.GetString("authToken", null);
        if (string.IsNullOrEmpty(token))
        {
            Debug.LogError("No authToken found in PlayerPrefs.");
            callback?.Invoke(false);
            yield break;
        }

        string json = JsonConvert.SerializeObject(updatedWeaponTypeRequest);
        byte[] bodyRaw = Encoding.UTF8.GetBytes(json);

        using (UnityWebRequest request = new UnityWebRequest(baseUrl + "WeaponType/" + weaponTypeId, "PUT"))
        {
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("Authorization", "Bearer " + token);

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError($"Error: {request.error}");
                callback?.Invoke(false);
            }
            else if (request.responseCode == 500)
            {
                Debug.LogError($"Server Error: {request.downloadHandler.text}");
                callback?.Invoke(false);
            }
            else
            {
                Debug.Log("Weapon type successfully updated.");
                callback?.Invoke(true);
            }
        }
    }
    public IEnumerator DeleteWeaponTypeById(int weaponTypeId, Action<bool> callback)
    {
        string token = PlayerPrefs.GetString("authToken", null);
        if (string.IsNullOrEmpty(token))
        {
            Debug.LogError("No authToken found in PlayerPrefs.");
            callback?.Invoke(false);
            yield break;
        }

        using (UnityWebRequest request = UnityWebRequest.Delete(baseUrl + "WeaponType/" + weaponTypeId))
        {
            request.SetRequestHeader("Authorization", "Bearer " + token);

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError($"Error: {request.error}");
                callback?.Invoke(false);
            }
            else if (request.responseCode == 500)
            {
                Debug.LogError($"Server Error: {request.downloadHandler.text}");
                callback?.Invoke(false);
            }
            else
            {
                Debug.Log("Weapon type successfully deleted.");
                callback?.Invoke(true);
            }
        }
    }
}