using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class GameStartManager : MonoBehaviour
{
    public Image countdown3;
    public Image countdown2;
    public Image countdown1;

    private float startTime; // Time when countdown ends
    private float endTime; // Time when 28 targets have been hit

    private GameManager gameManager; // Reference to the GameManager script
    public WeaponHandler weaponHandler; // Reference to the WeaponHandler script

    public PlayerController playerController; // Reference to the PlayerController script

    void Start()
    {
        countdown1.gameObject.SetActive(false);
        countdown2.gameObject.SetActive(false);
        countdown3.gameObject.SetActive(false);
        gameManager = GameManager.instance;
        if (weaponHandler == null)
        {
            Debug.LogError("WeaponHandler not assigned in GameStartManager!");
        }
        // Load weapon IDs from PlayerPrefs
        int primaryWeaponId = PlayerPrefs.GetInt("PrimaryWeaponId", -1); // -1 as default if nothing is saved
        int secondaryWeaponId = PlayerPrefs.GetInt("SecondaryWeaponId", -1);

        Debug.Log("Loaded Primary Weapon ID: " + primaryWeaponId);
        Debug.Log("Loaded Secondary Weapon ID: " + secondaryWeaponId);

        if (primaryWeaponId != -1 && secondaryWeaponId != -1)
        {
            Debug.Log("Yeah, that weapon id exists");
            weaponHandler.EquipWeapons(primaryWeaponId, secondaryWeaponId);
            Debug.Log("Weapons equipped!");
        }
        else
        {
            Debug.LogError("Failed to load weapon IDs — check if they're being saved correctly!");
        }
        StartCoroutine(StartCountdown());
    }

    private IEnumerator StartCountdown()
    {
        Debug.Log("Starting countdown...");
        // Disable player movement during countdown
        playerController.DisablePlayerMovement();
        // Show countdown3 image
        countdown3.gameObject.SetActive(true);
        yield return new WaitForSeconds(1f);
        countdown3.gameObject.SetActive(false);

        // Show countdown2 image
        countdown2.gameObject.SetActive(true);
        yield return new WaitForSeconds(1f);
        countdown2.gameObject.SetActive(false);

        // Show countdown1 image
        countdown1.gameObject.SetActive(true);
        yield return new WaitForSeconds(1f);
        countdown1.gameObject.SetActive(false);
        // Enable player movement after countdown
        playerController.EnablePlayerMovement();

        // Start the game
        gameManager.StartGame();
    }
    void Update()
    {
        // Check if 28 targets have been hit
        if (GameManager.instance.GetTotalFirstHits() >= 28 && endTime == 0)
        {
            // Record end time when 28 targets are hit
            endTime = Time.time;
            CalculateTimeTaken();
        }
    }
    private void CalculateTimeTaken()
    {
        float timeTaken = endTime - startTime;
        Debug.Log("Time taken to hit 28 targets: " + timeTaken.ToString("F2") + " seconds");
    }
}
