namespace NekogamiRanch.MapObjects
{
    public readonly struct MapCellObjectUseResult
    {
        private MapCellObjectUseResult(bool success, string message, int moneyDelta, int baseMoneyBonusDelta)
        {
            Success = success;
            Message = message;
            MoneyDelta = moneyDelta;
            BaseMoneyBonusDelta = baseMoneyBonusDelta;
        }

        public bool Success { get; }
        public string Message { get; }
        public int MoneyDelta { get; }
        public int BaseMoneyBonusDelta { get; }

        public static MapCellObjectUseResult Failed(string message = null)
        {
            return new MapCellObjectUseResult(false, message, 0, 0);
        }

        public static MapCellObjectUseResult Succeeded(string message = null, int moneyDelta = 0, int baseMoneyBonusDelta = 0)
        {
            return new MapCellObjectUseResult(true, message, moneyDelta, baseMoneyBonusDelta);
        }
    }
}
