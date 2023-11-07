/*  
 * Copyright October 2023 Barkın Zorlu 
 * All rights reserved. 
 */

using Godot;
using System;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;

public partial class AircraftPistonEngine : Node
{

	#region References, Globals
	//[Export] Controls aircraftControls; // can assign or find in start
	[Export] AircraftFuelManager aircraftFuelManager; // can assign in start
	#endregion
	
	[Export] private Node3D propeller = null;
	private enum PropellerTurnAxis // x, y, z in vectors, this is local
	{
		X,
		Y,
		Z
	}
	[Export] private PropellerTurnAxis propellerTurnAxis = PropellerTurnAxis.Z;

	private enum PropellerType
	{
		Fixed, // No adjustment
		TwoPosition, // High-Low manual
		ControllablePitch, // Manual
		ConstantSpeed // Auto
	}
	[Export] private PropellerType propellerType = PropellerType.ConstantSpeed;

	[Range(0, 100)] private float propellerPitch = 100f; // 17 to 87 degrees
	// higher (Coarse) pitch is better for cruising, lower engine RPMs
	// lower (Fine) pitch is better for more power and acceleration, higher engine rpms
	// make this nonexported after controls done
	//propeller max RPM are around 2500-3000
	private bool feathered = false;

	private enum RotationDirection
	{
		Left,
		Right
	};
	[Export] private RotationDirection rotationDirection = RotationDirection.Left;

	private bool engineOn = false; // make this nonexported after controls done
	[Range(0f, 100f)] public float throttle = 0f; // make this nonexported after controls done
	private bool WEP = false;
	public const float wepMultiplier = 1.15f; // %15 percent increase in power // only for 5 minutes
	[Export] public float RPM = 0f;
	[Export] public float idleRPM = 600f; // constant but need to set through editor 
	// radials have 600-700 idle RPM
	// v-type example, merlins are about 1000
	[Export] public float maxRPM = 2800f; // constant but need to set through editor
	[Export] public float power = 2500f; // HP
	[Export] // for test
	private float health = 100f;
	[Export] public float thrust = 3000f;
	//[Export] private float fuelConsumptionPerHour = 435.55f; // cruising, it's in Litres per Hour
	[Export] private float maxFuelConsumptionPerHour = 1107.95f; // max rpm in Litres per Hour
	private float fuelConsumptionPerSecond = -1f; // in Litres per Hour
	[Export] public float oilTemp = 50f; // not sure // make non exported after test
	[Export] public float waterTemp = 50f; // not sure // make non exported after test
	[Export] public float optimalOilTemp = 50f; // constant but need to set through editor // not sure
	[Export] public float optimalWaterTemp = 50f; // constant but need to set through editor //not sure
	[Export] [Range(0f,100f)] public float openRadiatorPercentage = 0f; // 0 is not open 100 is fully open, creates drag
	
	[Export] public bool onFire = false; // close after test
	[Export] private bool extinguished = false; // close after test
	[Export] public int extinguisherCount = 1;
	private int engineRestartChance = -1;
	private bool timedEngineFailure = false;
	private float engineFailCountdown = 600f; // 10 minutes at max
	private int engineFailCount = 0; // 10 minutes at max

	private int turnDirection = 1;
	//[Export]private bool startEngine = false; // change with input maps
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		switch(rotationDirection)
		{
			case RotationDirection.Left:
				turnDirection = 1;
				break;
			case RotationDirection.Right:
				turnDirection = -1;
				break;
		}

		fuelConsumptionPerSecond = maxFuelConsumptionPerHour / 3600f;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		SetThrottle();
		SetPropellerPitch();

		if(timedEngineFailure)
		{
			engineFailCountdown -= (float) delta;
			EngineFailure();
		}

		if(aircraftFuelManager.totalFuelAmount <= 0 || health <= 0)//TEMP
		{
			EngineFailure();
		} 
		
		if(onFire && engineOn)
		{
			OnFire((float) delta);
		}

		if(health < 50f && engineRestartChance == -1)
		{
			engineRestartChance = EngineRestartPossibility();
			EngineFailureDefiner();
		}
		
		if(GetMeta("EngineOn").AsBool())
		{
			if(!engineOn && !feathered)
				StartEngine();
		}
		else
		{
			if(engineOn)
				StopEngine();
		}

		if(engineOn)
			EngineCycle((float) delta);

