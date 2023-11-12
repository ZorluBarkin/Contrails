/*  
 * Copyright October 2023 Barkın Zorlu 
 * All rights reserved. 
 */

using Godot;
using System;
using System.ComponentModel.DataAnnotations;

public partial class FuelManager : Node
{
	private PlayerVehicleType playerVehicleType = PlayerVehicleType.Aircraft;
	private VehicleControls vehicleControls = null;
	private AircraftPistonEngine[] pistonEngines = null;
	//private AircraftJetEngine[] jetEngines = null;
	[Export] private float leanMultiplier = 0.58f;
	[Export] private float wepMultiplier = 1.12f;
	private bool isPistonEngine = false;
	private float[] fuelConsumptions;
	private bool consumptionSet = false;

	[Export] public Node3D[] fuelTanks; // fill in inspector
	[Export] public Node3D[] externalFuelTanks; // fill in inspector
	[Export] public float maxFuelAmount = 3795f; //for b26k 
	[Export] public float totalFuelAmount = -1f; // exported for testing
	private float internalFuelAmount = -1f;
	private float externalFuelAmount = -1f;
	public float fuelInSeconds = -1f;

	public enum FuelTransfer
	{
		None,
		LeftToRight,
		RightToLeft
	}
	[Export] public FuelTransfer fuelTransfer = FuelTransfer.None;
	private bool ableToTransferFuel = false;
	private bool transferFuel = false;

	// need a way to slowly transfer fuel between fuel tanks

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		playerVehicleType = GameManager.instance.playerVehicleType;
		vehicleControls = GameManager.instance.vehicleControls;

		switch(playerVehicleType)
		{
			case PlayerVehicleType.Aircraft:
				ableToTransferFuel = true;
			break;
			case PlayerVehicleType.GroundVehicle:
				ableToTransferFuel = false;
			break;
			case PlayerVehicleType.Ship:
				ableToTransferFuel = false;
			break;
			case PlayerVehicleType.StaticPlacement:
				ableToTransferFuel = false;
			break;
		}

		SetEngine();

		if(totalFuelAmount == -1f) // this is for if its predefined
			CalculateFuelAmount();
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if(!consumptionSet)
		{
			SetFuelConsumptions();
			consumptionSet = true;
		}

		UseFuel((float) delta);

		if(ableToTransferFuel && transferFuel)
			TransferFuel();
		
		fuelInSeconds = CalculateFuelInSeconds();
	}

	private void TransferFuel()
	{
		switch(fuelTransfer) // currently Instant
		{
			case FuelTransfer.None:
				for(int i = 0; i < fuelTanks.Length; i++)
				{
					fuelTanks[i].SetMeta("FuelAmount", totalFuelAmount / fuelTanks.Length);
				}
			break;
			case FuelTransfer.LeftToRight:
				for(int i = 0; i < fuelTanks.Length; i++)
				{
					if(!fuelTanks[i].GetMeta("OnLeft").AsBool())
					{
						fuelTanks[i].SetMeta("FuelAmount", totalFuelAmount);
					}
					else
					{
						fuelTanks[i].SetMeta("FuelAmount", 0f);
					}
				}
			break;
			case FuelTransfer.RightToLeft:
				for(int i = 0; i < fuelTanks.Length; i++)
				{
					if(fuelTanks[i].GetMeta("OnLeft").AsBool())
					{
						fuelTanks[i].SetMeta("FuelAmount", totalFuelAmount);
					}
					else
					{
						fuelTanks[i].SetMeta("FuelAmount", 0f);
					}
				}
			break;
		}
	}

	private void CalculateFuelAmount()
	{
		internalFuelAmount = 0f;
		externalFuelAmount = 0f;

		for(int i = 0; i < fuelTanks.Length; i++)
		{
			internalFuelAmount += (float) fuelTanks[i].GetMeta("FuelAmount");
		}

		for(int i = 0; i < externalFuelTanks.Length; i++)
		{
			externalFuelAmount += (float) externalFuelTanks[i].GetMeta("FuelAmount");
		}
		totalFuelAmount = internalFuelAmount + externalFuelAmount;
	}

	private void UseFuel(float delta)
	{
		if(totalFuelAmount < 0)
			return;

		float fuelUse = CalculateFuelConsumption(delta);
		//GD.Print(fuelUse * (1f / delta)); // DEBUG //	use of fuel per engine in a second
		if(externalFuelAmount > 0f)
		{
			for(int i = 0; i < externalFuelTanks.Length; i++)
			{
				externalFuelAmount -= fuelUse;
				externalFuelTanks[i].SetMeta("FuelAmount", externalFuelAmount / externalFuelTanks.Length);
			}
		}
		else // internal fuel tanks
		{
			for(int i = 0; i < fuelTanks.Length; i++)
			{
				internalFuelAmount -= fuelUse;
				fuelTanks[i].SetMeta("FuelAmount", internalFuelAmount / fuelTanks.Length);
			}
		}

		CalculateFuelAmount();
	}

	private float CalculateFuelConsumption(float delta)
	{
		float dividend = 0f; // per second
		float multiplier;
		if(isPistonEngine)
		{
			for(int i = 0; i < pistonEngines.Length; i++)
			{
				// between 0 - 1;
				float rpmPercent = (pistonEngines[i].maxContRPM - (pistonEngines[i].maxContRPM - pistonEngines[i].RPM)) / pistonEngines[i].maxContRPM;
				
				if(pistonEngines[i].WEP)
					multiplier = wepMultiplier;
				else if(pistonEngines[i].RPM <= pistonEngines[i].leanRPM)
					multiplier = leanMultiplier;
				else
					multiplier = 1f;
				
				dividend += rpmPercent * fuelConsumptions[i] * multiplier;
			}
			return dividend / pistonEngines.Length * delta;
		}
		else // Jet engines
		{
			return -1f;
		}
	}

	private float CalculateFuelInSeconds()
	{
		float avgConsumption = 0f;
		for(int i = 0; i < fuelConsumptions.Length; i++)
			avgConsumption += fuelConsumptions[i];

		avgConsumption /= fuelConsumptions.Length;
		return totalFuelAmount / avgConsumption;
	}

	private void SetEngine()
	{
		pistonEngines = new AircraftPistonEngine[vehicleControls.engines.Length];
		fuelConsumptions = new float[vehicleControls.engines.Length];
		//jetEngines = new AircraftJetEngine[vehicleControls.engines.Length];
		for(int i = 0; i < vehicleControls.engines.Length; i++)
		{
			if(vehicleControls.engines[i] is AircraftPistonEngine)
			{
				pistonEngines[i] = GetNode<AircraftPistonEngine>(vehicleControls.engines[i].GetPath());
				isPistonEngine = true;
				//Array.Clear(jetEngines);
			}
			else
			{GD.Print("Its not piston");}
			//else (vehicleControls.engines[i] is AircraftJetEngine)
			//{
			//	jetEngines = GetNode<AircraftJetEngine>(vehicleControls.engines[i].GetPath());
			//	isPistonEngine = false;
			//}
		}
		
	}

	private void SetFuelConsumptions()
	{
		for(int i = 0; i < fuelConsumptions.Length; i++)
			fuelConsumptions[i] = pistonEngines[i].fuelConsumptionPerSecond;
	}
}
