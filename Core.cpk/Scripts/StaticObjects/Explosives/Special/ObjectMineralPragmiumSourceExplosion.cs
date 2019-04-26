namespace AtomicTorch.CBND.CoreMod.StaticObjects.Explosives
{
    using System;
    using AtomicTorch.CBND.CoreMod.ClientComponents.FX;
    using AtomicTorch.CBND.CoreMod.ClientComponents.Rendering.Lighting;
    using AtomicTorch.CBND.CoreMod.ClientComponents.Timer;
    using AtomicTorch.CBND.CoreMod.SoundPresets;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Minerals;
    using AtomicTorch.CBND.CoreMod.Systems.ServerTimers;
    using AtomicTorch.CBND.CoreMod.Systems.Weapons;
    using AtomicTorch.CBND.GameApi;
    using AtomicTorch.CBND.GameApi.Data.Weapons;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Resources;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.ServicesClient.Components;

    /// <summary>
    /// Pragmium Source on destruction spawns this object which explodes
    /// after a short delay and annihilate everything in a large radius.
    /// </summary>
    public class ObjectMineralPragmiumSourceExplosion : ProtoObjectExplosive
    {
        private readonly Lazy<ObjectMineralPragmiumSource> protoPragmiumSource
            = new Lazy<ObjectMineralPragmiumSource>(
                GetProtoEntity<ObjectMineralPragmiumSource>);

        public override double DamageRadius => 9; // large annihilation radius

        public override bool ActivatesRaidModeForLandClaim => false;

        public override TimeSpan ExplosionDelay => TimeSpan.FromSeconds(3);

        [NotLocalizable]
        public override string Name => "Pragmium source explosion";

        public override ObjectSoundMaterial ObjectSoundMaterial => ObjectSoundMaterial.Stone;

        public override void ServerOnDestroy(IStaticWorldObject gameObject)
        {
            var tilePosition = gameObject.TilePosition;

            // spawn Pragmium nodes on explosion (right after the damage is dealt to all the objects there)
            ServerTimersSystem.AddAction(
                delaySeconds: this.ExplosionPreset.ServerDamageApplyDelay * 1.01,
                () => ObjectMineralPragmiumSource.ServerOnExplode(tilePosition, this.DamageRadius));

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
            return true; // hit
        }

        protected override void ClientInitialize(ClientInitializeData data)
        {
            base.ClientInitialize(data);
            var sceneObject = Client.Scene.GetSceneObject(data.GameObject);
            var componentBombCountdown = sceneObject.FindComponent<ClientComponentBombCountdown>();
            componentBombCountdown.IsRendering = false;
            this.ClientAddShakes(componentBombCountdown);

            var lightSource = ObjectMineralPragmiumHelper.ClientInitializeLightForSource(data.GameObject);
            sceneObject.AddComponent<ClientComponentLightSourceEffectPulsating>()
                       .Setup(lightSource,
                              fromPercents: 85,
                              toPercents: 120,
                              durationSeconds: 1);

            Api.Client.Audio.CreateSoundEmitter(data.GameObject,
                                                new SoundResource("Ambient/Earthquake"),
                                                true,
                                                radius: 7,
                                                isLooped: true);
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

            ClientComponentTimersManager.AddAction(shakesInterval, () => this.ClientAddShakes(component));
        }
    }
}