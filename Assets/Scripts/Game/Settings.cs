/*  
 * Copyright October 2023 Barkın Zorlu 
 * All rights reserved. 
 */

using Godot;
using System;
using System.ComponentModel;

public partial class Settings : Node
{

	public /*private*/ static Camera3D camera;
	private RichTextLabel fpsLabel;
	[Export] private DisplayServer.VSyncMode vSyncMode = DisplayServer.VSyncMode.Disabled;
	[Export] private int fpsLimit = 60;

	[Export] private bool apply = false; // remove export after ui is implemented

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{

		if(camera == null)
			for(int i = 0; i < GetParent().GetChildCount(); i++)
				if(GetParent().GetChild(i).HasMeta("MainCamera"))
					if(GetParent().GetChild(i).GetMeta("MainCamera").AsBool())
						camera = GetNode<Camera3D>(GetParent().GetChild(i).GetPath());

		if(fpsLabel == null)
			fpsLabel = camera.GetChild(0).GetChild<RichTextLabel>(0);
			//					HUD					// Label	

		ApplySettings();
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if(fpsLabel != null)
			fpsLabel.Text = "FPS: " + Engine.GetFramesPerSecond();

		if(apply)
			ApplySettings();

	}

	private void ApplySettings()
	{
		DisplayServer.WindowSetVsyncMode(vSyncMode);
		Engine.MaxFps = fpsLimit;
		
		//if(DisplayServer.GetScreenCount() > 1)
		//{
		//	for(int i = 0; i < DisplayServer.GetScreenCount(); i++)
		//	{
		//		DisplayServer.ScreenGetRefreshRate(/*(int)DisplayServer.ScreenOfMainWindow*/);
		//	}
		//}
		
		//Engine.MaxFps = fpsLimit;
	}
}
