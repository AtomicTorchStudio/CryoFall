namespace AtomicTorch.CBND.CoreMod.CharacterSkeletons
{
    using System;
    using System.Threading.Tasks;
    using System.Windows.Media;
    using AtomicTorch.CBND.CoreMod.Characters;
    using AtomicTorch.CBND.CoreMod.CharacterSkeletons.Mech;
    using AtomicTorch.CBND.CoreMod.Helpers.Primitives;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.Physics;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Resources;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.ServicesClient.Components;
    using AtomicTorch.GameEngine.Common.Primitives;

    public abstract class ProtoSkeletonTurret : ProtoCharacterSkeleton
    {
        private readonly Lazy<ITextureResource> lazyIcon;

        private ITextureResource textureResourceBarrel;

        private ITextureResource textureResourceBottom;

        private ITextureResource textureResourceTop;

        protected ProtoSkeletonTurret()
        {
            this.lazyIcon = new Lazy<ITextureResource>(
                () => new ProceduralTexture(
                    "Turret icon " + this.ShortId,
                    request => this.GenerateIcon(request),
                    isTransparent: true,
                    isUseCache: true));
        }

        public override string DefaultAnimationName => "animation";

        public override double DefaultMoveSpeed => 0.3;

        public abstract double DrawWorldPositionOffsetY { get; }

        public sealed override bool HasMoveStartAnimations => false;

        public ITextureResource Icon => this.lazyIcon.Value;

        public virtual Vector2D IconOffset => Vector2D.Zero;

        public virtual double IconScale => 0.5;

        public override float OrientationDownExtraAngle => 0;

        public override float OrientationThresholdDownHorizontalFlipDeg => 0;

        public override float OrientationThresholdDownToUpFlipDeg => 0;

        public override float OrientationThresholdUpHorizontalFlipDeg => 0;

        public override SkeletonResource SkeletonResourceBack { get; }
            = null;

        public override SkeletonResource SkeletonResourceFront { get; }
            = new("Turret/Turret");

        public override string SlotNameItemInHand => "TurretBarrel1";

        public override SoundResource SoundResourceAimingProcess { get; }
            = new("Objects/Vehicles/Mech/Aiming");

        public override double WorldScale => 0.5;

        protected override string SoundsFolderPath => "Skeletons/Turret";

        protected override double VolumeFootsteps => 0.5;

        public override IComponentSpriteRenderer ClientCreateShadowRenderer(
            IWorldObject worldObject,
            double scaleMultiplier)
        {
            return null;
        }

        public override void ClientGetAimingOrientation(
            ICharacter character,
            double angleRad,
            ViewOrientation lastViewOrientation,
            out ViewOrientation viewOrientation,
            out float aimCoef)
        {
            viewOrientation = default;
            aimCoef = (float)((MathConstants.DoublePI - angleRad) / MathConstants.DoublePI);

            //aimCoef = MathHelper.Clamp(aimCoef, 0, 1);
            if (aimCoef < 0)
            {
                aimCoef += 1;
            }
            else if (aimCoef > 1)
            {
                aimCoef -= 1;
            }

            //Logger.Dev($"Angle deg: {(angleRad * MathConstants.RadToDeg):F3} | {aimCoef:F3}");
        }

        public override void ClientSetupShadowRenderer(IComponentSpriteRenderer shadowRenderer, double scaleMultiplier)
        {
        }

        public override void CreatePhysics(IPhysicsBody physicsBody)
        {
            // no physics (the linked structure has a physical body with colliders and click area)
        }

        public async Task<ITextureResource> GenerateIcon(
            ProceduralTextureRequest request,
            ushort textureWidth = 384,
            ushort textureHeight = 384,
            sbyte spriteQualityOffset = 0)
        {
            var protoSkeleton = this;

            var scale = protoSkeleton.IconScale * textureWidth / 256.0;
            var renderingTag = request.TextureName;

            var renderTarget = Api.Client.Rendering.CreateRenderTexture(renderingTag, textureWidth, textureHeight);
            var cameraObject = Api.Client.Scene.CreateSceneObject(renderingTag);
            var camera = Api.Client.Rendering.CreateCamera(cameraObject,
                                                           renderingTag: renderingTag,
                                                           drawOrder: -100);

            camera.RenderTarget = renderTarget;
            camera.ClearColor = Color.FromArgb(0, 0, 0, 0);
            camera.SetOrthographicProjection(textureWidth, textureHeight);

            var currentSkeleton = ClientCharacterEquipmentHelper.CreateCharacterSkeleton(
                cameraObject,
                protoSkeleton,
                worldScale: scale,
                spriteQualityOffset: spriteQualityOffset);
            currentSkeleton.PositionOffset = (textureWidth / 2.0 + this.IconOffset.X * scale,
                                              -textureHeight * 0.7 + this.IconOffset.Y * scale);
            currentSkeleton.RenderingTag = renderingTag;
            currentSkeleton.SetAnimationFrame(0, this.DefaultAnimationName, timePositionFraction: 0);

            await camera.DrawAsync();
            cameraObject.Destroy();

            request.ThrowIfCancelled();

            var generatedTexture = await renderTarget.SaveToTexture(
                                       isTransparent: true,
                                       qualityScaleCoef:
                                       Api.Client.Rendering.CalculateCurrentQualityScaleCoefWithOffset(
                                           spriteQualityOffset));

            currentSkeleton.Destroy();
            renderTarget.Dispose();
            request.ThrowIfCancelled();
            return generatedTexture;
        }

        public override void OnSkeletonCreated(IComponentSkeleton skeleton)
        {
            base.OnSkeletonCreated(skeleton);

            if (skeleton.SceneObject.AttachedWorldObject is ICharacter characterTurret)
            {
                // reuse the mech aiming sound component for turret rotation sound 
                skeleton.SceneObject
                        .AddComponent<ComponentSkeletonMechAimingSoundManager>()
                        .Setup(skeleton, characterTurret, this.SoundResourceAimingProcess);
            }

            skeleton.PositionOffset += (0, this.DrawWorldPositionOffsetY);
            skeleton.DrawOrderOffsetY -= this.DrawWorldPositionOffsetY;

            skeleton.SetAttachmentSprite("TurretTop1",    "TurretTop1",    this.textureResourceTop);
            skeleton.SetAttachmentSprite("TurretBottom1", "TurretBottom1", this.textureResourceBottom);
            skeleton.SetAttachmentSprite("TurretBarrel1", "TurretBarrel1", this.textureResourceBarrel);
        }

        protected override void PrepareProtoCharacterSkeleton()
        {
            base.PrepareProtoCharacterSkeleton();

            var path = "StaticObjects/Structures/Defenses/Object" + this.GetType().Name.Replace("Skeleton", "");
            this.textureResourceTop = new TextureResource(path + "Top");
            this.textureResourceBottom = new TextureResource(path + "Bottom");
            this.textureResourceBarrel = new TextureResource(path + "Barrel");
        }
    }
}