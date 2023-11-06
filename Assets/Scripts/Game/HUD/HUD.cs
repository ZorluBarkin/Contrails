using Godot;
using System;

public partial class HUD : Control // Should not be a Singleton
{
	[Export] private Label FPSLabel = null;
	private bool lowFPS = true;
	private bool seeFPS = true;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		if(FPSLabel == null)
			this.GetChild<Label>(0);

		if(Settings.instance.seeFPS)
			seeFPS = true;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		UpdateFPSLabel();
	}

	private void UpdateFPSLabel()
	{
		if(seeFPS)
		{
			if(Engine.GetFramesPerSecond() < 30)
			{
				if(!lowFPS)
				{
					FPSLabel.LabelSettings.FontColor = Colors.Red;
					lowFPS = true;
				}
			}
			else
			{
				if(lowFPS)
				{
					FPSLabel.LabelSettings.FontColor = Colors.Green;
					lowFPS = false;
				}
			}
			FPSLabel.Text = "FPS: " + Engine.GetFramesPerSecond();
		}
	}
}
