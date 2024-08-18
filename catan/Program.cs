namespace catan{

    enum RessourceCard
    {
        Wheat, Iron, Sheep, Clay, Wood
    }
    enum BuildingCard{
        Village, City, Knight, Expansion, Street
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
                    res = new RessourceCardAmnts(2,3,0,1,0);
                    break;
                default: //Street
                    res = new RessourceCardAmnts(0,0,0,1,1);
                    break;
            }
            return res;
        }
    }

    class RessourceCardAmnts{
        
        public int wheat, iron, sheep, wood, clay;
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
            wheat -= trash.sheep;
        }

        public ref int correspondingAmnt(RessourceCard bc){
            if(bc == RessourceCard.Wheat) return ref wheat;
            else if(bc == RessourceCard.Iron) return ref iron;
            else if(bc == RessourceCard.Sheep) return ref sheep;
            else if(bc == RessourceCard.Wood) return ref wood;
            return ref clay;
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
        RessourceCardAmnts resCards;
        BuildingCardAmnts buildingCards;

        public PlayerState(){
            resCards = new RessourceCardAmnts();
            buildingCards = new BuildingCardAmnts(0,0,0,0,1);
        }

        public bool canBuyBuilding(BuildingCard bc){
            RessourceCardAmnts cost = CardCosts.cardCost(bc);
            return cost.clay <= resCards.clay && cost.sheep <= resCards.sheep && cost.iron <= resCards.iron && cost.wheat <= resCards.wheat && cost.wood <= resCards.wood;
        }

        public bool buyBuilding(BuildingCard bc){
            if(!canBuyBuilding(bc)) return false;
            resCards.subtract(CardCosts.cardCost(bc));
            buildingCards.correspondingAmnt(bc)++;
            return true;
        }
        
        //TODO: trade amnts, stealing, expansions, expansion effects, city effects, main game loop
    }

internal class Program
{
    private static void Main(string[] args)
    {
        Console.WriteLine("Hello, World!");
    }
}
}