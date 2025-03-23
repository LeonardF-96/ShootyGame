using System;
using System.Collections.Generic;

[System.Serializable]
public class WeaponRequest
{
    public string name;
    public int price;
    public float reloadSpeed;
    public int magSize;
    public int fireRate;
    public FireMode fireMode;
    public int weaponTypeId;
}

[System.Serializable]
public class WeaponResponse
{
    public int weaponId;
    public string name;
    public int price;
    public float reloadSpeed;
    public int magSize;
    public int fireRate;
    public FireMode fireMode;
    public Weapon_WeaponTypeResponse weaponType;
}
[System.Serializable]
public class Weapon_WeaponTypeResponse
{
    public int weaponTypeId;
    public string name;
    public EquipmentSlot equipmentSlot;
}

[System.Serializable]
public class WeaponResponseList
{
    public List<WeaponResponse> weapons;
}