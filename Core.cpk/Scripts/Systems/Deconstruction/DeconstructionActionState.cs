namespace AtomicTorch.CBND.CoreMod.Systems.Deconstruction
{
    using System;
    using AtomicTorch.CBND.CoreMod.Characters;
    using AtomicTorch.CBND.CoreMod.Items;
    using AtomicTorch.CBND.CoreMod.Items.Tools.Crowbars;
    using AtomicTorch.CBND.CoreMod.Skills;
    using AtomicTorch.CBND.CoreMod.SoundPresets;
    using AtomicTorch.CBND.CoreMod.StaticObjects;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.ConstructionSite;
    using AtomicTorch.CBND.CoreMod.Stats;
    using AtomicTorch.CBND.CoreMod.Systems.Creative;
    using AtomicTorch.CBND.CoreMod.Systems.ItemDurability;
    using AtomicTorch.CBND.CoreMod.Systems.LandClaim;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.Items;
    using AtomicTorch.CBND.GameApi.Data.State;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.GameEngine.Common.Helpers;
    using JetBrains.Annotations;

    public class DeconstructionActionState
        : BaseActionState<DeconstructionActionState.PublicState>
    {
        private const double DefaultDeconstructionStepDurationSeconds = 1.0;

        public readonly IItem ItemCrowbarTool;

        public readonly StaticObjectPublicState ObjectPublicState;

        [CanBeNull]
        public readonly IProtoItemToolCrowbar ProtoItemCrowbarTool;

        public readonly IStaticWorldObject WorldObject;

        private readonly IProtoObjectStructure protoStructure;

        private readonly float stageStructureRemoveValue;

        private readonly float structurePointsMax;

        private double currentStageDurationSeconds;

        private double currentStageTimeRemainsSeconds;

        public DeconstructionActionState(
            ICharacter character,
            IStaticWorldObject worldObject,
            IItem itemCrowbarTool)
            : base(character)
        {
            this.WorldObject = worldObject;
            this.ItemCrowbarTool = itemCrowbarTool;
            this.ProtoItemCrowbarTool = (IProtoItemToolCrowbar)itemCrowbarTool?.ProtoGameObject;
            this.protoStructure = (IProtoObjectStructure)worldObject.ProtoWorldObject;

            this.currentStageDurationSeconds = this.CalculateStageDurationSeconds(character, isFirstStage: true);

            this.currentStageTimeRemainsSeconds = this.currentStageDurationSeconds;
            this.ObjectPublicState = worldObject.GetPublicState<StaticObjectPublicState>();

            this.structurePointsMax = this.protoStructure.SharedGetStructurePointsMax(worldObject);

            // use build config to determine how many deconstruction steps required
            var stagesCount = this.protoStructure.GetStructureActiveConfig(worldObject)
                                  .StagesCount;

            if (stagesCount <= 0)
            {
                // force at least 1 deconstruction stage
                stagesCount = 1;
            }

            if (CreativeModeSystem.SharedIsInCreativeMode(character))
            {
                // force single stage
                stagesCount = 1;
            }

            this.stageStructureRemoveValue = this.structurePointsMax / stagesCount;
            if (this.stageStructureRemoveValue < 1)
            {
                this.stageStructureRemoveValue = 1;
            }
        }

        public override IWorldObject TargetWorldObject => this.WorldObject;

        public bool CheckIsAllowed()
        {
            return LandClaimSystem.SharedCheckCanDeconstruct(this.WorldObject, this.Character);
        }

        public bool CheckIsNeeded()
        {
            return true;
        }

        public override void SharedUpdate(double deltaTime)
        {
            if ((this.ItemCrowbarTool != null
                 && this.ItemCrowbarTool != this.CharacterPublicState.SelectedItem)
                || !this.CheckIsAllowed())
            {
                // selected hotbar item changed or the action is not allowed anymore
                this.AbortAction();
                return;
            }

            if (!DeconstructionSystem.SharedCheckCanInteract(
                    this.Character,
                    this.WorldObject,
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

            this.UpdateProgress();
        }

        protected override void AbortAction()
        {
            DeconstructionSystem.SharedAbortAction(
                this.Character,
                this.WorldObject);
        }

        protected override void SetupPublicState(PublicState state)
        {
            base.SetupPublicState(state);
            state.ProtoItemCrowbarTool = this.ProtoItemCrowbarTool;
        }

        private double CalculateStageDurationSeconds(ICharacter character, bool isFirstStage)
        {
            if (CreativeModeSystem.SharedIsInCreativeMode(character))
            {
                // force instant deconstruct
                return 0;
            }

            var durationSeconds = DefaultDeconstructionStepDurationSeconds;
            durationSeconds /= this.ProtoItemCrowbarTool?.DeconstructionSpeedMultiplier ?? 1;
            durationSeconds /= character.SharedGetFinalStatMultiplier(StatName.BuildingSpeed);
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
            if (Api.IsServer
                && this.ItemCrowbarTool != null)
            {
                // notify tool was used
                ServerItemUseObserver.NotifyItemUsed(this.Character, this.ItemCrowbarTool);

                // reduce tool durability
                ItemDurabilitySystem.ServerModifyDurability(this.ItemCrowbarTool, delta: -1);
            }

            this.currentStageDurationSeconds = this.CalculateStageDurationSeconds(this.Character, isFirstStage: false);
            this.currentStageTimeRemainsSeconds += this.currentStageDurationSeconds;

            var oldStructurePoints = this.ObjectPublicState.StructurePointsCurrent;
            var newStructurePoints = Math.Max(
                0,
                oldStructurePoints - this.stageStructureRemoveValue);

            this.protoStructure.SharedOnDeconstructionStage(
                this.WorldObject,
                this.Character,
                oldStructurePoints,
                newStructurePoints);

            if (Api.IsClient // client will simply always deconstruct until finished
                || newStructurePoints > 0)
            {
                // deconstruction progressed
                if (Api.IsServer)
                {
                    Logger.Important(
                        $"Deconstruction progressed: {this.WorldObject} structure points: {newStructurePoints}/{this.structurePointsMax}; by {this.Character}");
                }

                this.UpdateProgress();

                if (Api.IsServer
                    && this.ItemCrowbarTool != null
                    && this.ItemCrowbarTool.IsDestroyed)
                {
                    // tool was destroyed (durability 0)
                    this.AbortAction();
                    return;
                }

                return;
            }

            // deconstruction is completed
            if (Api.IsServer)
            {
                if (!(this.protoStructure is ProtoObjectConstructionSite))
                {
                    this.CharacterPrivateState.Skills.ServerAddSkillExperience<SkillBuilding>(
                        SkillBuilding.ExperienceAddWhenDeconstructionFinished);
                }

                Logger.Important(
                    $"Deconstruction completed: {this.WorldObject} structure points: {newStructurePoints}/{this.structurePointsMax}; by {this.Character}");
                this.ObjectPublicState.StructurePointsCurrent = newStructurePoints;
            }

            this.SetCompleted(isCancelled: false);
            DeconstructionSystem.SharedActionCompleted(this.Character, this);
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
            public IProtoItemToolCrowbar ProtoItemCrowbarTool { get; set; }

            protected override void ClientOnCompleted()
            {
                // don't play base sounds
                this.DestroyProcessSoundEmitter();
            }

            protected override ReadOnlySoundPreset<ObjectSound> SharedGetObjectSoundPreset()
            {
                return this.ProtoItemCrowbarTool?.ObjectInteractionSoundsPreset
                       ?? Api.GetProtoEntity<ItemCrowbar>().ObjectInteractionSoundsPreset;
            }
        }
    }
}