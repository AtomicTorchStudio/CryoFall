namespace AtomicTorch.CBND.CoreMod.Systems.FishingSystem
{
    using System;
    using AtomicTorch.CBND.CoreMod.Characters.Player;
    using AtomicTorch.CBND.CoreMod.Helpers.Client;
    using AtomicTorch.CBND.GameApi.Data.Logic;
    using AtomicTorch.CBND.GameApi.Data.State;
    using AtomicTorch.GameEngine.Common.Primitives;

    public class FishingActionPublicActionState : BasePublicActionState
    {
        private const string FishingInAnimationName = "Fishing_In";

        private const string FishingOutAnimationName = "Fishing_Out";

        [NonSerialized]
        private ComponentFishingVisualizer clientFishingVisualizer;

        [SyncToClient(isAllowClientSideModification: true)]
        public ILogicObject CurrentFishingSession { get; set; }

        [SyncToClient(isAllowClientSideModification: true)]
        public Vector2D FishingTargetPosition { get; set; }

        public void ClientOnCurrentPlayerFishingSessionReceived(ILogicObject currentFishingSession)
        {
            this.CurrentFishingSession = currentFishingSession;
            this.clientFishingVisualizer.OnFishingSessionReceived(currentFishingSession);
        }

        protected override void ClientOnCompleted()
        {
            ComponentFishingCompletedVisualizer.OnFishCaughtOrFishingCancelled(
                this.Character,
                protoItemFishCaught: null,
                this.FishingTargetPosition,
                caughtFishSizeValue: 0);

            if (!this.clientFishingVisualizer.IsDestroyed)
            {
                this.clientFishingVisualizer.Destroy(delay: ComponentFishingVisualizer.DelayFishingOut);
            }

            this.clientFishingVisualizer = null;

            var skeletonRenderer = PlayerCharacter.GetClientState(this.Character).SkeletonRenderer;
            skeletonRenderer.RemoveAnimationTrackNextEntries(AnimationTrackIndexes.Extra);
            skeletonRenderer.AddAnimation(AnimationTrackIndexes.Extra,
                                          FishingOutAnimationName,
                                          isLooped: false);

            ClientFishingSoundsHelper.PlaySoundCancel(this.Character);
        }

        protected override void ClientOnStart()
        {
            this.clientFishingVisualizer = this.Character.ClientSceneObject
                                               .AddComponent<ComponentFishingVisualizer>();

            this.clientFishingVisualizer.Setup(this.Character, this.FishingTargetPosition);

            if (this.CurrentFishingSession != null)
            {
                this.clientFishingVisualizer.OnFishingSessionReceived(this.CurrentFishingSession);
            }

            var skeletonRenderer = PlayerCharacter.GetClientState(this.Character).SkeletonRenderer;
            skeletonRenderer.RemoveAnimationTrackNextEntries(AnimationTrackIndexes.Extra);
            skeletonRenderer.AddAnimation(AnimationTrackIndexes.Extra,
                                          FishingInAnimationName,
                                          isLooped: false);

            ClientFishingSoundsHelper.PlaySoundStart(this.Character);
        }
    }
}