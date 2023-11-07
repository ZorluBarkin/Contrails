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

	[Export] private Label waterTempLabel = null;
	[Export] private Label oilTempLabel = null;

	// Details Container
	[Export] private Container detailsContainer = null;

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
		if(pistonEngine != null)
		{
			throttleLabel.Text = pistonEngine.throttle.ToString() + '%';
			RPMLabel.Text = pistonEngine.RPM.ToString();
			waterTempLabel.Text = pistonEngine.waterTemp.ToString() + "\u00B0C";
			oilTempLabel.Text = pistonEngine.oilTemp.ToString() + "\u00B0C";
		}
		
		//else if (jetEngine != null)
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

	}

	private void SetEngine()
	{
		if(vehicleControls.engines[engineIndex] is AircraftPistonEngine)
		{
			pistonEngine = GetNode<AircraftPistonEngine>(vehicleControls.engines[engineIndex].GetPath());
		}
		else
		{GD.Print("Its not piston");}
		//else (vehicleControls.engines[engineIndex] is AircraftJetEngine)
		//{
		//	jetEngine = GetNode<AircraftJetEngine>(vehicleControls.engines[engineIndex].GetPath());
		//}
	}
}
