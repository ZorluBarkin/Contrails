/*  
 * Copyright October 2023 Barkın Zorlu 
 * All rights reserved. 
 */

using Godot;
using System;
using System.Diagnostics;

public partial class LandingGear : Node
{
	public bool actuateGears = false;

	#region Landing Gear Mechanization
	[Export] private Node3D landingGearModel;
	// From closed to open.
	public enum LandingGearDeployDirection
	{
		Forward, //-X
		Back, //+X
		Left, //+Z
		Right //-Z
	}
	[Export] private LandingGearDeployDirection landingGearDeployDirection;
	
	[Export] private float gearUpDegree;
	[Export] private float gearDownDegree;

	public enum GearPosition
	{
		Down,
		Up
	}
	[Export] private GearPosition gearPosition;
	[Export] private float gearDeploySpeed = 0.5f;

	public bool landingGearSet = true;
	private bool gearRotateDirectionIsUp = true;
	private float gearRotateDirection = -1f;
	private bool canActuateDoors = false;
	[Export] CollisionShape3D gearCollider = null;
	#endregion

	#region Landing Gear Doors
	private class GearDoor
	{
		public Node3D door = null;
		public DoorOpeningDirection openingDirection = DoorOpeningDirection.Left;
		public float closedDegree;
		public float openDegree;
	}

	[Export] private Node3D[] gearDoorModels;
	private GearDoor[] gearDoors;

	private enum DoorOpeningDirection
	{
		Left, // -Z
		Right, // +Z
		Forward, // +X
		Backward // -X
	}
	[Export] private int[] doorsOpeningDirection;
	[Export] private float[] doorsClosedDegree;
	[Export] private float[] doorsOpenDegree;
	[Export] private float doorMovementSpeed = 0.5f;
	private bool canActuateGears = true;
	#endregion

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		InitilizeDoors();
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if(!gearCollider.Disabled)
		{
			
		}

		if(actuateGears)
		{
			ActuateGears((float) delta);
		}
	}
	
	private void ActuateGears(float delta)
	{
		if(landingGearSet)
		{
			landingGearSet = false;
			switch(gearPosition)
			{
				case GearPosition.Up:
					gearRotateDirectionIsUp = false;
					gearRotateDirection = 1f;
					gearPosition = GearPosition.Down;
				break;
				case GearPosition.Down:
					gearRotateDirectionIsUp = true;
					gearRotateDirection = -1f;
					gearPosition = GearPosition.Up;
				break;
			}
		}
		else
		{
			if(canActuateDoors)
				ActuateDoors(delta);

			if(canActuateGears)
				DeployGears(delta);
		}
	}

	private void DeployGears(float delta)
	{
		switch(landingGearDeployDirection)
		{
			case LandingGearDeployDirection.Forward:
			break;
			case LandingGearDeployDirection.Back: // default mode, gear closes backwards
				landingGearModel.RotateX(gearRotateDirection * gearDeploySpeed * delta);
				if(gearRotateDirectionIsUp)
				{
					if(!canActuateDoors && landingGearModel.RotationDegrees.X <= gearUpDegree / 2)
					{
						canActuateDoors = true;
						gearCollider.Disabled = true; // may move to above
					}
					
					if(landingGearModel.RotationDegrees.X <= gearUpDegree) // Gear Up
					{
						landingGearModel.RotationDegrees = new Vector3(gearUpDegree, 0, 0);
					}
				}
				else
				{
					if(landingGearModel.RotationDegrees.X > gearDownDegree) // Gear Down
					{
						canActuateDoors = false;
						actuateGears = false;
						landingGearSet = true;
						landingGearModel.RotationDegrees = new Vector3(gearDownDegree, 0, 0);
						gearCollider.Disabled = false;
					}
				}
			break;
			case LandingGearDeployDirection.Left:
			break;
			case LandingGearDeployDirection.Right:
			break;
		}
	}

	private void InitilizeDoors()
	{
		gearDoors = new GearDoor[gearDoorModels.Length];
		for(int i = 0; i < gearDoors.Length; i++)
		{
            gearDoors[i] = new GearDoor
            {
                door = gearDoorModels[i]
            };
            switch (doorsOpeningDirection[i])
			{
				case 0:
					gearDoors[i].openingDirection = DoorOpeningDirection.Left;
				break;
				case 1:
					gearDoors[i].openingDirection = DoorOpeningDirection.Right;
				break;
				case 2:
					gearDoors[i].openingDirection = DoorOpeningDirection.Forward;
				break;
				case 3:
					gearDoors[i].openingDirection = DoorOpeningDirection.Backward;
				break;
			}

			gearDoors[i].closedDegree = doorsClosedDegree[i];
			gearDoors[i].openDegree = doorsOpenDegree[i];
		}
	}

	private void ActuateDoors(float delta)
	{
		for(int i = 0; i < gearDoors.Length; i++)
		{
			switch(gearDoors[i].openingDirection)
			{
				case DoorOpeningDirection.Left:
					gearDoors[i].door.RotateZ(gearRotateDirection * -1 * doorMovementSpeed * delta);
					if(!gearRotateDirectionIsUp)
					{
						if(!canActuateGears && gearDoors[i].door.RotationDegrees.Z <= gearDoors[i].openDegree / 2) // opening
						{
							canActuateGears = true;
						}

						if(gearDoors[i].door.RotationDegrees.Z <= gearDoors[i].openDegree) // open
						{
							gearDoors[i].door.RotationDegrees = new Vector3(0, 0, gearDoors[i].openDegree);
						}
					}
					else
					{
						if(gearDoors[i].door.RotationDegrees.Z >= gearDoors[i].closedDegree) // closed
						{
							gearDoors[i].door.RotationDegrees = new Vector3(0, 0, gearDoors[i].closedDegree);
							canActuateGears = false;
							actuateGears = false;
							landingGearSet = true;
						}
					}
					
				break;
				case DoorOpeningDirection.Right:
					gearDoors[i].door.RotateZ(gearRotateDirection * doorMovementSpeed * delta);
					if(!gearRotateDirectionIsUp)
					{
						if(!canActuateGears && gearDoors[i].door.RotationDegrees.Z >= gearDoors[i].openDegree / 2) // opening
						{
							canActuateGears = true;
						}

						if(gearDoors[i].door.RotationDegrees.Z >= gearDoors[i].openDegree) // open
						{
							gearDoors[i].door.RotationDegrees = new Vector3(0, 0, gearDoors[i].openDegree);
						}
					}
					else
					{
						if(gearDoors[i].door.RotationDegrees.Z <= gearDoors[i].closedDegree) // closed
						{
							gearDoors[i].door.RotationDegrees = new Vector3(0, 0, gearDoors[i].closedDegree);
							canActuateGears = false;
							actuateGears = false;
							landingGearSet = true;
						}
					}
				break;
				case DoorOpeningDirection.Forward:
				break;
				case DoorOpeningDirection.Backward:
				break;
			}
		}
	}
}
