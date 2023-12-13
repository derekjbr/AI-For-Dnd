using Unity.VisualScripting;

public static class Dice 
{
    public enum RollType
    {
        D4,
        D6,
        D8,
        D12,
        D20
    }

    private static System.Random Rand = new System.Random();

    public static int Roll(RollType diceType)
    {
        switch (diceType)
        {
            case RollType.D4:
                return Rand.Next(1, 5);
            case RollType.D6:
                return Rand.Next(1, 7);
            case RollType.D8:
                return Rand.Next(1, 9);
            case RollType.D12:
                return Rand.Next(1, 13);
            case RollType.D20:
                return Rand.Next(1, 21);
            default:
                return -1;
        }
    }

    public static int ExpectedRoll(RollType diceType, int numDice)
    {
        int n = 0;
        switch (diceType)
        {
            case RollType.D4:
                n = 4;
                break;
            case RollType.D6:
                n = 6;
                break;
            case RollType.D8:
                n = 8;
                break;
            case RollType.D12:
                n = 12;
                break;
            case RollType.D20:
                n = 20;
                break;
            default:
                return -1;
        }

        return numDice * (n + 1) / 2;
    }
}
