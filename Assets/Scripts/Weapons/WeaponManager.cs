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
        using (UnityWebRequest request = UnityWebRequest.Get(baseUrl + "weapon"))
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
                callback?.Invoke(null); // Invoke callback with null on error
            }
            else
            {
                Debug.Log(request.downloadHandler.text);
                // Parse JSON array response
                string json = "{\"items\":" + request.downloadHandler.text + "}";
                Wrapper<WeaponResponse> wrapper = JsonUtility.FromJson<Wrapper<WeaponResponse>>(json);
                fetchedWeapons = wrapper.items; // Store fetched weapons
                callback?.Invoke(fetchedWeapons);
            }
        }
    }

    public WeaponResponse GetWeaponById(int weaponId)
    {
        return fetchedWeapons?.FirstOrDefault(weapon => weapon.WeaponId == weaponId);
    }

    [System.Serializable]
    private class Wrapper<T>
    {
        public List<T> items;
    }
}