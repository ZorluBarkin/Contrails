/*  
 * Copyright October 2023 Barkın Zorlu 
 * All rights reserved. 
 */

using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime;
using System.Runtime.CompilerServices;

public partial class IRM : AAM
{
	#region Missile Variables
	[Export] private bool allAspect = false;
	[Export] private bool caged = false; // only look forward if true
	[Export] private float seekerLimit = 8f; // degrees
	[Export] private float aspectAgle = 45f; // degrees
	[Export] private float burnTime = 2.2f;
	[Export] private float gLimit = 10f; // how good it can turn
	[Export] private float selfDestructTime = 26f; // 26 seconds for Aim9-B
	[Export] private float manueverTime = 20f; //may not implement this, no need // 21 seconds for R3
	private Vector3 forwardVector;
	#endregion

	#region Search and Lock Variables
	[Export] private float lockRange = 4000f; // meters, Max lock in at this range closer the better lock.
	[Export] private float launchRange = 9000f; // meters, can be launched not guarenteed to hit
	private float lockQuality = 0f; // 0-1 percentage, the better the less chance of missing lock in flight

	// high bypass engines have 300-500 degrees
	// low bypass engines have 600-700 degrees
	// after burning engines have 1400-1500 degrees
	// special engines (SR-71) have 1600-1700 degrees

	[Export] public bool locking = false;
	public bool locked = false;
	[Export] private float bestLockTemp = 700f; // anything above will give bonus
	[Export] private float bestLockRange = 1000f; // anything below will give bonus

	private bool rearLock = false;

	private List<Node3D> localAllAircraft = null; // private for now
	private List<Node3D> targets = new List<Node3D>(); // private for now

	private RigidBody3D target = null; // make it so if sun is in 10 degrees of missile lock on to it
	private float targetTemp = 0f;
	#endregion

	public bool launch = false;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		localAllAircraft = GameManager.instance.allAircraft;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		forwardVector = Transform.Basis * Vector3.Forward;
		
		QueryForTargets();

		if (locking)
		{
			LockTarget();

			if (target == null)
				return;
			else
			{
				locking = false;
				locked = true;
			}
		}
		
		if (locked && launch)
		{
			float distance = Position.DistanceTo(target.Position);

			if(rearLock)
				targetTemp = (float) target.GetMeta("ExhaustTemp");
			else
				targetTemp = (float) target.GetMeta("FrontTemp");

			lockQuality = CalculateLockQuality(targetTemp, distance, bestLockTemp, bestLockRange, lockRange);

			if(lockQuality == 0)
				target = null;
		}
	}

	/// <summary>
	/// Search for targets. Its better if its dynamic method.
	/// </summary>
	/// <returns></returns>
	private void QueryForTargets()
	{
		targets.Clear();

		// Elination
		for (int i = 0; i < localAllAircraft.Count; i++)
		{
			if (forwardVector.Dot((localAllAircraft[i].Position - Position).Normalized()) > MathF.Cos(Mathf.DegToRad(seekerLimit / 2f)))
			{
				if (Position.DistanceTo(localAllAircraft[i].Position) < lockRange) // in range
				{
					if(!allAspect)
						if(forwardVector.Dot(-1 * localAllAircraft[i].GlobalBasis.Z) > Mathf.DegToRad(aspectAgle)) // 45 degrees
							targets.Add(localAllAircraft[i]);
					else
						targets.Add(localAllAircraft[i]);
				}
			}
		}
	}

	private float temp = 0f;
	private float max = 0f;
	private void LockTarget() // selecting the target
	{
		// Radar select if missile and plane has it
		target = null;

		if(allAspect)
		{
			for(int i = 0; i < targets.Count; i++)
			{
				if(forwardVector.Dot(-1 * localAllAircraft[i].GlobalBasis.Z) > 0)
				{
					temp = CalculateLockQuality((float) targets[i].GetMeta("FrontTemp"), Position.DistanceTo(targets[i].Position), 
												bestLockTemp, bestLockRange, lockRange);
				}
				else
				{
					temp = CalculateLockQuality((float) targets[i].GetMeta("ExhaustTemp"), Position.DistanceTo(targets[i].Position), 
												bestLockTemp, bestLockRange, lockRange);
				}
				if(max <= temp)
				{
					max = temp;
					target = GetNode<RigidBody3D>(targets[i].GetPath());
				}
			}
		}
		else // rear aspect only
		{
			for(int i = 0; i < targets.Count; i++)
			{
				temp = CalculateLockQuality((float) targets[i].GetMeta("ExhaustTemp"), Position.DistanceTo(targets[i].Position), 
									bestLockTemp, bestLockRange, lockRange);
				if(max <= temp)
				{
					max = temp;
					rearLock = true;
					target = GetNode<RigidBody3D>(targets[i].GetPath());
				}
			}
		}
	}

	private static float CalculateLockQuality(float targetTemp, float distance, float bestLockTemp, float bestLockRange, float LockRange)
	{
		return 1 * (targetTemp / bestLockTemp) * Mathf.Clamp(((LockRange / distance) / (LockRange / bestLockRange)), 0f, 1f);
	}
}
