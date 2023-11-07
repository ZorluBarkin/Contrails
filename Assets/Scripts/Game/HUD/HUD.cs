/*  
 * Copyright October 2023 Barkın Zorlu 
 * All rights reserved. 
 */

using Godot;
using System;

public partial class HUD : Control // Should not be a Singleton
{
	[Export] private Label FPSLabel = null;
	private bool lowFPS = true;
	private bool seeFPS = true;

	[Export] private PackedScene EngineInfoSubScene = null;
	[Export] private EngineInfo[] engineInfos;
	public int NumberOfEngines = 0;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		if(FPSLabel == null)
			this.GetChild<Label>(0);

		if(Settings.instance.seeFPS)
			seeFPS = true;

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
				engineInfos[containerIndex].engineIndex = containerIndex + 1;
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
}
