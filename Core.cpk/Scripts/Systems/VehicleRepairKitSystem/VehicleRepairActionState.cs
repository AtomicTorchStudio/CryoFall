namespace AtomicTorch.CBND.CoreMod.Systems.VehicleRepairKitSystem
{
    using AtomicTorch.CBND.CoreMod.Items;
    using AtomicTorch.CBND.CoreMod.Items.Tools.Special;
    using AtomicTorch.CBND.CoreMod.SoundPresets;
    using AtomicTorch.CBND.CoreMod.Systems.ItemDurability;
    using AtomicTorch.CBND.CoreMod.Systems.VehicleSystem;
    using AtomicTorch.CBND.CoreMod.Vehicles;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.Items;
    using AtomicTorch.CBND.GameApi.Data.State;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.GameEngine.Common.Helpers;

    public class VehicleRepairActionState
        : BaseActionState<VehicleRepairActionState.PublicState>
    {
        public readonly IItem ItemVehicleRepairKit;

        public readonly IProtoItemVehicleRepairKit ProtoItemVehicleRepairKit;

        public readonly IDynamicWorldObject Vehicle;

        public readonly VehiclePublicState VehiclePublicState;

        private readonly double stageStructureAddValue;

        private readonly double structurePointsMax;

        private double currentStageDurationSeconds;

        private double currentStageTimeRemainsSeconds;

        public VehicleRepairActionState(
            ICharacter character,
            IDynamicWorldObject vehicle,
            IItem itemVehicleRepairKit)
            : base(character)
        {
            this.Vehicle = vehicle;
            this.ItemVehicleRepairKit = itemVehicleRepairKit;
            this.ProtoItemVehicleRepairKit = (IProtoItemVehicleRepairKit)itemVehicleRepairKit.ProtoGameObject;
            var protoVehicle = (IProtoVehicle)vehicle.ProtoWorldObject;

            this.currentStageTimeRemainsSeconds = this.currentStageDurationSeconds =
                                                      this.CalculateStageDurationSeconds(character, isFirstStage: true);
            this.VehiclePublicState = vehicle.GetPublicState<VehiclePublicState>();

            this.structurePointsMax = protoVehicle.SharedGetStructurePointsMax(vehicle);
            this.stageStructureAddValue = this.structurePointsMax / protoVehicle.RepairStagesCount;
        }

        public override bool IsDisplayingProgress => true;

        public double StructurePointsMax => this.structurePointsMax;

        public override IWorldObject TargetWorldObject => this.Vehicle;

        public bool CheckIsNeeded()
        {
            return this.VehiclePublicState.StructurePointsCurrent < this.structurePointsMax;
        }

        public override void SharedUpdate(double deltaTime)
        {
            if (this.CharacterPublicState.SelectedItem != this.ItemVehicleRepairKit)
            {
                this.AbortAction();
                return;
            }

            if (!VehicleRepairKitSystem.SharedCheckCanInteract(
                    this.Character,
                    this.Vehicle,
                    writeToLog: true))
            {
                this.AbortAction();
                return;
            }

            this.currentStageTimeRemainsSeconds -= deltaTime;
            if (this.currentStageTimeRemainsSeconds <= 0)
            {
                this.SharedOnStageCompleted();
            }
            else if (Api.IsClient)
            {
                if (this.VehiclePublicState.StructurePointsCurrent >= this.structurePointsMax)
                {
                    // apparently the repair finished before the client simulation was complete
                    this.AbortAction();
                }
            }

            this.UpdateProgress();
        }

        protected override void AbortAction()
        {
            VehicleRepairKitSystem.SharedAbortAction(
                this.Character,
                this.Vehicle);
        }

        protected override void SetupPublicState(PublicState state)
        {
            base.SetupPublicState(state);
            state.ProtoItemVehicleRepairKit = this.ProtoItemVehicleRepairKit;
        }

        private double CalculateStageDurationSeconds(ICharacter character, bool isFirstStage)
        {
            /*if (CreativeModeSystem.SharedIsInCreativeMode(character))
            {
                // force instant construct
                return 0;
            }*/

            var durationSeconds = VehicleRepairKitSystem.RepairStageDurationSeconds;
            durationSeconds /= this.ProtoItemVehicleRepairKit.RepairSpeedMultiplier;
            //durationSeconds /= character.SharedGetFinalStatMultiplier(StatName.VehicleRepairSpeed);
            durationSeconds = Api.Shared.RoundDurationByServerFrameDuration(durationSeconds);

            if (isFirstStage && Api.IsClient)
            {
                // Add ping to all client action durations.
                // Otherwise the client will not see immediately the result of the action
                // - the client will receive it only after RTT (ping) time.
                // TODO: currently it's possible to cancel action earlier and start a new one, but completed action result will come from server - which looks like a bug to player
                durationSeconds += Api.Client.CurrentGame.PingGameSeconds;
            }

            return durationSeconds;
        }

        private void SharedOnStageCompleted()
        {
            if (Api.IsServer)
            {
                // notify tool was used
                ServerItemUseObserver.NotifyItemUsed(this.Character, this.ItemVehicleRepairKit);

                // reduce tool durability
                ItemDurabilitySystem.ServerModifyDurability(this.ItemVehicleRepairKit, delta: -1);
            }
            else // if client
            {
                Api.Client.Audio.PlayOneShot(VehicleSystem.SoundResourceVehicleRepair);
            }

            this.currentStageDurationSeconds = this.CalculateStageDurationSeconds(this.Character, isFirstStage: false);
            this.currentStageTimeRemainsSeconds += this.currentStageDurationSeconds;

            var currentStructurePoints = this.VehiclePublicState.StructurePointsCurrent;
            var newStructurePoints = currentStructurePoints
                                     + this.stageStructureAddValue;

            if (Api.IsServer)
            {
                ((IProtoVehicle)this.Vehicle.ProtoGameObject)
                    .ServerOnRepair(this.Vehicle, this.Character);
            }

            if (currentStructurePoints < this.structurePointsMax)
            {
                // repairing is still possible - more stages are available
                this.UpdateProgress();

                if (Api.IsServer
                    && this.ItemVehicleRepairKit.IsDestroyed)
                {
                    // tool was destroyed (durability 0)
                    this.AbortAction();
                    return;
                }

                return;
            }

            // repairing is absolutely completed
            if (Api.IsServer)
            {
                // TODO: add skill experience for repair
                // this.CharacterPrivateState.Skills.ServerAddSkillExperience<SkillVehicles>(
                //     SkillVehicles.ExperienceAddWhenRepairFinished);
            }

            this.SetCompleted(isCancelled: false);
            VehicleRepairKitSystem.SharedActionCompleted(this.Character, this);
        }

        private void UpdateProgress()
        {
            var timeRemains = this.currentStageDurationSeconds - this.currentStageTimeRemainsSeconds;
            timeRemains = MathHelper.Clamp(timeRemains, 0, this.currentStageDurationSeconds);
            this.ProgressPercents = 100 * timeRemains / this.currentStageDurationSeconds;
        }

        public class PublicState : PublicActionStateWithTargetObjectSounds
        {
            [SyncToClient(receivers: SyncToClientReceivers.ScopePlayers)]
            public IProtoItemVehicleRepairKit ProtoItemVehicleRepairKit { get; set; }

            protected override void ClientOnCompleted()
            {
                // don't play base sounds
                this.DestroyProcessSoundEmitter();
            }

            protected override ReadOnlySoundPreset<ObjectSound> SharedGetObjectSoundPreset()
            {
                return this.ProtoItemVehicleRepairKit.ObjectInteractionSoundsPreset;
            }
        }
    }
}