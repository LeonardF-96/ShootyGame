using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FriendsManager : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] public GameObject friendsPanel;
    [SerializeField] public GameObject requestPanel;
    [SerializeField] public GameObject deleteFriendPanel;
    [SerializeField] public GameObject sendRequestPanel;

    [Header("Textfields")]
    [SerializeField] public GameObject usernameText;
    [SerializeField] public GameObject bestScoreText;
    [SerializeField] public GameObject avgAccuText;
    [SerializeField] public GameObject avgTimeText;

    [Header("Inputfields")]
    [SerializeField] public GameObject requestReceiverIdInput;

    [Header("Buttons")]
    [SerializeField] public Button addFriendButton;
    [SerializeField] public Button sendRequestButton;
    [SerializeField] public Button deleteFriendButton;
    [SerializeField] public Button friendRequestsButton;
    [SerializeField] public Button backButton;
    [SerializeField] public Button confirmDeleteButton;
    [SerializeField] public Button cancelDeleteButton;
    [SerializeField] public Button incomingFRButton;
    [SerializeField] public Button outgoingFRButton;

    [Header("Scroll View Elements")]
    [SerializeField] public GameObject friendEntryPrefab;
    [SerializeField] public Transform friendContent;
    [SerializeField] public GameObject friendScroll;

    [SerializeField] public GameObject friendScoreEntryPrefab;
    [SerializeField] public Transform friendScoreContent;
    [SerializeField] public GameObject friendScoreScroll;

    [SerializeField] public GameObject friendRequestEntryPrefab;
    [SerializeField] public GameObject outgoingRequestEntryPrefab;
    [SerializeField] public Transform friendRequestContent;
    [SerializeField] public GameObject friendRequestScroll;

    private UserManager userManager;
    private ImageAnimator imageAnimator;
    private int selectedFriendId;

    // Start is called before the first frame update
    void Start()
    {
        friendsPanel.SetActive(false);
        deleteFriendPanel.SetActive(false);
        requestPanel.SetActive(false);
        sendRequestPanel.SetActive(false);
        backButton.onClick.AddListener(OnBackButtonClicked);
        friendRequestsButton.onClick.AddListener(OnFriendRequestsButtonClicked);
        incomingFRButton.onClick.AddListener(OnIncomingFriendRequestsButtonClicked);
        outgoingFRButton.onClick.AddListener(OnOutgoingFriendRequestsButtonClicked);
        addFriendButton.onClick.AddListener(OnAddFriendButtonClicked);
        sendRequestButton.onClick.AddListener(OnSendRequestButtonClicked);
        deleteFriendButton.onClick.AddListener(OnDeleteButtonClicked);
        confirmDeleteButton.onClick.AddListener(OnConfirmDeleteButtonClicked);
        cancelDeleteButton.onClick.AddListener(OnCancelDeleteButtonClicked);
        userManager = FindObjectOfType<UserManager>();
        imageAnimator = deleteFriendPanel.GetComponentInChildren<ImageAnimator>();
    }

    // Update is called once per frame
    void Update()
    {

    }
    public void OnFriendsButtonClicked()
    {
        if (friendsPanel != null)
        {
            requestPanel.SetActive(false);
            sendRequestPanel.SetActive(false);
            friendsPanel.SetActive(!friendsPanel.activeSelf);
            if (friendsPanel.activeSelf)
            {
                if (friendsPanel.activeSelf)
                {
                    FetchAndDisplayFriendProfile();
                }
            }
        }
    }
    private void FetchAndDisplayFriendProfile()
    {
        int userId = PlayerPrefs.GetInt("userId", -1);
        if (userId == -1)
        {
            Debug.LogError("No userId found in PlayerPrefs.");
            return;
        }

        if (userManager == null)
        {
            userManager = FindObjectOfType<UserManager>();
        }

        if (userManager != null)
        {
            StartCoroutine(userManager.GetUserById(userId, userResponse =>
            {
                if (userResponse != null)
                {
                    PopulateFriendsList(userResponse.friends);
                }
                else
                {
                    Debug.LogError("Failed to fetch user profile.");
                }
            }));
        }
        else
        {
            Debug.LogError("UserManager not found in the scene!");
        }
    }
    private void PopulateFriendsList(List<FriendSummary> friends)
    {
        // Clear existing friend entries
        foreach (Transform child in friendContent)
        {
            Destroy(child.gameObject);
        }

        // Populate friend entries
        foreach (var friend in friends)
        {
            GameObject friendEntry = Instantiate(friendEntryPrefab, friendContent);
            friendEntry.SetActive(true);
            TMP_Text friendNameText = friendEntry.transform.Find("FriendNameText").GetComponent<TMP_Text>();
            Button button = friendEntry.GetComponent<Button>();

            if (button == null)
            {
                Debug.LogError("Button component not found in friend entry prefab.");
            }
            friendNameText.text = friend.userName;
            button.onClick.AddListener(() => OnFriendEntryClicked(friend));
        }
    }
    private void OnFriendEntryClicked(FriendSummary friend)
    {
        Debug.Log("Friend entry clicked: " + friend.userName + " ID: " + friend.userId);
        usernameText.GetComponent<TMP_Text>().text = friend.userName;

        if (friend.scores.Count > 0)
        {
            bestScoreText.GetComponent<TMP_Text>().text = "Best Score: " + friend.scores.Max(s => s.scoreValue).ToString();
            avgAccuText.GetComponent<TMP_Text>().text = "Average Precision: " + friend.scores.Average(s => s.averageAccuracy).ToString("F2") + "%";
            avgTimeText.GetComponent<TMP_Text>().text = "Average Time: " + friend.scores.Average(s => s.roundTime).ToString("F2") + "s";
        }
        else
        {
            bestScoreText.GetComponent<TMP_Text>().text = "Best Score: ";
            avgAccuText.GetComponent<TMP_Text>().text = "Average Precision: ";
            avgTimeText.GetComponent<TMP_Text>().text = "Average Time: ";
        }

        selectedFriendId = friend.userId;
        // Clear existing score entries
        ClearFriendScores();

        // Populate scores if available
        if (friend.scores.Count > 0)
        {
            PopulateFriendScores(friend.scores);
        }
    }
    private void ClearFriendScores()
    {
        Debug.Log("Clearing friend scores...");
        foreach (Transform child in friendScoreContent)
        {
            Debug.Log("Destroying child: " + child.gameObject.name);
            Destroy(child.gameObject);
        }
    }
    private void PopulateFriendScores(List<ScoreSummary> scores)
    {
        // Populate score entries
        for (int i = 0; i < scores.Count; i++)
        {
            var score = scores[i];
            GameObject scoreEntry = Instantiate(friendScoreEntryPrefab, friendScoreContent);
            scoreEntry.SetActive(true);
            TMP_Text scoreValueText = scoreEntry.transform.Find("ScoreText").GetComponent<TMP_Text>();
            TMP_Text roundTimeText = scoreEntry.transform.Find("TimeText").GetComponent<TMP_Text>();
            TMP_Text avgAccuracyText = scoreEntry.transform.Find("AccuText").GetComponent<TMP_Text>();
            TMP_Text rankText = scoreEntry.transform.Find("RankText").GetComponent<TMP_Text>();

            scoreValueText.text = score.scoreValue.ToString();
            roundTimeText.text = score.roundTime.ToString("F2");
            avgAccuracyText.text = score.averageAccuracy.ToString("F2");
            rankText.text = (i + 1).ToString(); // Rank starts from 1
        }
    }
    private void OnDeleteButtonClicked()
    {
        if (selectedFriendId == -1)
        {
            Debug.LogError("No friend selected for deletion.");
            return;
        }
        Debug.Log("Delete button clicked.");
        deleteFriendPanel.SetActive(true);
        if (imageAnimator != null)
        {
            imageAnimator.StartAnimation();
        }
    }
    private void OnConfirmDeleteButtonClicked()
    {
        Debug.Log("Confirm delete button clicked.");
        if (userManager == null)
        {
            userManager = FindObjectOfType<UserManager>();
        }
        if (userManager != null)
        {
            int userId = PlayerPrefs.GetInt("userId", -1);
            Debug.Log("userManager.DeleteFriendship called with userId: " + userId + " And selected friend: " + selectedFriendId);
            StartCoroutine(userManager.DeleteFriendship(userId, selectedFriendId, success =>
            {
                if (success)
                {
                    Debug.Log("Friendship deleted successfully.");
                    ClearFriendInfo();
                    FetchAndDisplayFriendProfile();
                    deleteFriendPanel.SetActive(false);
                }
                else
                {
                    Debug.LogError("Failed to delete friendship.");
                }
            }));
        }
        else
        {
            Debug.LogError("UserManager not found in the scene!");
        }
    }
    private void ClearFriendInfo()
    {
        usernameText.GetComponent<TMP_Text>().text = "";
        bestScoreText.GetComponent<TMP_Text>().text = "Best Score: ";
        avgAccuText.GetComponent<TMP_Text>().text = "Average Precision: ";
        avgTimeText.GetComponent<TMP_Text>().text = "Average Time: ";
        selectedFriendId = -1;
        ClearFriendScores();
    }
    private void OnCancelDeleteButtonClicked()
    {
        Debug.Log("Cancel delete button clicked.");
        deleteFriendPanel.SetActive(false);
    }
    private void OnBackButtonClicked()
    {
        Debug.Log("Back button clicked.");
        if (friendsPanel != null)
        {
            friendsPanel.SetActive(false);
        }
    }
    // Friend Requests - INCOMING AND OUTGOING //////////////////////////////////////////
    private void OnFriendRequestsButtonClicked()
    {
        Debug.Log("Friend requests button clicked.");
        if (requestPanel != null)
        {
            sendRequestPanel.SetActive(false);
            requestPanel.SetActive(!requestPanel.activeSelf);
            if (requestPanel.activeSelf)
            {
                FetchAndDisplayFriendRequests();
            }
        }
    }
    private void ClearRequestPanel()
    {
        // Clear existing friend request entries
        foreach (Transform child in friendRequestContent)
        {
            Destroy(child.gameObject);
        }
    }
    private void OnIncomingFriendRequestsButtonClicked()
    {
        FetchAndDisplayFriendRequests();
    }

    private void OnOutgoingFriendRequestsButtonClicked()
    {
        FetchAndDisplayOutgoingFriendRequests();
    }
    private void FetchAndDisplayFriendRequests()
    {
        int userId = PlayerPrefs.GetInt("userId", -1);
        if (userId == -1)
        {
            Debug.LogError("No userId found in PlayerPrefs.");
            return;
        }

        if (userManager == null)
        {
            userManager = FindObjectOfType<UserManager>();
        }

        if (userManager != null)
        {
            ClearRequestPanel();
            StartCoroutine(userManager.GetAllIncomingFriendRequestsById(userId, friendRequests =>
            {
                if (friendRequests != null)
                {
                    Debug.Log($"Fetched {friendRequests.Count} incoming friend requests.");
                    PopulateFriendRequestsList(friendRequests);
                }
                else
                {
                    Debug.LogError("Failed to fetch friend requests.");
                }
            }));
        }
        else
        {
            Debug.LogError("UserManager not found in the scene!");
        }
    }
    private void FetchAndDisplayOutgoingFriendRequests()
    {
        int userId = PlayerPrefs.GetInt("userId", -1);
        if (userId == -1)
        {
            Debug.LogError("No userId found in PlayerPrefs.");
            return;
        }

        if (userManager == null)
        {
            userManager = FindObjectOfType<UserManager>();
        }

        if (userManager != null)
        {
            ClearRequestPanel();
            StartCoroutine(userManager.GetAllOutgoingFriendRequestsById(userId, friendRequests =>
            {
                if (friendRequests != null)
                {
                    PopulateOutgoingFriendRequestsList(friendRequests);
                }
                else
                {
                    Debug.LogError("Failed to fetch outgoing friend requests.");
                }
            }));
        }
        else
        {
            Debug.LogError("UserManager not found in the scene!");
        }
    }

    private void PopulateFriendRequestsList(List<FriendRequestResponse> friendRequests)
    {
        // Clear existing friend request entries
        foreach (Transform child in friendRequestContent)
        {
            Destroy(child.gameObject);
        }

        // Populate friend request entries
        foreach (var request in friendRequests)
        {
            GameObject requestEntry = Instantiate(friendRequestEntryPrefab, friendRequestContent);
            requestEntry.SetActive(true);
            TMP_Text requestNameText = requestEntry.transform.Find("FriendRequestText").GetComponent<TMP_Text>();
            Button acceptButton = requestEntry.transform.Find("AcceptButton").GetComponent<Button>();
            Button declineButton = requestEntry.transform.Find("DenyButton").GetComponent<Button>();

            if (acceptButton == null || declineButton == null)
            {
                Debug.LogError("Accept or Decline button not found in friend request entry prefab.");
            }
            if (requestNameText == null)
            {
                Debug.LogError("FriendRequestText not found in friend request entry prefab.");
            }
            else
            {
                Debug.Log($"Setting requestNameText to: {request.requester.playerTag}");
                requestNameText.text = request.requester.playerTag;
            }
            acceptButton.onClick.AddListener(() => OnAcceptFriendRequestClicked(request.friendRequestId));
            declineButton.onClick.AddListener(() => OnDeclineFriendRequestClicked(request.friendRequestId));
        }
    }
    private void PopulateOutgoingFriendRequestsList(List<FriendRequestResponse> friendRequests)
    {
        // Clear existing friend request entries
        foreach (Transform child in friendRequestContent)
        {
            Destroy(child.gameObject);
        }

        // Populate outgoing friend request entries
        foreach (var request in friendRequests)
        {
            GameObject requestEntry = Instantiate(outgoingRequestEntryPrefab, friendRequestContent);
            requestEntry.SetActive(true);
            TMP_Text requestNameText = requestEntry.transform.Find("OutFriendRequestText").GetComponent<TMP_Text>();
            Button cancelButton = requestEntry.transform.Find("CancelRequestButton").GetComponent<Button>();

            if (cancelButton == null)
            {
                Debug.LogError("Cancel button not found in outgoing friend request entry prefab.");
            }
            if (requestNameText == null)
            {
                Debug.LogError("OutFriendRequestText not found in outgoing friend request entry prefab.");
            }
            else
            {
                Debug.Log($"Setting requestNameText to: {request.receiver.playerTag}");
                requestNameText.text = request.receiver.playerTag;
            }
            cancelButton.onClick.AddListener(() => OnCancelFriendRequestClicked(request.friendRequestId));
        }
    }

    private void OnAcceptFriendRequestClicked(int friendRequestId)
    {
        int userId = PlayerPrefs.GetInt("userId", -1);
        if (userId == -1)
        {
            Debug.LogError("No userId found in PlayerPrefs.");
            return;
        }

        if (userManager != null)
        {
            StartCoroutine(userManager.UpdateFriendRequestStatus(friendRequestId, FriendRequestStatus.Accepted, success =>
            {
                if (success)
                {
                    Debug.Log("Friend request accepted successfully.");
                    ClearRequestPanel();
                    FetchAndDisplayFriendRequests();

                    // Fetch updated user profile and populate friends list
                    StartCoroutine(userManager.GetUserById(userId, userResponse =>
                    {
                        if (userResponse != null)
                        {
                            PopulateFriendsList(userResponse.friends);
                        }
                        else
                        {
                            Debug.LogError("Failed to fetch updated user profile.");
                        }
                    }));
                }
                else
                {
                    Debug.LogError("Failed to accept friend request.");
                }
            }));
        }
        else
        {
            Debug.LogError("UserManager not found in the scene!");
        }
    }

    private void OnDeclineFriendRequestClicked(int friendRequestId)
    {
        int userId = PlayerPrefs.GetInt("userId", -1);
        if (userId == -1)
        {
            Debug.LogError("No userId found in PlayerPrefs.");
            return;
        }

        if (userManager != null)
        {
            StartCoroutine(userManager.UpdateFriendRequestStatus(friendRequestId, FriendRequestStatus.Declined, success =>
            {
                if (success)
                {
                    Debug.Log("Friend request declined successfully.");
                    FetchAndDisplayFriendRequests();
                    ClearRequestPanel();
                }
                else
                {
                    Debug.LogError("Failed to decline friend request.");
                }
            }));
        }
        else
        {
            Debug.LogError("UserManager not found in the scene!");
        }
    }
    private void OnCancelFriendRequestClicked(int friendRequestId)
    {
        int userId = PlayerPrefs.GetInt("userId", -1);
        if (userId == -1)
        {
            Debug.LogError("No userId found in PlayerPrefs.");
            return;
        }

        if (userManager != null)
        {
            StartCoroutine(userManager.UpdateFriendRequestStatus(friendRequestId, FriendRequestStatus.Canceled, success =>
            {
                if (success)
                {
                    Debug.Log("Friend request canceled successfully.");
                    FetchAndDisplayOutgoingFriendRequests();
                }
                else
                {
                    Debug.LogError("Failed to cancel friend request.");
                }
            }));
        }
        else
        {
            Debug.LogError("UserManager not found in the scene!");
        }
    }
    //Add Friends /////////////////////////////////////////////////////////////
    private void OnAddFriendButtonClicked()
    {
        Debug.Log("Add friend button clicked.");
        requestPanel.SetActive(false);
        sendRequestPanel.SetActive(!sendRequestPanel.activeSelf);
    }
    private void OnSendRequestButtonClicked()
    {
        string receiverIdText = requestReceiverIdInput.GetComponent<TMP_InputField>().text;
        if (string.IsNullOrEmpty(receiverIdText))
        {
            Debug.LogError("Receiver ID is empty.");
            return;
        }

        if (!int.TryParse(receiverIdText, out int receiverId))
        {
            Debug.LogError("Invalid Receiver ID.");
            return;
        }

        int userId = PlayerPrefs.GetInt("userId", -1);
        if (userId == -1)
        {
            Debug.LogError("No userId found in PlayerPrefs.");
            return;
        }

        if (userManager != null)
        {
            StartCoroutine(userManager.SendFriendRequest(userId, receiverId, success =>
            {
                if (success)
                {
                    Debug.Log("Friend request sent successfully.");
                    sendRequestPanel.SetActive(false);
                }
                else
                {
                    Debug.LogError("Failed to send friend request.");
                }
            }));
        }
        else
        {
            Debug.LogError("UserManager not found in the scene!");
        }
    }
}