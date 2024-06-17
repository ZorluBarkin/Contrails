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

public partial class IRM : AAM, IWeapon
{
	public WeaponType weaponType {get;} = WeaponType.IRM;
	[Export] private DataIRM data;

	#region Missile Variables
	private bool allAspect = false;
	private bool caged = false; // only look forward if true
	private float seekerLimit = 8f; // degrees
	private float aspectAgle = 45f; // degrees
	private float burnTime = 2.2f;
	private float gLimit = 10f; // how good it can turn
	private float selfDestructTime = 26f; // 26 seconds for Aim9-B
	//[Export] private float manueverTime = 20f; //may not implement this, no need // 21 seconds for R3
	private Vector3 forwardVector;
	#endregion

	#region Search and Lock Variables
	private float lockRange = 4000f; // meters, Max lock in at this range closer the better lock.
	private float launchRange = 9000f; // meters, can be launched not guarenteed to hit
	private float lockQuality = 0f; // 0-1 percentage, the better the less chance of missing lock in flight

	// high bypass engines have 300-500 degrees
	// low bypass engines have 600-700 degrees
	// after burning engines have 1400-1500 degrees
	// special engines (SR-71) have 1600-1700 degrees

	public bool search = false;

	public bool locking = false;
	public bool locked = false;
	private float bestLockTemp = 700f; // anything above will give bonus
	private float bestLockRange = 1000f; // anything below will give bonus

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
		localAllAircraft = GameManager.instance.allAircraft; // this is a very pattern for efficieny
		Initilize();
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		forwardVector = Transform.Basis * Vector3.Forward;
		
		if(search)
		{
			QueryForTargets();
			// Low Growl
		}

		if (locking)
		{
			LockTarget();

			if (target == null)
				return;
			else
			{
				//pitch the sound via lock quality
				//GD.Print(target.Name);
				locked = true;
			}
		}
		
		if (locked && launch)
		{
			locking = false;
			float distance = Position.DistanceTo(target.Position);

			if(rearLock)
				targetTemp = (float) target.GetMeta("ExhaustTemp");
			else
				targetTemp = (float) target.GetMeta("FrontTemp");

			lockQuality = CalculateLockQuality(targetTemp, distance, bestLockTemp, bestLockRange, lockRange);

			if(lockQuality == 0)
				target = null;

			if(lockQuality > 0.9f) // not sure about the top value, needs testing
				Activate();
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

	private float tempQ = 0f;
	private float maxQ = 0f;

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
					tempQ = CalculateLockQuality((float) targets[i].GetMeta("FrontTemp"), Position.DistanceTo(targets[i].Position), 
												bestLockTemp, bestLockRange, lockRange);
				}
				else
				{
					tempQ = CalculateLockQuality((float) targets[i].GetMeta("ExhaustTemp"), Position.DistanceTo(targets[i].Position), 
												bestLockTemp, bestLockRange, lockRange);
				}
				
				if(maxQ <= tempQ)
				{
					maxQ = tempQ;
					target = GetNode<RigidBody3D>(targets[i].GetPath());
				}
			}
		}
		else // rear aspect only
		{
			for(int i = 0; i < targets.Count; i++)
			{
				tempQ = CalculateLockQuality((float) targets[i].GetMeta("ExhaustTemp"), Position.DistanceTo(targets[i].Position), 
									bestLockTemp, bestLockRange, lockRange);
				
				//GD.Print(temp); // used for debugging

				if(maxQ <= tempQ)
				{
					maxQ = tempQ;
					rearLock = true;
					target = GetNode<RigidBody3D>(targets[i].GetPath()); // need velocity and position
				}
			}
		}
	}

	private static float CalculateLockQuality(float targetTemp, float distance, float bestLockTemp, float bestLockRange, float LockRange)
	{
		return 1 * (targetTemp / bestLockTemp) * Mathf.Clamp((LockRange / distance) / (LockRange / bestLockRange), 0f, 1f);
	}

	#region Interface Implementations
    
	public void Initilize()
    {
		if(data != null)
		{
			allAspect 	= data.allAspect;
			caged 		= data.caged;
			seekerLimit	= data.seekerLimit;
			aspectAgle	= data.aspectAgle;
			burnTime	= data.burnTime;
			gLimit		= data.gLimit;
			selfDestructTime = data.selfDestructTime;
			lockRange 	= data.lockRange;
			launchRange = data.launchRange;
			bestLockTemp = data.bestLockTemp;
			bestLockRange = data.bestLockRange;
		}
		else
			GD.PrintErr($"{this.Name} is missing DataIRM!");
    }

	public void Activate()
	{
        throw new NotImplementedException();
	}

	#endregion
}
