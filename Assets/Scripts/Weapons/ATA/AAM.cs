/*  
 * Copyright October 2023 Barkın Zorlu 
 * All rights reserved. 
 */

using Godot;
using System;

public partial class AAM : RigidBody3D
{
	//// Called when the node enters the scene tree for the first time.
	//public override void _Ready()
	//{
	//}
	//
	//// Called every frame. 'delta' is the elapsed time since the previous frame.
	//public override void _Process(double delta)
	//{
	//}

	public void DoGuidance(RigidBody3D target, float gLimit, float delta) // every missile does propotional Guidance (Every missile leads)
	{
		
	}

	public void Explode()
	{
	}

}
