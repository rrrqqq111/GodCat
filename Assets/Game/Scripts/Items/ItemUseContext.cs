using NekogamiRanch.Ranch;

namespace NekogamiRanch.Items
{
    public readonly struct ItemUseContext
    {
        public ItemUseContext(RanchManager ranchManager, RanchEconomyService economy, int day, ItemTriggerType triggerType)
        {
            RanchManager = ranchManager;
            Economy = economy;
            Day = day;
            TriggerType = triggerType;
        }

        public RanchManager RanchManager { get; }
        public RanchEconomyService Economy { get; }
        public int Day { get; }
        public ItemTriggerType TriggerType { get; }
    }
}
