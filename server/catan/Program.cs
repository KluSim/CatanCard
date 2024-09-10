using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text.Json;
using System.Text.Encodings;
using System.Text;
using System.Net.NetworkInformation;
using shared;

namespace catan{



    class PlayerState{
        public string playerName;
        int victoryPoints;
        public RessourceCardAmnts resCards;
        BuildingCardAmnts buildingCards;
        ExpansionCardAmnts expansionCards;
        Dictionary<BuildingCard, bool> canBuild;
        int availableTrades = 0;
        public NetworkStream stream;
        public bool isConnected = true;

        public PlayerState(string playerName, NetworkStream ns){
            this.stream = ns;
            this.playerName = playerName;
            //resCards = new RessourceCardAmnts();
            buildingCards = new BuildingCardAmnts(1,0,0,0,1);
            expansionCards = new ExpansionCardAmnts(0,0,0,0,0,0);
            canBuild = new Dictionary<BuildingCard, bool>();
            foreach(BuildingCard b in Enum.GetValues(typeof(BuildingCard))) canBuild.Add(b, true);
        }

        public void init(RessourceCardAmnts initCards){
            resCards = initCards;
            startTurn();
        }

        public bool canBuyBuilding(BuildingCard bc, RessourceCardAmnts proposedResources, CityExpansion? proposedExpansion){
            if(bc == BuildingCard.Expansion && proposedExpansion == null) return false;
            if(bc == BuildingCard.Expansion && expansionCards.correspondingAmnt(proposedExpansion.Value) != 0) return false;
            if(canBuild[bc] == false) return false;

            switch(bc){
                case BuildingCard.Village:
                    break;
                case BuildingCard.Street:
                    break;
                case BuildingCard.Knight:
                    break;
                case BuildingCard.City:
                    if(buildingCards.village <= 0) return false;
                    break;
                case BuildingCard.Expansion:
                    if(buildingCards.city <= 0) return false;
                    break;
            }

            RessourceCardAmnts copy = new RessourceCardAmnts(proposedResources);
            RessourceCardAmnts cost = CardCosts.cardCost(bc);
            if(proposedExpansion == CityExpansion.CityHall) cost.correspondingAmnt(RessourceCard.Iron) += 2;

            // resources match directly
            foreach(RessourceCard r in Enum.GetValues(typeof(RessourceCard))) {
                int sub = Math.Min(copy.correspondingAmnt(r), cost.correspondingAmnt(r));
                copy.correspondingAmnt(r) -= sub;
                cost.correspondingAmnt(r) -= sub;
            }

            // resources replaced by "3 of a kind" trade
            foreach(RessourceCard r1 in Enum.GetValues(typeof(RessourceCard))) {
                while(cost.correspondingAmnt(r1) != 0){
                    bool threeTradeMade = false;
                    foreach(RessourceCard r2 in Enum.GetValues(typeof(RessourceCard))){
                        if(copy.correspondingAmnt(r2) >= 3){
                            copy.correspondingAmnt(r2) -= 3;
                            cost.correspondingAmnt(r1) -= 1;
                            threeTradeMade = true;
                            break;
                        }
                    }

                    if(!threeTradeMade) break;
                }
            }

            // resources replaced by university trade
            
            if(expansionCards.university > 0) {
                bool universityTradeMade = false;
                foreach(RessourceCard r1 in Enum.GetValues(typeof(RessourceCard))) {
                    if(cost.correspondingAmnt(r1) != 0 && !universityTradeMade){
                        foreach(RessourceCard r2 in Enum.GetValues(typeof(RessourceCard))){
                            if(copy.correspondingAmnt(r2) >= 1){
                                copy.correspondingAmnt(r2) -= 1;
                                cost.correspondingAmnt(r1) -= 1;
                                universityTradeMade = true;
                                break;
                            }
                        }

                        if(universityTradeMade) break;
                    }
                }
            }

            foreach(RessourceCard r in Enum.GetValues(typeof(RessourceCard))) if(cost.correspondingAmnt(r) != 0) return false;
            return true;
        }

