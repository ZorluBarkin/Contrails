/*  
 * Copyright October 2023 Barkın Zorlu 
 * All rights reserved. 
 */

using Godot;
using System;
using System.ComponentModel.DataAnnotations;

public partial class AircraftFuelManager : Node
{
	[Export] public Node3D[] fuelTanks; // fill in inspector
	[Export] public Node3D[] externalFuelTanks; // fill in inspector
	[Export] public float totalFuelAmount = -1f;

	public enum FuelTransfer
	{
		None,
		LeftToRight,
		RightToLeft
	}
	[Export] public FuelTransfer fuelTransfer = FuelTransfer.None;

	// need a way to slowly transfer fuel between fuel tanks

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		if(totalFuelAmount == -1f) // this is for if its predefined
			CalculateTotalFuelAmount();
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
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

	private void CalculateTotalFuelAmount()
	{
		totalFuelAmount = 0f;
		for(int i = 0; i < fuelTanks.Length; i++)
		{
			totalFuelAmount += (float) fuelTanks[i].GetMeta("FuelAmount");
		}

		for(int i = 0; i < externalFuelTanks.Length; i++)
		{
			totalFuelAmount += (float) externalFuelTanks[i].GetMeta("FuelAmount");
		}
	}
}
