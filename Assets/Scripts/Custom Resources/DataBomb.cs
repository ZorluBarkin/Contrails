using Godot;
using System;

public partial class DataBomb : Resource
{
    [Export] public float MaxLaunchSpeed {get; private set;} = 340.27f * 1.2f; // 1.2 Mach
	[Export] public float[] DragPerMach {get; private set;} = {0.152f, 0.1918f, 0.3460f};

	//[Export] public bool highDrag {get; private set;} = false;
	//[Export] public AnimationPlayer highDragAnimation = null;
	//[Export] public string animationName = null;
	//[Export] public float highDragCoeff {get; private set;} = 1f; //1.35f; // works best, 3.4 is the real number
}
