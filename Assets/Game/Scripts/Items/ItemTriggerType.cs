namespace NekogamiRanch.Items
{
    public enum ItemTriggerType
    {
        ManualUse = 0,
        DayStart = 1,
        SettlementStart = 2,
        SettlementEnd = 3,
        AnimalAdded = 4,
        AnimalRemoved = 5,
        AnimalSold = 6,
        AnimalMoved = 7,
        AnimalGrown = 8,
        AnimalTransformed = 9,
        PreySucceeded = 10,
        PreyFailed = 11,
        BreedSucceeded = 12,
        TileChanged = 13,
        OfferRolled = 14,
        ShopRefreshed = 15,
        ItemSold = 16,
        Custom = 100
    }
}
