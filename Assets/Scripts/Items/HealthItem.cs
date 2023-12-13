using Unity.VisualScripting;

public class HealthItem
{
    Dice.RollType DiceType;
    int NumOfDiceToRoll;

    public HealthItem(Dice.RollType diceType, int numOfDiceToRoll)
    {
        DiceType = diceType;
        NumOfDiceToRoll = numOfDiceToRoll;
    }

    public int RollForHealth()
    {
        int heatlh = 0;
        for (int i = 0; i < NumOfDiceToRoll; i++)
        {
            heatlh += Dice.Roll(DiceType);
        }

        return heatlh;
    }

    public int ExpectedRoll()
    {
        return Dice.ExpectedRoll(DiceType, NumOfDiceToRoll);
    }
}

public class SmallPotion : HealthItem
{
    static SmallPotion Instance = new SmallPotion();
    private SmallPotion() : base(Dice.RollType.D4, 1) { }

    public static SmallPotion GetInstance() { return Instance; }
}