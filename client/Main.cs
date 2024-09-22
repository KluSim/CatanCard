using Godot;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Text.Json;
using System.Linq;
using shared;

public class TCPTestClient{  
	private Main main;
	private TcpClient socketConnection; 
	private Thread clientReceiveThread; 
	public void Start () {
		ConnectToTcpServer();     
	}  	

	public TCPTestClient(Main m){
		main = m;
	}

	private void ConnectToTcpServer () { 		
		try {  			
			clientReceiveThread = new Thread (new ThreadStart(ListenForData)); 			
			clientReceiveThread.IsBackground = true; 			
			clientReceiveThread.Start();  		
		} 		
		catch (Exception e) { 			
			Console.WriteLine("On client connect exception " + e); 		
		} 	
	}  	

	public int uid = -1;
	public string ip = "localhost";
	public int gameid = -1;
	public string username = "placeholderusername";
   
	private void ListenForData() { 
		while(true){		
		try { 			
			socketConnection = new TcpClient(ip, 8052);
			Byte[] bytes = new Byte[1024];             
			while (true) { 				
				// Get a stream object for reading 				
				using (NetworkStream stream = socketConnection.GetStream()) { 					
					int length; 					
					// Read incomming stream into byte arrary. 					
					while ((length = stream.Read(bytes, 0, bytes.Length)) != 0) { 						
						var incommingData = new byte[length]; 						
						Array.Copy(bytes, 0, incommingData, 0, length); 						
						// Convert byte array to string message. 						
						string serverMessage = Encoding.ASCII.GetString(incommingData); 						
						Console.WriteLine("server message received as: " + serverMessage); 

						ServerToClientDecode? deJsoned = null;
						try{
							deJsoned = JsonSerializer.Deserialize<ServerToClientDecode>(serverMessage);
						} 
						catch(Exception e){
							Console.WriteLine("Bad JSON");
						}
						if(deJsoned == null) continue;
						ServerToClient deCoded = new ServerToClient(deJsoned);
						main.handleServerMsg(deCoded);	
					} 				
				} 			
			}         
		}         
		catch (SocketException socketException) {             
			//Console.WriteLine("Socket exception: " + socketException);         
			Console.WriteLine("Couldn't connect, trying again");
			System.Threading.Thread.Sleep(1000);
		}
		catch(Exception bozoDown){
			Console.WriteLine("Server Died");
			System.Threading.Thread.Sleep(1000);
		}
		}    
	}  	

	public void SendMessage(string msg) {         
		if (socketConnection == null) {             
			return;         
		}  		
		try { 			
			// Get a stream object for writing. 			
			NetworkStream stream = socketConnection.GetStream(); 			
			if (stream.CanWrite) {      
				string clientMessage = "";
				ClientToServer clientToServer = new ClientToServer(gameid, msg, username);           
				clientMessage = JsonSerializer.Serialize<ClientToServerDecode>(clientToServer.decode());
				// Convert string message to byte array.                 
				byte[] clientMessageAsByteArray = Encoding.ASCII.GetBytes(clientMessage);
				// Write byte array to socketConnection stream.                 
				stream.Write(clientMessageAsByteArray, 0, clientMessageAsByteArray.Length);                 
				Console.WriteLine("Sent: "+clientMessage);           
			}         
		} 		
		catch (SocketException socketException) {             
			Console.WriteLine("Socket exception: " + socketException);         
		}     
	}

	
}


public partial class Main : Node2D
{
	
	private TCPTestClient client;
	[Export]
	private RichTextLabel textField;
	[Export]
	private TextEdit textEdit;

	private List<Card> marketCards;
	private List<Card> playerIcons;

	Queue<ServerToClient> serverMsgs;
	[Export] Player MainPlayer;
	[Export] public Player[] otherPlayers;
	[Export]
	PackedScene card;

