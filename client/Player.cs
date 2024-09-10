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
	
	[Export]
	public bool isMainPlayer;
	[Export] public HBoxContainer scroller;
	
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

	public void buySelectedCallback(string type, bool isExpansion){
		if(client == null) return;
		if(isExpansion) return; //TODO this is dumb
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
				cardInstance.MoveLocalX((tmpCardObjs.Count-1)*80f);
				cardInstance.SetTypeBuy(r);
				//cardInstance.selectedCallback = buySelectedCallback;
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
		foreach(TextureRect n in scroller.GetChildren()){
			scroller.RemoveChild(n);
			n.QueueFree();
		}
		foreach(RessourceCard r in Enum.GetValues(typeof(RessourceCard))) {
			int num = resCards.correspondingAmnt(r);
			for(int i = 0; i < num; i++){
				Card cardInstance = card.Instantiate<Card>();
				cardInstance.selectedCallback = GetParent<Main>().playerSelectedCallback;
				AddChild(cardInstance);
				TextureRect c = cardInstance.GetChild(0) as TextureRect;
				cardInstance.RemoveChild(c);
				scroller.AddChild(c);
				cardInstance.GlobalTransform = resCardStart.GlobalTransform;
				cardInstance.setPlayer(this);
				resCardObjs.Add(cardInstance);
				cardInstance.MoveLocalX((resCardObjs.Count-1)*100f);
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
			cardInstance.MoveLocalX((resCardObjs.Count-1)*10f);
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
					cardInstance.MoveLocalY(-(streetCardObjs.Count)*30f);
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
	
	
	
	//
	// FROM HERE ON OUT -> UI THINGS
	//
	
	[Export] Control ConnectionInterface;
	[Export] Control GameInterface;
	[Export] Control TradeInterface;
	[Export] Control BuyInterface;
	[Export] Control ExpansionInterface;
	
	public void GeneralBack(){ GameInterface.Show(); TradeInterface.Hide(); BuyInterface.Hide(); }
	
	public void BuyButton(){ GameInterface.Hide(); BuyInterface.Show(); }
	public void TradeButton(){ GameInterface.Hide(); TradeInterface.Show(); }
	public void EndTurnButton(){ client.SendMessage("EndTurn|"); }
	
	public void MarketTradeButton(){ /*TODO*/ }
	public void PlayerTradeButton(){ /*TODO*/ }
	public void PlayerStealButton(){ /*TODO*/ }
	
	public void BuyStreetButton(){ buySelectedCallback("Street", false); }
	public void BuyVillageButton(){ buySelectedCallback("Village", false); }
	public void BuyCityButton(){ buySelectedCallback("City", false); }
	public void BuyExpansionButton(){ BuyInterface.Hide(); ExpansionInterface.Show(); }
	public void BuyKnightButton(){ buySelectedCallback("Knight", false); }
	
	public void BuyLibraryButton(){ buySelectedCallback("Libray", true); }
	public void BuyCastleButton(){ buySelectedCallback("Castle", true); }
	public void BuyUniversityButton(){ buySelectedCallback("University", true); }
	public void BuyTradersGuildButton(){ buySelectedCallback("TradersGuild", true); }
	public void BuyCityHallButton(){ buySelectedCallback("CityHall", true); }
	public void ExpansionBackButton(){ ExpansionInterface.Hide(); BuyInterface.Show(); }
}
