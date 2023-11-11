/*  
 * Copyright October 2023 Barkın Zorlu 
 * All rights reserved. 
 */

using Godot;
using System;
using System.Dynamic;

public partial class EngineInfo : VBoxContainer
{
	private HUD hud = null;
	public int engineIndex = 0; // cannot be 0
	public bool engineIndexSet = false; // cannot be 0
	private AircraftPistonEngine pistonEngine = null;
	// private JetEngine jetEngine = null;
	private bool isPistonEngine = false;
	private VehicleControls vehicleControls = null;

	// Container Variables
	[Export] private Label engineLabel = null; // Engine Number

	// Engine Base information Container
	[Export] private TextureRect throttleSymbol = null;
	[Export] private Label throttleLabel = null;
	[Export] private Texture2D[] throttleSymbols; // from Increase to Decrease;
	[Export] private TextureRect RPMSymbol = null;
	[Export] private Label RPMLabel = null;
	[Export] private Texture2D[] RPMSymbols; // from Low to High;
	[Export] private Texture2D WEPSymbol; // from Low to High;

	[Export] private Label waterTempLabel = null;
	[Export] private Label oilTempLabel = null;

	// Details Container
	[Export] private Container detailsContainer = null;

	[Export] private Label propellerPitchLabel = null;
	[Export] private TextureRect waterCowlSymbol = null;
	[Export] private Label waterCowlLabel = null;
	[Export] private Texture2D[] waterCowlSymbols; // from closed to Open;

	[Export] private TextureRect oilCowlSymbol = null;
	[Export] private Label oilCowlLabel = null;
	[Export] private Texture2D[] oilCowlSymbols; // from Closed to Open;

	[Export] private CheckButton feathering = null;
	[Export] private bool seeDetails = false; // make this change via button
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		if(hud == null)
			hud = GetNode<HUD>(this.GetParent().GetPath());
		
		vehicleControls = GameManager.instance.vehicleControls;
		SetEngine();
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if(!engineIndexSet)
		{
			engineLabel.Text = engineIndex.ToString();
			engineIndexSet = true;
		}

		UpdateEngineInfo();
		DetailVisibility();
		
		if(detailsContainer.Visible)
			UpdateDetailInfo();
	}

	private void UpdateEngineInfo()
	{
		if(isPistonEngine)
		{
			throttleLabel.Text = ((int)pistonEngine.throttle).ToString() + "%";
			if((int)pistonEngine.throttle > 82) // change to max crusing throttle
			{
				throttleSymbol.Texture = throttleSymbols[1];
				if(pistonEngine.WEP)
				{
					throttleLabel.LabelSettings.FontColor = Colors.Red;
				}
				else if (pistonEngine.dryWep)
				{
					throttleLabel.LabelSettings.FontColor = Colors.Orange;
				}
				else
				{
					throttleLabel.LabelSettings.FontColor = Colors.White;
				}
			} 
			else
			{
				throttleSymbol.Texture = throttleSymbols[0];
			}

			RPMLabel.Text = ((int)pistonEngine.RPM).ToString();
			RPMSymbol.Texture = GetRPMSymbol();
			waterTempLabel.Text = ((int)pistonEngine.waterTemp).ToString() + "\u00B0C";
			oilTempLabel.Text = ((int)pistonEngine.oilTemp).ToString() + "\u00B0C";
		}	
		//else
		//	throttleLabel.Text = jetEngine.throttle.ToString() + '%';
	}

	private void DetailVisibility()
	{
		if(!seeDetails)
		{
			if(detailsContainer.Visible)
				detailsContainer.Visible = false;
		}
		else
		{
			if(!detailsContainer.Visible)
				detailsContainer.Visible = true;
		}
	}

	private void UpdateDetailInfo()
	{
		if(isPistonEngine)
		{
			propellerPitchLabel.Text = ((int)pistonEngine.propellerPitch).ToString() + "%";
			waterCowlSymbol.Texture = GetWaterCowlSymbol();
			waterCowlLabel.Text = ((int)pistonEngine.waterCowlPercentage).ToString() + "%";
			oilCowlSymbol.Texture = GetOilCowlSymbol();
			oilCowlLabel.Text = ((int)pistonEngine.oilCowlPercentage).ToString() + "%";
			feathering.ButtonPressed = pistonEngine.feathered;
		}
		//else // Jet Engine
		//{}
	}

	private Texture2D GetRPMSymbol()
	{
		if(isPistonEngine)
		{
			if(pistonEngine.WEP)
				return WEPSymbol;

			float rpmPercent = (pistonEngine.maxContRPM - (pistonEngine.maxContRPM - pistonEngine.RPM)) / pistonEngine.maxContRPM * 100f;
			return RPMSymbols[Math.Clamp((int)(rpmPercent / (100f / RPMSymbols.Length)), 0, RPMSymbols.Length - 1)];
		}
		else
		{
			//float rpmPercent = (jetEngine.maxRPM - (jetEngine.maxRPM - jetEngine.RPM)) / jetEngine.maxRPM * 100f;
			//return RPMSymbols[Math.Clamp((int)(rpmPercent / (100f / fanRPMSymbols.Length)), 0, 6)];
			return null;
		}
	}

	private Texture2D GetWaterCowlSymbol()
	{
		return waterCowlSymbols[(int)((pistonEngine.waterCowlPercentage - 1f) / (100f / waterCowlSymbols.Length))];
	}

	private Texture2D GetOilCowlSymbol()
	{
		return oilCowlSymbols[(int)((pistonEngine.oilCowlPercentage - 1f) / (100f / oilCowlSymbols.Length))];
	}

	private void SetEngine()
	{
		if(vehicleControls.engines[engineIndex] is AircraftPistonEngine)
		{
			pistonEngine = GetNode<AircraftPistonEngine>(vehicleControls.engines[engineIndex].GetPath());
			isPistonEngine = true;
		}
		else
		{GD.Print("Its not piston");}
		//else (vehicleControls.engines[engineIndex] is AircraftJetEngine)
		//{
		//	jetEngine = GetNode<AircraftJetEngine>(vehicleControls.engines[engineIndex].GetPath());
		//	isPistonEngine = false;
		//}
	}
}
