using Godot;
using System;

public partial class GuidedBomb : Bomb
{
	public Vector3? targetPosition;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		BombReady();
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		BombProcess();
		DoGuidance(targetPosition);
	}

	private void DoGuidance(Vector3? target)
	{
		if(target == null)
			return;
			//GD.Print("Nurbanu <3");
	}
}
