using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class FriendRequest
{
    public int requesterId;
    public int receiverId;
}
[System.Serializable]
public class FriendRequestResponse
{
    public int friendRequestId;
    public Request_UserFriend requester;
    public Request_UserFriend receiver;
    public status status;
}
[System.Serializable]
public class FriendRequestResponseList
{
    public List<FriendRequestResponse> friendRequests;
}
