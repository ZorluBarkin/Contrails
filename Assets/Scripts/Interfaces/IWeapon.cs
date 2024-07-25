using Godot;
using System;

public enum WeaponType
{
    Empty, // a weapon cannot be empty
    GunPod,
    IRM,
    SARH,
    ARH,
    BeamRider,
    SmallBomb,
    MediumBomb,
    LargeBomb,
    GBU,
    CBU,
    Napalm,
    SmallRocket,
    MediumRocket,
    LargeRocket,
    AGM,
    ARM,
    AShM,
    ExternalFuelTank
}

public interface IWeapon
{
    WeaponType weaponType {get;}
    void Initilize();
    void Activate(); // activate weapon on pylon, start search in AAMs release in bombs etc.
}
