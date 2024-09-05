using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text.Json;
using System.Text.Encodings;
using System.Text;
using System.Net.NetworkInformation;

namespace catan{

    enum RessourceCard
    {
        Wheat, Iron, Sheep, Clay, Wood
    }
    class RessourceCardMths{
        static public string toString(RessourceCard rc){
            switch(rc){
                case RessourceCard.Wheat:
                    return "Wheat";
                case RessourceCard.Iron:
                    return "Iron";
                case RessourceCard.Wood:
                    return "Wood";
                case RessourceCard.Sheep:
                    return "Sheep";
                case RessourceCard.Clay:
                    return "Clay";
            }
            return "";
        }
    }
    enum BuildingCard{
        Village, City, Knight, Expansion, Street
    }

    class BuildingCardMths {
        public static string toString(BuildingCard b){
            switch(b){
                case BuildingCard.Village:
                    return "Village";
                case BuildingCard.City:
                    return "City";
                case BuildingCard.Knight:
                    return "Knight";
                case BuildingCard.Expansion:
                    return "Expansion";
                case BuildingCard.Street:
                    return "Street";
            }
            return "";
        }
    }

    enum CityExpansion {
        Library, TradersGuild, Church, CityHall, Castle, University
    }
    class CityExpansionMths{
        public static string toString(CityExpansion ce){
            switch(ce){
                case CityExpansion.Library:
                    return "Library";
                case CityExpansion.TradersGuild:
                    return "TradersGuild";
                case CityExpansion.Church:
                    return "Church";
                case CityExpansion.CityHall:
                    return "CityHall";
                case CityExpansion.Castle:
                    return "Castle";
                case CityExpansion.University:
                    return "University";
            }
            return "";
        }
    }

    class CardCosts{
        public static RessourceCardAmnts cardCost(BuildingCard bc) {
            RessourceCardAmnts res;
            switch(bc){
                case BuildingCard.Village:
                    res = new RessourceCardAmnts(1,0,1,1,1);
                    break;
                case BuildingCard.City:
                    res = new RessourceCardAmnts(2,3,0,0,0);
                    break;
                case BuildingCard.Knight:
                    res = new RessourceCardAmnts(1,1,1,0,0);
                    break;
                case BuildingCard.Expansion:
                    res = new RessourceCardAmnts(0,1,3,0,0);
                    break;
                default: //Street
                    res = new RessourceCardAmnts(0,0,0,1,1);
                    break;
            }
            return res;
        }
    }

    class RessourceCardAmnts{
        
        public int wheat, iron, sheep, wood, clay; // durch dick ersetzt
        public int total{
            get {
                return wheat+iron+sheep+clay+wood;
            }
        }

        public string stringify(){
            return "["+wheat+","+iron+","+sheep+","+wood+","+clay+"]";
        }

        public RessourceCardAmnts(string str){
            string r = str[1..^1];
            string[] splited = r.Split(',');
            wheat = Int32.Parse(splited[0]);
            iron = Int32.Parse(splited[1]);
            sheep = Int32.Parse(splited[2]);
            wood = Int32.Parse(splited[3]);
            clay = Int32.Parse(splited[4]);
        }

        public RessourceCard takeRandomRes(){
            Random rnd = new Random();
            int nr = rnd.Next(0,total);
            if(nr < wheat) {wheat--; return RessourceCard.Wheat;}
            else if(nr < wheat+iron) {iron--; return RessourceCard.Iron;}
            else if(nr < wheat+iron+sheep) {sheep--; return RessourceCard.Sheep;}
            else if(nr < wheat+iron+sheep+wood) {wood--; return RessourceCard.Wood;}
            else {clay--; return RessourceCard.Clay;}
        }

        public RessourceCardAmnts(int wheat, int iron, int sheep, int wood, int clay){
            this.wheat = wheat;
            this.iron = iron;
            this.sheep = sheep;
            this.wood = wood;
            this.clay = clay;
        }
        public RessourceCardAmnts(RessourceCardAmnts old){ //copy constr
            this.wheat = old.wheat;
            this.iron = old.iron;
            this.sheep = old.sheep;
            this.wood = old.wood;
            this.clay = old.clay;
        }
        public RessourceCardAmnts(){}

        public void subtract(RessourceCardAmnts trash){
            wood -= trash.wood;
            clay -= trash.clay;
            iron -= trash.iron;
            sheep -= trash.sheep;
            wheat -= trash.wheat;
        }

        public void add(RessourceCardAmnts antitrash) {
            wood += antitrash.wood;
            clay += antitrash.clay;
            iron += antitrash.iron;
            sheep += antitrash.sheep;
            wheat += antitrash.wheat;
        }

