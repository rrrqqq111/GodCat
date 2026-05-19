using NekogamiRanch.Ranch;

namespace NekogamiRanch.Toys
{
    public readonly struct ToyUseContext
    {
        public ToyUseContext(RanchManager ranchManager, RanchEconomyService economy, int day, ToyTriggerType triggerType)
        {
            RanchManager = ranchManager;
            Economy = economy;
            Day = day;
            TriggerType = triggerType;
        }

        public RanchManager RanchManager { get; }
        public RanchEconomyService Economy { get; }
        public int Day { get; }
        public ToyTriggerType TriggerType { get; }
    }
}
