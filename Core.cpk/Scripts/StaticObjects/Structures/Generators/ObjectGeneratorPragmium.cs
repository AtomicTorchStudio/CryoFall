namespace AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Generators
{
    using System;
    using AtomicTorch.CBND.CoreMod.ClientComponents.Rendering;
    using AtomicTorch.CBND.CoreMod.ClientComponents.Rendering.Lighting;
    using AtomicTorch.CBND.CoreMod.Items.Generic;
    using AtomicTorch.CBND.CoreMod.SoundPresets;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Explosives;
    using AtomicTorch.CBND.CoreMod.Systems;
    using AtomicTorch.CBND.CoreMod.Systems.Construction;
    using AtomicTorch.CBND.CoreMod.Systems.LandClaim;
    using AtomicTorch.CBND.CoreMod.Systems.Physics;
    using AtomicTorch.CBND.CoreMod.Systems.PowerGridSystem;
    using AtomicTorch.CBND.CoreMod.Systems.PvE;
    using AtomicTorch.CBND.CoreMod.Systems.Weapons;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.State;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Resources;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.ServicesClient.Components;
    using AtomicTorch.GameEngine.Common.Primitives;

    public class ObjectGeneratorPragmium : ProtoObjectGeneratorPragmium
    {
        private const double DrawOrderOffsetY = 1.8;

        private const double ReactorRendererActiveAnimationLigthSourceSize = 8;

        private static readonly Vector2D ReactorRendererActiveAnimationPositionOffset
            = new(23 / 256.0, 137 / 256.0);

        private static readonly Vector2D ReactorRendererActiveAnimationLightSourcePositionOffset
            = ReactorRendererActiveAnimationPositionOffset + (0.77, 0.18);

        private ITextureResource[] spriteSheetReactorActive;

        private ITextureResource textureResourceReactor;

        public override ElectricityThresholdsPreset DefaultGenerationElectricityThresholds
            => new(startupPercent: 99,
                   shutdownPercent: 100);

        public override string Description =>
            "A large pragmium power plant can serve as the main and only power source for a massive base, but requires a steady supply of pragmium.";

        public override bool HasIncreasedScopeSize => true;

        public override byte ItemSlotsCountPerReactor => 3 * 3;

        public override string Name => "Pragmium power plant";

        public override ObjectMaterial ObjectMaterial => ObjectMaterial.Metal;

        public override double ObstacleBlockDamageCoef => 1;

        public override byte ReactorsCountInitial => 1;

        public override byte ReactorsCountMax => 2;

        public override double ShutdownDuration => 30 * 60; // 30 minutes

        public override double StartupDuration => 60 * 60; // 1 hour

        public override float StructurePointsMax => 15000;

        protected override double PsiEmissionDistanceMultiplier => 0.75;

        protected override double PsiEmissionDistanceOffset => 0.15;

        public override Vector2D SharedGetObjectCenterWorldOffset(IWorldObject worldObject)
        {
            return (3.0, 2.5);
        }

        protected override void ClientSetupReactorSpriteRenderer(
            IStaticWorldObject worldObject,
            IComponentSpriteRenderer reactorSpriteRenderer,
            ObjectGeneratorPragmiumReactorPublicState reactorState,
            int index)
        {
            reactorSpriteRenderer.TextureResource = this.textureResourceReactor;
            reactorSpriteRenderer.PositionOffset = index switch
            {
                0 => new Vector2D(179, 251) / 256.0,
                1 => new Vector2D(932, 251) / 256.0,
                _ => throw new ArgumentOutOfRangeException()
            };

            reactorSpriteRenderer.DrawOrderOffsetY += DrawOrderOffsetY;
            reactorSpriteRenderer.DrawOrderOffsetY -= reactorSpriteRenderer.PositionOffset.Y;

            // add animation for active reactor state
            var sceneObject = worldObject.ClientSceneObject;
            var rendererAnimation = Client.Rendering.CreateSpriteRenderer(sceneObject);
            rendererAnimation.PositionOffset = reactorSpriteRenderer.PositionOffset
                                               + ReactorRendererActiveAnimationPositionOffset;
            rendererAnimation.DrawOrderOffsetY += DrawOrderOffsetY;
            rendererAnimation.DrawOrderOffsetY -= rendererAnimation.PositionOffset.Y;

            var spriteSheetAnimator = sceneObject.AddComponent<ClientComponentSpriteSheetAnimator>();
            spriteSheetAnimator.Setup(rendererAnimation,
                                      this.spriteSheetReactorActive,
                                      isLooped: true,
                                      frameDurationSeconds: 2 / 60.0,
                                      initialFrameOffset: 0);

            var lightSource = ClientLighting.CreateLightSourceSpot(
                worldObject.ClientSceneObject,
                color: LightColors.PragmiumLuminescenceSource,
                size: ReactorRendererActiveAnimationLigthSourceSize,
                spritePivotPoint: (0.5, 0.5),
                positionOffset: reactorSpriteRenderer.PositionOffset
                                + ReactorRendererActiveAnimationLightSourcePositionOffset);

            reactorState.ClientSubscribe(_ => _.ActivationProgressPercents,
                                         _ => RefreshActiveState(),
                                         GetClientState(worldObject));

            RefreshActiveState();

            void RefreshActiveState()
            {
                var isEnabled = reactorState.ActivationProgressPercents > 0;
                lightSource.IsEnabled = isEnabled;
                spriteSheetAnimator.IsEnabled = isEnabled;

                if (!isEnabled)
                {
                    rendererAnimation.TextureResource = this.spriteSheetReactorActive[0];
                    spriteSheetAnimator.Reset();
                }
            }
        }

        protected override void ClientSetupRenderer(IComponentSpriteRenderer renderer)
        {
            base.ClientSetupRenderer(renderer);
            renderer.DrawOrderOffsetY += DrawOrderOffsetY;
        }

        protected override void CreateLayout(StaticObjectLayout layout)
        {
            layout.Setup("######",
                         "######",
                         "######");
        }

        protected override void PrepareConstructionConfigGenerator(
            ConstructionTileRequirements tileRequirements,
            ConstructionStageConfig build,
            ConstructionStageConfig repair,
            InputItems buildAdditionalReactor,
            out ProtoStructureCategory category)
        {
            tileRequirements.Add(LandClaimSystem.ValidatorIsOwnedLand);
            category = GetCategory<StructureCategoryElectricity>();

            build.StagesCount = 10;
            build.StageDurationSeconds = BuildDuration.VeryLong;
            build.AddStageRequiredItem<ItemIngotSteel>(count: 5);
            build.AddStageRequiredItem<ItemComponentsElectronic>(count: 5);
            build.AddStageRequiredItem<ItemComponentsHighTech>(count: 2);

            repair.StagesCount = 10;
            repair.StageDurationSeconds = BuildDuration.VeryLong;
            repair.AddStageRequiredItem<ItemIngotSteel>(count: 4);
            repair.AddStageRequiredItem<ItemComponentsElectronic>(count: 2);

            buildAdditionalReactor.Add<ItemIngotSteel>(count: 25)
                                  .Add<ItemIngotLithium>(count: 10)
                                  .Add<ItemComponentsHighTech>(count: 10);
        }

        protected override ITextureResource PrepareDefaultTexture(Type thisType)
        {
            var filePathReactorTexture = GenerateTexturePath(thisType) + "Reactor";
            this.textureResourceReactor = new TextureResource(filePathReactorTexture);

            this.spriteSheetReactorActive
                = ClientComponentSpriteSheetAnimator.CreateAnimationFrames(
                    new TextureAtlasResource(filePathReactorTexture + "Active",
                                             columns: 8,
                                             rows: 2,
                                             isTransparent: true));

            return base.PrepareDefaultTexture(thisType);
        }

        protected override void PrepareDefense(DefenseDescription defense)
        {
            defense.Set(ObjectDefensePresets.Tier1);
        }

        protected override void ServerOnStaticObjectZeroStructurePoints(
            WeaponFinalCache weaponCache,
            ICharacter byCharacter,
            IWorldObject targetObject)
        {
            base.ServerOnStaticObjectZeroStructurePoints(weaponCache, byCharacter, targetObject);

            if (weaponCache is null
                || (weaponCache.ProtoWeapon is null
                    && weaponCache.ProtoExplosive is null)
                || PveSystem.ServerIsPvE)
            {
                return;
            }

            // the damage was dealt by a weapon or explosive - try to explode the reactor
            var reactors = GetPrivateState((IStaticWorldObject)targetObject).ReactorStates;

            for (var reactorIndex = 0; reactorIndex < reactors.Length; reactorIndex++)
            {
                var reactor = reactors[reactorIndex];
                if (reactor is null)
                {
                    continue;
                }

                var isActiveReactor = reactor.ActivationProgressPercents > 0;
                var protoObjectExplosion
                    = isActiveReactor
                          ? (IProtoStaticWorldObject)Api.GetProtoEntity<ObjectGeneratorPragmiumReactorActiveExplosion>()
                          : Api.GetProtoEntity<ObjectGeneratorPragmiumReactorInactiveExplosion>();

                var explosionPosition = targetObject.TilePosition.ToVector2D()
                                        + this.SharedGetReactorWorldPositionOffset(reactorIndex)
                                        - protoObjectExplosion.Layout.Center;

                Server.World.CreateStaticWorldObject(
                    protoObjectExplosion,
                    tilePosition: ((ushort)Math.Round(explosionPosition.X, MidpointRounding.AwayFromZero),
                                   ((ushort)Math.Round(explosionPosition.Y, MidpointRounding.AwayFromZero))));
            }
        }

        protected override void SharedCreatePhysics(CreatePhysicsData data)
        {
            data.PhysicsBody
                // default
                .AddShapeCircle(radius: 0.85, center: (0.95, 1.15))
                .AddShapeRectangle(size: (4.0, 1.7), offset: (1.0, 0.3))
                .AddShapeCircle(radius: 0.85, center: (5.05, 1.15))
                .AddShapeRectangle(size: (2.8, 0.4), offset: (1.8, 2.0))

                // hit
                .AddShapeRectangle(size: (5.0, 1.1), offset: (0.6, 1.1), group: CollisionGroups.HitboxMelee)
                .AddShapeRectangle(size: (5.0, 1.1), offset: (0.6, 1.1), group: CollisionGroups.HitboxRanged)

                // click
                .AddShapeRectangle(size: (5.2, 2.0), offset: (0.6, 0.5), group: CollisionGroups.ClickArea);
        }

        protected override Vector2D SharedGetReactorWorldPositionOffset(int reactorIndex)
        {
            return reactorIndex switch
            {
                0 => (1.5, 1.15),
                _ => (4.5, 1.15)
            };
        }
    }
}