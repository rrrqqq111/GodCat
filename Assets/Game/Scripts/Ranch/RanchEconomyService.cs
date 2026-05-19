using NekogamiRanch.Animals;

namespace NekogamiRanch.Ranch
{
    public class RanchEconomyService
    {
        private readonly RanchGameState state;

        public RanchEconomyService(RanchGameState state)
        {
            this.state = state;
        }

        public int Money => state.Money;
        public int Cans => state.Cans;

        public void AddMoney(int amount)
        {
            state.AddMoney(amount);
        }

        public void AddExtraMoney(Animal source, int amount)
        {
            if (source != null)
            {
                amount = source.ResolveExtraMoney(amount);
            }

            AddMoney(amount);
        }

        public void AddCans(int amount)
        {
            if (amount <= 0)
            {
                return;
            }

            state.AddCans(amount);
        }

        public bool TrySpendCans(int amount)
        {
            return state.TrySpendCans(amount);
        }
    }
}
