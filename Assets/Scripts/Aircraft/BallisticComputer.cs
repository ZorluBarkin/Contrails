using Godot;
using System;

public partial class BallisticComputer : Node3D
{
	[Export] private PackedScene CCIP_SubScene;
	[Export] private PackedScene CCRP_SubScene;
	[Export] private PackedScene gunLead_SubScene;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		//if(CCIP_SubScene != null)
		//Load scene as child
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
