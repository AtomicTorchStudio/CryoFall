namespace AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Misc
{
    using System;
    using System.Linq;
    using AtomicTorch.CBND.CoreMod.ClientComponents.Rendering;
    using AtomicTorch.CBND.CoreMod.Items.Generic;
    using AtomicTorch.CBND.CoreMod.Objects;
    using AtomicTorch.CBND.CoreMod.SoundPresets;
    using AtomicTorch.CBND.CoreMod.Systems.Construction;
    using AtomicTorch.CBND.CoreMod.Systems.LandClaimShield;
    using AtomicTorch.CBND.CoreMod.Systems.Physics;
    using AtomicTorch.CBND.CoreMod.Systems.PowerGridSystem;
    using AtomicTorch.CBND.GameApi.Data.State;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Resources;
    using AtomicTorch.CBND.GameApi.ServicesClient.Components;

    public class ObjectPsionicFieldGenerator
        : ProtoObjectStructure
          <ObjectPsionicFieldGenerator.PrivateState,
              StaticObjectElectricityConsumerPublicState,
              StaticObjectClientState>,
          IProtoObjectPsiSource,
          IProtoObjectElectricityConsumerWithCustomRate
    {
        private static readonly SoundResource SoundResourceActive
            = new("Objects/Structures/ObjectPsionicFieldGenerator/Active");

        private ITextureAtlasResource textureAtlasActive;

        public virtual ElectricityThresholdsPreset DefaultConsumerElectricityThresholds
            => new(startupPercent: 1,
                   shutdownPercent: 0);

        public override string Description =>
            "Defensive structure that projects psionic field of high intensity while active.";

        public double ElectricityConsumptionPerSecondWhenActive => 1;

        public override string InteractionTooltipText => InteractionTooltipTexts.TogglePower;

        public override bool IsRelocatable => false;

        public override string Name => "Psionic field generator";

        public override ObjectMaterial ObjectMaterial => ObjectMaterial.Metal;

        public override double ObstacleBlockDamageCoef => 1;

        public double PsiIntensity => 1.0;

        public double PsiRadiusMax => 8;

        public double PsiRadiusMin => 4;

        public override float StructurePointsMax => 2000;

        public bool ServerIsPsiSourceActive(IWorldObject worldObject)
        {
            var staticWorldObject = (IStaticWorldObject)worldObject;
            var publicState = GetPublicState(staticWorldObject);
            return publicState.ElectricityConsumerState == ElectricityConsumerState.PowerOnActive
                   && this.SharedGetCurrentElectricityConsumptionRate(staticWorldObject) > 0;
        }

        public double SharedGetCurrentElectricityConsumptionRate(IStaticWorldObject worldObject)
        {
            return LandClaimShieldProtectionSystem.SharedIsUnderShieldProtection(worldObject)
                       ? 0
                       : 1;
        }

        IObjectElectricityStructurePrivateState IProtoObjectElectricityConsumer.GetPrivateState(
            IStaticWorldObject worldObject)
        {
            return GetPrivateState(worldObject);
        }

        IObjectElectricityConsumerPublicState IProtoObjectElectricityConsumer.GetPublicState(
            IStaticWorldObject worldObject)
        {
            return GetPublicState(worldObject);
        }

        protected override void ClientInitialize(ClientInitializeData data)
        {
            base.ClientInitialize(data);

            var worldObject = data.GameObject;
            var publicState = data.PublicState;
            var renderer = data.ClientState.Renderer;
            var sceneObject = worldObject.ClientSceneObject;

            PowerGridSystem.ClientInitializeConsumerOrProducer(worldObject);
            var soundEmitter = Client.Audio.CreateSoundEmitter(
                worldObject,
                SoundResourceActive,
                is3D: true,
                volume: 1.0f,
                isLooped: true);
            soundEmitter.CustomMinDistance = 3.0f;
            soundEmitter.CustomMaxDistance = 6.0f;

            // add sprite sheet animation for fan sprite
            var componentAnimator = sceneObject.AddComponent<ClientComponentSpriteSheetAnimator>();
            componentAnimator.Setup(renderer,
                                    ClientComponentSpriteSheetAnimator.CreateAnimationFrames(this.textureAtlasActive)
                                                                      .Skip(1)
                                                                      .ToArray(),
                                    isLooped: true,
                                    frameDurationSeconds: 6 / 60.0,
                                    randomizeInitialFrame: true);

            publicState.ClientSubscribe(
                _ => _.ElectricityConsumerState,
                _ => RefreshActiveState(),
                data.ClientState);

            RefreshActiveState();

            void RefreshActiveState()
            {
                var isActive = publicState.ElectricityConsumerState == ElectricityConsumerState.PowerOnActive;
                componentAnimator.IsEnabled = isActive;
                soundEmitter.IsEnabled = isActive;

                if (!isActive)
                {
                    // reset to default texture
                    renderer.TextureResource = this.DefaultTexture;
                }
            }
        }

        protected override async void ClientInteractFinish(ClientObjectData data)
        {
            base.ClientInteractFinish(data);

            // toggle power
            await PowerGridSystem.ClientSetPowerMode(
                isOn: data.PublicState.ElectricityConsumerState
                      != ElectricityConsumerState.PowerOnActive,
                data.GameObject);
        }

        protected override void ClientSetupRenderer(IComponentSpriteRenderer renderer)
        {
            base.ClientSetupRenderer(renderer);
            renderer.DrawOrderOffsetY = 0.33;
        }

        protected override void PrepareConstructionConfig(
            ConstructionTileRequirements tileRequirements,
            ConstructionStageConfig build,
            ConstructionStageConfig repair,
            ConstructionUpgradeConfig upgrade,
            out ProtoStructureCategory category)
        {
            category = GetCategory<StructureCategoryElectricity>();

            build.StagesCount = 10;
            build.StageDurationSeconds = BuildDuration.Medium;
            build.AddStageRequiredItem<ItemIngotSteel>(count: 2);
            build.AddStageRequiredItem<ItemOrePragmium>(count: 5);
            build.AddStageRequiredItem<ItemPlastic>(count: 2);
            build.AddStageRequiredItem<ItemComponentsHighTech>(count: 1);

            repair.StagesCount = 10;
            repair.StageDurationSeconds = BuildDuration.Medium;
            repair.AddStageRequiredItem<ItemIngotSteel>(count: 2);
            repair.AddStageRequiredItem<ItemOrePragmium>(count: 2);
            repair.AddStageRequiredItem<ItemPlastic>(count: 1);
        }

        protected override ITextureResource PrepareDefaultTexture(Type thisType)
        {
            this.textureAtlasActive = new TextureAtlasResource(
                GenerateTexturePath(thisType),
                columns: 5,
                rows: 1,
                isTransparent: true);

            return this.textureAtlasActive;
        }

        protected override void SharedCreatePhysics(CreatePhysicsData data)
        {
            data.PhysicsBody
                .AddShapeRectangle(size: (0.9, 0.6),  offset: (0.05, 0.15))
                .AddShapeRectangle(size: (0.8, 0.5),  offset: (0.1, 0.75),  group: CollisionGroups.HitboxMelee)
                .AddShapeRectangle(size: (0.8, 0.25), offset: (0.1, 0.95),  group: CollisionGroups.HitboxRanged)
                .AddShapeRectangle(size: (0.9, 1.17), offset: (0.05, 0.15), group: CollisionGroups.ClickArea);
        }

        public class PrivateState : StructurePrivateState, IObjectElectricityStructurePrivateState
        {
            [SyncToClient]
            public ElectricityThresholdsPreset ElectricityThresholds { get; set; }

            [SyncToClient]
            [TempOnly]
            public byte PowerGridChargePercent { get; set; }
        }
    }
}