        public bool buyBuilding(BuildingCard bc, RessourceCardAmnts proposedResources, CityExpansion? proposedExpansion){
            if(!canBuyBuilding(bc, proposedResources, proposedExpansion)) return false;
            resCards.subtract(proposedResources);
            buildingCards.correspondingAmnt(bc)++;
            if(bc == BuildingCard.Expansion) expansionCards.correspondingAmnt(proposedExpansion.Value)++;
            canBuild[bc] = false;

            switch(bc){
                case BuildingCard.Village:
                    break;
                case BuildingCard.Knight:
                    break;
                case BuildingCard.Street:
                    break;
                case BuildingCard.City:
                    buildingCards.village--;
                    break;
                case BuildingCard.Expansion:
                    buildingCards.city--;
                    break;
            }

            return true;
        }

        public void startTurn(){
            foreach(KeyValuePair<BuildingCard, bool> kvp in canBuild ) canBuild[kvp.Key] = true;
            availableTrades = 1;
            if(buildingCards.street >= 2) availableTrades += (buildingCards.street - 1) / 2;
        }

        public bool endTurn(RessourceCardAmnts drawnCards){
            resCards.add(drawnCards);
            calculateVictoryPoints();
            return victoryPoints >= 10;
        }

        public bool startPlayerSteal(PlayerState other){
            if(availableTrades <= 0) return false;
            RessourceCardAmnts res = other.getRes();
            RessourceCard stolenCard = res.takeRandomRes();
            
            resCards.correspondingAmnt(stolenCard)++;
            availableTrades--;
            return true;
        }

        public bool endPlayerSteal(PlayerState other, RessourceCard c){
            if(resCards.correspondingAmnt(c) <= 0) return false;
            other.getRes().correspondingAmnt(c)++;
            resCards.correspondingAmnt(c)--;

            return true;
        }

        public bool startMarketTrade(RessourceCardAmnts market, RessourceCard c){
            if(availableTrades <= 0) return false;
            if(market.correspondingAmnt(c) <= 00) return false;
            resCards.correspondingAmnt(c)++;
            market.correspondingAmnt(c)--;
            return true;
        }

        public bool endMarketTrade(RessourceCardAmnts market, RessourceCard c){
            if(resCards.correspondingAmnt(c) <= 0) return false;
            resCards.correspondingAmnt(c)--;
            market.correspondingAmnt(c)++;
            availableTrades--;
            return true;
        }

        public void calculateVictoryPoints(){
            victoryPoints  = 0;
            victoryPoints += 1 * buildingCards.village;
            victoryPoints += 2 * buildingCards.city;
            victoryPoints += 3 * buildingCards.expansion;
            victoryPoints += buildingCards.street / 2; 
            victoryPoints += buildingCards.knight / 2; 

            if(expansionCards.cityHall > 0) victoryPoints += 1;
        }

        public int getDrawCards(){
            if(buildingCards.knight == 0) return 2;
            else return 2 + (buildingCards.knight+1)/2;
        }
        
        public RessourceCardAmnts getRes() { return resCards; }

        public ref BuildingCardAmnts getBuildings() {return ref buildingCards; }

        public ref ExpansionCardAmnts getExpansions() {return ref expansionCards; }
        //TODO: expansion effects, city effects
    }

    

    class ServerState {
        string[] adminCommands = {"StartGame"};
        List<PlayerState> players;
        int numberOfPlayers = 0;
        bool gameStarted = false;
        int currentTurnPlayer; //Index into players
        int playerTradingAction = 0; // 0=nothing, 1 = isStealing,  2 = isTrading, 4 = isPicking
        int targetPlayer;
        RessourceCardAmnts drawPile;
        BuildingCardAmnts buildingPile;
        ExpansionCardAmnts expansionPile;
        bool CardOfFate = true;
        RessourceCardAmnts market;


        public ServerState(){
            players = new List<PlayerState>();
        }

