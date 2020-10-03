namespace AtomicTorch.CBND.CoreMod.Systems.FishingSystem
{
    using System;
    using AtomicTorch.CBND.CoreMod.Characters.Player;
    using AtomicTorch.CBND.CoreMod.Helpers.Client;
    using AtomicTorch.CBND.CoreMod.Items.Fishing.Base;
    using AtomicTorch.CBND.CoreMod.Skills;
    using AtomicTorch.CBND.CoreMod.UI;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.WorldObjects;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.Items;
    using AtomicTorch.CBND.GameApi.Data.Logic;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.GameEngine.Common.Helpers;
    using AtomicTorch.GameEngine.Common.Primitives;

    public class FishingActionState
        : BaseSystemActionState<
            FishingSystem,
            FishingActionRequest,
            FishingActionState,
            FishingActionPublicActionState>
    {
        private bool clientIsAbortSent;

        public FishingActionState(
            ICharacter character,
            double durationSeconds,
            IItem itemFishingRod,
            Vector2D fishingTargetPosition)
            : base(character, null, durationSeconds)
        {
            this.ItemFishingRod = itemFishingRod;
            this.FishingTargetPosition = fishingTargetPosition;

            if (Api.IsServer)
            {
                this.SharedFishingSession = Api.Server.World.CreateLogicObject<FishingSession>();
                FishingSession.ServerSetup(this.SharedFishingSession, character);
            }
        }

        public Vector2D FishingTargetPosition { get; }

        public override bool IsBlocksMovement => true;

        public override bool IsDisplayingProgress => false;

        public IItem ItemFishingRod { get; }

        public bool ServerIsBaitDeducted { get; private set; }

        public bool ServerIsPlayerTriedToCatch { get; set; }

        public bool ServerIsSuccess { get; set; }

        public ILogicObject SharedFishingSession { get; private set; }

        public void ClientOnItemUse()
        {
            var skeletonRenderer = PlayerCharacter.GetClientState(this.Character).SkeletonRenderer;
            var currentAnimationName = skeletonRenderer.GetCurrentAnimationName(AnimationTrackIndexes.Extra);
            if (currentAnimationName is not null
                && currentAnimationName.IndexOf("Fishing", StringComparison.Ordinal) >= 0)
            {
                // the animation is not yet finished
                return;
            }

            // player clicked a mouse so the rod should be removed
            if (this.SharedFishingSession is not null
                && FishingSession.GetPublicState(this.SharedFishingSession)
                                 .IsFishBiting)
            {
                // fish baiting, request pulling
                FishingSystem.ClientPullFish(this.SharedFishingSession);
                // Alas we don't know what the fish was caught yet
                // so the game cannot stop action immediately and display a pulling animation.
                // (Please note that players with high ping are not penalized for having
                // a shorter time to react on fish baiting - the baiting period is extended for them by ping duration)
            }

            // Not yet bating on the client side, but probably bating on the server side.
            // Let's just send an abort command to server and wait for a server-side cancellation command.
            // This way we can be certain there will be no discrepancy with the server state.
            if (this.clientIsAbortSent)
            {
                return;
            }

            System.ClientSendAbortAction(this.Request);
            this.clientIsAbortSent = true;
        }

        public void ClientSetCurrentFishingSession(ILogicObject currentFishingSession)
        {
            this.SharedFishingSession = currentFishingSession;
            var publicActionState = (FishingActionPublicActionState)this.CharacterPublicState.CurrentPublicActionState;
            publicActionState.ClientOnCurrentPlayerFishingSessionReceived(currentFishingSession);
        }

        public void OnFishCaught()
        {
            this.SetCompleted(isCancelled: false);
        }

        public void ServerSetCompleted()
        {
            this.SetCompleted(isCancelled: false);
        }

        public bool ServerTryToDeductTheBait()
        {
            if (this.ServerIsBaitDeducted)
            {
                return true;
            }

            var itemFishingRod = this.ItemFishingRod;
            var rodPublicState = itemFishingRod.GetPublicState<ItemFishingRodPublicState>();

            var itemBait = FishingSystem.SharedFindBaitItem(this.Character,
                                                            rodPublicState.CurrentProtoBait);
            if (itemBait is null)
            {
                // no bait to deduct
                return false;
            }

            // roll the chance to save the bait
            if (this.Character.SharedHasSkillFlag(SkillFishing.Flags.FishingChanceToSaveBait)
                && RandomHelper.RollWithProbability(SkillFishing.FishingChanceToSaveBait))
            {
                // saved the bait! just assume it was deducted
                this.ServerIsBaitDeducted = true;
                return true;
            }

            this.ServerIsBaitDeducted = FishingSystem.ServerTryDeductBait(this.Character,
                                                                          rodPublicState.CurrentProtoBait);
            return this.ServerIsBaitDeducted;
        }

        public override void SharedUpdate(double deltaTime)
        {
            // don't invoke the base SharedUpdate implementation as we don't advance the timer here
            if (!ReferenceEquals(this.ItemFishingRod, this.CharacterPublicState.SelectedItem))
            {
                // player changed the selected item
                this.SetCompleted(isCancelled: true);
                this.AbortAction();
                return;
            }

            if (FishingSystem.SharedIsTooFar(this.Character, this.FishingTargetPosition))
            {
                if (Api.IsClient)
                {
                    CannotInteractMessageDisplay.ClientOnCannotInteract(this.Character,
                                                                        CoreStrings.Notification_TooFar,
                                                                        isOutOfRange: true);
                }

                this.Cancel();
                return;
            }

            if (this.SharedFishingSession is not null
                && this.SharedFishingSession.IsDestroyed)
            {
                this.Complete();
            }
        }

        protected override void OnCompletedOrCancelled()
        {
            base.OnCompletedOrCancelled();

            if (Api.IsClient)
            {
                this.SharedFishingSession = null;
                return;
            }

            Api.Server.World.DestroyObject(this.SharedFishingSession);
            this.SharedFishingSession = null;

            if (this.IsCancelled)
            {
                // cancelled so bait was not used
                return;
            }

            this.ServerTryToDeductTheBait();

            if (this.ServerIsSuccess)
            {
                return;
            }

            if (this.ServerIsPlayerTriedToCatch)
            {
                FishingSystem.ServerSendNotificationFishSlipOfTheHook(this.Character);
            }
            else
            {
                FishingSystem.ServerSendNotificationTooLate(this.Character);
            }
        }

        protected override void SetupPublicState(FishingActionPublicActionState state)
        {
            base.SetupPublicState(state);
            state.CurrentFishingSession = this.SharedFishingSession;
            state.FishingTargetPosition = this.FishingTargetPosition;
        }
    }
}