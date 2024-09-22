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
	Node2D habitatStart;
	[Export]
	Node2D roadsStart;
	[Export]
	Node2D knightStart;
	[Export]
	Node2D PlayerIconStart;
	
	[Export]
	public bool isMainPlayer;
	public int playerIcon;
	public string playerName;
	private bool iconHasBeenSpawned = false;
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
	
	public Card playerIconCard;

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
		expansionCardObjs = new List<Card>();
	}

	public void init(TCPTestClient cl, int playerIcon){
		client = cl;
		this.playerIcon = playerIcon;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	public void buySelectedCallback(string type, bool isExpansion){
		if(client == null) return;
		string resources = type;
		if(isExpansion) resources = "Expansion," + resources;
		foreach(Card c in resCardObjs) if(c.isSelected) resources += "," + c.type;
		client.SendMessage("Buy|"+resources);
	}
	
	public void setPlayerName(string n){ playerName = n; }
	
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
				cardInstance.isSelectable = true;
				cardInstance.selectedCallback = GetParent<Main>().playerResSelectedCallback;
				AddChild(cardInstance);
				TextureRect c = cardInstance.GetChild(0) as TextureRect;
				cardInstance.RemoveChild(c);
				scroller.AddChild(c);
				cardInstance.setPlayer(this);
				resCardObjs.Add(cardInstance);
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
	
	public Card SetPlayerIcon(int i){
		if(iconHasBeenSpawned) return playerIconCard;
		Card cardInstance = card.Instantiate<Card>();
		playerIconCard = cardInstance;
		cardInstance.isSelectable = true; 
		AddChild(cardInstance);
		cardInstance.setPlayer(this);
		cardInstance.setPlayerIcon(i);
		cardInstance.selectedCallback = GetParent<Main>().PlayerIconSelectedCallback;
		cardInstance.ZIndex = -69; 
		cardInstance.GlobalTransform = PlayerIconStart.GlobalTransform;
		iconHasBeenSpawned = true;
		return cardInstance;
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
				if(r == BuildingCard.Expansion) continue; 
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
	
	[Export] Node2D castleStart;
	[Export] Node2D universityStart;
	[Export] Node2D churchStart;
	[Export] Node2D tradersGuildStart;
	[Export] Node2D cityHallStart;
	[Export] Node2D libraryStart;
	
	List<Card> expansionCardObjs;
	
	private void spawnExpansion(Node2D location, CityExpansion e){
		Card cardInstance = card.Instantiate<Card>();
		AddChild(cardInstance);
		cardInstance.SetExpansionType(e);
		cardInstance.GlobalTransform = location.GlobalTransform;
		expansionCardObjs.Add(cardInstance);
	}
	
	public void setExpansions(ExpansionCardAmnts ecs){
		foreach(Card c in expansionCardObjs) c.QueueFree();
		expansionCardObjs.Clear();
		
		if(ecs.castle > 0) spawnExpansion(castleStart, CityExpansion.Castle);
		if(ecs.university > 0) spawnExpansion(universityStart, CityExpansion.University);
		if(ecs.church > 0) spawnExpansion(churchStart, CityExpansion.Church);
		if(ecs.tradersGuild > 0) spawnExpansion(tradersGuildStart, CityExpansion.TradersGuild);
		if(ecs.cityHall > 0) spawnExpansion(cityHallStart, CityExpansion.CityHall);
		if(ecs.library > 0) spawnExpansion(libraryStart, CityExpansion.Library);
	}
	
	
	//
	// FROM HERE ON OUT -> UI THINGS
	//
	
	[Export] Control ConnectionInterface;
	[Export] Control GameInterface;
	[Export] Control TradeInterface;
	[Export] Control BuyInterface;
	[Export] Control ExpansionInterface;
	[Export] Control AdminInterface;
	
	public void GeneralBack(){ GameInterface.Show(); TradeInterface.Hide(); BuyInterface.Hide(); AdminInterface.Hide(); }
	
	public void BuyButton(){ GameInterface.Hide(); BuyInterface.Show(); }
	public void TradeButton(){ GameInterface.Hide(); TradeInterface.Show(); }
	public void EndTurnButton(){ client.SendMessage("EndTurn|"); }
	public void AdminButton(){ GameInterface.Hide(); AdminInterface.Show(); }
	
	public void BuyStreetButton(){ buySelectedCallback("Street", false); }
	public void BuyVillageButton(){ buySelectedCallback("Village", false); }
	public void BuyCityButton(){ buySelectedCallback("City", false); }
	public void BuyExpansionButton(){ BuyInterface.Hide(); ExpansionInterface.Show(); }
	public void BuyKnightButton(){ buySelectedCallback("Knight", false); }
	
	public void BuyLibraryButton(){ buySelectedCallback("Library", true); }
	public void BuyChurchButton(){ buySelectedCallback("Church", true); }
	public void BuyCastleButton(){ buySelectedCallback("Castle", true); }
	public void BuyUniversityButton(){ buySelectedCallback("University", true); }
	public void BuyTradersGuildButton(){ buySelectedCallback("TradersGuild", true); }
	public void BuyCityHallButton(){ buySelectedCallback("CityHall", true); }
	public void ExpansionBackButton(){ ExpansionInterface.Hide(); BuyInterface.Show(); }
	
	public void StopServerButton(){ client.SendMessage("KillServer|"); }
	public void ResetGameButton(){ client.SendMessage("RestartGame|"); }
}