        public Schriftrolle stateToSend(int player){
            Schriftrolle sr = new Schriftrolle();
            sr.markt = market.stringify();

            sr.destiny = CardOfFate ? "Clock" : "Counterclock";

            sr.expansions = expansionPile.stringify();

            sr.leftBuys = new string[4];
            int buycnt = 0;
            foreach(BuildingCard r in Enum.GetValues(typeof(BuildingCard))) {
                    if(r == BuildingCard.Expansion) continue;
                    sr.leftBuys[buycnt] = buildingPile.correspondingAmnt(r).ToString();
                    buycnt++;
            }

            RessourceCardAmnts playerRes = players[player].getRes(); 
            sr.yourRes = playerRes.stringify();

            sr.players = new PlayerInformation[4];
            for(int i = 0; i < players.Count; ++i) {
                PlayerInformation playerInformation = new PlayerInformation();

                playerInformation.playerName = "" + players[i].playerName;
                playerInformation.numOfCards = "" + players[i].resCards.total;

                BuildingCardAmnts playerBuilds = players[i].getBuildings();
                playerInformation.yourBuildings = playerBuilds.stringify();

                ExpansionCardAmnts playerExpansions = players[i].getExpansions();
                playerInformation.yourExpansions = playerExpansions.stringify();
                
                sr.players[i] = playerInformation;
            }
            
            return sr;
        }

        public void init(){
            if(numberOfPlayers < 2 || numberOfPlayers > 4) Console.WriteLine("replace me with actual error handling. if you read this, haha your code does the big stupid");
            currentTurnPlayer = 0;

            buildingPile = new BuildingCardAmnts(10,10,10,10,1);
            expansionPile = new ExpansionCardAmnts(0,0,0,0,0,0); //TODO expansion amounts for n players is wrong, check with actual game
            if(numberOfPlayers == 2){ buildingPile = new BuildingCardAmnts(7,  7,  2, 5, 5); expansionPile = new ExpansionCardAmnts(1,1,1,1,1,1); }
            if(numberOfPlayers == 3){ buildingPile = new BuildingCardAmnts(11, 11, 4, 7, 7); expansionPile = new ExpansionCardAmnts(1,1,1,1,1,1); } 
            if(numberOfPlayers == 4){ buildingPile = new BuildingCardAmnts(15, 15, 5, 9, 9); expansionPile = new ExpansionCardAmnts(1,1,1,1,1,1); }

            drawPile = new RessourceCardAmnts(14, 16, 15, 11, 11);

            market = new RessourceCardAmnts();
            for(int i = 0; i < 5; ++i) market.correspondingAmnt(drawPile.takeRandomRes()) += 1;
        }

