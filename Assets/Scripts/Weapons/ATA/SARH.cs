/*  
 * Copyright October 2023 Barkın Zorlu 
 * All rights reserved. 
 */

using Godot;
using System;

public partial class SARH : AAM, IWeapon, IAAM
{
    public WeaponType weaponType {get;} = WeaponType.SARH;

    // SARH missiles will not track in burn time but will allign for the target then it will track
    // AIM-7e has a 10 degree seeker with a +-35 degree gimball allowance so, AIM-7E Gimbal limit is 45 Degrees
    // AIM-7F has a 55 degree gimbal limit

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	#region IWeapon Interface Methods
	
    public void Initilize()
    {
        throw new NotImplementedException();
    }

    public void Activate()
    {
        throw new NotImplementedException();
    }
	#endregion

	#region IAAM Interface Methods

    public bool LockTarget()
    {
        throw new NotImplementedException();
    }
    public void Launch()
    {
        throw new NotImplementedException();
    }
	#endregion
}
