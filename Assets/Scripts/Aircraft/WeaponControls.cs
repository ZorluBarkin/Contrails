/*  
 * Copyright October 2023 Barkın Zorlu 
 * All rights reserved. 
 */

using Godot;
using System;

public partial class WeaponControls : Node
{
	[Export] private Camera3D camera = null;
	public WeaponType selectedWeaponType = WeaponType.Empty; // enums can be negative or exceed the limit need a wrap around
	[Export] private Node3D[] guns; // always ready
	
	// Plyons
	[Export] private Pylon[] leftPylons; // selected via weapon type
	[Export] private Pylon[] centralPylons; // selected via weapon type
	[Export] private Pylon[] rightPylons; // selected via weapon type
	[Export] private Pylon leftWingtip; // selected via weapon type
	[Export] private Pylon rightWingtip; // selected via weapon type
	

	private string[][][] ordinancePlacement;
	private string[][] leftOrdinanceNames; // index 0
	private string[][] centralOrdinanceNames; // index 1
	private string[][] rightOrdinanceNames; // index 2

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		//GD.Print("Name: " + leftPylons[0].availableOrdinance[0].);
		//GD.Print(leftPylons[0].availableOrdinance[0].GetState().GetNodeName(0)); // works
		SetWeaponNames();
		//GD.Print(ordinancePlacement[0][0][0]); // example access
		camera = Settings.camera;
		//if(camera == null)
		//	for(int i = 0; i < GetParent().GetChildCount(); i++)
		//		if(GetParent().GetChild(i).HasMeta("MainCamera"))
		//			if(GetParent().GetChild(i).GetMeta("MainCamera").AsBool())
		//				camera = GetNode<Camera3D>(GetParent().GetChild(i).GetPath());
		
		
		//PlaceWeapon();
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		
	}

	private void SetWeaponNames()
	{
		// Left Ordinance Arrays
		leftOrdinanceNames = new string[leftPylons.Length][];
		for(int i = 0; i < leftPylons.Length; i++)
		{
			leftOrdinanceNames[i] = new string[leftPylons[i].availableOrdinance.Length];

			for(int j = 0; j < leftPylons[i].availableOrdinance.Length; j++)
			{
				leftOrdinanceNames[i][j] = leftPylons[i].availableOrdinance[j].GetState().GetNodeName(0);
			}
		}

		// Central Ordinance Arrays
		centralOrdinanceNames = new string[centralPylons.Length][];
		for(int i = 0; i < centralPylons.Length; i++)
		{
			centralOrdinanceNames[i] = new string[centralPylons[i].availableOrdinance.Length];

			for(int j = 0; j < centralPylons[i].availableOrdinance.Length; j++)
			{
				centralOrdinanceNames[i][j] = centralPylons[i].availableOrdinance[j].GetState().GetNodeName(0);
			}
		}

		// Right Ordinance Arrays
		rightOrdinanceNames = new string[rightPylons.Length][];
		for(int i = 0; i < rightPylons.Length; i++)
		{
			rightOrdinanceNames[i] = new string[rightPylons[i].availableOrdinance.Length];

			for(int j = 0; j < rightPylons[i].availableOrdinance.Length; j++)
			{
				rightOrdinanceNames[i][j] = rightPylons[i].availableOrdinance[j].GetState().GetNodeName(0);
			}
		}

		// Saving to All
		ordinancePlacement = new string[3][][];
		ordinancePlacement[0] = leftOrdinanceNames; // Left
		ordinancePlacement[1] = centralOrdinanceNames; // Central
		ordinancePlacement[2] = rightOrdinanceNames; // Right
	}

	private int pylonIndex = 0;
	[Export] private bool mirrored = false; // selected via weapon type
	private void PlaceWeapon()
	{
		//if(left pylon is selected)
		camera.Position = leftPylons[pylonIndex].Position;
		camera.Position += new Vector3(0,0,1);
		camera.LookAt(leftPylons[pylonIndex].GlobalPosition);
		GD.Print(leftPylons[pylonIndex].Position);

		if(mirrored)
		{
			
		}
		else
		{
			SpawnWeapon();
		}
	}

	private void SpawnWeapon()
	{

	}
}
