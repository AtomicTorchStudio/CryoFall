namespace AtomicTorch.CBND.CoreMod.StaticObjects.Explosives
{
    using System;
    using System.Windows;
    using System.Windows.Media;
    using AtomicTorch.CBND.CoreMod.CharacterStatusEffects;
    using AtomicTorch.CBND.CoreMod.CharacterStatusEffects.Debuffs;
    using AtomicTorch.CBND.CoreMod.Items.Weapons.MobWeapons;
    using AtomicTorch.CBND.CoreMod.Systems.Weapons;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.WorldObjects.Special;
    using AtomicTorch.CBND.CoreMod.Vehicles;
    using AtomicTorch.CBND.GameApi;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.Physics;
    using AtomicTorch.CBND.GameApi.Data.Weapons;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Resources;
    using AtomicTorch.CBND.GameApi.ServicesClient.Components;
    using AtomicTorch.GameEngine.Common.Primitives;

    public class ObjectMobSandTyrantMissileExplosion : ProtoObjectExplosive
    {
        /// <summary>
        /// In order to balance the boss against mechs, apply an additional damage to them.
        /// </summary>
        private const double VehicleAdditionalDamageMultiplier = 1.0; // Apply +100% damage to vehicles

        public override bool AllowNpcToNpcDamage => true;

        public override double DamageRadius => 2.333;

        public override ITextureResource DefaultTexture
            => this.ExplosionPreset.SpriteAtlasResources[0].Chunk(6, 0);

        public override TimeSpan ExplosionDelay => TimeSpan.FromSeconds(2);

        public override bool IsActivatesRaidBlock => false;

        public override bool IsDamageThroughObstacles => true;

        public override StaticObjectKind Kind => StaticObjectKind.SpecialAllowDecals;

        [NotLocalizable]
        public override string Name => "Sand Tyrant Missile";

        public override void ServerExecuteExplosion(
            Vector2D positionEpicenter,
            IPhysicsSpace physicsSpace,
            WeaponFinalCache weaponFinalCache)
        {
            WeaponExplosionSystem.ServerProcessExplosionCircle(
                positionEpicenter: positionEpicenter,
                physicsSpace: physicsSpace,
                damageDistanceMax: this.DamageRadius,
                weaponFinalCache: weaponFinalCache,
                damageOnlyDynamicObjects: true,
                isDamageThroughObstacles: this.IsDamageThroughObstacles,
                callbackCalculateDamageCoefByDistanceForStaticObjects:
                this.ServerCalculateDamageCoefByDistanceForStaticObjects,
                callbackCalculateDamageCoefByDistanceForDynamicObjects: this
                    .ServerCalculateDamageCoefByDistanceForDynamicObjects,
                // Missiles are falling from the sky and the explosion circles are clearly designated.
                // Players expecting that they will be not damaged when they stand outside the circles. 
                collisionGroups: new[] { CollisionGroup.Default });
        }

        public override void ServerOnObjectHitByExplosion(
            IWorldObject worldObject,
            double damage,
            WeaponFinalCache weaponCache)
        {
            base.ServerOnObjectHitByExplosion(worldObject, damage, weaponCache);

            if (worldObject is ICharacter character)
            {
                if (damage >= 5)
                {
                    character.ServerAddStatusEffect<StatusEffectDazed>(intensity: 0.5);
                }
            }
            else if (worldObject is IDynamicWorldObject dynamicWorldObject
                     && dynamicWorldObject.ProtoGameObject is IProtoVehicle protoVehicle)
            {
                // apply additional vehicle damage
                var vehicleDamage = damage * VehicleAdditionalDamageMultiplier;
                protoVehicle.ServerApplyDamage(dynamicWorldObject, vehicleDamage);
            }
        }

        protected override void ClientInitialize(ClientInitializeData data)
        {
            base.ClientInitialize(data);
            var sceneObject = data.GameObject.ClientSceneObject;

            var componentBombCountdown = sceneObject.FindComponent<ClientComponentBombCountdown>();
            componentBombCountdown.IsRendering = false;

            Client.UI.AttachControl(
                sceneObject,
                new DangerAreaControl()
                {
                    RenderTransform = new ScaleTransform(2 * this.DamageRadius,
                                                         2 * this.DamageRadius),
                    RenderTransformOrigin = new Point(0.5, 0.5)
                },
                isFocusable: false,
                positionOffset: this.SharedGetObjectCenterWorldOffset(data.GameObject));

            var delay = 0.5;
            ClientTimersSystem.AddAction(
                delay,
                () =>
                {
                    if (sceneObject.IsDestroyed)
                    {
                        // object has left the scope
                        return;
                    }

                    var component = sceneObject.AddComponent<ItemWeaponMobSandTyrantMissiles.ComponentProjectileDrop>();
                    component.TimeDuration = this.ExplosionDelay.TotalSeconds - delay;
                    component.WorldOffset = (0.5, 0.5);
                });
        }

        protected override void ClientSetupRenderer(IComponentSpriteRenderer renderer)
        {
            renderer.IsEnabled = false;
        }

        protected override void PrepareDamageDescriptionCharacters(
            out double damageValue,
            out double armorPiercingCoef,
            out double finalDamageMultiplier,
            DamageDistribution damageDistribution)
        {
            damageValue = 80;
            armorPiercingCoef = 0.333;
            finalDamageMultiplier = 1.25;

            damageDistribution.Set(DamageType.Kinetic, 0.9)
                              .Set(DamageType.Heat, 0.1);
        }

        protected override void PrepareDamageDescriptionStructures(
            out double damageValue,
            out double defencePenetrationCoef)
        {
            // similar to the Modern bomb (though there should be no structures near the boss area)
            damageValue = 3_000;
            defencePenetrationCoef = 0.5;
        }

        protected override ITextureResource PrepareDefaultTexture(Type thisType)
        {
            return TextureResource.NoTexture;
        }

        protected override void PrepareProtoObjectExplosive(out ExplosionPreset explosionPresets)
        {
            explosionPresets = ExplosionPresets.MobSandTyrantMissile;
        }
    }
}