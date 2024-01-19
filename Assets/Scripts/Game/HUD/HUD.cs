/*  
 * Copyright October 2023 Barkın Zorlu 
 * All rights reserved. 
 */

using Godot;
using System;

public partial class HUD : Control // Should not be a Singleton
{
	private Node3D playerVehicle = null;
	private RigidBody3D vehicleRb = null;
	private FuelManager fuelManager = null;
	private Mechanization mechanization = null;

	private DisplayUnitType displayUnitType = DisplayUnitType.Metric;
	private FlapsDisplaySetting flapsDisplaySetting = FlapsDisplaySetting.Degrees;

	// FPS Label
	[Export] private Label FPSLabel = null;
	private bool lowFPS = true;
	private bool seeFPS = true;

	// Engine Info Containers
	//[Export] private PackedScene EngineInfoSubScene = null;
	private EngineInfo[] engineInfos;
	public int NumberOfEngines = 0;

	// Aircraft Info Container
	[Export] private Label speedLabel;
	[Export] private Label altitudeLabel;

	[Export] private TextureRect fuelSymbol;
	[Export] private Label fuelLabel;
	[Export] private Texture2D[] fuelSymbols;

	[Export] private TextureRect flapsSymbol;
	[Export] private Label flapsLabel;
	[Export] private Texture2D[] flapsSymbols;

	[Export] public bool debug = false;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		if(FPSLabel == null)
			this.GetChild<Label>(0);

		if(Settings.instance.seeFPS)
			seeFPS = true;

		if(debug)
			return;

		playerVehicle = GameManager.instance.playerVehicle;
		vehicleRb = GetNode<RigidBody3D>(playerVehicle.GetPath());
		fuelManager = GameManager.instance.fuelManager;
		mechanization = GameManager.instance.mechanization;

		displayUnitType = Settings.instance.displayUnitType;
		flapsDisplaySetting = Settings.instance.flapsDisplaySetting;

		NumberOfEngines = GameManager.instance.vehicleControls.engines.Length; // create EngineNumber of EngineInfoContainer side by side
		engineInfos = new EngineInfo[NumberOfEngines];
		GetEngineInfoContainers();
		// does not work currently place two close one on the right if only 1 controlable engine
		//InstantiateEngineInfo();

	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		UpdateFPSLabel();

		if(debug)
			return;

		UpdateAircraftInfo();
	}

	private int containerIndex = 0;
	private void GetEngineInfoContainers()
	{
		for(int i = 0; i < this.GetChildCount(); i++)
		{
			if(GetChild(i) is EngineInfo)
			{
				if(containerIndex == NumberOfEngines)
				{
					GetNode<CanvasItem>(GetChild(i).GetPath()).QueueFree(); // Delete Container
				}
				engineInfos[containerIndex] = GetNode<EngineInfo>(GetChild(i).GetPath());
				engineInfos[containerIndex].engineIndex = containerIndex;
				containerIndex++;
			}
		}
	}

	/*
	private void InstantiateEngineInfo()
	{
		//for(int i = 0; i < NumberOfEngines; i++)
		//{
		//	Node engineInfo = EngineInfoSubScene.Instantiate();
		//	if(engineInfo.IsInsideTree())
		//	{
		//		engineInfos[i] = GetNode<EngineInfo>(engineInfo.GetPath());
		//		engineInfos[i].engineIndex = i + 1;
		//		if(i == 0)
		//			engineInfos[i].SetGlobalPosition(Vector2.Zero);
		//		else
		//			engineInfos[i].SetGlobalPosition(new Vector2(i * engineInfos[i].Size.X, 0));	
		//	}
		//}
	}
	*/

	private void UpdateFPSLabel()
	{
		if(seeFPS)
		{
			if(Engine.GetFramesPerSecond() < 30)
			{
				if(!lowFPS)
				{
					FPSLabel.LabelSettings.FontColor = Colors.Red;
					lowFPS = true;
				}
			}
			else
			{
				if(lowFPS)
				{
					FPSLabel.LabelSettings.FontColor = Colors.Green;
					lowFPS = false;
				}
			}
			FPSLabel.Text = "FPS: " + Engine.GetFramesPerSecond();
		}
	}

	private void UpdateAircraftInfo()
	{
		switch (displayUnitType)
		{
			case DisplayUnitType.Metric:
				speedLabel.Text = ((int)(vehicleRb.LinearVelocity.Length() * 3.6f)).ToString() + " km/h";
				altitudeLabel.Text = ((int)playerVehicle.Position.Y).ToString() + " m";
				fuelLabel.Text = GetFuelInTime();
				flapsLabel.Text = GetFlapValue();
				flapsSymbol.Texture = GetFlapSymbol();
			break;
			case DisplayUnitType.Impreial:

			break;
			case DisplayUnitType.Aeronautical:

			break;
		}
	}

	private string GetFuelInTime()
	{
		int hour = (int)(fuelManager.fuelInSeconds / 3600f);
		int minutes = (int)(fuelManager.fuelInSeconds % 3600f / 60f);
		int seconds = (int)(fuelManager.fuelInSeconds % 3600f % 60f);
		return hour.ToString() + ":" + minutes.ToString() + ":" + seconds.ToString(); // concatting strings are faster
	}

	private string GetFlapValue()
	{
		switch(flapsDisplaySetting)
		{
			case FlapsDisplaySetting.Degrees:
				return ((int) mechanization.flapActuation).ToString() +  "\u00B0";
			case FlapsDisplaySetting.Percentage:
				return ((int) ((mechanization.flapMaxDegree - mechanization.flapActuation) / mechanization.flapMaxDegree) ).ToString() + "%";
		}
		return "";
	}

	private Texture2D GetFlapSymbol()
	{
		float flapPercent = (mechanization.flapMaxDegree - mechanization.flapActuation) / mechanization.flapMaxDegree * 100;
		return flapsSymbols[Math.Clamp((int) (flapPercent / (100f / flapsSymbols.Length)), 0, flapsSymbols.Length - 1)];
	}
}