	public bool isRäubering = false;
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		base._Ready();
		marketCards = new List<Card>();
		playerIcons = new List<Card>();
		serverMsgs = new Queue<ServerToClient>();
		otherPlayerHandCardObjs = new List<Card>();
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		while(serverMsgs.Count != 0){
			ServerToClient serverMessage = serverMsgs.Dequeue();
			GD.Print(serverMessage.kind);
			if(serverMessage.kind == "update" || serverMessage.kind == "init"){
				if(serverMessage.kind == "init"){
					ConnectionInterface.Hide();
					GameInterface.Show();
				}

				//market
				foreach(Card c in marketCards){
					c.QueueFree();
				}
				marketCards.Clear();
				Schriftrolle sr = serverMessage.schriftrolle;
				RessourceCardAmnts marketCardsAmnts = new RessourceCardAmnts(sr.markt);
				foreach(RessourceCard r in Enum.GetValues(typeof(RessourceCard))) {
					int num = marketCardsAmnts.correspondingAmnt(r);
					for(int i = 0; i < num; i++){
						Card cardInstance = card.Instantiate<Card>();
						cardInstance.isSelectable = true;
						cardInstance.selectedCallback = marketCardSelectedCallback;
						AddChild(cardInstance);
						cardInstance.GlobalTransform = marketStart.GlobalTransform;
						marketCards.Add(cardInstance);
						cardInstance.MoveLocalX((marketCards.Count-1)*110f);
						cardInstance.SetTypeRc(r);
					}
				}

				//rb
				isRäubering = sr.räuberCollecting == "true";

				//player
				int count = 0;
				MainPlayer.SetRes(new RessourceCardAmnts(sr.yourRes));
				
				
				for(int i = 0; i < sr.players.Length; i++){
					if(sr.players[i] == null) break;
					string currPlayerName = sr.players[i].playerName;
					if(currPlayerName == client.username){
						MainPlayer.setExpansions(new ExpansionCardAmnts(sr.players[i].yourExpansions));
						MainPlayer.setPlayerName(currPlayerName);
						MainPlayer.setBuildings(new BuildingCardAmnts(sr.players[i].yourBuildings));
						continue;
					}
					otherPlayers[count].setPlayerName(currPlayerName);
					otherPlayers[count].setBuildings(new BuildingCardAmnts(sr.players[i].yourBuildings));
					otherPlayers[count].SetUnknownRes(Int32.Parse(sr.players[i].numOfCards));
					//otherPlayers[count].setExpansions(new ExpansionCardAmnts(sr.players[i].yourExpansions));
					Card c = otherPlayers[count].SetPlayerIcon(Int32.Parse(sr.players[i].iconIndex));
					if(c != null) playerIcons.Add(c);
					count++;
				}
			}
			
			if(serverMessage.kind == "demandedPlayerInfo"){
				foreach(Card c in otherPlayerHandCardObjs) c.QueueFree();
				GD.Print(1);
				otherPlayerHandCardObjs.Clear();
				foreach(TextureRect n in otherPlayerScroller.GetChildren()){
					otherPlayerScroller.RemoveChild(n);
					n.QueueFree();
				}
				
				Schriftrolle sr = serverMessage.schriftrolle;
				RessourceCardAmnts otherPlayerCardsAmnts = new RessourceCardAmnts(sr.yourRes);
				foreach(RessourceCard r in Enum.GetValues(typeof(RessourceCard))) {
					int num = otherPlayerCardsAmnts.correspondingAmnt(r);
					if(num == 0) continue;
					Card cardInstance = card.Instantiate<Card>();
					cardInstance.isSelectable = true;
					cardInstance.selectedCallback = OtherPlayerResSelected;
					AddChild(cardInstance);
					TextureRect c = cardInstance.GetChild(0) as TextureRect;
					cardInstance.RemoveChild(c);
					otherPlayerScroller.AddChild(c);
					//cardInstance.setPlayer(this);
					otherPlayerHandCardObjs.Add(cardInstance);
					cardInstance.SetTypeRc(r);
				}
				
				isPlayerLibraryTrading++;
			}
		}
	}

	public void handleServerMsg(ServerToClient serverMessage){
		serverMsgs.Enqueue(serverMessage);
	}
	
	[Export]
	Node2D marketStart;

	private void OnTextEditSet(){
		string curText = textEdit.Text;
		if(curText.Length < 1) return;
		char last = curText[^1];
		if(last != '\n') return;
		string cmd = curText[0..^1];
		textEdit.Text = "";
		textField.Text+= "send cmd: "+cmd;
		/*if(cmd == "add"){
			testPlayer.AddRes(RessourceCard.Clay);
		}*/
		client.SendMessage(cmd);
	}
	
	// Market trade functions and vars
	
	[Export]
	Button marketBtn;
	int isTradingMarket = 0;

	public void marketCardSelectedCallback(string type){
		if(isTradingMarket != 1) return;
		client.SendMessage("StartMarketTrade|" + type);
		marketBtn.Text = "Select Your Card";
		isTradingMarket++;
	}
	
	public void MarketTradeBtn(){
		if(isPlayerTrading != 0) return;
		if(isPlayerStealingGuild != 0) return;
		if(isPlayerLibraryTrading != 0) return;
		if(isTradingMarket == 0){
			foreach(Card c in MainPlayer.resCardObjs) c.SetSelectNoCb(false);
			foreach(Card c in marketCards) c.SetSelectNoCb(false);
			marketBtn.Text = "Select Market Card";
			isTradingMarket = 1;
		}
		//if(!isTradingMarket) MarketTradeStart();
		//else MarketTradeEnd();
	}
	
	// Player trade functions and vars
	
	[Export] Button playerTradeBtn;
	int isPlayerTrading = 0;
	
	public void PlayerTradeBtn(){
		if(isTradingMarket != 0) return;
		if(isPlayerStealingGuild != 0) return;
		if(isPlayerLibraryTrading != 0) return;
		if(isPlayerTrading == 0){ 
			foreach(Card c in MainPlayer.resCardObjs) c.SetSelectNoCb(false);
			foreach(Card c in playerIcons) c.SetSelectNoCb(false);
			playerTradeBtn.Text = "Select Player";
			isPlayerTrading = 1;
		}
	}
	
	// player steal with Guild
	[Export] Button playerStealGuildBtn;
	int isPlayerStealingGuild = 0;
	
	public void PlayerStealGuildBtn(){
		if(isTradingMarket != 0) return;
		if(isPlayerTrading != 0) return;
		if(isPlayerLibraryTrading != 0) return;
		if(isPlayerStealingGuild == 0){
			foreach(Card c in playerIcons) c.SetSelectNoCb(false);
			playerStealGuildBtn.Text = "Select Player";
			isPlayerStealingGuild = 1;
		}
	}
	
	// player trade with Library
	
	[Export] Button playerLibraryTradeBtn;
	[Export] Control OtherPlayerHandCards;
	[Export] HBoxContainer otherPlayerScroller;
	List<Card> otherPlayerHandCardObjs;
	int isPlayerLibraryTrading = 0;
	
	public void PlayerTradeLibraryBtn(){
		if(isTradingMarket != 0) return;
		if(isPlayerTrading != 0) return;
		if(isPlayerStealingGuild != 0) return;
		if(isPlayerLibraryTrading == 0){
			foreach(Card c in playerIcons) c.SetSelectNoCb(false);
			foreach(Card c in MainPlayer.resCardObjs) c.SetSelectNoCb(false);
			playerLibraryTradeBtn.Text = "Select Player";
			isPlayerLibraryTrading = 1;
		}
	} 
	
	public void OtherPlayerResSelected(string type){
		if(isPlayerLibraryTrading == 3){
			client.SendMessage("PlayerSelectLib|" + type);
			playerLibraryTradeBtn.Text = "Select own card";
			OtherPlayerHandCards.Hide();
			isPlayerLibraryTrading++;
		}
	}
	
	// Method needed for multiple trades
	
	public void PlayerIconSelectedCallback(string type){
		if(isPlayerTrading == 1){
			client.SendMessage("StartPlayerSteal|" + type);
			playerTradeBtn.Text = "Select Your Card";
			isPlayerTrading++;
		}
		
		if(isPlayerStealingGuild == 1){
			client.SendMessage("PlayerStealGuild|" + type);
			playerStealGuildBtn.Text = "Steal using Traders Guild";
			isPlayerStealingGuild = 0;
		}
		
		if(isPlayerLibraryTrading == 1){
			client.SendMessage("PlayerTradeLib|" + type);
			playerLibraryTradeBtn.Text = "Select Resource Card";
			OtherPlayerHandCards.Show();
			isPlayerLibraryTrading++;
		}
	}
	
	public void playerResSelectedCallback(string type){
		if(isRäubering){
			client.SendMessage("Räubergabe|"+type);
			return;
		}

		if(isTradingMarket == 2){
			client.SendMessage("EndMarketTrade|"+type);
			marketBtn.Text = "Trade with Market";
			isTradingMarket = 0;
		}
		
		if(isPlayerTrading == 2){
			client.SendMessage("EndPlayerSteal|"+type);
			playerTradeBtn.Text = "Trade with Player";
			isPlayerTrading = 0;
		}
		
		if(isPlayerLibraryTrading == 4){
			client.SendMessage("EndPlayerSteal|"+type);
			playerLibraryTradeBtn.Text = "Trade using Library";
			isPlayerLibraryTrading = 0;
		}
	}
	
	// CONNECTION STUFF HERE
	
	[Export] LineEdit IPEdit;
	[Export] LineEdit GameIDEdit;
	[Export] LineEdit UserNameEdit;
	[Export] OptionButton AvatarSelector;
	[Export] Control ConnectionInterface;
	[Export] Control GameInterface;
	
	public void OnConnectBtnDown(){
		client = new TCPTestClient(this);
		client.ip = IPEdit.Text;
		client.gameid = Int32.Parse(GameIDEdit.Text);
		client.username = UserNameEdit.Text;
		client.Start();
		MainPlayer.init(client, AvatarSelector.Selected);
		System.Threading.Thread.Sleep(400);
		client.SendMessage("" + AvatarSelector.Selected);
	}
	private void OnStartGameBtnDown(){
		client.SendMessage("StartGame|");
		ConnectionInterface.Hide();
		GameInterface.Show();
	}
}
