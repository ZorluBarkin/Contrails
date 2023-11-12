/*  
 * Copyright October 2023 Barkın Zorlu 
 * All rights reserved. 
 */

using Godot;
using System;
using System.Diagnostics;

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
		SetEngines();
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		PlayEngineSounds();
	}

	private void PlayEngineSounds()
	{
		if(isPistonEngine)
		{
			int activeEngines = 0;
			bool rpmIsSame = true;
			for(int i = 0; i < pistonEngines.Length; i++)
			{
				if(pistonEngines[i].engineOn)
				{
					if(activeEngines > 0)
						if(pistonEngines[i].RPM != pistonEngines[i-1].RPM)
							rpmIsSame = false;
					activeEngines++;
				}
				else
					rpmIsSame = false;
			}

			PlayPistonEngineSound(activeEngines , rpmIsSame);
			StopPistonEngineSound(activeEngines);
		}
	}

	private void PlayPistonEngineSound(int activeEngineNumber, bool sameRPM)
	{
		if(sameRPM)
		{
			for(int i = 0; i < engineSoundPlayers.Length; i++)
				if(engineSoundPlayers[i].Playing)
					engineSoundPlayers[i].Stop();

			if(!centralSound.Playing)
				centralSound.Play();
			else
			{
				// volume *= 2;
				if(pistonEngines[0].RPM < pistonEngines[0].idleRPM + 600f) // Idle
				{
					if(centralSound.Stream != idleSound)
						centralSound.Stream = idleSound;
					
					centralSound.PitchScale = pistonEngines[0].RPM / pistonEngines[0].idleRPM;
				}
				else
				{
					if(centralSound.Stream != cruisingSound)
						centralSound.Stream = cruisingSound;
					
					centralSound.PitchScale = 1f + ((pistonEngines[0].RPM - pistonEngines[0].leanRPM) / pistonEngines[0].leanRPM);
				}
			}
		}
		else if(activeEngineNumber > 0)
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
						if(pistonEngines[i].RPM < pistonEngines[i].idleRPM + 600f) // Idle
						{
							if(engineSoundPlayers[i].Stream != idleSound)
								engineSoundPlayers[i].Stream = idleSound;
							
							engineSoundPlayers[i].PitchScale = pistonEngines[i].RPM / pistonEngines[i].idleRPM;
						}
						else
						{
							if(engineSoundPlayers[i].Stream != cruisingSound)
								engineSoundPlayers[i].Stream = cruisingSound;
							
							engineSoundPlayers[i].PitchScale = 1f + ((pistonEngines[i].RPM - pistonEngines[i].leanRPM) / pistonEngines[i].leanRPM);
						}		
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

	private void StopPistonEngineSound(int activeEngineNumber)
	{
		if(activeEngineNumber == 0)
		{
			if(centralSound.Playing)
				centralSound.Stop();
		}

		for(int i = 0; i < engineSoundPlayers.Length; i++)
		{
			if(!pistonEngines[i].engineOn)
			{
				if(engineSoundPlayers[i].Playing)
					engineSoundPlayers[i].Stop();
			}
		}
	}

	private void SetEngines()
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