        public void ActionExecutor(string action, string[] parameters){
            PlayerState current = players[currentTurnPlayer];
            switch(action){
                case "StartGame":
                    if(gameStarted) break;
                    init();
                    for(int i = 0; i < numberOfPlayers; i++){
                        RessourceCardAmnts initCards = new RessourceCardAmnts();
                        initCards.correspondingAmnt(drawPile.takeRandomRes())++;
                        initCards.correspondingAmnt(drawPile.takeRandomRes())++;
                        initCards.correspondingAmnt(drawPile.takeRandomRes())++;
                        players[i].init(initCards);
                    }
                    gameStarted = true;
                    break;
                case "EndTurn":
                    if(playerTradingAction != 0) break;
                    RessourceCardAmnts drawncards = new RessourceCardAmnts();
                    for(int i = 0; i < current.getDrawCards(); ++i){
                        drawncards.correspondingAmnt(drawPile.takeRandomRes()) += 1;
                    }

                    if(current.endTurn(drawncards)) break; //TODO END GAME

                    currentTurnPlayer++;
                    if(currentTurnPlayer >= players.Count) currentTurnPlayer = 0;
                    players[currentTurnPlayer].startTurn();

                    break;
                case "Buy":
                    string thing = parameters[0];
                    BuildingCard buything = BuildingCard.Expansion;
                    switch(thing){
                        case "Village":
                            buything = BuildingCard.Village;
                            break;
                        case "City":
                            buything = BuildingCard.City;
                            break;
                        case "Expansion":
                            buything = BuildingCard.Expansion;
                            //TODO
                            break;
                        case "Street":
                            buything = BuildingCard.Street;
                            break;
                        case "Knight":
                            buything = BuildingCard.Knight;
                            break;
                    }
                    if(buildingPile.correspondingAmnt(buything) <= 0 && buything != BuildingCard.Street && buything != BuildingCard.Knight) break;

                    RessourceCardAmnts proposed = new RessourceCardAmnts();
                    for(int i = 1; i < parameters.Length; i++){
                        RessourceCard curCard = RessourceCardMths.toEnum(parameters[i]);
                        proposed.correspondingAmnt(curCard)++;
                    }

                    if(current.buyBuilding(buything, proposed, null)) {
                        buildingPile.correspondingAmnt(buything)--;
                        bool stolen = false;
                        int stolenPlayer = currentTurnPlayer;
                        //steal street if none available
                        if(buything == BuildingCard.Street && buildingPile.correspondingAmnt(buything) < 0){
                            while(!stolen){
                                if(CardOfFate) stolenPlayer++;
                                else stolenPlayer--;

                                if(stolenPlayer < 0) stolenPlayer = players.Count;
                                if(stolenPlayer >= players.Count) stolenPlayer = 0;

                                int stolenThreshold = 0;
                                if(players[stolenPlayer].getExpansions().castle > 0) stolenThreshold = 3; 

                                if(players[stolenPlayer].getBuildings().street > stolenThreshold){
                                    players[stolenPlayer].getBuildings().street--;
                                    stolen = true;
                                }
                            }
                        }
                        //steal knight if none available
                        if(buything == BuildingCard.Knight && buildingPile.correspondingAmnt(buything) < 0){
                            while(!stolen){
                                if(CardOfFate) stolenPlayer++;
                                else stolenPlayer--;

                                if(stolenPlayer < 0) stolenPlayer = players.Count;
                                if(stolenPlayer >= players.Count) stolenPlayer = 0;

                                int stolenThreshold = 0;
                                if(players[stolenPlayer].getExpansions().church > 0) stolenThreshold = 1; 

                                if(players[stolenPlayer].getBuildings().knight > stolenThreshold){
                                    players[stolenPlayer].getBuildings().knight--;
                                    stolen = true;
                                }
                            }
                        }
                    }

                    break;
                case "StartPlayerSteal":
                    if(playerTradingAction != 0) break;
                    targetPlayer = Int32.Parse(parameters[0]);
                    if(targetPlayer == currentTurnPlayer) break;
                    if(current.startPlayerSteal(players[targetPlayer])) playerTradingAction = 1;
                    break;
                case "EndPlayerSteal":
                    if(playerTradingAction != 2) break;
                    if(current.endPlayerSteal(players[targetPlayer], RessourceCardMths.toEnum(parameters[0]))) playerTradingAction = 0;
                    break;
                case "StartMarketTrade":
                    if(playerTradingAction != 0) break;
                    if(current.startMarketTrade(market, RessourceCardMths.toEnum(parameters[0]))) playerTradingAction = 2;
                    break;
                case "EndMarketTrade":
                    if(playerTradingAction != 2) break;
                    if(current.endMarketTrade(market, RessourceCardMths.toEnum(parameters[0]))) playerTradingAction = 0;
                    break;
                
                //current.startTurn();
                // player builds shit (player actions)
                
            }
        }

        public int connectant(NetworkStream stream, ClientToServer msg){
            int returningPlayerIndex = players.FindIndex(player  => player.playerName == msg.playerName && !player.isConnected);
            if(gameStarted && returningPlayerIndex != -1){//only accept returning players
                players[returningPlayerIndex].isConnected = true;
                players[returningPlayerIndex].stream = stream;
                inform(msg);
                return numberOfPlayers;
            }
            else{ //accept everyone, but also reconnects
                if(returningPlayerIndex == -1){
                    string jsonString = JsonSerializer.Serialize(new ServerToClient().decode());
                    stream.WriteAsync(Encoding.ASCII.GetBytes(jsonString));
                    players.Add(new PlayerState(msg.playerName, stream));
                    numberOfPlayers++;
                    return numberOfPlayers;
                }
                else{
                    players[returningPlayerIndex].isConnected = true;
                    players[returningPlayerIndex].stream = stream;
                    inform(msg);
                    return numberOfPlayers;
                }
            }
            
        }

