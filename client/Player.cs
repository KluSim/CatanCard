using shared;
using Godot;
using System;

using System.Collections.Generic;

public partial class Player : Node3D
{
	[Export]
	PackedScene card;
	[Export]
	Node3D resCardStart;

	List<Card> allCards; //CONTAINS DELETEDS!!

	RessourceCardAmnts resCards;
	ExpansionCardAmnts expCards;
	BuildingCardAmnts buildCards;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		resCards = new RessourceCardAmnts();
		expCards = new ExpansionCardAmnts(0,0,0,0,0,0);
		buildCards = new BuildingCardAmnts(0,0,0,0,0);
		allCards = new List<Card>();
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	public void AddRes(RessourceCard rc){
		resCards.correspondingAmnt(rc)+=1;
		Card cardInstance = card.Instantiate<Card>();
		AddChild(cardInstance);
		cardInstance.GlobalTransform = resCardStart.GlobalTransform;
		cardInstance.setPlayer(this);
		cardInstance.TranslateObjectLocal(new Vector3((resCards.total-1)*1.0001f,0,0));
		allCards.Add(cardInstance);
		cardInstance.SetTypeRc(rc);
	}

}