        public ref int correspondingAmnt(RessourceCard bc){
            if(bc == RessourceCard.Wheat) return ref wheat;
            else if(bc == RessourceCard.Iron) return ref iron;
            else if(bc == RessourceCard.Sheep) return ref sheep;
            else if(bc == RessourceCard.Wood) return ref wood;
            return ref clay;
        }
    }

    class ExpansionCardAmnts{
        public int library, tradersGuild, church, cityHall, castle, university;
        public int total{
            get {
                return library+tradersGuild+church+cityHall+castle+university;
            }
        }

        public ExpansionCardAmnts(int li, int tr, int ch, int ci, int ca, int un) {
            library = li; tradersGuild = tr; church = ch; cityHall = ci; castle = ca; university = un;
        }

        public string stringify(){
            return "["+library+","+tradersGuild+","+church+","+cityHall+","+castle+","+university+"]";
        }

        public ExpansionCardAmnts(string str){
            string r = str[1..^1];
            string[] splited = r.Split(',');
            library = Int32.Parse(splited[0]);
            tradersGuild = Int32.Parse(splited[1]);
            church = Int32.Parse(splited[2]);
            cityHall = Int32.Parse(splited[3]);
            castle = Int32.Parse(splited[4]);
            university = Int32.Parse(splited[5]);
        }

        public ref int correspondingAmnt(CityExpansion ex){
            switch(ex) {
                case CityExpansion.Library: return ref library;
                case CityExpansion.TradersGuild: return ref tradersGuild;
                case CityExpansion.Church: return ref church;
                case CityExpansion.CityHall: return ref cityHall;
                case CityExpansion.Castle: return ref castle;
                case CityExpansion.University: return ref university;
            }

            return ref library; // if you reach this, I guess youre building a library now
        }
    }

    class BuildingCardAmnts{
        public int village, city, knight, expansion, street;
        public int total{
            get {
                return village+city+knight+expansion+street;
            }
        }
        public BuildingCardAmnts(int v, int c, int k, int e, int s){
            village = v; city = c; knight = k; expansion = e; street = s;
        }

        public string stringify(){
            return "["+village+","+city+","+knight+","+expansion+","+street+"]";
        }

        public BuildingCardAmnts(string str){
            string r = str[1..^1];
            string[] splited = r.Split(',');
            village = Int32.Parse(splited[0]);
            city = Int32.Parse(splited[1]);
            knight = Int32.Parse(splited[2]);
            expansion = Int32.Parse(splited[3]);
            street = Int32.Parse(splited[4]);
        }

        public ref int correspondingAmnt(BuildingCard bc){
            if(bc == BuildingCard.City) return ref city;
            else if(bc == BuildingCard.Village) return ref village;
            else if(bc == BuildingCard.Knight) return ref knight;
            else if(bc == BuildingCard.Expansion) return ref expansion;
            return ref street;
        }
    }

    class PlayerState{
        int playerID;
        int victoryPoints;
        RessourceCardAmnts resCards;
        BuildingCardAmnts buildingCards;
        ExpansionCardAmnts expansionCards;
        Dictionary<BuildingCard, bool> canBuild;
        public NetworkStream stream;

        public PlayerState(int playerID, NetworkStream ns){
            this.stream = ns;
            this.playerID = playerID;
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

            foreach(RessourceCard r in Enum.GetValues(typeof(RessourceCard))) {
                int sub = Math.Min(copy.correspondingAmnt(r), cost.correspondingAmnt(r));
                copy.correspondingAmnt(r) -= sub;
                cost.correspondingAmnt(r) -= sub;
            }

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
        }

        public bool endTurn(RessourceCardAmnts drawnCards){
            resCards.add(drawnCards);
            calculateVictoryPoints();
            return victoryPoints >= 10;
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

        public BuildingCardAmnts getBuildings() {return buildingCards; }

        public ExpansionCardAmnts getExpansions() {return expansionCards; }
        //TODO: trade amnts, stealing, expansion effects, city effects, main game loop
    }

    class Schriftrolle{
        public string markt {get; set;}
        public string destiny {get; set;}
        public string expansions {get; set;}
        public string[] leftBuys {get; set;}//list of ints
        public string yourRes {get; set;}
        public string yourBuildings {get; set;}
        public string yourExpansions {get; set;}
    }

    class ServerState {
        string[] adminCommands = {"StartGame"};
        List<PlayerState> players;
        int numberOfPlayers = 0;
        int currentTurnPlayer;

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

            BuildingCardAmnts playerBuilds = players[player].getBuildings();
            sr.yourBuildings = playerBuilds.stringify();

            ExpansionCardAmnts playerExpansions = players[player].getExpansions();
            sr.yourExpansions = playerExpansions.stringify();
            return sr;
        }

