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
	[Export] Node3D leftAileron = null; // can merge into one object, they move the same
	[Export] Node3D RightAileron = null; // can merge into one object, they move the same
	[Export] float aileronDeflectionDegree = 25f;
	[Export] float aileronSpeed = 1f;
	[Range(-1,1)] public float aileronActuation = 0f;

	[Export] Node3D[] leftFlaps = null; // same movement but can be damaged indipendently
	[Export] Node3D[] rightFlaps = null; // same movement but can be damaged indipendently
	[Export] float flapMaxDegree = 40f;
	[Export] float flapSpeed = 0.5f;
	[Range(-1,1)] public float flapActuation = 0f;

	[Export] Node3D[] elevators = null; // same movement but can be damaged indipendently
	[Export] float elevatorDeflectionDegrees = 35f;
	[Export] float elevatorSpeed = 1f;
	[Range(-1,1)] public float elevatorActuation = 0f;

	[Export] Node3D[] rudders = null; // same movement but can be damaged indipendently
	[Export] float rudderDeflectionDegree = 35f;
	[Export] float rudderSpeed = 1f;
	[Range(-1,1)] public float rudderActuation = 0f;

	[Export] bool hasSlats = false;
	[Export] Node3D leftSlats = null; // can merge into one object, they move the same
	[Range(0,1)] public float leftSlatActuation = 0f;
	[Export] Node3D rightSlats = null; // can merge into one object, they move the same
	[Range(0,1)] public float rightSlatActuation = 0f;
	[Export] float slatMaxDistance = 0.5f;
	[Export] float slatSpeed = 5f;
	#endregion

	#region Landing Gears
	[Export] LandingGear[] landingGears = null;
	public bool actuateGears = false;
	private bool onGround = false;
	#endregion
	public override void _Ready()
	{
	}

	public override void _Process(double delta)
	{
		ActuateControlSurfaces((float) delta);
		ActuateGears();
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
		ActuateAilerons(delta);
		ActuateFlaps(delta);
		ActuateElevators(delta);
		ActuateRudders(delta);
		
		if(hasSlats)
			ActuateSlats(delta);
	}

	private void ActuateAilerons(float delta)
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

			for(int i = 0; i < landingGears.Length; i++)
			{
				landingGears[i].actuateGears = true;
				landingGears[i].landingGearSet = true;
			}
		}
	}
}
