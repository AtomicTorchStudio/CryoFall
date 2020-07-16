namespace AtomicTorch.CBND.CoreMod.Systems.FishingSystem
{
    using System;
    using System.Collections.Generic;
    using System.Windows.Media;
    using AtomicTorch.CBND.CoreMod.Characters;
    using AtomicTorch.CBND.CoreMod.Characters.Player;
    using AtomicTorch.CBND.CoreMod.ClientComponents.Rendering;
    using AtomicTorch.CBND.CoreMod.Items.Tools;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.Logic;
    using AtomicTorch.CBND.GameApi.Resources;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.Scripting.ClientComponents;
    using AtomicTorch.CBND.GameApi.ServicesClient.Components;
    using AtomicTorch.GameEngine.Common.Helpers;
    using AtomicTorch.GameEngine.Common.Primitives;

    public class ComponentFishingVisualizer : ClientComponent
    {
        public const double DelayFishingOut = 1.0;

        private const double DelayFishingIn = 0.65;

        private const double FloatMovementAmplitude = 0.025;

        private const double FloatMovementMaxSpeed = 1;

        private static readonly Dictionary<ICharacter, ComponentFishingVisualizer> ComponentCharacterBinding
            = new Dictionary<ICharacter, ComponentFishingVisualizer>();

        private static readonly Vector2D FloatLineConnectionOffsetDown = (0.027, 0.001);

        private static readonly Vector2D FloatLineConnectionOffsetUp = (0.0285, 0.097);

        private static readonly Color LineColor = Color.FromArgb(0xAA, 0xAA, 0xAA, 0xAA);

        private static readonly ITextureAtlasResource TextureResourceFishingFloat
            = new TextureAtlasResource("FX/Fishing/Float.png",
                                       columns: 6,
                                       rows: 1,
                                       isTransparent: true);

        private static readonly TextureResource TextureResourceFishingLine
            = new TextureResource("FX/Fishing/Line.png");

        private ICharacter character;

        private ClientComponentSpriteSheetAnimator componentSpriteSheetAnimatorFloater;

        private ILogicObject currentFishingSession;

        private FishingSession.PublicState currentFishingSessionPublicState;

        private Vector2D fishingTargetPosition;

        private double lastX;

        private double lastY;

        private IComponentSpriteRenderer spriteRendererFloat;

        private IComponentSpriteRenderer spriteRendererLine;

        private bool wasFishBiting;

        public ComponentFishingVisualizer() : base(isLateUpdateEnabled: true)
        {
        }

        public ComponentFishingCompletedVisualizer FishingCompletedVisualizer { get; set; }

        public static bool TryGetFor(ICharacter character, out ComponentFishingVisualizer componentFishingVisualizer)
        {
            return ComponentCharacterBinding.TryGetValue(character, out componentFishingVisualizer);
        }

        public override void LateUpdate(double deltaTime)
        {
            this.UpdateFloat(deltaTime);
        }

        public void OnFishingSessionReceived(ILogicObject fishingSession)
        {
            this.currentFishingSession = fishingSession;
            this.currentFishingSessionPublicState = FishingSession.GetPublicState(this.currentFishingSession);
        }

        public void Setup(ICharacter character, Vector2D fishingTargetPosition)
        {
            this.character = character;
            ComponentCharacterBinding[character] = this;
            this.fishingTargetPosition = fishingTargetPosition;

            this.spriteRendererFloat.DrawMode = this.GetDrawModeForFloat();

            this.UpdateFloat(0);

            // hide the sprite renderers until the character fishing rod animation is almost finished
            this.spriteRendererFloat.IsEnabled = false;
            this.spriteRendererLine.IsEnabled = false;

            ClientTimersSystem.AddAction(
                DelayFishingIn,
                () =>
                {
                    if (this.IsDestroyed)
                    {
                        return;
                    }

                    this.spriteRendererFloat.IsEnabled = true;
                    this.spriteRendererLine.IsEnabled = true;

                    // play floater animation once
                    this.CreateFloaterAnimationComponent();
                    this.componentSpriteSheetAnimatorFloater.IsLooped = false;
                });
        }

        protected override void OnDisable()
        {
            this.spriteRendererFloat.Destroy();
            this.spriteRendererLine.Destroy();

            if (ComponentCharacterBinding.TryGetValue(this.character, out var componentFishingVisualizer)
                && ReferenceEquals(this, componentFishingVisualizer))
            {
                ComponentCharacterBinding.Remove(this.character);
            }
        }

        protected override void OnEnable()
        {
            this.spriteRendererFloat = Api.Client.Rendering.CreateSpriteRenderer(
                this.SceneObject,
                TextureResourceFishingFloat,
                drawOrder: DrawOrder.OverDefault,
                scale: 0.667,
                spritePivotPoint: (0.5, 0.5));

            this.spriteRendererLine = Api.Client.Rendering.CreateSpriteRenderer(
                this.SceneObject,
                TextureResourceFishingLine,
                drawOrder: DrawOrder.OverDefault + 1,
                spritePivotPoint: (0, 0.5));
            this.spriteRendererLine.Color = LineColor;
        }

        private Vector2D CalculateFloaterOffset()
        {
            var animationProgress = 0.0;

            if (this.componentSpriteSheetAnimatorFloater != null)
            {
                var framesCount = this.componentSpriteSheetAnimatorFloater.FramesCount;
                var frameIndex = this.componentSpriteSheetAnimatorFloater.FrameIndex;
                animationProgress = frameIndex / (double)(framesCount - 1);

                //Logger.Dev(
                //    $"Animation progress: {animationProgress} frame number: {this.componentSpriteSheetAnimatorFloater.FrameIndex}/{this.componentSpriteSheetAnimatorFloater.FramesCount}");

                // handle reverse direction
                animationProgress *= 2;
                if (animationProgress > 1)
                {
                    animationProgress = 2 - animationProgress;
                }
            }

            var x = MathHelper.Lerp(FloatLineConnectionOffsetUp.X,
                                    FloatLineConnectionOffsetDown.X,
                                    animationProgress);

            var y = MathHelper.Lerp(FloatLineConnectionOffsetUp.Y,
                                    FloatLineConnectionOffsetDown.Y,
                                    animationProgress);

            if (this.spriteRendererFloat.DrawMode == DrawMode.FlipHorizontally)
            {
                x = -x;
            }

            return (x, y);
        }

        private Vector2D CalculateLineStartPosition()
        {
            var clientState = this.character.GetClientState<BaseCharacterClientState>();
            var skeletonRenderer = clientState.SkeletonRenderer;
            if (skeletonRenderer is null)
            {
                return default;
            }

            var protoItemToolFishing = PlayerCharacter.GetPublicState(this.character).SelectedItem?.ProtoItem
                                           as IProtoItemToolFishing;

            var protoSkeleton = clientState.CurrentProtoSkeleton;
            var slotName = protoSkeleton.SlotNameItemInHand;
            var slotOffset = skeletonRenderer.GetSlotScreenOffset(attachmentName: slotName);

            var boneWorldPosition = skeletonRenderer.TransformSlotPosition(
                slotName,
                slotOffset + protoItemToolFishing?.FishingLineStartScreenOffset,
                out _);

            return boneWorldPosition;
        }

        private void CreateFloaterAnimationComponent()
        {
            this.componentSpriteSheetAnimatorFloater?.Destroy();
            this.componentSpriteSheetAnimatorFloater = this.SceneObject
                                                           .AddComponent<ClientComponentSpriteSheetAnimator>();
            var animationFrames = ClientComponentSpriteSheetAnimator.CreateAnimationFrames(
                (ITextureAtlasResource)this.spriteRendererFloat.TextureResource,
                autoInverse: true);

            this.componentSpriteSheetAnimatorFloater.Setup(
                this.spriteRendererFloat,
                animationFrames,
                isLooped: true,
                frameDurationSeconds: 1 / 20.0);
        }

        private DrawMode GetDrawModeForFloat()
        {
            return this.character.Position.X <= this.fishingTargetPosition.X
                       ? DrawMode.Default
                       : DrawMode.FlipHorizontally;
        }

        private void UpdateFloat(double deltaTime)
        {
            if (this.FishingCompletedVisualizer != null)
            {
                this.fishingTargetPosition = this.FishingCompletedVisualizer.SceneObject.Position;
            }

            if (!(this.character.GetPublicState<ICharacterPublicState>()
                      .SelectedItem?.ProtoItem is IProtoItemToolFishing))
            {
                // character switched to another item
                this.Destroy();
                return;
            }

            // assume character position is the scene object position
            var floatPosition = this.fishingTargetPosition - this.character.Position;
            var isFishBiting = this.currentFishingSessionPublicState?.IsFishBiting ?? false;

            if (isFishBiting
                && !this.wasFishBiting)
            {
                // fish started biting, start a floater spritesheet animation
                this.wasFishBiting = true;
                this.CreateFloaterAnimationComponent();
                ClientFishingSoundsHelper.PlaySoundBaiting(this.character);
            }

            // calculate and apply float position movement
            var time = Api.Client.Core.ClientRealTime;
            var newX = FloatMovementAmplitude * Math.Cos(FloatMovementMaxSpeed * time);
            var newY = FloatMovementAmplitude * Math.Sin(FloatMovementMaxSpeed * time);

            newX = MathHelper.LerpWithDeltaTime(this.lastX, newX, deltaTime, rate: 10);
            newY = MathHelper.LerpWithDeltaTime(this.lastY, newY, deltaTime, rate: 10);
            this.lastX = newX;
            this.lastY = newY;

            floatPosition += (newX, newY);
            this.spriteRendererFloat.PositionOffset = floatPosition;

            // update line start-end positions and rotation angle
            var lineStartWorldPosition = this.CalculateLineStartPosition();
            var lineEndWorldPosition = this.character.Position + floatPosition;

            lineEndWorldPosition += this.CalculateFloaterOffset();

            var lineDirection = lineEndWorldPosition - lineStartWorldPosition;
            this.spriteRendererLine.RotationAngleRad = (float)-Math.Atan2(lineDirection.Y, lineDirection.X);
            this.spriteRendererLine.Scale = (8 * lineDirection.Length, 0.5);
            this.spriteRendererLine.PositionOffset = lineStartWorldPosition - this.character.Position;
        }
    }
}