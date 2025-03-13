using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using TMPro;
using System.Collections;

public class SignUpManager : MonoBehaviour
{
    public TMP_InputField userNameInput;
    public TMP_InputField emailInput;
    public TMP_InputField passwordInput;
    public Button ConfirmSignUpButton;
    public string signUpUrl = "https://localhost:7098/User/authenticate"; // Replace with your actual API endpoint

    void Start()
    {
        // Add listener to the ConfirmSignUpButton
        ConfirmSignUpButton.onClick.AddListener(OnConfirmSignUpButtonClicked);
    }

    void OnConfirmSignUpButtonClicked()
    {
        // When ConfirmSignUpButton is clicked, initiate sign up process
        string userName = userNameInput.text;
        string email = emailInput.text;
        string password = passwordInput.text;

        StartCoroutine(SignUp(userName, email, password));
    }

    IEnumerator SignUp(string userName, string email, string password)
    {
        // Log the content of the user request
        Debug.Log("Signing up with:");
        Debug.Log("UserName: " + userName);
        Debug.Log("Email: " + email);
        Debug.Log("Password: " + password);

        // Create a form and add fields
        WWWForm form = new WWWForm();
        form.AddField("UserName", userName);
        form.AddField("Email", email);
        form.AddField("Password", password);

        // Create a UnityWebRequest to send the form
        UnityWebRequest www = UnityWebRequest.Post(signUpUrl, form);

        // Send the request and wait for a response
        yield return www.SendWebRequest();

        // Check for errors
        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Sign Up Failed: " + www.error);
        }
        else
        {
            Debug.Log("Sign Up Successful: " + www.downloadHandler.text);
        }
    }
}
