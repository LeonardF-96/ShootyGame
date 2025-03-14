using UnityEngine.SceneManagement;
using UnityEngine;

public class LogoutButton : MonoBehaviour
{
    public void OnLogoutButtonClick()
    {
        Debug.Log("Logout button clicked.");
        if (AuthenticationManager.instance != null)
        {
            PlayerPrefs.DeleteKey("authToken");
            PlayerPrefs.DeleteKey("userId");
            PlayerPrefs.SetString("userData", ""); // Ensure userData is blank
            PlayerPrefs.Save(); // Force save to make sure changes persist

            Debug.Log("Logged out successfully.");

            // Find MainMenuController and update UI
            MainMenuController mainMenuController = FindObjectOfType<MainMenuController>();
            if (mainMenuController != null)
            {
                mainMenuController.UpdateUI(); // Ensure UI reflects logout
            }
        }
        else
        {
            Debug.LogError("AuthenticationManager.instance is null. Ensure ApiManager is properly initialized.");
        }
    }
}
