namespace AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Misc
{
    using System;
    using AtomicTorch.CBND.CoreMod.Helpers.Client;
    using AtomicTorch.CBND.CoreMod.Items.Generic;
    using AtomicTorch.CBND.CoreMod.SoundPresets;
    using AtomicTorch.CBND.CoreMod.Systems.Construction;
    using AtomicTorch.CBND.CoreMod.Systems.Creative;
    using AtomicTorch.CBND.CoreMod.Systems.LandClaim;
    using AtomicTorch.CBND.CoreMod.Systems.Physics;
    using AtomicTorch.CBND.CoreMod.Systems.WorldObjectOwners;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Resources;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.ServicesClient.Components;
    using AtomicTorch.GameEngine.Common.Primitives;

    public class ObjectVehicleAssemblyBay : ProtoObjectVehicleAssemblyBay
    {
        private static readonly TextureResource TextureResourceConsole
            = new TextureResource("StaticObjects/Structures/Misc/ObjectVehicleAssemblyBay_Console");

        private static readonly Vector2D TextureResourceConsoleOffset = (93 / 256.0,
                                                                         338 / 256.0);

        private static readonly TextureResource TextureResourcePlatform
            = new TextureResource("StaticObjects/Structures/Misc/ObjectVehicleAssemblyBay_Platform");

        private static readonly Vector2D WorldPositionOffset = (0, 0.2);

        public override string Description =>
            "Automated platform for vehicle assembly. Can be used to manufacture any vehicle, given a particular schematic.";

        public override string Name => "Vehicle assembly bay";

        public override ObjectMaterial ObjectMaterial => ObjectMaterial.Metal;

        public override double ObstacleBlockDamageCoef => 1.0;

        public override Vector2D PlatformCenterWorldOffset => WorldPositionOffset + (1, 0.83);

        public override float StructurePointsMax => 20000;

        protected override BoundsDouble BoundsNoObstaclesTest
            => new BoundsDouble(offset: WorldPositionOffset + (0.2, 0.2),
                                size: (1.6, 1.1));

        public override void ClientSetupBlueprint(Tile tile, IClientBlueprint blueprint)
        {
            var renderer = blueprint.SpriteRenderer;
            renderer.TextureResource = this.Icon;
            renderer.PositionOffset = WorldPositionOffset;
            // set custom size (don't use auto scaling)
            renderer.Scale = null;
            var qualityScaleCoef = Api.Client.Rendering.CalculateCurrentQualityScaleCoefWithOffset(0);
            renderer.Size = (512 * 0.5 / qualityScaleCoef, 
                             572 * 0.5 / qualityScaleCoef);
        }

        public override bool SharedCanInteract(ICharacter character, IStaticWorldObject worldObject, bool writeToLog)
        {
            if (!base.SharedCanInteract(character, worldObject, writeToLog))
            {
                return false;
            }

            if (LandClaimSystem.SharedIsObjectInsideOwnedOrFreeArea(worldObject, character)
                || CreativeModeSystem.SharedIsInCreativeMode(character))
            {
                return true;
            }

            if (IsClient && writeToLog)
            {
                WorldObjectOwnersSystem.ClientOnCannotInteractNotOwner(worldObject);
            }

            return false;
        }

        public override Vector2D SharedGetObjectCenterWorldOffset(IWorldObject worldObject)
            => WorldPositionOffset + (1, 1.5);

        protected override ITextureResource ClientCreateIcon()
        {
            // size and all offsets are set in pixels
            return ClientProceduralTextureHelper.CreateComposedTexture(
                "Composed " + this.Id,
                isTransparent: true,
                isUseCache: true,
                customSize: (512, 572),
                textureResourcesWithOffsets: new[]
                {
                    new TextureResourceWithOffset(TextureResourcePlatform,
                                                  offset: (0, -180)),
                    new TextureResourceWithOffset(TextureResourceConsole,
                                                  offset: (TextureResourceConsoleOffset.X * 256, 0))
                });
        }

        protected override void ClientInitialize(ClientInitializeData data)
        {
            base.ClientInitialize(data);

            var platformRenderer = data.ClientState.Renderer;
            platformRenderer.DrawOrder = DrawOrder.Floor + 1;
            platformRenderer.PositionOffset = WorldPositionOffset;

            var consoleRenderer = Client.Rendering.CreateSpriteRenderer(
                data.GameObject,
                TextureResourceConsole);
            consoleRenderer.PositionOffset = WorldPositionOffset + TextureResourceConsoleOffset;
            consoleRenderer.DrawOrderOffsetY = 0.1;
        }

        protected override void CreateLayout(StaticObjectLayout layout)
        {
            layout.Setup("##",
                         "##");
        }

        protected override void PrepareConstructionConfig(
            ConstructionTileRequirements tileRequirements,
            ConstructionStageConfig build,
            ConstructionStageConfig repair,
            ConstructionUpgradeConfig upgrade,
            out ProtoStructureCategory category)
        {
            category = GetCategory<StructureCategoryOther>();

            build.StagesCount = 10;
            build.StageDurationSeconds = BuildDuration.Short;
            build.AddStageRequiredItem<ItemCement>(count: 10);
            build.AddStageRequiredItem<ItemIngotSteel>(count: 5);
            build.AddStageRequiredItem<ItemComponentsMechanical>(count: 1);
            build.AddStageRequiredItem<ItemComponentsElectronic>(count: 1);
            
            repair.StagesCount = 10;
            repair.StageDurationSeconds = BuildDuration.Short;
            repair.AddStageRequiredItem<ItemIngotSteel>(count: 4);
            repair.AddStageRequiredItem<ItemComponentsMechanical>(count: 1);
            repair.AddStageRequiredItem<ItemComponentsElectronic>(count: 1);
        }

        protected override ITextureResource PrepareDefaultTexture(Type thisType)
        {
            return TextureResourcePlatform;
        }

        protected override void SharedCreatePhysics(CreatePhysicsData data)
        {
            data.PhysicsBody
                // only console has physical collider and hitboxes
                .AddShapeRectangle(size: (1.1, 0.2),
                                   offset: WorldPositionOffset + (0.45, 1.4))
                .AddShapeRectangle(size: (0.9, 0.2),
                                   offset: WorldPositionOffset + (0.55, 1.95),
                                   group: CollisionGroups.HitboxMelee)
                .AddShapeRectangle(size: (0.7, 0.15),
                                   offset: WorldPositionOffset + (0.65, 2.1),
                                   group: CollisionGroups.HitboxRanged)
                // console click area
                .AddShapeRectangle(size: (1, 0.75),
                                   offset: WorldPositionOffset + (0.5, 1.5),
                                   group: CollisionGroups.ClickArea);
        }
    }
}