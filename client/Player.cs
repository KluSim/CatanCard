using shared;
using Godot;
using System;

using System.Collections.Generic;

public partial class Player : Node2D
{
	[Export]
	PackedScene card;
	[Export]
	Node2D resCardStart;
	[Export]
	Node2D tmpCardsStart;

	List<Card> resCardObjs;
	List<Card> tmpCardObjs;

	RessourceCardAmnts resCards;
	ExpansionCardAmnts expCards;
	BuildingCardAmnts buildCards;

	TCPTestClient client;
	bool showingBuyOptins = false;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		resCards = new RessourceCardAmnts();
		expCards = new ExpansionCardAmnts(0,0,0,0,0,0);
		buildCards = new BuildingCardAmnts(0,0,0,0,0);
		resCardObjs = new List<Card>();
		tmpCardObjs = new List<Card>();
	}

	public void init(TCPTestClient cl){
		client = cl;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
	private void OnStartGameBtnDown(){
		client.SendMessage("StartGame|");
	}
	private void OnEndTurnBtnDown(){
		client.SendMessage("EndTurn|");
	}

	public void buySelectedCallback(string type){
		Console.WriteLine(type);
	}
	private void OnBuyBtnDown(){
		//client.SendMessage("EndTurn|");
		if(!showingBuyOptins){
			foreach(BuildingCard r in Enum.GetValues(typeof(BuildingCard))) {
				Card cardInstance = card.Instantiate<Card>();
				AddChild(cardInstance);
				cardInstance.GlobalTransform = tmpCardsStart.GlobalTransform;
				cardInstance.setPlayer(this);
				tmpCardObjs.Add(cardInstance);
				cardInstance.MoveLocalX((tmpCardObjs.Count-1)*110f);
				cardInstance.SetTypeBuy(r);
				cardInstance.selectedCallback = buySelectedCallback;
			}
			showingBuyOptins = true;
		}
		else{
			foreach(Card c in tmpCardObjs) c.QueueFree();
			tmpCardObjs.Clear();
		}
	}

	public void AddRes(RessourceCard rc){
		resCards.correspondingAmnt(rc)+=1;
		Card cardInstance = card.Instantiate<Card>();
		AddChild(cardInstance);
		cardInstance.GlobalTransform = resCardStart.GlobalTransform;
		cardInstance.setPlayer(this);
		cardInstance.MoveLocalX((resCardObjs.Count-1)*110f);
		resCardObjs.Add(cardInstance);
		cardInstance.SetTypeRc(rc);
	}

	public void SetRes(RessourceCardAmnts rca){
		resCards = rca;
		foreach(RessourceCard r in Enum.GetValues(typeof(RessourceCard))) {
			int num = resCards.correspondingAmnt(r);
			for(int i = 0; i < num; i++){
				Card cardInstance = card.Instantiate<Card>();
				AddChild(cardInstance);
				cardInstance.GlobalTransform = resCardStart.GlobalTransform;
				cardInstance.setPlayer(this);
				resCardObjs.Add(cardInstance);
				cardInstance.MoveLocalX((resCardObjs.Count-1)*110f);
				cardInstance.SetTypeRc(r);
			}
		}
	}

	

}
