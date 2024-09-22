using System;

namespace shared
{
        public enum RessourceCard
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

        public static RessourceCard toEnum(string s) {
            switch(s){
                case "Wheat":
                    return RessourceCard.Wheat;
                case "Iron":
                    return RessourceCard.Iron;
                case "Wood":
                    return RessourceCard.Wood;
                case "Sheep":
                    return RessourceCard.Sheep;
                case "Clay":
                    return RessourceCard.Clay;
                default:
                    return RessourceCard.Clay;
            }
        }
    }
    public enum BuildingCard{
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

    public enum CityExpansion {
        Library, TradersGuild, Church, CityHall, Castle, University
    }
    public class CityExpansionMths{
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
        public static CityExpansion toEnum(string ce){
            switch(ce){
                case "Library":
                    return CityExpansion.Library;
                case "TradersGuild":
                    return CityExpansion.TradersGuild;
                case "Church":
                    return CityExpansion.Church;
                case "CityHall":
                    return CityExpansion.CityHall;
                case "Castle":
                    return CityExpansion.Castle;
                case "University":
                    return CityExpansion.University;
            }
            return CityExpansion.CityHall;
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
                    res = new RessourceCardAmnts(0,2,3,0,0);
                    break;
                default: //Street
                    res = new RessourceCardAmnts(0,0,0,1,1);
                    break;
            }
            return res;
        }
    }

    public class RessourceCardAmnts{
        
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

        public RessourceCard? takeRandomRes(){
            Random rnd = new Random();
            if(total <= 0) return null;
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

    public class ExpansionCardAmnts{
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

    public class BuildingCardAmnts{
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

    public class Schriftrolle{
        public string markt {get; set;}
        public string destiny {get; set;}
        public string expansions {get; set;}
        public string[] leftBuys {get; set;}//list of ints
        public string yourRes {get; set;}
        public string räuberCollecting {get;set;}
        public string logthing{get;set;}
        public string currentTurnPlayer{get;set;}
        public PlayerInformation[] players {get; set;}
    }

    public class PlayerInformation{
        public string index{get; set;}
        public string iconIndex{get; set;}
        public string numOfCards {get; set;}
        public string playerName {get; set;}
        public string yourBuildings {get; set;}
        public string yourExpansions {get; set;}
    }
    class ClientToServer{
        public int game_id;
        public string msg;
        public string playerName;
        public ClientToServer(int gid){
            game_id = gid;
            msg = "connectmepls";
        }
        public ClientToServer(int gid,string msg, string pn){
            game_id = gid;
            this.msg =  msg;
            playerName = pn;
        }
        public ClientToServer(ClientToServerDecode d){
            game_id = Int32.Parse(d.game_id);
            msg = d.msg;
            playerName = d.playerName;
        }

        public ClientToServerDecode decode(){
            return new ClientToServerDecode(game_id.ToString(), msg, playerName);
        }
    }

    class ClientToServerDecode{
        public string game_id{get;set;}
        public string msg{get;set;}
        public string playerName{get;set;}
        public ClientToServerDecode() {}

        public ClientToServerDecode(string g, string m, string u){
            game_id = g; msg = m; playerName = u;
        }
    }

    public class ServerToClient{
        public string kind;
        public Schriftrolle? schriftrolle;

        public ServerToClient(){
            kind = "recognize";
        }

        public ServerToClient(Schriftrolle sr, bool reconnect = false){
            kind = reconnect ? "init" : "update";
            schriftrolle = sr;
        }
        public ServerToClient(ServerToClientDecode d){
            kind = d.kind;
            schriftrolle = d.schriftrolle;
        }

        public ServerToClientDecode decode(){
            return new ServerToClientDecode(kind, schriftrolle);
        }
    }

    public class ServerToClientDecode{
        public string kind{get;set;}
        public Schriftrolle? schriftrolle{get;set;}
        public ServerToClientDecode(){}

        public ServerToClientDecode(string k, Schriftrolle? sr){
            kind = k; schriftrolle = sr;
        }
    }
}


public delegate void selectedCallback(string type);