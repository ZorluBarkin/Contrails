/*  
 * Copyright October 2023 Barkın Zorlu 
 * All rights reserved. 
 */

using Godot;
using System;
using System.Diagnostics;

public partial class AircraftSoundManager : Node
{
	[Export] bool centralPlayerOnly = true;

	private AircraftPistonEngine[] pistonEngines;
	private bool isPistonEngine = false;
	[Export] private AudioStreamPlayer3D centralSound;
	[Export] private AudioStreamPlayer3D[] engineSoundPlayers;
	[Export] private AudioStreamOggVorbis idleSound;
	[Export] private AudioStreamOggVorbis cruisingSound;

	[Export] private float idleSoundUpperRPMLimit = 1200f;
	private float centralSoundVolume = 2.5f;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		SetEngines();
		centralSoundVolume = centralSound.VolumeDb;
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
			bool idleRPMs = true;
			bool highRPMs = true;

			for(int i = 0; i < pistonEngines.Length; i++)
			{
				if(pistonEngines[i].engineOn)
				{
					idleRPMs &= pistonEngines[i].RPM < idleSoundUpperRPMLimit ? true : false;
					highRPMs &= pistonEngines[i].RPM > idleSoundUpperRPMLimit ? true : false;
					activeEngines++;
				}
			}

			if(idleRPMs || highRPMs) // same audio are playing
			{
				centralSound.VolumeDb = centralSoundVolume * activeEngines / pistonEngines.Length;
				PlayCentralSound();
			}
			else // different audio are playing
			{
				if(centralSound.Playing)
					centralSound.Stop();
				PlayMultipleEngineSounds();
			}
			
			StopPistonEngineSound(activeEngines);
		}
		else // Jet engines
		{

		}
	}

	private void PlayCentralSound()
	{
		if(isPistonEngine)
		{
			if(!centralSound.Playing)
				centralSound.Play();
			else
			{
				bool idling = false;
				for(int i = 0; i < pistonEngines.Length; i++)
					if(pistonEngines[i].engineOn)
						idling |= pistonEngines[i].RPM < idleSoundUpperRPMLimit ? true : false;

				if(idling)
				{
					if(centralSound.Stream != idleSound)
						centralSound.Stream = idleSound;

					float averageScale = 0;
					for(int i = 0; i < pistonEngines.Length; i++)
						averageScale += (pistonEngines[i].RPM < pistonEngines[i].idleRPM ? pistonEngines[i].idleRPM : pistonEngines[i].RPM) / pistonEngines[i].idleRPM;
					averageScale /= pistonEngines.Length;
					centralSound.PitchScale = averageScale;
				}
				else
				{
					if(centralSound.Stream != cruisingSound)
						centralSound.Stream = cruisingSound;
					
					float averageScale = 0;
					for(int i = 0; i < pistonEngines.Length; i++)
						averageScale += 1 + (pistonEngines[i].RPM - pistonEngines[i].leanRPM) / pistonEngines[i].leanRPM;
					averageScale /= pistonEngines.Length;
					centralSound.PitchScale = averageScale;
				}
			}	
		}
	}

	private void PlayMultipleEngineSounds()
	{
		if(isPistonEngine)
		{
			for(int i = 0; i < pistonEngines.Length; i++)
			{
				if(pistonEngines[i].engineOn)
					if(!engineSoundPlayers[i].Playing)
						engineSoundPlayers[i].Play();
				else
					return;

				if(pistonEngines[0].RPM < idleSoundUpperRPMLimit) // Idle
				{
					if(engineSoundPlayers[i].Stream != idleSound)
						engineSoundPlayers[i].Stream = idleSound;
					
					engineSoundPlayers[i].PitchScale = pistonEngines[0].RPM / pistonEngines[0].idleRPM;
				}
				else
				{
					if(engineSoundPlayers[i].Stream != cruisingSound)
						engineSoundPlayers[i].Stream = cruisingSound;

					engineSoundPlayers[i].PitchScale = 1 + (pistonEngines[i].RPM - pistonEngines[i].leanRPM) / pistonEngines[i].leanRPM;;
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
