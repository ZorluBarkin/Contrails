/*  
 * Copyright October 2023 Barkın Zorlu 
 * All rights reserved. 
 */

using Godot;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;

public enum PlayerVehicleType
{
	Aircraft,
	GroundVehicle,
	Ship,
	StaticPlacement
}

public enum CombatState
{
	Idle,
	Active
}

public partial class GameManager : Node
{
	public static GameManager instance = null;
	
	//public Node3D[] AircraftArray;

	[Export] public CombatState combatState = CombatState.Idle;

	#region Scene variables

	private float timer = 0f;
	public float ActiveUpdateInterval = 3f; // start Scene update mothods every active combat seconds
	public float IdleUpdateInterval = 60f; // start Scene update mothods every idle active seconds

	// these are all arrays because they are nice to see in editor
	// if possible make these lists, it will make the methods faster as well
	[Export] public Node3D[] FriendlyUnitArray = null; // no aircraft
	[Export] public Node3D[] friendlyAircraftArray = null; // if friendly check for META "Friend" then add
	
	[Export] public Node3D[] enemyUnitArray = null; // no aircraft
	[Export] public Node3D[] enemyAircraftArray = null; // else add here
	
	// public Node3D[] neutralAircraftList = null: // civilian aircraft 

	#endregion

	#region Player Variables
	public Camera3D mainCamera = null;
	[Export] public Camera3D[] cameras;
	public Node3D playerVehicle = null;
	[Export] public PlayerVehicleType playerVehicleType = PlayerVehicleType.Aircraft;
	public VehicleControls vehicleControls = null;
	public FuelManager fuelManager = null;
	public Mechanization mechanization = null;
	// may change class the name to controls and change to aircarft inside
	
	#endregion

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		instance = this;

		mainCamera = FindMainCamera();
		playerVehicle = FindPlayerVehicle();
		vehicleControls = FindVehicleControls();
		fuelManager = FindAircraftFuelManager();
		mechanization = FindMechanization();

		if(!SetUnitLists(ref FriendlyUnitArray, ref friendlyAircraftArray, ref enemyUnitArray, ref enemyAircraftArray))
		{
			GD.PrintErr("Game Manager, failed to set units");
		}
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		switch(combatState)
		{
			case CombatState.Idle:
				InfoUpdate((float)delta, IdleUpdateInterval);
			break;
			case CombatState.Active:
				InfoUpdate((float)delta, ActiveUpdateInterval);
			break;
		}
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

	#region PLayer Vehicle Methods
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

	private FuelManager FindAircraftFuelManager()
	{
		if(playerVehicle != null)
			for(int i = 0; i < playerVehicle.GetChildCount(); i++)
				if(playerVehicle.GetChild(i) is FuelManager)
					return GetNode<FuelManager>(playerVehicle.GetChild(i).GetPath());
		
		GD.PrintErr("Player vehicle does not have a fuel manager!");
		return null;
	}

	private Mechanization FindMechanization()
	{
		if(playerVehicle != null)
			for(int i = 0; i < playerVehicle.GetChildCount(); i++)
				if(playerVehicle.GetChild(i) is Mechanization)
					return GetNode<Mechanization>(playerVehicle.GetChild(i).GetPath());

		GD.PrintErr("Player vehicle does not have mechanization!");
		return null;
	}
	#endregion

	#region Scene Methods

	private void InfoUpdate(float delta, float updateInterval) // may be made multithreaded in the future
	{
		timer += delta;
		if(timer >= updateInterval)
		{
			bool infoUpdateSuccess = true;
			infoUpdateSuccess &= SetUnitLists(ref FriendlyUnitArray, ref friendlyAircraftArray, ref enemyUnitArray, ref enemyAircraftArray);
			
			timer = 0f;
			if(!infoUpdateSuccess)
				GD.PrintErr("Game Manager, Info Update Failed");
		}
	}

	/// <summary>
	/// Search for every unit and aircraft, thos should be included in every scene info update
	/// </summary>
	/// <returns></returns>
	private static bool SetUnitLists(ref Node3D[] friendlyUnits, ref Node3D[] friendlyAircraft, 
									ref Node3D[] enemyUnits, ref Node3D[] enemyAircraft)
	{
		// Friendly
		List<Node3D> friendlyUnitList = new List<Node3D>();
		List<Node3D> friendlyAircraftList = new List<Node3D>();

		// Enemy
		List<Node3D> enemyUnitList = new List<Node3D>();
		List<Node3D> enemyAircraftList = new List<Node3D>();

		Node rootNode = instance.GetParent(); // instance.GetParent() will always be RootNode
		bool operativeSuccess = false; // if this stays false, operation failed.	

		for(int i = 0; i < rootNode.GetChildCount(); i++)
		{
			if(rootNode.GetChild(i).HasMeta("Aircraft")) // Is a vehicle // This META only exists on vehicles of any type
			{
				//if(rootNode.GetChild(i).GetInstanceId() == instance.playerVehicle.GetInstanceId()) // excemt player vehicle from these list
				//	continue;

				if(rootNode.GetChild(i).GetMeta("Aircraft").AsBool())
				{
					if(rootNode.GetChild(i).HasMeta("Friend"))
						if(rootNode.GetChild(i).GetMeta("Friend").AsBool())
							friendlyAircraftList.Add(rootNode.GetChild(i).GetNode<Node3D>(rootNode.GetChild(i).GetPath()));
						else
							enemyAircraftList.Add(rootNode.GetChild(i).GetNode<Node3D>(rootNode.GetChild(i).GetPath()));
				}
				else // it is not an aircraft
				{
					if(rootNode.GetChild(i).HasMeta("Friend"))
						if(rootNode.GetChild(i).GetMeta("Friend").AsBool())
							friendlyUnitList.Add(rootNode.GetChild(i).GetNode<Node3D>(rootNode.GetChild(i).GetPath()));
						else
							enemyUnitList.Add(rootNode.GetChild(i).GetNode<Node3D>(rootNode.GetChild(i).GetPath()));
				}
			}
		}
		
		friendlyUnits = friendlyUnitList.ToArray();
		friendlyAircraft = friendlyAircraftList.ToArray();

		enemyUnits = enemyUnitList.ToArray();
		enemyAircraft = enemyAircraftList.ToArray();
		
		operativeSuccess |= true;
		return operativeSuccess;
	}
	#endregion
}
