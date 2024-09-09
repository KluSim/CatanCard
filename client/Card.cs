using shared;
using Godot;
using System;

public partial class Card : Node2D
{

	[Export]
	TextureRect textureSlot;
	[Export]
	TextureRect outline;
	[Export]
	TextureRect victorypoint;
	[Export]
	Texture2D wood;
	[Export]
	Texture2D clay;
	[Export]
	Texture2D iron;
	[Export]
	Texture2D wheat;
	[Export]
	Texture2D sheep;

	[Export]
	Texture2D village;
	[Export]
	Texture2D city;
	[Export]
	Texture2D knight;
	[Export]
	Texture2D expansion;
	[Export]
	Texture2D street;
	
	[Export]
	Texture2D unknown_res;
	
	public Player player;
	public bool isSelected = false;
	public bool isMarketCard = false;
	public selectedCallback selectedCallback;

	public string type;
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
	public void setPlayer(Player p){
		player = p;
	}

	public void SetTypeRc(RessourceCard rc){
		switch(rc){
			case RessourceCard.Clay:
				textureSlot.Texture = clay;
				type = "Clay";
				break;
			case RessourceCard.Wood:
				textureSlot.Texture = wood;
				type = "Wood";
				break;
			case RessourceCard.Sheep:
				textureSlot.Texture = sheep;
				type = "Sheep";
				break;
			case RessourceCard.Iron:
				textureSlot.Texture = iron;
				type = "Iron";
				break;
			case RessourceCard.Wheat:
				textureSlot.Texture = wheat;
				type = "Wheat";
				break;
		}
	}
	
	public void SetTypeUnknown(){
		textureSlot.Texture = unknown_res;
		type = "unknown_res";
		outline.Show();
	}

	public void SetTypeBuy(BuildingCard bc){
		switch(bc){
			case BuildingCard.Village:
				textureSlot.Texture = village;
				type = "Village";
				break;
			case BuildingCard.City:
				textureSlot.Texture = city;
				type = "City";
				break;
			case BuildingCard.Knight:
				textureSlot.Texture = knight;
				type = "Knight";
				break;
			case BuildingCard.Expansion:
				textureSlot.Texture = expansion;
				type = "Expansion";
				break;
			case BuildingCard.Street:
				textureSlot.Texture = street;
				type = "Street";
				break;
		}
	}

	public void SelectToggle(){
		if(!isMarketCard && (player == null || !player.isMainPlayer)) return;
		isSelected = !isSelected;
		if(isSelected){
			outline.Show();
			if(selectedCallback != null)selectedCallback(type);
		}
		else
			outline.Hide();
	}

	public void SetSelectNoCb(bool s){
		isSelected = s;
		if(isSelected){
			outline.Show();
		}
		else
			outline.Hide();
	}

	public override void _Input(InputEvent @event)
	{
		if (@event is InputEventMouseButton eventMouseButton && eventMouseButton.Pressed && eventMouseButton.ButtonIndex == MouseButton.Left)
		{
			InputEventMouseButton e = @event as InputEventMouseButton;
			GD.Print(new Rect2(new Vector2(), outline.Size));
			if(outline.GetGlobalRect().HasPoint(e.Position)){
				SelectToggle();
			}
		}
	}
	
	public void toggleVictory(bool b) { //Nute gunray: ahhhh... victory
		if(b) victorypoint.Show();
		else victorypoint.Hide();
	}
}