		if(feathered)
			TurnPropeller((float) delta);
	}

	private void SetThrottle()
	{
		throttle = (float) GetMeta("Throttle");
		if(throttle == 100f)
			WEP = GetMeta("WEP").AsBool();
	}

	private void SetPropellerPitch()
	{
		if(GetMeta("Feathering").AsBool())
		{
			if(!feathered)
				feathered = true;

			propellerPitch = 100f;

			// close radiators

			StopEngine();
			return;
		}
		else
		{
			if(feathered)
				feathered = false;
		}

		if(GetMeta("ManualPitch").AsBool()) // Manual mode
			propellerPitch = (float) GetMeta("PropellerPitch");
		else
		{
			// Auto propeller pitch logic
			propellerPitch = 100 - throttle; // very basic algoithm
			// high throttle means desire to accelerate
			// lower thrttole means more cruising
		}
	}

	private void EngineCycle(float delta)
	{
		if(WEP)
			RPM = (idleRPM + throttle / 100f * ((maxRPM - idleRPM) * health / 100f )) * wepMultiplier;
		else
			RPM = idleRPM + throttle / 100f * ((maxRPM - idleRPM) * health / 100f );

		aircraftFuelManager.totalFuelAmount -= fuelConsumptionPerSecond * RPM / maxRPM * delta; // Temp
		
		TurnPropeller(delta);
	}

	private void TurnPropeller(float delta)
	{
		// if speed is above 200kmh turn slowly esle dont turn
		float turnDegree = 0.05f;
		if(!feathered)
			turnDegree = Mathf.DegToRad(RPM * CalculatePropellerPitchIndegrees(propellerPitch) * turnDirection) * delta;

		switch(propellerTurnAxis)
		{
			case PropellerTurnAxis.X:
				propeller.RotateX(turnDegree);
			break;
			case PropellerTurnAxis.Y:
				propeller.RotateY(turnDegree);
			break;
			case PropellerTurnAxis.Z:
				propeller.RotateZ(turnDegree);
			break;
		}
	}
	
	private float CalculatePropellerPitchIndegrees(float propellerPitch)
	{
		return (-0.7f) * propellerPitch + 87f;
	}

	private void StartEngine() // make this an event
	{	
		if(!engineOn)
		{
			if(aircraftFuelManager.totalFuelAmount <= 0)
				return;

			if(health > 50f)
			{
				engineOn = true;
			}
			else if(health > 0f) // restart
			{
				if(GD.RandRange(0, health + engineRestartChance) > 50 + engineFailCount * 10)
				{ // Success
					engineOn = true;
					engineFailCountdown = 600 * health / 100f - engineFailCount * 25;
					timedEngineFailure = true;
					// restart sounds
				}
				else // Fail
				{
					engineOn = false;
					// Specific throttle is 0
					// failure, misfires etc sounds
				}
			}
			else // Engine dead
			{
				engineOn = false;
			}
		}
		SetMeta("EngineOn", engineOn);
	}

	private void StopEngine()
	{
		engineOn = false;
		SetMeta("EngineOn", engineOn);
	}
	
	private void OnFire(float delta)
	{
		health -= 5 * delta; // 5 hp damage per second
		// initilize animation / partical effect and fire sounds

		if(extinguished && extinguisherCount > 0)
		{
			EngineFailure();
		}
	}

	private int EngineRestartPossibility()
	{
		return GD.RandRange(0, 50);
	}

	private void EngineFailureDefiner()
	{
		if(GD.Randi() % 100 < 100 - (int)health)
		{
			if(GD.Randi() % 100 < 50 - engineFailCount * 10) // immideate engine failure
			{
				engineFailCountdown = 0f;
				EngineFailure();
			}
			else
			{
				timedEngineFailure = true;
				engineFailCountdown *= health / 100f;
				engineFailCount++;
				// play broken engine sounds
			}
		}
	}

	private void EngineFailure()
	{
		if(health <= 0)
		{
			health = 0f;
			engineOn = false;
			// Specific throttle is 0
			// play engine stop sound
			timedEngineFailure = false;

			if(onFire)
			{
				onFire = false;
				extinguished = false;
				// cease partical effet / animation and stop fire sound
			}
		}
		else if(engineFailCountdown <= 0f)
		{
			engineOn = false;
			// Specific throttle is 0
			// play engine stop sound
			engineFailCount++;
			timedEngineFailure = false;
		}
		else if(aircraftFuelManager.totalFuelAmount <= 0f) // Fuel Starvation //Temp
		{
			engineOn = false;
			// Specific throttle is 0
			// play engine stop sound
		}
		else if(extinguished)
		{
			onFire = false;
			extinguisherCount--;
			extinguished = false;
			// close animation
			// cease fire sounds
			engineOn = false;
			engineFailCount++;
			timedEngineFailure = false;
		}
	}
}

