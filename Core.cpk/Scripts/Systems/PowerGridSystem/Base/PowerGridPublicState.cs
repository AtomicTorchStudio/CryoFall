namespace AtomicTorch.CBND.CoreMod.Systems.PowerGridSystem
{
    using System;
    using System.Collections.Generic;
    using AtomicTorch.CBND.GameApi.Data.Logic;
    using AtomicTorch.CBND.GameApi.Data.State;
    using AtomicTorch.CBND.GameApi.Data.World;

    public class PowerGridPublicState : BasePublicState
    {
        [NonSerialized]
        public bool ServerNeedToSortCacheConsumers = true;

        [NonSerialized]
        public bool ServerNeedToSortCacheProducers = true;

        [SyncToClient]
        [TempOnly]
        public double EfficiencyMultiplier { get; set; }

        [SyncToClient]
        public double ElectricityAmount { get; set; }

        [SyncToClient]
        [TempOnly]
        public double ElectricityCapacity { get; set; }

        [SyncToClient]
        [TempOnly]
        public double ElectricityConsumptionCurrent { get; set; }

        [SyncToClient]
        [TempOnly]
        public double ElectricityConsumptionTotalDemand { get; set; }

        [SyncToClient]
        [TempOnly]
        public double ElectricityProductionCurrent { get; set; }

        [SyncToClient]
        [TempOnly]
        public double ElectricityProductionTotalAvailable { get; set; }

        [SyncToClient]
        [TempOnly]
        public ushort NumberConsumers { get; set; }

        [SyncToClient]
        [TempOnly]
        public ushort NumberConsumersActive { get; set; }

        [SyncToClient]
        [TempOnly]
        public byte NumberLandClaims { get; set; }

        [SyncToClient]
        [TempOnly]
        public ushort NumberProducers { get; set; }

        [SyncToClient]
        [TempOnly]
        public ushort NumberProducersActive { get; set; }

        [SyncToClient]
        [TempOnly]
        public ushort NumberStorages { get; set; }

        public ILogicObject ServerAreasGroup { get; set; }

        [TempOnly]
        public List<IStaticWorldObject> ServerCacheConsumers { get; private set; }

        [TempOnly]
        public List<IStaticWorldObject> ServerCacheProducers { get; private set; }

        [TempOnly]
        public List<IStaticWorldObject> ServerCacheStorage { get; private set; }

        [TempOnly]
        public bool ServerIsDirty { get; set; }

        public void ServerInitState()
        {
            this.ServerCacheConsumers = new List<IStaticWorldObject>();
            this.ServerCacheProducers = new List<IStaticWorldObject>();
            this.ServerCacheStorage = new List<IStaticWorldObject>();
        }
    }
}