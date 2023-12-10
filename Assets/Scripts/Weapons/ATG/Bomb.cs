/*  
 * Copyright October 2023 Barkın Zorlu 
 * All rights reserved. 
 */

using Godot;
using System;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Runtime.CompilerServices;

public partial class Bomb : RigidBody3D
{
	private Node rootNode = null;
	private Node3D model = null;
	private Vector3 forwardVector;
	[Export] public float releaseSpeed = 1f;
	[Export] public bool guided {get; private set;} = false;

	private bool dropped = false;
	[Export] private bool drop = false;
    // Called when the node enters the scene tree for the first time.

    public override void _Ready()
    {
        BombReady();
    }

    public  override void _Process(double delta)
	{
		BombProcess();
	}

	public void BombReady()
	{
		Freeze = true;
		FreezeMode = FreezeModeEnum.Static;
		rootNode = GetTree().Root.GetChild(0);
		
		if(model == null)
			model = GetNode<Node3D>(GetChild(0).GetPath());

		GD.Print(model.GetParent().Name);
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public void BombProcess()
	{
		forwardVector = Transform.Basis * Vector3.Forward; // forward vector

		//if(!dropped && GetMeta("Trigger").AsBool())
		if(!dropped && drop)
		{
			Freeze = false;
			dropped = true;
			Owner = rootNode;

			if(releaseSpeed == 0)
				releaseSpeed++;
			LinearVelocity = forwardVector * releaseSpeed;
		}
		else if(dropped)
		{
			// we add forward vector position and look target will not be the same
			// adding forward vector will change its rotation slight not noticable
			model.LookAt(Position + (LinearVelocity + forwardVector)); 
		}
	}
}
