/*  
 * Copyright October 2023 Barkın Zorlu 
 * All rights reserved. 
 */

using Godot;
using System;
using System.Collections.Generic;

public partial class Pylon : MeshInstance3D
{
	
	[Export] private bool pylonExists = true; // if true pylon is visible

	[Export] public PackedScene[] availableOrdinance = null;
	//[Export] public string[] availableOrdinanceNames = null;
	public List<Node> loadedWeapons = new List<Node>();
	[Export] public int selectionIndex = 0;

	[Export] WeaponType loadedWeaponType = WeaponType.Empty;
	// A pylon can only have 1 type of weapon even if there is multiple

	[Export] bool spawn = false; // delete export when spawning is implemented
	[Export] public bool trigger = false; // delete export after controls done

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
				Node ordinance = availableOrdinance[selectionIndex].Instantiate();
				AddChild(ordinance); // temp
				//ordinance.GetNode<Node3D>(ordinance.GetPath()).RotateObjectLocal(Vector3.Up, Mathf.DegToRad(90f)); // face forward
				//wrong //GD.Print("weapon radius: " + ordinance.GetChild<CollisionShape3D>(1));
				
				ordinance.GetNode<Node3D>(ordinance.GetPath()).TranslateObjectLocal(Vector3.Down * (float) ordinance.GetMeta("Offset"));

				loadedWeapons.Add(ordinance);
				//AddChild(availableOrdinance[selectionIndex].Instantiate()); // temp
				SetWeaponType();
				GD.Print("pylon weapon: " + loadedWeaponType.ToString());
			}
		}
	}

	//private void SpawnWeapon()

	public void SetWeaponType()
	{
		//if(GetChildCount() > 0)
		//{
		//	loadedWeaponType = (WeaponType)GetChild(0).GetMeta("WeaponType").AsInt32();
		//}
		if(loadedWeapons.Count > 0)
		{
			loadedWeaponType = (WeaponType)GetChild(0).GetMeta("WeaponType").AsInt32();
		}
		else
		{
			loadedWeaponType = WeaponType.Empty;
		}
	}
}
