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
	
	[Export]
	Node2D habitatStart;
	[Export]
	Node2D roadsStart;
	[Export]
	Node2D knightStart;
	
	public List<Card> resCardObjs;
	public List<Card> streetCardObjs;
	public List<Card> inhabitCardObjs;
	public List<Card> knightCardObjs;
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
		streetCardObjs = new List<Card>();
		inhabitCardObjs = new List<Card>();
		knightCardObjs = new List<Card>();
	}

	public void init(TCPTestClient cl){
		client = cl;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
	
	private void OnEndTurnBtnDown(){
		client.SendMessage("EndTurn|");
	}

	public void buySelectedCallback(string type){
		if(client == null) return;
		string resources = type;
		foreach(Card c in resCardObjs) if(c.isSelected) resources += "," + c.type;
		client.SendMessage("Buy|"+resources);
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
			showingBuyOptins = false;
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
		foreach(Card c in resCardObjs) c.QueueFree();
		resCardObjs.Clear();
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
	
	public void SetUnknownRes(int num){
		foreach(Card c in resCardObjs) c.QueueFree();
		resCardObjs.Clear();
		for(int i = 0; i < num; i++){
			Card cardInstance = card.Instantiate<Card>();
			AddChild(cardInstance);
			cardInstance.GlobalTransform = resCardStart.GlobalTransform;
			cardInstance.setPlayer(this);
			resCardObjs.Add(cardInstance);
			cardInstance.MoveLocalX((resCardObjs.Count-1)*40f);
			cardInstance.SetTypeUnknown();
		}
	}

	public void setBuildings(BuildingCardAmnts bca){
		buildCards = bca;
		foreach(Card c in streetCardObjs) c.QueueFree();
		streetCardObjs.Clear();
		foreach(Card c in inhabitCardObjs) c.QueueFree();
		inhabitCardObjs.Clear();
		foreach(Card c in knightCardObjs) c.QueueFree();
		knightCardObjs.Clear();
		foreach(BuildingCard r in Enum.GetValues(typeof(BuildingCard))) {
			int num = buildCards.correspondingAmnt(r);
			for(int i = 0; i < num; i++){
				Card cardInstance = card.Instantiate<Card>();
				AddChild(cardInstance);
				cardInstance.SetTypeBuy(r);
				if(r == BuildingCard.Street){
					cardInstance.GlobalTransform = roadsStart.GlobalTransform;
					cardInstance.MoveLocalY(-(streetCardObjs.Count)*50f);
					cardInstance.ZIndex = -i; 
					streetCardObjs.Add(cardInstance);
					if(i % 2 == 1)cardInstance.toggleVictory(true);
				}
				else if(r == BuildingCard.Knight){
					cardInstance.GlobalTransform = knightStart.GlobalTransform;
					cardInstance.MoveLocalY(-(knightCardObjs.Count)*50f);
					cardInstance.ZIndex = -i;
					knightCardObjs.Add(cardInstance);
					if(i % 2 == 1)cardInstance.toggleVictory(true);
				}
				else {
					cardInstance.GlobalTransform = habitatStart.GlobalTransform;
					cardInstance.MoveLocalY(-(inhabitCardObjs.Count)*50f);
					cardInstance.ZIndex = -i;
					inhabitCardObjs.Add(cardInstance);
					cardInstance.toggleVictory(true);
				}
				cardInstance.setPlayer(this);
			}
		}
	}

}
