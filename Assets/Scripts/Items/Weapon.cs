using Unity.VisualScripting;

public class Weapon
{
    Dice.RollType DiceType;
    int NumOfDiceToRoll;

    public Weapon(Dice.RollType diceType, int numOfDiceToRoll)
    {
        DiceType = diceType;
        NumOfDiceToRoll = numOfDiceToRoll;
    }

    public int RollForDamage()
    {
        int damage = 0;
        for(int i = 0; i <  NumOfDiceToRoll; i++)
        {
            damage += Dice.Roll(DiceType);
        }

        return damage;
    }
}

public class Sword : Weapon
{
    static Sword Instance = new Sword();
    private Sword() : base(Dice.RollType.D6, 1) { }

    public static Sword GetInstance() { return Instance; }
}