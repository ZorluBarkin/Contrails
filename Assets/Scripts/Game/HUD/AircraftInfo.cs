using Godot;
using System;

public partial class AircraftInfo : GridContainer
{
	private HUD hud = null;
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		if(hud == null)
			hud = GetNode<HUD>(this.GetParent().GetPath());
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
