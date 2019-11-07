namespace AtomicTorch.CBND.CoreMod.StaticObjects.Vegetation.Trees
{
    using AtomicTorch.CBND.CoreMod.Characters;
    using AtomicTorch.CBND.CoreMod.Helpers.Client;
    using AtomicTorch.CBND.CoreMod.Items.Tools;
    using AtomicTorch.CBND.CoreMod.Items.Weapons.Melee;
    using AtomicTorch.CBND.CoreMod.SoundPresets;
    using AtomicTorch.CBND.CoreMod.Stats;
    using AtomicTorch.CBND.CoreMod.Systems.Notifications;
    using AtomicTorch.CBND.CoreMod.Systems.Physics;
    using AtomicTorch.CBND.CoreMod.Systems.PvE;
    using AtomicTorch.CBND.CoreMod.Systems.Weapons;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.ServicesClient.Components;
    using AtomicTorch.GameEngine.Common.Helpers;
    using AtomicTorch.GameEngine.Common.Primitives;

    public abstract class ProtoObjectTree<TPrivateState, TPublicState, TClientState>
        : ProtoObjectVegetation
          <TPrivateState,
              TPublicState,
              TClientState>,
          IProtoObjectTree
        where TPrivateState : VegetationPrivateState, new()
        where TPublicState : VegetationPublicState, new()
        where TClientState : VegetationClientState, new()
    {
        public const string NotificationUseAxe = "Use an axe to cut this tree.";

        public override ObjectSoundMaterial ObjectSoundMaterial => ObjectSoundMaterial.Wood;

        public override double ObstacleBlockDamageCoef => 0.5;

        public override double ServerUpdateIntervalSeconds => 60.0;

        public override float StructurePointsMax => 600;

        public override BoundsInt ViewBoundsExpansion => new BoundsInt(minX: -1,
                                                                       minY: 0,
                                                                       maxX: 1,
                                                                       maxY: 4);

        protected override void ClientAddShadowRenderer(ClientInitializeData data)
        {
            // no shadow
        }

        protected virtual void ClientApplyTreeRandomScale(
            IStaticWorldObject worldObject,
            IComponentSpriteRenderer renderer)
        {
            // if this is not a blueprint, apply random scale (depending on the tree position and seed)
            const int maxDifferencePercents = 30;

            var scaleCoef = PositionalRandom.Get(worldObject.TilePosition,
                                                 minInclusive: 100 - maxDifferencePercents / 2,
                                                 maxExclusive: 100 + maxDifferencePercents / 2,
                                                 seed: (uint)(worldObject.Id + this.ShortId.GetHashCode()))
                            / 100.0;

            renderer.Scale *= scaleCoef;
            renderer.DrawOrderOffsetY *= scaleCoef;
        }

        protected override void ClientInitialize(ClientInitializeData data)
        {
            base.ClientInitialize(data);
            var renderer = data.ClientState.Renderer;

            this.ClientApplyTreeRandomScale(data.GameObject, data.ClientState.Renderer);

            if (data.ClientState.RendererShadow != null)
            {
                data.ClientState.RendererShadow.DrawOrderOffsetY = renderer.DrawOrderOffsetY;
            }

            if (data.ClientState.RendererOcclusion != null)
            {
                data.ClientState.RendererOcclusion.DrawOrderOffsetY = renderer.DrawOrderOffsetY;
            }
        }

        protected override void ClientOnObjectDestroyed(Vector2D position)
        {
            // play custom sound
            var emitter = Client.Audio.PlayOneShot(
                MaterialDestroySoundPresets.TreeDestroy.GetSound(repetitionProtectionKey: this),
                worldPosition: position + this.Layout.Center,
                volume: SoundConstants.VolumeDestroy,
                pitch: RandomHelper.Range(0.95f, 1.05f));

            var distance = MaterialDestroySoundPresets.Default.CustomDistance;
            emitter.CustomMinDistance = distance?.min;
            emitter.CustomMaxDistance = distance?.max;
        }

        protected override void ClientSetupRenderer(IComponentSpriteRenderer renderer)
        {
            base.ClientSetupRenderer(renderer);

            renderer.Scale = 1.6f;
            renderer.DrawOrderOffsetY = 0.4;
        }

        protected override double SharedCalculateDamageByWeapon(
            WeaponFinalCache weaponCache,
            double damagePreMultiplier,
            IStaticWorldObject targetObject,
            out double obstacleBlockDamageCoef)
        {
            obstacleBlockDamageCoef = 1;
            if (!PveSystem.SharedIsAllowStructureDamage(weaponCache.Character,
                                                        targetObject,
                                                        showClientNotification: false))
            {
                return 0;
            }

            if (weaponCache.ProtoWeapon is IProtoItemToolWoodcutting protoItemToolWoodCutting)
            {
                // get damage multiplier ("woodcutting speed")
                var damageMultiplier = weaponCache.Character
                                                  .SharedGetFinalStatMultiplier(StatName.WoodcuttingSpeed);

                return protoItemToolWoodCutting.DamageToTree
                       * damageMultiplier
                       * ToolsConstants.ActionWoodcuttingSpeedMultiplier;
            }

            if (weaponCache.ProtoWeapon is ItemNoWeapon)
            {
                // no damage with hands
                if (IsClient)
                {
                    NotificationSystem.ClientShowNotification(NotificationUseAxe,
                                                              icon: this.Icon);
                }

                return 0;
            }

            // not a wood-cutting tool - call default damage apply method
            return base.SharedCalculateDamageByWeapon(weaponCache,
                                                      damagePreMultiplier,
                                                      targetObject,
                                                      out obstacleBlockDamageCoef);
        }

        protected override void SharedCreatePhysics(CreatePhysicsData data)
        {
            data.PhysicsBody
                .AddShapeCircle(radius: 0.25, center: (0.5, 0.35))
                .AddShapeRectangle(size: (0.75, 1),   offset: (0.125, 0.1), group: CollisionGroups.HitboxMelee)
                .AddShapeRectangle(size: (0.4, 0.35), offset: (0.3, 0.9),   group: CollisionGroups.HitboxRanged);
        }
    }

    public abstract class ProtoObjectTree
        : ProtoObjectTree
            <VegetationPrivateState,
                VegetationPublicState,
                VegetationClientState>
    {
    }
}