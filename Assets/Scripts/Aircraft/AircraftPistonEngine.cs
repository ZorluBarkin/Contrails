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
	[Export] FuelManager fuelManager; // can assign in start
	#endregion
	
	[Export] private float turn = 1f;

	// max 3 Size, index: 0 is very slow turning, 1 is blurry, 2 is broken
	[Export] private Node3D[] propellerModels = null;
	private bool propellerBroken = false;
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

	public bool highPitch = false;

	[Range(0, 100)] public float propellerPitch = 100f; // 17 to 87 degrees
	// higher (Coarse) pitch is better for cruising, lower engine RPMs
	// lower (Fine) pitch is better for more power and acceleration, higher engine rpms
	// make this nonexported after controls done
	//propeller max RPM are around 2500-3000
	public bool manualPitchControl = false;
	public bool feathered = false; // state

	private enum RotationDirection
	{
		Left,
		Right
	};
	[Export] private RotationDirection rotationDirection = RotationDirection.Left;

	private bool engineOn = false; // make this nonexported after controls done
	public bool startEngine = false; // make this nonexported after controls done
	public bool stopEngine = false; // make this nonexported after controls done
	[Range(0f, 100f)] public float throttle = 0f; // make this nonexported after controls done
	public bool WEP = false;
	public const float wepMultiplier = 1.10f; // %10 percent increase in rpm // only for 5 minutes
	private float wepTime = 300f; // %10 percent increase in rpm // only for 5 minutes
	[Export] public bool dryWep = false; // State
	public float RPM = 0f;
	[Export] public float idleRPM = 600f; // constant but need to set through editor 
	// radials have 600-700 idle RPM
	// v-type example, merlins are about 1000
	[Export] public float leanRPM = 2250f; // when Auto lean is active, less power less than half fuel consumption
	[Export] public float maxContRPM = 2600f; // constant but need to set through editor
	[Export] private float power = 1900f; // HP //normal injection at normal rpm
	[Export] private float emergencyPower = 2500f; // HP // wet injection at WEP RPM
	[Export] private float dryWepPower = 2200f; // Dry injection
	[Export] private float currentPowerOutput = 2500f; // HP // wet injection at WEP RPM
	public float health = 100f;
	[Export] public float thrust = 3000f;
	//[Export] private float fuelConsumptionPerHour = 435.55f; // cruising, it's in Litres per Hour
	[Export] private float maxFuelConsumptionPerHour = 1041f; // max rpm in Litres per Hour
	public float fuelConsumptionPerSecond = -1f; // in Litres per Hour
	[Export] public float oilTemp = 50f; // not sure // make non exported after test
	[Export] public float waterTemp = 50f; // not sure // make non exported after test
	[Export] public float optimalOilTemp = 50f; // constant but need to set through editor // not sure
	[Export] public float optimalWaterTemp = 50f; // constant but need to set through editor //not sure
	[Export] [Range(0f,100f)] public float waterCowlPercentage = 0; // 0 is not open 100 is fully open, creates drag
	[Export] [Range(0f,100f)] public float oilCowlPercentage = 0; // 0 is not open 100 is fully open, creates drag
	
	// Engine Startup
	[Export] private float turnoverTimeStamp = 5f;
	[Export] private float ignitionTimeStamp = 5f + 15f;
	[Export] private AudioStreamOggVorbis turnoverSound = null;
	[Export] private AudioStreamOggVorbis ignitionSound = null;
	[Export] private AudioStreamOggVorbis idleSound = null;
	[Export] private AudioStreamOggVorbis cruisingSound = null;
	[Export] private AudioStreamPlayer3D audioPlayer = null;

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
		fuelManager = GameManager.instance.fuelManager;
		fuelConsumptionPerSecond = maxFuelConsumptionPerHour / 3600f;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		SetPropellerPitch();
		UseWep((float) delta);

		if(timedEngineFailure)
		{
			engineFailCountdown -= (float) delta;
			EngineFailure();
		}

		if(fuelManager.totalFuelAmount <= 0 || health <= 0)//TEMP
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
		
		OnEngineStateChange(delta);	

		if(engineOn)
			EngineCycle((float) delta);

		if(feathered)
			TurnPropeller((float) delta);
	}

	private void SetPropellerPitch()
	{
		if(feathered)
		{
			if(propellerPitch < 100f)
			{
				propellerPitch = 100f;
				// close radiators
				StopEngine();
			}
		}
		else
		{	
			switch(propellerType)
			{
				case PropellerType.ControllablePitch:
				{
					if(!manualPitchControl)
					{
						// Auto propeller pitch logic
						propellerPitch = 100 - throttle; // very basic algoithm
						// high throttle means desire to accelerate
						// lower thrttole means more cruising
					}
					break;
				}
				case PropellerType.TwoPosition:
				{
					if(highPitch)
					{
						propellerPitch = 0f;
					}
					else
					{
						if(propellerPitch < 100f)
							propellerPitch = 100f;
					}
					break;
				}
			}
		}
	}

	private void UseWep(float delta)
	{
		if(throttle < 100f)
			WEP = false;

		if(WEP)
		{
			if(wepTime > 0)
				wepTime -= delta;
			else
			{
				if(!dryWep)
					dryWep = true; // make engines overheat quickly
			}
		}
	}

	private void EngineCycle(float delta)
	{
		if(WEP || dryWep)
			RPM = (idleRPM + throttle / 100f * ((maxContRPM - idleRPM) * health / 100f )) * wepMultiplier;
		else
			RPM = idleRPM + throttle / 100f * ((maxContRPM - idleRPM) * health / 100f );
		
		if(WEP && !dryWep)
			currentPowerOutput = emergencyPower;
		else if(WEP && dryWep)
			currentPowerOutput = dryWepPower;
		
		else
		{
			//currentPowerOutput = RPM * power
		}

		TurnPropeller(delta);
	}

	private void TurnPropeller(float delta)
	{
		if(propellerBroken)
			return;

		// if speed is above 200kmh turn slowly esle dont turn
		float turnDegree = 0.05f * turnDirection;
		if(!feathered)
			turnDegree = Mathf.DegToRad(RPM * CalculatePropellerPitchIndegrees(propellerPitch) * turnDirection) * delta;

		// makes sense but have to change this with a blurred mesh and reduce rotation speed
		//turnDegree = turn * turnDirection * delta; // after 20f, it gets wierd
		switch(propellerTurnAxis)
		{
			case PropellerTurnAxis.X:
				propellerModels[0].RotateX(turnDegree);
			break;
			case PropellerTurnAxis.Y:
				propellerModels[0].RotateY(turnDegree);
			break;
			case PropellerTurnAxis.Z:
				propellerModels[0].RotateZ(turnDegree);
			break;
		}
	}
	
	/// <summary>
	/// Method is static because it does not access instance members so this does not need a check. Making this static will make it faster.
	/// </summary>
	/// <param name="propellerPitch"></param>
	/// <returns></returns>
	private static float CalculatePropellerPitchIndegrees(float propellerPitch) 
	{
		return -0.7f * propellerPitch + 87f;
	}

	private void OnEngineStateChange(double delta)
	{
		if(startEngine)
		{
			startEngine = false;

			if(!engineOn && !feathered)
			{
				StartEngine((float)delta);
			}
		}
		else if(stopEngine)
		{
			stopEngine = false;
			StopEngine();
		}
	}

	private void StartEngine(float delta) // make this an event
	{			
		if(!engineOn)
		{
			if(fuelManager.totalFuelAmount <= 0)
				return;

			if(health > 50f)
			{
				EngineStartup(delta); // Re look Execution steps
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
	}

	private float time = 0f;
	private void EngineStartup(float delta)
	{
		time += delta;
		if(time < turnoverTimeStamp)
		{
			GD.Print("Turnover");
			//audioStream.Stream = turnoverSound;
			//audioStream.Play();
		}
		else if(time < ignitionTimeStamp)
		{
			GD.Print("Ignition");
			//audioStream.Stop();
			//audioStream.Stream = ignitionSound;
			//audioStream.Play();
		}
		else
		{
			GD.Print("started");
			//audioStream.Stop();
			//audioStream.Stream = idleSound;
			//audioStream.Play();
		}
	}

	private void StopEngine()
	{
		//if(audioPlayer.Playing)
		//	audioPlayer.Stop();
		RPM = 0f;
		WEP = false;
		engineOn = false;
	}

	// --------------------------------------------------- Engine Failures ---------------------------------------------------
	#region Engine Failures
	private void OnFire(float delta)
	{
		health -= 5 * delta; // 5 hp damage per second
		// initilize animation / partical effect and fire sounds

		if(extinguished && extinguisherCount > 0)
		{
			EngineFailure();
		}
	}

	private static int EngineRestartPossibility()
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
		else if(fuelManager.totalFuelAmount <= 0f) // Fuel Starvation //Temp
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
	#endregion
}

