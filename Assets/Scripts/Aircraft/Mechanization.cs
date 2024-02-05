/*  
 * Copyright October 2023 Barkın Zorlu 
 * All rights reserved. 
 */

using Godot;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;

public partial class Mechanization : Node3D
{
	AircraftDamage aircraftDamage = null;
	
	#region Wings
	[Export] private bool hasAilerons = true;
	[Export] Node3D leftAileron = null; // Rotate local // can merge into one object, they move the same
	[Export] Node3D RightAileron = null; // Rotate local // can merge into one object, they move the same
	[Export] float aileronDeflectionDegree = 25f;
	[Export] float aileronSpeed = 1f;
	[Range(-1,1)] public float aileronActuation = 0f;

	[Export] private bool hasSpoilers = false;
	[Export] Node3D leftSpoilers = null; // Rotate Local
	[Export] Node3D rightSpoilers = null; // Rotate Local
	[Export] float spoilerMaxDegree = 30f;
	[Export] float spoilerSpeed = 1f;
	[Range(0,1)] public float spoilerActuation = 0f;

	[Export] private bool hasFlaps = true;
	[Export] Node3D[] flaps = null; // Rotate local // same movement but can be damaged indipendently
	[Export] public float flapMaxDegree = 40f;
	[Export] private float flapSpeed = 0.5f;
	[Range(-1,1)] public float flapActuation = 0f;

	[Export] Node3D[] elevators = null; // Rotate local // same movement but can be damaged indipendently
	[Export] float elevatorDeflectionDegrees = 35f;
	[Export] float elevatorSpeed = 1f;
	[Range(-1,1)] public float elevatorActuation = 0f;

	[Export] Node3D[] rudders = null; // Rotate local // same movement but can be damaged indipendently
	[Export] float rudderDeflectionDegree = 35f;
	[Export] float rudderSpeed = 1f;
	[Range(-1,1)] public float rudderActuation = 0f;

	[Export] bool hasSlats = false;
	[Export] Node3D leftSlats = null; // Rotate local // can merge into one object, they move the same
	[Range(0,1)] public float leftSlatActuation = 0f;
	[Export] Node3D rightSlats = null; // Rotate local // can merge into one object, they move the same
	[Range(0,1)] public float rightSlatActuation = 0f;
	[Export] float slatMaxDistance = 0.5f;
	[Export] float slatSpeed = 5f;
	#endregion

	#region Landing Gears
	[Export] private LandingGearType landingGearType = LandingGearType.Kinematic;
	[Export] private LandingGear[] landingGears = null;
	[Export] private AnimationPlayer landingGearAnimation = null;
	[Export] private string animationName = null;
	private bool landingGearsOpen = true; // true if they are open
	[Export] private CollisionShape3D[] wheelColliders = null;
	[Export] public bool actuateGears = false; // exported for test remove when done
	[Export] private bool onGround = false;
	#endregion
	public override void _Ready()
	{
	}

	public override void _Process(double delta)
	{
		ActuateControlSurfaces((float) delta);
	}

	public void OnBodyEntered(Node body)
	{
		if(body.GetParent().HasMeta("IsGround"))
		{
			if(body.GetParent().GetMeta("IsGround").AsBool())
				onGround = true;
		}
	}

	public void OnBodyExited(Node body)
	{
		if(body.GetParent().HasMeta("IsGround"))
		{
			if(body.GetParent().GetMeta("IsGround").AsBool())
				onGround = false;
		}
	}

	private void ActuateControlSurfaces(float delta)
	{
		if (hasAilerons)
			ActuateAilerons(delta);

		if (hasSpoilers)
			ActuateSpoilers(delta);
		
		if(hasFlaps)
			ActuateFlaps(delta);

		ActuateElevators(delta);
		ActuateRudders(delta);
		
		if (hasSlats)
			ActuateSlats(delta);

		ActuateGears();
	}

	private void ActuateAilerons(float delta)
	{
		
	}

	private void ActuateSpoilers(float Delta)
	{

	}

	private void ActuateFlaps(float delta)
	{

	}

	private void ActuateElevators(float delta)
	{

	}

	private void ActuateRudders(float delta)
	{

	}

	private void ActuateSlats(float delta)
	{

	}

	private void ActuateGears()
	{
		if(actuateGears)
		{
			actuateGears = false;
			if(onGround)
			{
				GD.Print("Cannot close landing Gears on the ground.");
				return;
			}

			switch(landingGearType)
			{
				case LandingGearType.Kinematic:
					for(int i = 0; i < landingGears.Length; i++)
					{
						landingGears[i].actuateGears = true;
						landingGears[i].landingGearSet = true;
					}
				break;
				case LandingGearType.Animation:
					if(landingGearsOpen) // closing
					{
						landingGearAnimation.CurrentAnimation = animationName;
						landingGearAnimation.Active = true;
						landingGearAnimation.Play(animationName);
						landingGearsOpen = false;
						for(int i = 0; i < wheelColliders.Length; i++)
							wheelColliders[i].Disabled = true;
					}
					else // opening
					{
						landingGearAnimation.PlayBackwards(animationName);
						landingGearsOpen = true;
						for(int i = 0; i < wheelColliders.Length; i++)
							wheelColliders[i].Disabled = false;
					}
				break;
			}
			
		}
	}
}
