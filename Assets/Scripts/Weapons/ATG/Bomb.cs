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
	[Export]private Node rootNode = null;
	private Node3D model = null;
	private Vector3 forwardVector;
	[Export] public float releaseSpeed = 1f;
	[Export] public float MaxLaunchSpeed {get; private set;} = 340.27f * 1.2f; // 1.2 Mach
	[Export] public float[] DragMachNumber = {0.95f, 1.05f};
	[Export] public float[] DragPerMach {get; private set;} = {0.152f, 0.1918f, 0.3460f};

	#region High Drag Bomb Variables
	[Export] public bool highDrag {get; private set;} = false;
	[Export] public AnimationPlayer highDragAnimation = null;
	[Export] public string animationName = null;
	[Export] public float highDragCoeff {get; private set;} = 1f; //1.35f; // works best, 3.4 is the real number
	#endregion
	
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

		//GD.Print(model.GetParent().Name);
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public void BombProcess()
	{
		//forwardVector = Transform.Basis * Vector3.Forward; // forward vector
		forwardVector = -GlobalBasis.Z;

		if(!dropped && drop)
		{
			Freeze = false;
			dropped = true;

			//Vector3 oldRotation = GlobalRotation;
			//Vector3 oldPosition = GlobalPosition;
			//Transform3D oldTransform = GlobalTransform;

			//GD.Print(Mathf.RadToDeg(oldRotation.X) + " " + Mathf.RadToDeg(oldRotation.Y) + " " + Mathf.RadToDeg(oldRotation.Z));
			
			//Owner = rootNode;
			// /Owner.RemoveChild(this);
			//Owner = null;
			//rootNode.AddChild(this);

			this.Reparent(rootNode, true);

			//Rotation = oldRotation;
			//Position = oldPosition;
			
			//Transform = oldTransform;

			//GlobalRotation = oldRotation;
			//GD.Print(GlobalRotationDegrees);
			
			if(releaseSpeed == 0)
				releaseSpeed++;
			LinearVelocity = forwardVector * 10f; //releaseSpeed;
		}
		else if(dropped)
		{
			if(highDrag)
			{
				BrakeActivation();
			}
			else
			{
				float machSpeed = LinearVelocity.Length() / 340.27f;

				if (machSpeed < DragMachNumber[0])
					if(LinearDamp != DragPerMach[0])
						LinearDamp = DragPerMach[0];
				else if (machSpeed < DragMachNumber[1])
					if(LinearDamp != DragPerMach[1])
						LinearDamp = DragPerMach[1];
				else // above mach 1.05
					if(LinearDamp != DragPerMach[2])
						LinearDamp = DragPerMach[2];
			}

			model.LookAt(Position - LinearVelocity); // makes it look forwards
		}
	}

	/// <summary>
	/// For high drag bombs activation of animations and effects, its async because it has to wait for animation finish.
	/// </summary>
	private async void BrakeActivation()
	{
		if(LinearDamp != highDragCoeff)
		{
			if(!highDragAnimation.Active)
			{
				highDragAnimation.CurrentAnimation = animationName;
				//GD.Print(highDragAnimation.CurrentAnimation);
				highDragAnimation.Active = true;
				await ToSignal(GetTree().CreateTimer(highDragAnimation.CurrentAnimationLength), SceneTreeTimer.SignalName.Timeout);
				// do not use highDragAnimation.CurrentAnimation.Length, it returns wrong for parallel animations
				LinearDamp = highDragCoeff;
			}
		}
	}
}
