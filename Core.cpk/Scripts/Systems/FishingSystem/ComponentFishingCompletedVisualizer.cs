namespace AtomicTorch.CBND.CoreMod.Systems.FishingSystem
{
    using System;
    using AtomicTorch.CBND.CoreMod.Characters.Player;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.Items;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.Scripting.ClientComponents;
    using AtomicTorch.CBND.GameApi.ServicesClient.Components;
    using AtomicTorch.GameEngine.Common.Helpers;
    using AtomicTorch.GameEngine.Common.Primitives;
    using JetBrains.Annotations;

    public class ComponentFishingCompletedVisualizer : ClientComponent
    {
        private const double DistanceThreshold = 0.1;

        private const double FishingCompletedAnimationDelay = 0.333;

        private const double MaxAnimationDistance = 10;

        private const double ValueInterpolationRate = 5;

        private ICharacter character;

        private double currentAnimationFraction;

        private Vector2D currentPosition;

        private Vector2D fishingTargetPosition;

        private IComponentSpriteRenderer spriteRenderer;

        public static void OnFishCaughtOrFishingCancelled(
            ICharacter character,
            [CanBeNull] IProtoItem protoItemFishCaught,
            Vector2D fishingTargetPosition,
            float caughtFishSizeValue)
        {
            ClientTimersSystem.AddAction(
                FishingCompletedAnimationDelay,
                () =>
                {
                    ComponentFishingVisualizer.TryGetFor(character, out var fishingVisualizer);
                    if (fishingVisualizer is null)
                    {
                        return;
                    }

                    if (fishingVisualizer.FishingCompletedVisualizer is null)
                    {
                        Api.Client.Scene.CreateSceneObject(nameof(ComponentFishingCompletedVisualizer))
                           .AddComponent<ComponentFishingCompletedVisualizer>()
                           .Setup(fishingTargetPosition,
                                  character,
                                  protoItemFishCaught,
                                  caughtFishSizeValue);
                    }
                    else if (protoItemFishCaught is not null)
                    {
                        fishingVisualizer.FishingCompletedVisualizer
                                         .SetupFish(protoItemFishCaught, caughtFishSizeValue);
                    }
                });
        }

        public override void Update(double deltaTime)
        {
            if (!this.character.IsInitialized)
            {
                this.Destroy();
                return;
            }

            Vector2D endPosition;

            var skeletonRender = PlayerCharacter.GetClientState(this.character).SkeletonRenderer;
            if (skeletonRender is null
                || !skeletonRender.IsReady)
            {
                endPosition = this.character.Position
                              + (0, this.character.ProtoCharacter.CharacterWorldHeight / 2.0);
            }
            else
            {
                var boneWorldPosition = skeletonRender.TransformSlotPosition(
                    slotName: skeletonRender.CurrentSkeleton.SkeletonName.IndexOf("Front", StringComparison.Ordinal)
                              >= 0
                                  ? "HandRight"
                                  : "HandLeft",
                    screenPositionOffset: (0, 0),
                    out _);
                endPosition = boneWorldPosition;
            }

            this.currentAnimationFraction = MathHelper.LerpWithDeltaTime(
                this.currentAnimationFraction,
                1,
                deltaTime,
                rate: ValueInterpolationRate);

            this.currentPosition = (x: MathHelper.Lerp(
                                        this.fishingTargetPosition.X,
                                        endPosition.X,
                                        this.currentAnimationFraction),
                                    y: MathHelper.Lerp(
                                        this.fishingTargetPosition.Y,
                                        endPosition.Y,
                                        this.currentAnimationFraction));

            var sqrDistance = this.currentPosition.DistanceSquaredTo(endPosition);
            if (sqrDistance >= MaxAnimationDistance * MaxAnimationDistance
                || sqrDistance <= DistanceThreshold * DistanceThreshold)
            {
                // cannot travel that far or destination point reached
                this.Destroy();
                return;
            }

            this.SceneObject.Position = this.currentPosition;
        }

        protected override void OnDisable()
        {
            this.spriteRenderer.Destroy();
            this.spriteRenderer = null;
            
            if (!this.SceneObject.IsDestroyed)
            {
                this.SceneObject.Destroy();
            }
        }

        protected override void OnEnable()
        {
            this.spriteRenderer = Api.Client.Rendering.CreateSpriteRenderer(this.SceneObject,
                                                                            drawOrder: DrawOrder.Overlay,
                                                                            spritePivotPoint: (0.5, 0.5));
        }

        private void Setup(
            Vector2D fishingTargetPosition,
            ICharacter character,
            [CanBeNull] IProtoItem protoItemFishCaught,
            float caughtFishSizeValue)
        {
            this.character = character;
            this.currentAnimationFraction = 0;
            this.currentPosition = this.fishingTargetPosition = fishingTargetPosition;
            this.SceneObject.Position = this.currentPosition;

            this.SetupFish(protoItemFishCaught, caughtFishSizeValue);

            this.Update(0);

            if (ComponentFishingVisualizer.TryGetFor(character, out var fishingVisualizer))
            {
                fishingVisualizer.FishingCompletedVisualizer = this;
            }
        }

        private void SetupFish(IProtoItem protoItemFishCaught, float caughtFishSizeValue)
        {
            if (protoItemFishCaught is null)
            {
                this.spriteRenderer.IsEnabled = false;
                return;
            }

            this.spriteRenderer.TextureResource = protoItemFishCaught.Icon;
            // apply non-linear scaling otherwise small fish will appear too small
            caughtFishSizeValue = (float)Math.Pow(caughtFishSizeValue, 0.667);
            this.spriteRenderer.Scale = 0.667 * caughtFishSizeValue;
            this.spriteRenderer.IsEnabled = true;

            ClientFishingSoundsHelper.PlaySoundSuccess(this.character);
        }
    }
}