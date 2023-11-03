/*  
 * Copyright October 2023 Barkın Zorlu 
 * All rights reserved. 
 */

using Godot;
using System;

public partial class Bomb : RigidBody3D
{
	private Node baseNode = null;
	private bool dropped = false;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		Freeze = true;
		FreezeMode = FreezeModeEnum.Static;
		baseNode = GetTree().Root.GetChild(0);
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if(!dropped && GetMeta("Trigger").AsBool())
		{
			Freeze = false;
			dropped = true;
			Owner = GetTree().Root.GetChild(0);
		}
	}
}
