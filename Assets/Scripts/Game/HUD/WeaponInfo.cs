/*  
 * Copyright October 2023 Barkın Zorlu 
 * All rights reserved. 
 */

using Godot;
using System;

public partial class WeaponInfo : GridContainer
{
	private HUD hud = null;
	private WeaponControls weaponControls = null;
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		if(hud == null)
			hud = GetNode<HUD>(this.GetParent().GetPath());
		// Get player vehicle from HUD
		// Get weapon control from player vehicle
		// weaponControls = playerVehicle.weaponControls;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{

	}
}
