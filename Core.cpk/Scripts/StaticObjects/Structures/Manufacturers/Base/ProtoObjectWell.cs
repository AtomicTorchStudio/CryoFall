namespace AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Manufacturers
{
    using AtomicTorch.CBND.CoreMod.Characters.Player;
    using AtomicTorch.CBND.CoreMod.CharacterStatusEffects.Debuffs;
    using AtomicTorch.CBND.CoreMod.ItemContainers;
    using AtomicTorch.CBND.CoreMod.Items.Food;
    using AtomicTorch.CBND.CoreMod.Rates;
    using AtomicTorch.CBND.CoreMod.SoundPresets;
    using AtomicTorch.CBND.CoreMod.Systems.Construction;
    using AtomicTorch.CBND.CoreMod.Systems.LiquidContainer;
    using AtomicTorch.CBND.CoreMod.Systems.PowerGridSystem;
    using AtomicTorch.CBND.CoreMod.Tiles;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.WorldObjects.Manufacturers;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.WorldObjects.Manufacturers.Data;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.State;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.Scripting.Network;
    using AtomicTorch.GameEngine.Common.Helpers;

    public abstract class ProtoObjectWell
        : ProtoObjectManufacturer<
              ObjectWellPrivateState,
              ObjectManufacturerPublicState,
              StaticObjectClientState>,
          IProtoObjectWell
    {
        // water table is a technical term
        public const string ErrorWellNotAllowed = "This ground type doesn't offer access to the water table.";

        public override byte ContainerFuelSlotsCount => 0;

        public override byte ContainerInputSlotsCount => 1;

        public override byte ContainerOutputSlotsCount => 1;

        public override bool IsAutoSelectRecipe => true;

        public override bool IsFuelProduceByproducts => false;

        public override double ServerUpdateIntervalSeconds => 0.25;

        public override double ServerUpdateRareIntervalSeconds => 2;

        public abstract double WaterCapacity { get; }

        public abstract double WaterProductionAmountPerSecond { get; }

        protected LiquidContainerConfig LiquidContainerConfig { get; private set; }

        public static bool SharedIsProvidingStaleWater(IStaticWorldObject objectWell)
        {
            return objectWell.OccupiedTile.ProtoTile
                       is IProtoTileWellAllowed protoTileWellAllowed
                   && protoTileWellAllowed.IsStaleWellWater;
        }

        public void ClientDrink(IStaticWorldObject objectWell)
        {
            if (!SharedValidateCanDrink(Client.Characters.CurrentPlayerCharacter, objectWell))
            {
                return;
            }

            this.CallServer(_ => _.ServerRemote_Drink(objectWell));
        }

        protected override BaseUserControlWithWindow ClientOpenUI(ClientObjectData data)
        {
            return WindowWell.Open(
                new ViewModelWindowWell(
                    data.GameObject,
                    data.PrivateState,
                    this.ManufacturingConfig,
                    data.PrivateState.LiquidStateWater,
                    this.LiquidContainerConfig));
        }

        protected override void PrepareConstructionConfig(
            ConstructionTileRequirements tileRequirements,
            ConstructionStageConfig build,
            ConstructionStageConfig repair,
            ConstructionUpgradeConfig upgrade,
            out ProtoStructureCategory category)
        {
            tileRequirements.Add(ErrorWellNotAllowed,
                                 c => c.Tile.ProtoTile is IProtoTileWellAllowed);

            this.PrepareConstructionConfigWell(tileRequirements, build, repair, upgrade, out category);
        }

        protected abstract void PrepareConstructionConfigWell(
            ConstructionTileRequirements tileRequirements,
            ConstructionStageConfig build,
            ConstructionStageConfig repair,
            ConstructionUpgradeConfig upgrade,
            out ProtoStructureCategory category);

        protected override void PrepareProtoStaticWorldObject()
        {
            base.PrepareProtoStaticWorldObject();

            this.LiquidContainerConfig = new LiquidContainerConfig(
                capacity: this.WaterCapacity,
                amountAutoIncreasePerSecond: this.WaterProductionAmountPerSecond,
                // water decrease happens automatically when crafting of the "bottle with water" recipe finishes
                amountAutoDecreasePerSecondWhenUse: 0);
        }

        protected override void ServerInitialize(ServerInitializeData data)
        {
            base.ServerInitialize(data);

            // setup input container to allow only empty water bottles on input
            Server.Items.SetContainerType<ItemsContainerEmptyBottles>(
                data.PrivateState.ManufacturingState.ContainerInput);

            var privateState = data.PrivateState;
            privateState.LiquidStateWater ??= new LiquidContainerState();
        }

        protected override void ServerUpdate(ServerUpdateData data)
        {
            var privateState = data.PrivateState;
            var publicState = data.PublicState;

            var liquidContainerState = privateState.LiquidStateWater;

            var isFull = liquidContainerState.Amount >= this.LiquidContainerConfig.Capacity;

            var isActive = !isFull;
            if (isActive)
            {
                // perform electricity checks (if applies)
                isActive = this.ElectricityConsumptionPerSecondWhenActive <= 0
                           || publicState.ElectricityConsumerState == ElectricityConsumerState.PowerOnActive;
            }

            publicState.IsActive = isActive;

            LiquidContainerSystem.UpdateWithManufacturing(
                data.GameObject,
                liquidContainerState,
                this.LiquidContainerConfig,
                privateState.ManufacturingState,
                this.ManufacturingConfig,
                data.DeltaTime * RateManufacturingSpeedMultiplier.SharedValue,
                isProduceLiquid: isActive,
                forceUpdateRecipe: !isFull);
        }

        private static bool SharedValidateCanDrink(ICharacter character, IStaticWorldObject objectWell)
        {
            if (SharedIsProvidingStaleWater(objectWell))
            {
                // cannot drink stale water directly
                return false;
            }

            return !StatusEffectNausea.SharedCheckIsNauseous(
                       character,
                       showNotificationIfNauseous: true);
        }

        [RemoteCallSettings(DeliveryMode.ReliableUnordered, keyArgIndex: 0)]
        private void ClientRemote_CharacterDrankWater(ICharacter character)
        {
            ItemsSoundPresets.ItemFoodDrink.PlaySound(ItemSound.Use,
                                                      character,
                                                      pitch: RandomHelper.Range(0.95f, 1.05f));
        }

        private void ServerRemote_Drink(IStaticWorldObject objectWell)
        {
            this.VerifyGameObject(objectWell);

            var character = ServerRemoteContext.Character;
            if (!this.SharedCanInteract(character, objectWell, writeToLog: true))
            {
                // too far or other obstacles between the well and the character
                return;
            }

            if (!SharedValidateCanDrink(character, objectWell))
            {
                return;
            }

            var liquidContainerState = GetPrivateState(objectWell).LiquidStateWater;
            var characterStats = PlayerCharacter.GetPublicState(character).CurrentStatsExtended;

            // calculate how many points the water of bottle restores per unit of water
            var protoBottleWater = GetProtoEntity<ItemBottleWater>();
            var waterPointsRestorePerUnit = protoBottleWater.WaterRestore / (double)protoBottleWater.Capacity;

            // calculate amount to drink to completely restore the water level of the character
            var amountToDrink = (characterStats.WaterMax - characterStats.WaterCurrent) / waterPointsRestorePerUnit;
            if (amountToDrink <= 0f)
            {
                // no need to drink
                return;
            }

            if (liquidContainerState.Amount < amountToDrink)
            {
                // the well doesn't have enough amount of water
                // select all remaining amount to drink
                amountToDrink = liquidContainerState.Amount;
                if (amountToDrink <= 0f)
                {
                    // nothing to drink - the well is empty
                    return;
                }
            }

            // reduce water level in the well
            liquidContainerState.Amount -= amountToDrink;

            // add water points to the character stats
            var addedWaterPoints = waterPointsRestorePerUnit * amountToDrink;
            characterStats.ServerSetWaterCurrent((float)(characterStats.WaterCurrent + addedWaterPoints));

            Logger.Important($"{character} drank water from {objectWell}");

            // notify clients (to play a sound)
            using var scopedBy = Api.Shared.GetTempList<ICharacter>();
            Server.World.GetScopedByPlayers(character, scopedBy);
            scopedBy.Add(character);
            this.CallClient(scopedBy.AsList(), _ => _.ClientRemote_CharacterDrankWater(character));
        }
    }
}