        public void init(){
            if(numberOfPlayers < 2 || numberOfPlayers > 4) Console.WriteLine("replace me with actual error handling. if you read this, haha your code does the big stupid");
            currentTurnPlayer = 0;

            buildingPile = new BuildingCardAmnts(0,0,0,0,0);
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
                    init();
                    for(int i = 0; i < numberOfPlayers; i++){
                        RessourceCardAmnts initCards = new RessourceCardAmnts();
                        initCards.correspondingAmnt(drawPile.takeRandomRes())++;
                        initCards.correspondingAmnt(drawPile.takeRandomRes())++;
                        initCards.correspondingAmnt(drawPile.takeRandomRes())++;
                        players[i].init(initCards);
                    }
                    break;
                case "EndTurn": 
                    RessourceCardAmnts drawncards = new RessourceCardAmnts();
                    for(int i = 0; i < current.getDrawCards(); ++i){
                        drawncards.correspondingAmnt(drawPile.takeRandomRes()) += 1;
                    }

                    if(current.endTurn(drawncards)) break;

                    currentTurnPlayer++;
                    if(currentTurnPlayer >= players.Count) currentTurnPlayer = 0;
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
                    RessourceCardAmnts proposed = new RessourceCardAmnts();
                    for(int i = 1; i < parameters.Length; i++){
                        RessourceCard curCard = RessourceCard.Clay;
                        switch (parameters[i]){
                            case "Wood":
                                curCard = RessourceCard.Wood;
                                break;
                            case "Wheat":
                                curCard = RessourceCard.Wheat;
                                break;
                            case "Iron":
                                curCard = RessourceCard.Iron;
                                break;
                            case "Sheep":
                                curCard = RessourceCard.Sheep;
                                break;
                            case "Clay":
                                curCard = RessourceCard.Clay;
                                break;
                        }
                        proposed.correspondingAmnt(curCard)++;
                    }
                    bool res = current.buyBuilding(buything, proposed, null);
                    break;
                
                //current.startTurn();
                // player builds shit (player actions)
                
            }
        }

        public int connectant(NetworkStream stream){
            //if(clients.ContainsKey(stream)){
           //     Console.WriteLine("Returning connection, but no user id, dafuq");//TODO handle that
          //  }
            stream.WriteAsync(Encoding.ASCII.GetBytes("{\"kind\":\"init\",\"user_id\":\""+numberOfPlayers+"\"}"));
            numberOfPlayers++;
            players.Add(new PlayerState(numberOfPlayers, stream));
            return numberOfPlayers;
        }

        //Client tells us smth
        public void inform(Message msg){
            if(msg.user_id.Value != currentTurnPlayer) return;

            string[] splited = msg.msg.Split('|');
            string[] parameters = splited[1].Split(',');
            if(msg.user_id.Value != 0 && adminCommands.Contains(splited[0])) return;
            ActionExecutor(splited[0], parameters);

            Console.WriteLine(msg.user_id+": "+msg.msg);
            for(int i = 0; i < numberOfPlayers; ++i){
                string jsonString = JsonSerializer.Serialize(stateToSend(i));
                Console.WriteLine(jsonString);
                players[i].stream.WriteAsync(Encoding.ASCII.GetBytes(jsonString));
            }
        }
    }

class MessageDecode{
        public string id{get; set;}
        public string msg{get; set;}
        public string? user_id{get; set;}
    }

    class Message{
        public int id;
        public string msg;
        public int? user_id;
        public Message(MessageDecode md){
            id = Int32.Parse(md.id);
            user_id = md.user_id != null ? Int32.Parse(md.user_id) : null;
            msg = md.msg;
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
                while (true)
                {
                    // Get a stream object for reading 				
                    using (NetworkStream stream = socketConnection.GetStream())
                    {
                        int length;
                        // Read incomming stream into byte arrary. BLOCKING!!			
                        while ((length = stream.Read(bytes, 0, bytes.Length)) != 0)
                        {
                            var incommingData = new byte[length];
                            Array.Copy(bytes, 0, incommingData, 0, length);
                            // Convert byte array to string message. 						
                            string serverMessage = Encoding.ASCII.GetString(incommingData);
                            Console.WriteLine("client message received as: " + serverMessage);
                            MessageDecode? deJsoned = JsonSerializer.Deserialize<MessageDecode>(serverMessage);
                            if(deJsoned == null) continue;
                            Message decoded = new Message(deJsoned);
                            if(!gameInstances.ContainsKey(decoded.id)){
                                gameInstances[decoded.id] = new ServerState();
                            }
                            ServerState thisgame = gameInstances[decoded.id];
                            if(decoded.user_id == null)
                                thisgame.connectant(stream);
                            else
                                thisgame.inform(decoded);
                        }  
                    }
                }
            }
            catch (SocketException socketException)
            {
                Console.WriteLine("Socket exception: " + socketException);
            }
            catch (InvalidOperationException disconnect)
            {
                Console.WriteLine("Disconnect?? : " + disconnect);
            }
        }

    }
}