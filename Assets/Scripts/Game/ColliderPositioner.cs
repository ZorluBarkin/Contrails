using Godot;
using System;
using System.Diagnostics;
using System.Reflection.Metadata;

/// <summary>
/// Meant to set the position of the collider with the respective object
/// Example: Flap collider pos = Flaps pos
/// </summary>
public partial class ColliderPositioner : CollisionShape3D
{
	// collider is this object
	[Export] private Node3D originalNode = null;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		if(originalNode == null)
		{
			GD.PrintErr("No Reference Object for \'" + this.Name + "\' exists.");
			return;
		}
	}
	
	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if(originalNode != null)
			this.GlobalRotationDegrees = originalNode.GlobalRotationDegrees;
	}
}
