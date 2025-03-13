using System;
using System.Collections.Generic;

[System.Serializable]
public class  WeaponRequest
{
    public string Name { get; set; }
    public int Price { get; set; }
    public float ReloadSpeed { get; set; }
    public int MagSize { get; set; }
    public int FireRate { get; set; }
    public FireMode FireMode { get; set; }
    public int WeaponTypeId { get; set; }

}
[System.Serializable]
public class WeaponResponse
{
    public int WeaponId { get; set; }
    public string Name { get; set; }
    public int Price { get; set; }
    public float ReloadSpeed { get; set; }
    public int MagSize { get; set; }
    public int FireRate { get; set; }
    public FireMode FireMode { get; set; }
    public Weapon_WeaponTypeResponse WeaponType { get; set; }
}
[System.Serializable]
public class Weapon_WeaponTypeResponse
{
    public int WeaponTypeId { get; set; }
    public string Name { get; set; }
    public EquipmentSlot EquipmentSlot { get; set; }
}
[System.Serializable]
public class WeaponResponseList
{
    public List<WeaponResponse> Weapons;
}