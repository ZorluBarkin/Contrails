using Godot;
using System;

[Tool]
public partial class DataIRM : Resource
{
	[Export] public bool allAspect = false;
	[Export] public bool caged = false; // only look forward if true
	[Export] public float seekerLimit = 8f; // degrees
	[Export] public float aspectAgle = 45f; // degrees
	[Export] public float burnTime = 2.2f;
	[Export] public float gLimit = 10f; // how good it can turn
	[Export] public float selfDestructTime = 26f; // 26 seconds for Aim9-B // may not need this
	//[Export] public float manueverTime = 20f; //may not implement this, no need // 21 seconds for R3
	[Export] public float lockRange = 4000f; // meters, Max lock in at this range closer the better lock.
	[Export] public float launchRange = 9000f; // meters, can be launched not guarenteed to hit
	[Export] public float bestLockTemp = 700f; // anything above will give bonus
	[Export] public float bestLockRange = 1000f; // anything below will give bonus
}
