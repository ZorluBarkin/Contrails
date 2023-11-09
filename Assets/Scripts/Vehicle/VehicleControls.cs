/*  
 * Copyright October 2023 Barkın Zorlu 
 * All rights reserved. 
 */

using Godot;
using System;
using System.ComponentModel.DataAnnotations;

public partial class VehicleControls : Node
{	
	private PlayerVehicleType playerVehicleType = PlayerVehicleType.Aircraft; // not implemented yet

	[Export] private AircraftDamage aircraftDamage = null;
	[Export] private Mechanization mechanization = null;

	[Export] public Node[] engines;
	private enum EngineStartMode
	{
		Specific,
		Combined
	}
	[Export] private EngineStartMode engineStartMode = EngineStartMode.Combined;

	private enum EngineControlMode
	{
		Combined,
		Specific,
		Inner,
		Outer
	}
	[Export] private EngineControlMode engineControlMode = EngineControlMode.Combined; // remove export after control implementation
	[Export] [Range(0,100)] public int throttle = 0; // make these switch for each engine in non fighter aircraft
	[Export] public bool WEP = false; // %15 percent increase in power // only for 5 minutes
	private bool wepEnabled = true;
	private float wepTime = 300f; //5 minutes, 300 seconds

	[Export] private bool manualPitchControl = false;
	private bool hasPitchControl = false;
	[Export] [Range(0f, 100f)] private float propellerPitch = 0f;

	[Export] private bool featherPropeller = false;
	[Export] private bool startEngine = false; // TEMP
	[Export] private bool stopEngine = false; // TEMP
	[Export] private bool actuateGears = false; // TEMP

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		playerVehicleType = GameManager.instance.playerVehicleType;

		if(engines[0].HasMeta("ManualPitch"))
			hasPitchControl = true;
		else
			hasPitchControl = false;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		// after WEP Input is gotten
		if(wepEnabled)
			UseWEP((float) delta);
		else
			WEP = false;
			

		if(startEngine) // temp
		{
			OnStartEngine();
			startEngine = false;
		}

		if(stopEngine)
		{
			OnStopEngine();
			stopEngine = false;
		}

		SetThrottle();

		if(hasPitchControl && ! featherPropeller)
		{
			ManualPropellerPitch();
		}

		FeatherPropellers();

		if(actuateGears)// temp
		{
			actuateGears = false;
			mechanization.actuateGears = true;
		}
	}

	private void UseWEP(float delta)
	{
		if(wepTime <= 0)
		{
			WEP = false;
			wepEnabled = false;
			return;
		}

		if(WEP)
		{
			wepTime -= delta;
		}
	}

	private void OnStartEngine()
	{
		switch(engineStartMode)
		{
			case EngineStartMode.Specific:
			// do with designated Engine number
			break;
			case EngineStartMode.Combined:
				for(int i = 0; i < engines.Length; i++)
				{
					engines[i].SetMeta("EngineOn", true);
				}
			break;
		}
	}

	private void OnStopEngine()
	{
		switch(engineStartMode)
		{
			case EngineStartMode.Specific:
			// do with designated Engine number
			break;
			case EngineStartMode.Combined:
				for(int i = 0; i < engines.Length; i++)
				{
					engines[i].SetMeta("EngineOn", false);
				}
			break;
		}
	}

	private void ManualPropellerPitch()
	{
		if(manualPitchControl)
		{
			for(int i = 0; i < engines.Length; i++)
			{
				if(!engines[i].GetMeta("ManualPitch").AsBool())
					engines[i].SetMeta("ManualPitch", true);
				
				SetPropellerPitch();
			}
		}

		if(!manualPitchControl)
		{
			for(int i = 0; i < engines.Length; i++)
			{
				if(engines[i].GetMeta("ManualPitch").AsBool())
					engines[i].SetMeta("ManualPitch", false);
			}
		}
	}

	private void SetThrottle()
	{
		switch(engineControlMode)
		{
			case EngineControlMode.Combined:
				for(int i = 0; i < engines.Length; i++)
				{
					engines[i].SetMeta("Throttle", throttle);
				}
			break;
			case EngineControlMode.Specific:
			// do with designated Engine number
			break;
			case EngineControlMode.Inner:
			// do with designated Engine number on both sides
			break;
			case EngineControlMode.Outer:
			// do with designated Engine number on outer both sides
			break;
		}

		if(WEP)
		{
			for(int i = 0; i < engines.Length; i++)
			{
				if(engines[i].HasMeta("WEP"))
					engines[i].SetMeta("WEP", true);
			}
		}
		else
		{
			for(int i = 0; i < engines.Length; i++)
			{
				if(engines[i].HasMeta("WEP"))
					engines[i].SetMeta("WEP", false);
			}
		}
	}

	private void SetPropellerPitch()
	{
		switch(engineControlMode)
		{
			case EngineControlMode.Combined:
				for(int i = 0; i < engines.Length; i++)
				{
					engines[i].SetMeta("PropellerPitch", propellerPitch);
				}
			break;
			case EngineControlMode.Specific:
			// do with designated Engine number
			break;
			case EngineControlMode.Inner:
			// do with designated Engine number on both sides
			break;
			case EngineControlMode.Outer:
			// do with designated Engine number on outer both sides
			break;
		}
	}

	private void FeatherPropellers()
	{
		if(featherPropeller)
		{
			for(int i = 0; i < engines.Length; i++)
			{
				if(!engines[i].GetMeta("Feathering").AsBool())
					engines[i].SetMeta("Feathering", true);
			}
		}
		else
		{
			for(int i = 0; i < engines.Length; i++)
			{
				if(engines[i].GetMeta("Feathering").AsBool())
					engines[i].SetMeta("Feathering", false);
			}
		}
	}
}
