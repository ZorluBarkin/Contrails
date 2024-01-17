/*  
 * Copyright October 2023 Barkın Zorlu 
 * All rights reserved. 
 */

using Godot;
using System;
using System.Runtime;

public partial class IRM : AAM
{
	[Export] private bool allAspect = false;
	[Export] private float lockRange = 4000f; // meters, Max lock in at this range closer the better lock.
	[Export] private float launchRange = 9000f; // meters, can be launched not guarenteed to hit
	private float lockQuality = 0f; // 0-1 percentage, the better the less chance of missing lock in flight

	// high bypass engines have 300-500 degrees
	// low bypass engines have 600-700 degrees
	// after burning engines have 1400-1500 degrees
	// special engines (SR-71) have 1600-1700 degrees

	[Export] private float bestLockTemp = 700f; // anything above will give bonus
	[Export] private float bestLockRange = 1000f; // anything below will give bonus

	private bool rearLock = false;
	private RigidBody3D target = null; // make it so if sun is in 10 degrees of missile lock on to it
	private float targetTemp = 0f;


	public bool launch = false;
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if(launch)
		{
			float distance = Position.DistanceTo(target.Position);

			if(rearLock)
				targetTemp = (float) target.GetMeta("ExhaustTemp");
			else
				targetTemp = (float) target.GetMeta("FrontTemp");

				lockQuality = CalculateLockQuality(targetTemp, distance, bestLockTemp, bestLockRange, lockRange);
		}
	}

	private void QueryForTargets() // looking for targets
	{

	}

	private static RigidBody3D LockTarget() // selecting the target
	{
		// Radar select if missile and plane has it
		RigidBody3D selectedTarget = null;
		// calculate rear lock or not
		//check degrees if at the back 
		//rearLock = true;
		// targetTemp = target.GetMeta("ExhaustTemp");
		//else
		//rearLock = false;
		// targetTemp = target.GetMeta("FrontTemp");

		return selectedTarget;
	}

	private static float CalculateLockQuality(float targetTemp, float distance, float bestLockTemp, float bestLockRange, float LockRange)
	{
		return 1 * (targetTemp / bestLockTemp) * ((LockRange / distance) / (LockRange / bestLockRange));
	}
}
