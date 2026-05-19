namespace NekogamiRanch.Ranch
{
    public class RanchGameState
    {
        public RanchGameState(int day, int money, int cans)
        {
            Day = day;
            Money = money;
            Cans = cans;
            Phase = RanchPhase.Playing;
            RandomizeAnimalPositionsInTestMode = true;
        }

        public int Day { get; private set; }
        public int Money { get; private set; }
        public int Cans { get; private set; }
        public RanchPhase Phase { get; private set; }
        public bool RandomizeAnimalPositionsInTestMode { get; private set; }

        private bool testMode;

        public bool IsWaitingForOfferSelection => Phase == RanchPhase.OfferSelection;
        public bool IsWaitingToEnterNextDay => Phase == RanchPhase.DayTransition;
        public bool IsTestMode => testMode;

        public void AddDay()
        {
            Day++;
        }

        public void AddMoney(int amount)
        {
            Money += amount;
        }

        public void AddCans(int amount)
        {
            Cans += amount;
        }

        public bool TrySpendCans(int amount)
        {
            if (amount <= 0)
            {
                return true;
            }

            if (Cans < amount)
            {
                return false;
            }

            Cans -= amount;
            return true;
        }

        public void SetPhase(RanchPhase phase)
        {
            Phase = phase;
        }

        public void EnterTestMode()
        {
            testMode = true;
            Phase = RanchPhase.TestMode;
        }

        public void ExitTestMode()
        {
            testMode = false;
            Phase = RanchPhase.Playing;
        }

        public void SetRandomizeAnimalPositionsInTestMode(bool enabled)
        {
            RandomizeAnimalPositionsInTestMode = enabled;
        }
    }
}
