namespace AtomicTorch.CBND.CoreMod.Events
{
    using AtomicTorch.CBND.CoreMod.Helpers.Server;
    using AtomicTorch.CBND.CoreMod.Items.Fishing.Base;
    using AtomicTorch.CBND.CoreMod.Skills;
    using AtomicTorch.CBND.CoreMod.Zones;
    using AtomicTorch.CBND.GameApi.Data.Logic;
    using AtomicTorch.CBND.GameApi.Data.State;
    using AtomicTorch.CBND.GameApi.ServicesServer;
    using AtomicTorch.GameEngine.Common.Primitives;

    public abstract class ProtoEventFishing
        : ProtoEventWithArea<
            EmptyPrivateState,
            EventWithAreaPublicState,
            EmptyClientState>
    {
        // Localization: there are various fishing terms used (school, spawn) that require accurate translation.
        public const string Description_Format =
            "A school of rare fish called {0} just appeared near the ocean shore! Take your chance to catch such a unique species, only found during their spawn period.";

        public override bool ConsolidateNotifications => true;

        public override string Description => string.Format(Description_Format,
                                                            this.ProtoItemFish.Name)
                                              + "[br][br]"
                                              + string.Format(SkillFishing.SkillLevelRequirement,
                                                              this.RequiredFishingSkillLevel);

        public IProtoItemFish ProtoItemFish { get; private set; }

        public byte RequiredFishingSkillLevel { get; private set; }

        protected override bool ServerCreateEventSearchArea(
            IWorldServerService world,
            Vector2Ushort eventPosition,
            ushort circleRadius,
            out Vector2Ushort circlePosition)
        {
            var biome = world.GetTile(eventPosition).ProtoTile;
            return ServerSearchAreaHelper.GenerateSearchArea(eventPosition,
                                                             biome,
                                                             circleRadius,
                                                             out circlePosition,
                                                             maxAttempts: 100,
                                                             waterMaxRatio: 0.80);
        }

        protected override void ServerInitializeEvent(ServerInitializeData data)
        {
        }

        protected override bool ServerIsValidEventPosition(Vector2Ushort tilePosition)
        {
            return true;
        }

        protected override void ServerOnEventDestroyed(ILogicObject worldEvent)
        {
        }

        protected sealed override void ServerOnEventWithAreaStarted(ILogicObject worldEvent)
        {
            this.ServerOnFishingEventStarted(worldEvent);
        }

        protected virtual void ServerOnFishingEventStarted(ILogicObject worldEvent)
        {
        }

        protected sealed override void ServerPrepareEvent(Triggers triggers)
        {
            this.ServerPrepareFishingEvent(triggers);
        }

        protected abstract void ServerPrepareFishingEvent(Triggers triggers);

        protected sealed override void SharedPrepareEvent()
        {
            base.SharedPrepareEvent();

            this.SharedPrepareFishingEvent(out var protoItemFish,
                                           out var requiredFishingSkillLevel);
            this.ProtoItemFish = protoItemFish;
            this.RequiredFishingSkillLevel = requiredFishingSkillLevel;
        }

        protected abstract void SharedPrepareFishingEvent(
            out IProtoItemFish protoItemFish,
            out byte requiredFishingSkillLevel);
    }
}