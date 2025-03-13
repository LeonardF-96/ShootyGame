using System;
using System.Collections.Generic;

[System.Serializable]
public class UserRequest
{
    public string Username { get; set; }
    public string Email { get; set; }
    public string Password { get; set; }
    public int Role { get; set; }
}

[System.Serializable]
public class UserResponse
{
    public int UserId { get; set; }
    public string Email { get; set; }
    public string UserName { get; set; }
    public string PlayerTag { get; set; }
    public int Money { get; set; }
    public string Role { get; set; }
    public List<User_WeaponResponse> Weapons { get; set; } = new();
    public List<User_ScoreResponse> Scores { get; set; } = new();
}

[System.Serializable]
public class SignInResponse
{
    public int UserId { get; set; }
    public string Email { get; set; }
    public string UserName { get; set; }
    public string PlayerTag { get; set; }
    public int Money { get; set; }
    public string Role { get; set; }
    public string Token { get; set; }
    public List<User_WeaponResponse> Weapons { get; set; } = new();
    public List<User_ScoreResponse> Scores { get; set; } = new();
}

public class User_WeaponResponse
{
    public int WeaponId { get; set; }
    public string Name { get; set; }
    public int Price { get; set; }
    public float ReloadSpeed { get; set; }
    public int MagSize { get; set; }
    public float FireRate { get; set; }
    public FireMode FireMode { get; set; }
    public User_Weapon_WeaponTypeResponse WeaponType { get; set; }
}

public class User_Weapon_WeaponTypeResponse
{
    public int WeaponTypeId { get; set; }
    public string Name { get; set; }
    public EquipmentSlot EquipmentSlot { get; set; }
}

public class User_ScoreResponse
{
    public int ScoreId { get; set; }
    public int UserId { get; set; }
    public int ScoreValue { get; set; }
    public float RoundTime { get; set; }
    public float AverageAccuracy { get; set; }
}

[System.Serializable]
public class User_ResponseList
{
    public List<UserResponse> Users { get; set; }
}
