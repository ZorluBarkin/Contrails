/*  
 * Copyright October 2023 Barkın Zorlu 
 * All rights reserved. 
 */

using System;
using System.ComponentModel.DataAnnotations;

public enum WeaponType
{
    Empty, // a weapon cannot be empty
    GunPod,
    IR,
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

public class Weapon
{
    
}