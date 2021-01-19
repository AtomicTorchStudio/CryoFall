namespace AtomicTorch.CBND.CoreMod.Items.Weapons.MobWeapons
{
    using System;
    using System.Collections.Generic;
    using System.Windows.Media;
    using AtomicTorch.CBND.CoreMod.Characters;
    using AtomicTorch.CBND.CoreMod.Characters.Mobs;
    using AtomicTorch.CBND.CoreMod.CharacterSkeletons;
    using AtomicTorch.CBND.CoreMod.ClientComponents.FX;
    using AtomicTorch.CBND.CoreMod.Helpers;
    using AtomicTorch.CBND.CoreMod.Items.Ammo;
    using AtomicTorch.CBND.CoreMod.Skills;
    using AtomicTorch.CBND.CoreMod.SoundPresets;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Explosives;
    using AtomicTorch.CBND.CoreMod.Systems.Physics;
    using AtomicTorch.CBND.CoreMod.Systems.ServerTimers;
    using AtomicTorch.CBND.CoreMod.Systems.Weapons;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.Items;
    using AtomicTorch.CBND.GameApi.Data.Physics;
    using AtomicTorch.CBND.GameApi.Data.Weapons;
    using AtomicTorch.CBND.GameApi.Extensions;
    using AtomicTorch.CBND.GameApi.Resources;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.Scripting.ClientComponents;
    using AtomicTorch.CBND.GameApi.ServicesClient.Components;
    using AtomicTorch.CBND.GameApi.ServicesClient.Rendering;
    using AtomicTorch.GameEngine.Common.Extensions;
    using AtomicTorch.GameEngine.Common.Helpers;
    using AtomicTorch.GameEngine.Common.Primitives;

    public class ItemWeaponMobSandTyrantMissiles : ProtoItemWeaponMelee
    {
        private const double ClientLaunchOffsetY = 0.1;

        private const double ClientMissileLaunchDelaySeconds = ServerMissileLaunchDelaySeconds - 0.2;

        private const double MinionsSpawnDelaySeconds = 1.2;

        private const int MissileCountPerShot = 32;

        private const double MissileProbabilityToTargetDirectly = 0.7;

        private const double MissileServerExplosionSpawnDelay = 0.8;

        private const ushort MissileSpawnRadiusMax = 18;

        private const ushort MissileSpawnRadiusMin = 3;

        private const int MissileSpreadMinDistanceBetween = 4;

        private const double MissileSpreadTimeInterval = 0.75;

        private const double ServerMissileLaunchDelaySeconds = 0.7;

        private static readonly TextureResource TextureProjectile
            = new("FX/WeaponTraces/TraceMobWeaponSandTyrantMissile.png");

        public override ushort AmmoCapacity => 0;

        public override double AmmoReloadDuration => 0;

        public override bool CanDamageStructures => false;

        public override CollisionGroup CollisionGroup => CollisionGroups.HitboxRanged;

        public override double DamageApplyDelay => 0;

        public override string Description => null;

        public override uint DurabilityMax => 0;

        public override double FireAnimationDuration => 1.3;

        public override double FireInterval => this.FireAnimationDuration;

        public override ITextureResource Icon => null;

        public override bool IsLoopedAttackAnimation => false;

        public override string Name => this.ShortId;

        public override double ReadyDelayDuration => 0;

        public override (float min, float max) SoundPresetWeaponDistance
            => (15, 45);

        public override (float min, float max) SoundPresetWeaponDistance3DSpread
            => (10, 35);

        public override double SpecialEffectProbability =>
            1; // Must always be 1 for all animal weapons. Individual effects will be rolled in the effect function.

        public override string WeaponAttachmentName => "Head";

        protected override ProtoSkillWeapons WeaponSkill => null;

        protected override TextureResource WeaponTextureResource => null;

        public override void ClientOnWeaponShot(ICharacter character)
        {
            ClientTimersSystem.AddAction(
                ClientMissileLaunchDelaySeconds,
                () =>
                {
                    // screen shakes on missiles launch
                    ClientCreateScreenShakes();

                    // launch missiles
                    var sceneObject = Client.Scene.CreateSceneObject(this.ShortId + " projectiles out");
                    sceneObject.Destroy(delay: 5);

                    var skeletonRenderer = character
                                           .GetClientState<BaseCharacterClientState>()
                                           .SkeletonRenderer;

                    var headWorldPosition = skeletonRenderer.TransformSlotPosition("ProjectilesOrigin", default, out _)
                                            + (0, ClientLaunchOffsetY);

                    sceneObject.Position = headWorldPosition;
                    for (var index = 0; index < MissileCountPerShot; index++)
                    {
                        ClientTimersSystem.AddAction(
                            MissileSpreadTimeInterval * RandomHelper.NextDouble() * 0.5,
                            () => sceneObject.AddComponent<ComponentProjectileShot>());
                    }
                });
        }

        public override void ClientSetupSkeleton(
            IItem item,
            ICharacter character,
            ProtoCharacterSkeleton protoCharacterSkeleton,
            IComponentSkeleton skeletonRenderer,
            List<IClientComponent> skeletonComponents)
        {
            Client.Rendering.PreloadTextureAsync(TextureProjectile);
        }

        public override string GetCharacterAnimationNameFire(ICharacter character)
        {
            return "AttackRanged";
        }

        public override bool SharedCanSelect(IItem item, ICharacter character, bool isAlreadySelected, bool isByPlayer)
        {
            return character.ProtoCharacter is IProtoCharacterMob;
        }

        public override bool SharedOnFire(ICharacter character, WeaponState weaponState)
        {
            if (IsClient)
            {
                return true;
            }

            ServerTimersSystem.AddAction(
                ServerMissileLaunchDelaySeconds,
                () =>
                {
                    if (character.IsDestroyed
                        || character.GetPublicState<ICharacterPublicState>().IsDead)
                    {
                        return;
                    }

                    this.ServerSpawnMissiles(character);
                });

            ServerTimersSystem.AddAction(
                MinionsSpawnDelaySeconds,
                () =>
                {
                    // spawn minions after the missiles launch
                    (character.ProtoGameObject as MobBossSandTyrant)
                        ?.ServerTrySpawnMinions(character);
                });

            return true;
        }

        // no fire scatter and so no hit scan shots
        protected sealed override WeaponFireScatterPreset PrepareFireScatterPreset()
        {
            return new(Array.Empty<double>());
        }

        protected override void PrepareProtoWeapon(
            out IEnumerable<IProtoItemAmmo> compatibleAmmoProtos,
            ref DamageDescription overrideDamageDescription)
        {
            compatibleAmmoProtos = null;

            // no damage by the firing weapon (similar to a grenade launcher)
            overrideDamageDescription = new DamageDescription(
                damageValue: 0,
                armorPiercingCoef: 0,
                finalDamageMultiplier: 1,
                rangeMax: 20.5, // the range is important here
                damageDistribution: new DamageDistribution());
        }

        protected override ReadOnlySoundPreset<ObjectMaterial> PrepareSoundPresetHit()
        {
            return MaterialHitsSoundPresets.Ranged;
        }

        protected override ReadOnlySoundPreset<WeaponSound> PrepareSoundPresetWeapon()
        {
            return new SoundPreset<WeaponSound>()
                .Add(WeaponSound.Shot, "Skeletons/SandTyrant/Weapon/ShotMissiles");
        }

        private static void ClientCreateScreenShakes()
        {
            const float shakesDuration = 0.2f,
                        shakesDistanceMin = 0.7f,
                        shakesDistanceMax = 0.9f;
            ClientComponentCameraScreenShakes.AddRandomShakes(duration: shakesDuration,
                                                              worldDistanceMin: -shakesDistanceMin,
                                                              worldDistanceMax: shakesDistanceMax);
        }

        private void ServerSpawnMissiles(ICharacter character)
        {
            var centerPosition = (character.TilePosition + (0, 1)).ToVector2Ushort();

            var selectedLocations = new List<Vector2Ushort>();

            var protoObjectToSpawn = Api.GetProtoEntity<ObjectMobSandTyrantMissileExplosion>();
            var sqrMinDistanceBetweenSpawnedObjects = MissileSpreadMinDistanceBetween * MissileSpreadMinDistanceBetween;

            using var tempListScopedByPlayers = Api.Shared.GetTempList<ICharacter>();
            Server.World.GetScopedByPlayers(character, tempListScopedByPlayers);

            if (tempListScopedByPlayers.Count > 0)
            {
                // perform a random roll to attempt targeting each player
                var listPlayers = tempListScopedByPlayers.AsList();
                listPlayers.Shuffle();

                foreach (var playerCharacter in listPlayers)
                {
                    if (RandomHelper.RollWithProbability(MissileProbabilityToTargetDirectly))
                    {
                        TrySpawnMissile(playerCharacter.TilePosition);
                    }
                }
            }

            // fire random missiles
            while (selectedLocations.Count < MissileCountPerShot)
            {
                var attempts = 500;
                do
                {
                    var spawnPosition = SharedCircleLocationHelper.SharedSelectRandomPositionInsideTheCircle(
                        centerPosition,
                        circleRadiusMin: MissileSpawnRadiusMin,
                        circleRadiusMax: MissileSpawnRadiusMax);

                    if (TrySpawnMissile(spawnPosition))
                    {
                        break;
                    }
                }
                while (--attempts > 0);

                if (attempts > 0)
                {
                    // successfully spawned all
                    continue;
                }

                var message = $"Cannot spawn all the missiles ({this})";
                if (Api.IsEditor)
                {
                    Logger.Error(message);
                }
                else
                {
                    Logger.Warning(message);
                }
            }

            bool TrySpawnMissile(Vector2Ushort spawnPosition)
            {
                foreach (var selectedLocation in selectedLocations)
                {
                    if (spawnPosition.TileSqrDistanceTo(selectedLocation)
                        > sqrMinDistanceBetweenSpawnedObjects)
                    {
                        continue;
                    }

                    // too close
                    return false;
                }

                selectedLocations.Add(spawnPosition);
                ServerTimersSystem.AddAction(
                    MissileServerExplosionSpawnDelay + MissileSpreadTimeInterval * RandomHelper.NextDouble(),
                    () =>
                    {
                        var objectMissile = Server.World.CreateStaticWorldObject(protoObjectToSpawn, spawnPosition);
                        if (objectMissile is not null)
                        {
                            protoObjectToSpawn.ServerSetup(objectMissile, deployedByCharacter: character);
                        }
                    });
                return true;
            }
        }

        public class ComponentProjectileDrop : ClientComponent
        {
            private const double MinScale = 0.6;

            private const double VerticalRange = 20; // a bit larger than the player's max view scope by height

            private static readonly Interval<double> MaxScaleRange = new(1.0, 1.2);

            public Vector2D WorldOffset;

            private double currentTime;

            private IComponentSpriteRenderer spriteRenderer;

            private double startScale;

            private double timeDuration;

            public double TimeDuration
            {
                get => this.timeDuration;
                set
                {
                    this.timeDuration = value;
                    this.currentTime = 0;
                }
            }

            public override void Update(double deltaTime)
            {
                this.currentTime += deltaTime;

                var t = (this.timeDuration - this.currentTime) / this.timeDuration;
                t = MathHelper.Clamp(t, 0, 1.0);

                // zero additional vertical offset on impact
                var additionalVerticalOffset = t * VerticalRange;
                this.spriteRenderer.PositionOffset =
                    (this.WorldOffset.X, this.WorldOffset.Y + additionalVerticalOffset);
                // full scale on impact
                this.spriteRenderer.Scale = MathHelper.Lerp(MinScale, this.startScale, t);

                // full opacity on the last 15% before the impact
                var k = 0.15;
                t -= k;
                t = Math.Max(0, t);
                t *= 1 / (1 - k);
                var opacity = (1.0 - t);
                this.spriteRenderer.Color = Color.FromArgb((byte)(opacity * byte.MaxValue), 0xFF, 0xFF, 0xFF);
            }

            protected override void OnDisable()
            {
                this.spriteRenderer.Destroy();
                this.spriteRenderer = null;
            }

            protected override void OnEnable()
            {
                this.startScale =
                    MaxScaleRange.Min + (MaxScaleRange.Max - MaxScaleRange.Min) * RandomHelper.NextDouble();
                this.spriteRenderer = Api.Client.Rendering.CreateSpriteRenderer(this.SceneObject, TextureProjectile);
                this.spriteRenderer.SpritePivotPoint = (1, 0.5);
                this.spriteRenderer.RotationAngleRad = 90 * MathConstants.DegToRad;
                this.spriteRenderer.DrawOrder = DrawOrder.Light;
                this.spriteRenderer.BlendMode = BlendMode.AdditivePremultiplied;

                if (RandomHelper.Next(0, 2) == 0)
                {
                    this.spriteRenderer.DrawMode = DrawMode.FlipVertically; // horizontal flip as it's rotated on 90 deg
                }

                this.Update(0);
            }
        }

        public class ComponentProjectileShot : ClientComponent
        {
            private const double OpacitySpeed = 1.0;

            private const double ScaleSpeed = 1.5;

            private static readonly Vector2D MaxOffset = (1.5, 0.5);

            private static readonly Interval<double> ScaleRange = new(0.333, 0.5);

            private static readonly Interval<double> SpeedRange = new(14, 16);

            private double opacity;

            private double scale;

            private double speed;

            private IComponentSpriteRenderer spriteRenderer;

            private double targetScale;

            public override void Update(double deltaTime)
            {
                this.spriteRenderer.PositionOffset += (0, deltaTime * this.speed);

                this.scale += deltaTime * ScaleSpeed;
                this.scale = Math.Min(this.scale, this.targetScale);
                this.spriteRenderer.Scale = this.scale;

                this.opacity -= deltaTime * OpacitySpeed;
                this.opacity = Math.Max(this.opacity, 0);
                this.spriteRenderer.Color = Color.FromArgb((byte)(this.opacity * byte.MaxValue), 0xFF, 0xFF, 0xFF);
            }

            protected override void OnDisable()
            {
                this.spriteRenderer.Destroy();
                this.spriteRenderer = null;
            }

            protected override void OnEnable()
            {
                this.speed = SpeedRange.Min + (SpeedRange.Max - SpeedRange.Min) * RandomHelper.NextDouble();
                this.targetScale = ScaleRange.Min + (ScaleRange.Max - ScaleRange.Min) * RandomHelper.NextDouble();
                this.opacity = 1.0;

                this.spriteRenderer = Api.Client.Rendering.CreateSpriteRenderer(this.SceneObject, TextureProjectile);
                this.spriteRenderer.PositionOffset += ((MaxOffset.X * (RandomHelper.NextDouble()) - 0.5),
                                                       (MaxOffset.Y * (RandomHelper.NextDouble()) - 0.5));
                this.spriteRenderer.RotationAngleRad = -90 * MathConstants.DegToRad;
                this.spriteRenderer.DrawOrder = DrawOrder.Light;
                this.spriteRenderer.BlendMode = BlendMode.AdditivePremultiplied;

                if (RandomHelper.Next(0, 2) == 0)
                {
                    this.spriteRenderer.DrawMode = DrawMode.FlipVertically; // horizontal flip as it's rotated on 90 deg
                }

                this.Update(0);
            }
        }
    }
}