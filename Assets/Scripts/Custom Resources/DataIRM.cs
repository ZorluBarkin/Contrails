using Godot;
using System;

[Tool]
public partial class DataIRM : Resource
{
	[Export] public bool allAspect = false;
	[Export] public bool caged = false; // only look forward if true
	//[Export] public bool slaving = false; // no radar slaving before 1984 (R-73)
	[Export] public float seekerLimit = 8f; // +-4 ==> 4 + 4 = 8 in degrees
	[Export] public float aspectAgle = 45f; // degrees
	[Export] public float burnTime = 2.2f;
	[Export] public float gLimit = 10f; // how good it can turn
	[Export] public float selfDestructTime = 24f; // 26 seconds for Aim9-B // may not need this
	//[Export] public float manueverTime = 20f; //may not implement this, no need // 21 seconds for R3
	[Export] public float launchToSteerTime = 0.5f; // time before steering where the missile will go straight. (In seconds)
	[Export] public float lockRange = 4000f; // meters, Max lock in at this range closer the better lock.
	[Export] public float launchRange = 9000f; // meters, can be launched not guarenteed to hit
	[Export] public float bestLockTemp = 700f; // anything above will give bonus
	[Export] public float bestLockRange = 1000f; // anything below will give bonus

	// high bypass engines have 300-500 degrees
	// low bypass engines have 600-700 degrees
	// after burning engines have 1400-1500 degrees
	// special engines (SR-71) have 1600-1700 degrees
}
