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
	public int gameid = -1;
   
	private void ListenForData() { 
		while(true){		
		try { 			
			socketConnection = new TcpClient("localhost", 8052);
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
						if(deCoded.kind == "init"){
							uid = deCoded.user_id.Value;
						}
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
				if (uid != -1) {
					ClientToServer clientToServer = new ClientToServer(gameid, uid, msg);           
					clientMessage = JsonSerializer.Serialize<ClientToServerDecode>(clientToServer.decode());	
				}
				else {
					ClientToServer clientToServer = new ClientToServer(gameid);           
					clientMessage = JsonSerializer.Serialize<ClientToServerDecode>(clientToServer.decode());
				}
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

	Queue<ServerToClient> serverMsgs;
	[Export]
	PackedScene card;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		base._Ready();
		marketCards = new List<Card>();
		serverMsgs = new Queue<ServerToClient>();
		//textField = GetNode<RichTextLabel>("Camera3D/CanvasLayer/RichTextLabel");
		client = new TCPTestClient(this);
		client.Start();
		client.gameid = 32;
		testPlayer.init(client);
		//textField.Text = "banana";
		//Console.WriteLine("Give id of game to join/create");
		//client.gameid = Int32.Parse(Console.ReadLine());
		//client.SendMessage("connectmepls");
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		while(serverMsgs.Count != 0){
			ServerToClient serverMessage = serverMsgs.Dequeue();
			if(serverMessage.kind == "update"){
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
						AddChild(cardInstance);
						cardInstance.GlobalTransform = marketStart.GlobalTransform;
						marketCards.Add(cardInstance);
						cardInstance.MoveLocalX((marketCards.Count-1)*110f);
						cardInstance.SetTypeRc(r);
					}
				}

				//player
				testPlayer.SetRes(new RessourceCardAmnts(sr.yourRes));
				
			}
		}
	}

	public void handleServerMsg(ServerToClient serverMessage){
		serverMsgs.Enqueue(serverMessage);
	}

	[Export]
	Player testPlayer;
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

	private const float RayLength = 1000.0f;
	private bool doRaycast = false;
}