        //Client tells us smth
        public void inform(ClientToServer msg){
            int playerIndex = players.FindIndex(player  => player.playerName == msg.playerName);
            string[] splited = msg.msg.Split('|');
            string[] parameters = splited[1].Split(',');

            if(players[currentTurnPlayer].playerName != msg.playerName)
                if (!(playerIndex == 0 && adminCommands.Contains(splited[0]))) return;

            ActionExecutor(splited[0], parameters);

            Console.WriteLine(msg.playerName+": "+msg.msg);
            for(int i = 0; i < numberOfPlayers; ++i){
                ServerToClient toClient = new ServerToClient(stateToSend(i));
                string jsonString = JsonSerializer.Serialize(toClient.decode());
                Console.WriteLine(jsonString);
                players[i].stream.WriteAsync(Encoding.ASCII.GetBytes(jsonString));
            }
        }

        public void playerDisconnected(NetworkStream stream){
            int playerIndex = players.FindIndex(player  => player.stream == stream);
            players[playerIndex].isConnected = false;
        }
    }
  
    class TCPConnector
    {
        static TcpListener server;
        static Dictionary<int, ServerState> gameInstances;
        static void Main(string[] args)
        {
            Console.WriteLine("hi i run now");
            gameInstances = new Dictionary<int, ServerState>();
            server = new TcpListener(IPAddress.Any, 8052);
                server.Start();
            server.BeginAcceptTcpClient(handle_connection, server);
            while (true)
            {
                string inpt = Console.ReadLine() ?? "";
                if (inpt.Equals("exit"))
                {
                    break;
                }
            }
        }

        //the parameter is a delegate, used to communicate between threads. no idea lol
        static private void handle_connection(IAsyncResult result)  
        {
            try
            {
                TcpClient socketConnection = server.EndAcceptTcpClient(result);  //creates the TcpClient
                server.BeginAcceptTcpClient(handle_connection, server); //reopen for new connections

                Byte[] bytes = new Byte[65535];
                NetworkStream? theStream = null;
                int theGameId = -1;
                bool firstMsg = true;
                while (true)
                {
                    try{
                    // Get a stream object for reading 				
                    using (NetworkStream stream = socketConnection.GetStream())
                    {
                        int length;
                        // Read incomming stream into byte arrary. BLOCKING!!			
                        while ((length = stream.Read(bytes, 0, bytes.Length)) != 0)
                        {
                            theStream = stream;
                            var incommingData = new byte[length];
                            Array.Copy(bytes, 0, incommingData, 0, length);
                            // Convert byte array to string message. 						
                            string serverMessage = Encoding.ASCII.GetString(incommingData);
                            Console.WriteLine("client message received as: " + serverMessage);
                            ClientToServerDecode? deJsoned = null;
                            try{
                                deJsoned = JsonSerializer.Deserialize<ClientToServerDecode>(serverMessage);
                            }
                            catch (InvalidOperationException disconnect)
                            {
                                Console.WriteLine("JSON bad?? : " + disconnect);
                            }
                            if(deJsoned == null) continue;
                            ClientToServer decoded = new ClientToServer(deJsoned);

                            if(decoded.game_id < 0) return; //TODO tell player to fuck of
                            if(theGameId != -1) theGameId = decoded.game_id;
                            if(theGameId != decoded.game_id) return;//TODO tell player to fuck of

                            if(!gameInstances.ContainsKey(decoded.game_id)){
                                gameInstances[decoded.game_id] = new ServerState();
                            }
                            ServerState thisgame = gameInstances[decoded.game_id];
                            if(firstMsg)
                                thisgame.connectant(stream, decoded);
                            else
                                thisgame.inform(decoded);
                            firstMsg = false;
                        }  
                    }
                    }
                    catch(InvalidOperationException ioe){
                        Console.WriteLine("Client Disconnect");
                        if(theStream!=null)gameInstances[theGameId].playerDisconnected(theStream);
                        //gameInstances = new Dictionary<int, ServerState>();
                    }
                    catch(IOException ioe){
                        Console.WriteLine("Client Disconnect");
                        if(theStream!=null)gameInstances[theGameId].playerDisconnected(theStream);
                        //gameInstances = new Dictionary<int, ServerState>();
                    }
                }
            }
            catch (SocketException socketException)
            {
                Console.WriteLine("Socket exception: " + socketException);
            }
            
            
        }

    }
}