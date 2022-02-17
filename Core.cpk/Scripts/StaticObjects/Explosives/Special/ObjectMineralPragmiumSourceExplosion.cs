namespace AtomicTorch.CBND.CoreMod.StaticObjects.Explosives
{
    using System;
    using AtomicTorch.CBND.CoreMod.ClientComponents.FX;
    using AtomicTorch.CBND.CoreMod.ClientComponents.Rendering.Lighting;
    using AtomicTorch.CBND.CoreMod.SoundPresets;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Minerals;
    using AtomicTorch.CBND.CoreMod.Systems.ServerTimers;
    using AtomicTorch.CBND.CoreMod.Systems.Weapons;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.State;
    using AtomicTorch.CBND.GameApi.Data.Weapons;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Resources;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.ServicesClient.Components;

    /// <summary>
    /// Pragmium Source on destruction spawns this object which explodes
    /// after a short delay and annihilate everything in a large radius.
    /// </summary>
    public class ObjectMineralPragmiumSourceExplosion
        : ProtoObjectExplosive
            <ObjectExplosivePrivateState,
                ObjectMineralPragmiumSourceExplosion.PublicState,
                StaticObjectClientState>
    {
        public const int ExplosionDamageRadius = 13;

        private readonly Lazy<ObjectMineralPragmiumSource> protoPragmiumSource
            = new(GetProtoEntity<ObjectMineralPragmiumSource>);

        public override double DamageRadius => ExplosionDamageRadius; // large annihilation radius

        public override TimeSpan ExplosionDelay => TimeSpan.FromSeconds(3);

        public override bool IsActivatesRaidBlock => false;

        public override bool IsDamageThroughObstacles => true;

        public override bool IsExplosionDelaySkippedOnDamage => false;

        public override string Name => "Pragmium source explosion";

        public override ObjectMaterial ObjectMaterial => ObjectMaterial.Stone;

        public override float VolumeExplosion => 3;

        public override void ServerOnDestroy(IStaticWorldObject gameObject)
        {
            var tilePosition = gameObject.TilePosition;

            var byCharacter = GetPublicState(gameObject).ExplodedByCharacter;

            // spawn Pragmium nodes on explosion (right after the damage is dealt to all the objects there)
            ServerTimersSystem.AddAction(
                delaySeconds: this.ExplosionPreset.ServerDamageApplyDelay * 1.01,
                () => ObjectMineralPragmiumSource.ServerOnExplode(tilePosition, this.DamageRadius, byCharacter));

            base.ServerOnDestroy(gameObject);
        }

        public override bool SharedOnDamage(
            WeaponFinalCache weaponCache,
            IStaticWorldObject targetObject,
            double damagePreMultiplier,
            out double obstacleBlockDamageCoef,
            out double damageApplied)
        {
            obstacleBlockDamageCoef = this.protoPragmiumSource.Value.ObstacleBlockDamageCoef;
            damageApplied = 0; // no damage
            return true;       // hit
        }

        protected override void ClientInitialize(ClientInitializeData data)
        {
            base.ClientInitialize(data);
            var sceneObject = data.GameObject.ClientSceneObject;
            var componentBombCountdown = sceneObject.FindComponent<ClientComponentBombCountdown>();
            componentBombCountdown.IsRendering = false;
            this.ClientAddShakes(componentBombCountdown);

            var sceneObjectEffect = Client.Scene
                                          .CreateSceneObject("Pragmium source explosion effect",
                                                             position: sceneObject.Position);
            var soundEmitter = Api.Client.Audio.CreateSoundEmitter(
                sceneObjectEffect,
                soundResource: new SoundResource("Ambient/Earthquake"),
                is3D: true,
                worldPositionOffset: this.Layout.Center,
                radius: 7,
                isLooped: true);

            soundEmitter.Seek(0);
            soundEmitter.CustomMinDistance = ExplosionDamageRadius;
            soundEmitter.CustomMaxDistance = 10 + ExplosionDamageRadius;

            var lightSource = ObjectMineralPragmiumHelper.ClientInitializeLightForSource(sceneObjectEffect);
            sceneObjectEffect
                .AddComponent<ClientComponentLightSourceEffectPulsating>()
                .Setup(data.GameObject,
                       safeDistance: ExplosionDamageRadius + 1, // add extra tile just to ensure safety
                       lightSource,
                       soundEmitter,
                       fromPercents: 85,
                       toPercents: 120,
                       durationSeconds: 1);
        }

        protected override void ClientSetupRenderer(IComponentSpriteRenderer renderer)
        {
            renderer.SpritePivotPoint = (0, 0);
            renderer.PositionOffset = (0, 0);
            renderer.DrawOrderOffsetY = 0.7;
        }

        protected override void CreateLayout(StaticObjectLayout layout)
        {
            layout.Setup("##");
        }

        protected override void PrepareDamageDescriptionCharacters(
            out double damageValue,
            out double armorPiercingCoef,
            out double finalDamageMultiplier,
            DamageDistribution damageDistribution)
        {
            // nobody can survive this explosion
            damageValue = 10_000;
            armorPiercingCoef = 1;
            finalDamageMultiplier = 1;
            damageDistribution.Set(DamageType.Kinetic, 1);
        }

        protected override void PrepareDamageDescriptionStructures(
            out double damageValue,
            out double defencePenetrationCoef)
        {
            // nothing can survive this explosion
            damageValue = 10_000_000;
            defencePenetrationCoef = 1;
        }

        protected override ITextureResource PrepareDefaultTexture(Type thisType)
        {
            return new TextureResource(GenerateTexturePath(typeof(ObjectMineralPragmiumSource)));
        }

        protected override void PrepareProtoObjectExplosive(out ExplosionPreset explosionPresets)
        {
            explosionPresets = ExplosionPresets.SpecialPragmiumSourceExplosion;
        }

        protected override double SharedPrepareStatDamageToCharacters(double damageValue)
        {
            // we want to damage characters even if there is a reduced explosion damage rate (such as on a PvE server)
            return damageValue;
        }

        protected override void SharedProcessCreatedPhysics(CreatePhysicsData data)
        {
            // inherit physics
            this.protoPragmiumSource.Value.SharedCreatePhysics(data.GameObject);
        }

        private void ClientAddShakes(ClientComponentBombCountdown component)
        {
            const float shakesInterval = 0.5f,
                        shakesDuration = 1f,
                        shakesDistanceMin = 0.2f,
                        shakesDistanceMax = 0.25f;

            if (component.IsDestroyed)
            {
                return;
            }

            var intensity = 1 - component.SecondsRemains / this.ExplosionDelay.TotalSeconds;
            if (intensity < 0.3)
            {
                intensity = 0.3;
            }

            ClientComponentCameraScreenShakes.AddRandomShakes(duration: shakesDuration,
                                                              worldDistanceMin: (float)(intensity * -shakesDistanceMin),
                                                              worldDistanceMax: (float)(intensity * shakesDistanceMax));

            ClientTimersSystem.AddAction(shakesInterval, () => this.ClientAddShakes(component));
        }

        public class PublicState : StaticObjectPublicState
        {
            [TempOnly]
            public ICharacter ExplodedByCharacter { get; set; }
        }
    }
}