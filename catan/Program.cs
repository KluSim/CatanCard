using System;
using System.Collections.Generic;

namespace catan{

    enum RessourceCard
    {
        Wheat, Iron, Sheep, Clay, Wood
    }
    
    enum BuildingCard{
        Village, City, Knight, Expansion, Street
    }

    enum CityExpansion {
        Library, TradersGuild, Chruch, CityHall, Castle, University
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
        
        public int wheat, iron, sheep, wood, clay; //todo ersätzen durch dick
        public int total{
            get {
                return wheat+iron+sheep+clay+wood;
            }
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

        public ExpansionCardAmnts(int li, int tr, int ch, int ci, int ca, int un) {
            library = li; tradersGuild = tr; church = ch; cityHall = ci; castle = ca; university = un;
        }

        public ref int correspondingAmnt(CityExpansion ex){
            switch(ex) {
                case CityExpansion.Library: return ref library;
                case CityExpansion.TradersGuild: return ref tradersGuild;
                case CityExpansion.Chruch: return ref church;
                case CityExpansion.CityHall: return ref cityHall;
                case CityExpansion.Castle: return ref castle;
                case CityExpansion.University: return ref university;
            }

            return ref library; // if you reach this, I guess youre building a library now
        }
    }

    class BuildingCardAmnts{
        public int village, city, knight, expansion, street;

        public BuildingCardAmnts(int v, int c, int k, int e, int s){
            village = v; city = c; knight = k; expansion = e; street = s;
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

        public PlayerState(int playerID){
            this.playerID = playerID;
            resCards = new RessourceCardAmnts();
            buildingCards = new BuildingCardAmnts(1,0,0,0,1);
            expansionCards = new ExpansionCardAmnts(0,0,0,0,0,0);
            canBuild = new Dictionary<BuildingCard, bool>();
            foreach(BuildingCard b in Enum.GetValues(typeof(BuildingCard))) canBuild.Add(b, true);
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

        public bool buyBuilding(BuildingCard bc, RessourceCardAmnts proposedResources, CityExpansion proposedExpansion){
            if(!canBuyBuilding(bc, proposedResources, proposedExpansion)) return false;
            resCards.subtract(proposedResources);
            buildingCards.correspondingAmnt(bc)++;
            if(bc == BuildingCard.Expansion) expansionCards.correspondingAmnt(proposedExpansion)++;
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
        
        //TODO: trade amnts, stealing, expansion effects, city effects, main game loop
    }

    class ServerState {
        List<PlayerState> players;
        int currentTurnPlayer;

        RessourceCardAmnts drawPile;
        BuildingCardAmnts buildingPile;
        ExpansionCardAmnts expansionPile;
        bool CardOfFate = true;
        RessourceCardAmnts market;


        public ServerState(){
            players = new List<PlayerState>();
        }

        public void init(int numberOfPlayers){
            players.Clear();
            if(numberOfPlayers < 2 && numberOfPlayers > 4) Console.WriteLine("replace me with actual error handling. if you read this, haha your code does the big stupid");
            for(int i = 0; i < numberOfPlayers; ++i) players.Add(new PlayerState(i));
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

        public void GameLoop(){
            while(true){
                PlayerState current = players[currentTurnPlayer];
                current.startTurn();
                // player builds shit (player actions)
                
                RessourceCardAmnts drawncards = new RessourceCardAmnts();
                for(int i = 0; i < current.getDrawCards(); ++i){
                    drawncards.correspondingAmnt(drawPile.takeRandomRes()) += 1;
                }

                if(current.endTurn(drawncards)) break;

                currentTurnPlayer++;
                if(currentTurnPlayer >= players.Count) currentTurnPlayer = 0;
            }
        }
    }

internal class Program
{
    private static void Main(string[] args)
    {
        Console.WriteLine("Hello, World!");
    }
}
}