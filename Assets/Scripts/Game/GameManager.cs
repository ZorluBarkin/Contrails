/*  
 * Copyright October 2023 Barkın Zorlu 
 * All rights reserved. 
 */

using Godot;
using System;

public partial class GameManager : Node
{
	public static GameManager instance = null;

	public Camera3D mainCamera = null;
	[Export] public Camera3D[] cameras;
	public Node3D playerVehicle = null;
	public VehicleControls vehicleControls = null; 
	// may change class the name to controls and change to aircarft inside
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		instance = this;

		mainCamera = FindMainCamera();
		playerVehicle = FindPlayerVehicle();
		vehicleControls = FindVehicleControls();
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	private Camera3D FindMainCamera()
	{
		if(mainCamera == null)
			for(int i = 0; i < GetParent().GetChildCount(); i++)
				if(GetParent().GetChild(i).HasMeta("MainCamera"))
					if(GetParent().GetChild(i).GetMeta("MainCamera").AsBool())
						return GetNode<Camera3D>(GetParent().GetChild(i).GetPath());
		
		// will get here if no camera is marked as main.
		GD.Print("No main camera set.");
		return null;

	}
	private Node3D FindPlayerVehicle()
	{
		// search for player vehicle
		if(playerVehicle == null)
			for(int i = 0; i < GetParent().GetChildCount(); i++)
				if(GetParent().GetChild(i).HasMeta("PlayerVehicle"))
					if(GetParent().GetChild(i).GetMeta("PlayerVehicle").AsBool())
						return GetNode<Node3D>(GetParent().GetChild(i).GetPath());
		
		// will get here if no player vehicle is found
		GD.Print("Player does not have a vehicle.");
		return null;
	}

	private VehicleControls FindVehicleControls()
	{
		if(playerVehicle != null)
			for(int i = 0; i < playerVehicle.GetChildCount(); i++)
				if(playerVehicle.GetChild(i) is VehicleControls)
					return GetNode<VehicleControls>(playerVehicle.GetChild(i).GetPath());
		
		GD.PrintErr("Player vehicle is null!");
		return null;
	}
}
