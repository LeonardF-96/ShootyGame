using JetBrains.Annotations;
using System;
using System.Collections.Generic;

[System.Serializable]
public class UserRequest
{
    public string userName;
    public string email;
    public string password;
    public int role;
}

[System.Serializable]
public class UserResponse
{
    public int userId;
    public string email;
    public string userName;
    public string playerTag;
    public int money;
    public string role;
    public List<User_WeaponResponse> weapons = new();
    public List<User_ScoreResponse> scores = new();
    public List<FriendSummary> friends = new();
}

[System.Serializable]
public class SignInResponse
{
    public int userId;
    public string email;
    public string userName;
    public string playerTag;
    public int money;
    public string role;
    public string token;
}
[System.Serializable]
public class User_WeaponResponse
{
    public int weaponId;
    public string name;
    public int price;
    public float reloadSpeed;
    public int magSize;
    public int fireRate;
    public string fireMode;
    public User_Weapon_WeaponTypeResponse weaponType;
}
[System.Serializable]
public class User_Weapon_WeaponTypeResponse
{
    public int weaponTypeId;
    public string name;
    public EquipmentSlot equipmentSlot;
}
[System.Serializable]
public class User_ScoreResponse
{
    public int scoreId;
    public int userId;
    public int scoreValue;
    public float roundTime;
    public float averageAccuracy;
}
[System.Serializable]
public class FriendSummary
{
    public int userId;
    public string userName;
    public string playerTag;
    public List<ScoreSummary> scores = new();
}
[System.Serializable]
public class Request_UserFriend
{
    public int userId;
    public string userName;
    public string playerTag;
}
[System.Serializable]
public class User_ResponseList
{
    public List<UserResponse> users;
}