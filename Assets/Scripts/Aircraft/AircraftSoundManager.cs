/*  
 * Copyright October 2023 Barkın Zorlu 
 * All rights reserved. 
 */

using Godot;
using System;

public partial class AircraftSoundManager : Node
{
	private AircraftPistonEngine[] pistonEngines;
	private bool isPistonEngine = false;
	[Export] private AudioStreamPlayer3D centralSound;
	[Export] private AudioStreamPlayer3D[] engineSoundPlayers;
	[Export] private AudioStreamOggVorbis idleSound;
	[Export] private AudioStreamOggVorbis cruisingSound;

	[Export] private bool testCloseBool = false;
	[Export] private float testThrottle = 50f;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		SetEngine();
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		//if(testCloseBool)
		//{
		//	pistonEngines[1].engineOn = false;
		//}
		//else
		//{
		//	pistonEngines[1].throttle = testThrottle;
		//}
		
		//PlayEngineSounds();
	}

	private void PlayEngineSounds()
	{
		if(isPistonEngine)
		{
			PlayPistonEngineSound();
		}
	}

	private void PlayPistonEngineSound()
	{
		int activeEngineNumber = 0;
		bool sameRPM = true;
		for(int i = 0; i < pistonEngines.Length;i++)
		{
			if(pistonEngines[i].engineOn)
				activeEngineNumber++;
			else
				sameRPM = false;
		}

		if(sameRPM)
		{
			if(!centralSound.Playing)
				centralSound.Play();
			else
			{
				// volume *= 2;
				centralSound.PitchScale = 1f + ((pistonEngines[0].RPM - pistonEngines[0].leanRPM) / pistonEngines[0].leanRPM);
			}
			return;
		}

		if(activeEngineNumber > 0)
		{
			if(centralSound.Playing)
				centralSound.Stop();

			for(int i = 0; i < pistonEngines.Length; i++)
			{
				if(pistonEngines[i].engineOn)
				{
					if(!engineSoundPlayers[i].Playing)
						engineSoundPlayers[i].Play();
					else
					{
						engineSoundPlayers[i].PitchScale = 1f + ((pistonEngines[i].RPM - pistonEngines[i].leanRPM) / pistonEngines[i].leanRPM);
					}
				}
				else
				{
					if(engineSoundPlayers[i].Playing)
						engineSoundPlayers[i].Stop();
				}
			}
		}
	}

	private void SetEngine()
	{
		pistonEngines = new AircraftPistonEngine[GameManager.instance.vehicleControls.engines.Length];
		//jetEngines = new AircraftJetEngine[vehicleControls.engines.Length];
		for(int i = 0; i <  GameManager.instance.vehicleControls.engines.Length; i++)
		{
			if( GameManager.instance.vehicleControls.engines[i] is AircraftPistonEngine)
			{
				pistonEngines[i] = GetNode<AircraftPistonEngine>( GameManager.instance.vehicleControls.engines[i].GetPath());
				isPistonEngine = true;
				//Array.Clear(jetEngines);
			}
			else
			{GD.Print("Its not piston");}
			//else (vehicleControls.engines[i] is AircraftJetEngine)
			//{
			//	jetEngines = GetNode<AircraftJetEngine>(vehicleControls.engines[i].GetPath());
			//	isPistonEngine = false;
			//}
		}
		
	}
}
