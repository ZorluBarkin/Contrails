/*  
 * Copyright October 2023 Barkın Zorlu 
 * All rights reserved. 
 */

using Godot;
using System;

public partial class Pylon : MeshInstance3D
{
	
	[Export] private bool pylonExists = true; // if true pylon is visible

	[Export] public PackedScene[] availableOrdinance = null;
	//[Export] public string[] availableOrdinanceNames = null;

	WeaponType loadedWeaponType = WeaponType.Empty;
	// A pylon can only have 1 type of weapon even if there is multiple
	[Export] bool spawn = false; // delete export when spawning is implemented
	
	public bool trigger = false;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		if(!pylonExists)
		{
			this.Visible = false;
			loadedWeaponType = WeaponType.Empty;
		}
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if(pylonExists)
		{
			if(trigger)
			{

			}

			if(spawn)
			{
				spawn = false;
				//AddChild(mediumBomb.Instantiate()); // temp
				SetWeaponType();
			}
		}
	}

	//private void SpawnWeapon()

	public void SetWeaponType()
	{
		if(GetChildCount() > 0)
		{
			loadedWeaponType = (WeaponType)GetChild(0).GetMeta("WeaponType").AsInt32();
		}
		else
		{
			loadedWeaponType = WeaponType.Empty;
		}
	}
}
