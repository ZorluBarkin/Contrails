using Godot;
using System;

public partial class EngineInfo : VBoxContainer
{
	private HUD hud = null;
	[Export] private Label engineLabel = null; // Engine Number

	// Engine Base information Container
	[Export] private TextureRect throttleSymbol = null;
	[Export] private Label throttleLabel = null;
	[Export] private Texture2D[] throttleSymbols; // from Increase to Decrease;
	[Export] private TextureRect RPMSymbol = null;
	[Export] private Label RPMLabel = null;
	[Export] private Texture2D[] RPMSymbols; // from Low to High;

	[Export] private Label waterTempLabel = null;
	[Export] private Label oilTempLabel = null;

	// Details Container
	[Export] private Container detailsContainer = null;

	[Export] private TextureRect waterCowlSymbol = null;
	[Export] private Label waterCowlLabel = null;
	[Export] private Texture2D[] waterCowlSymbols; // from closed to Open;

	[Export] private TextureRect oilCowlSymbol = null;
	[Export] private Label oilCowlLabel = null;
	[Export] private Texture2D[] oilCowlSymbols; // from Closed to Open;

	[Export] private CheckButton feathering = null;
	[Export] private bool seeDetails = false; // make this change via button
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		if(hud == null)
			hud = GetNode<HUD>(this.GetParent().GetPath());
		
		//engineLabel.Text = engineNumber.ToString();
		engineLabel.Text = "1";
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		DetailVisibility();
	}

	private void DetailVisibility()
	{
		if(!seeDetails)
		{
			if(detailsContainer.Visible)
				detailsContainer.Visible = false;
		}
		else
		{
			if(!detailsContainer.Visible)
				detailsContainer.Visible = true;
		}
	}
}
