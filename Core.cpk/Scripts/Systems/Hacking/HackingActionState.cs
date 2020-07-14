namespace AtomicTorch.CBND.CoreMod.Systems.Hacking
{
    using System;
    using AtomicTorch.CBND.CoreMod.SoundPresets;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Misc.Events;
    using AtomicTorch.CBND.CoreMod.Systems.Creative;
    using AtomicTorch.CBND.CoreMod.Systems.WorldObjectClaim;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.GameEngine.Common.Helpers;

    public class HackingActionState
        : BaseActionState<HackingActionState.PublicState>
    {
        public readonly IStaticWorldObject WorldObject;

        private readonly IProtoObjectHackableContainer protoHackableContainer;

        private double currentStageDurationSeconds;

        private double currentStageTimeRemainsSeconds;

        public HackingActionState(
            ICharacter character,
            IStaticWorldObject worldObject)
            : base(character)
        {
            this.WorldObject = worldObject;
            this.protoHackableContainer = (IProtoObjectHackableContainer)worldObject.ProtoWorldObject;
            this.currentStageDurationSeconds = this.CalculateStageDurationSeconds(character, isFirstStage: true);
            this.currentStageTimeRemainsSeconds = this.currentStageDurationSeconds;

            this.SharedTryClaimObject();
        }

        public override IWorldObject TargetWorldObject => this.WorldObject;

        public bool CheckIsNeeded()
        {
            return true;
        }

        public override void SharedUpdate(double deltaTime)
        {
            if (!HackingSystem.SharedCheckCanInteract(
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
            HackingSystem.SharedAbortAction(this.Character,
                                            this.WorldObject);
        }

        private double CalculateStageDurationSeconds(ICharacter character, bool isFirstStage)
        {
            if (CreativeModeSystem.SharedIsInCreativeMode(character))
            {
                // force instant hacking
                return 0;
            }

            var durationSeconds = this.protoHackableContainer.HackingStageDuration;
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
            this.SharedTryClaimObject();

            this.currentStageDurationSeconds = this.CalculateStageDurationSeconds(this.Character, isFirstStage: false);
            this.currentStageTimeRemainsSeconds += this.currentStageDurationSeconds;

            if (Api.IsServer)
            {
                this.protoHackableContainer.ServerOnHackingStage(
                    this.WorldObject,
                    this.Character);
            }

            if (Api.IsClient) // client will simply always progress until finished
            {
                // hacking progressed
                this.UpdateProgress();
                return;
            }

            // server-side code
            if (!this.WorldObject.IsDestroyed)
            {
                return;
            }

            Logger.Important(
                $"Hacking completed: {this.WorldObject} by {this.Character}");

            this.SetCompleted(isCancelled: false);
            HackingSystem.SharedActionCompleted(this.Character, this);
        }

        private void SharedTryClaimObject()
        {
            if (Api.IsServer)
            {
                WorldObjectClaimSystem.ServerTryClaim(
                    this.WorldObject,
                    this.Character,
                    durationSeconds: Math.Max(this.currentStageDurationSeconds + 5,
                                              WorldObjectClaimDuration.EventObjects));
            }
        }

        private void UpdateProgress()
        {
            var timeRemains = this.currentStageDurationSeconds - this.currentStageTimeRemainsSeconds;
            timeRemains = MathHelper.Clamp(timeRemains, 0, this.currentStageDurationSeconds);
            this.ProgressPercents = 100 * timeRemains / this.currentStageDurationSeconds;
        }

        public class PublicState : PublicActionStateWithTargetObjectSounds
        {
            protected override void ClientOnCompleted()
            {
                this.DestroyProcessSoundEmitter();

                if (this.TargetWorldObject == null)
                {
                    return;
                }

                var objectSoundPreset = this.SharedGetObjectSoundPreset();
                objectSoundPreset?.PlaySound(
                    this.TargetWorldObject != null
                    && !this.TargetWorldObject.IsDestroyed
                        ? ObjectSound.InteractFail
                        : ObjectSound.InteractSuccess,
                    this.Character,
                    volume: 0.5f);
            }

            protected override void ClientOnStart()
            {
                base.ClientOnStart();
                this.soundEmitter.Volume = 0.2f;
            }
        }
    }
}