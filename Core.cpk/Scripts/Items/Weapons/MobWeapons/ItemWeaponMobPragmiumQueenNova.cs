namespace AtomicTorch.CBND.CoreMod.Items.Weapons.MobWeapons
{
    using System.Collections.Generic;
    using System.Windows.Media;
    using AtomicTorch.CBND.CoreMod.Characters.Mobs;
    using AtomicTorch.CBND.CoreMod.CharacterStatusEffects;
    using AtomicTorch.CBND.CoreMod.CharacterStatusEffects.Debuffs;
    using AtomicTorch.CBND.CoreMod.ClientComponents.FX;
    using AtomicTorch.CBND.CoreMod.Items.Ammo;
    using AtomicTorch.CBND.CoreMod.SoundPresets;
    using AtomicTorch.CBND.CoreMod.Systems.Weapons;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.Weapons;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Extensions;
    using AtomicTorch.CBND.GameApi.Resources;
    using AtomicTorch.CBND.GameApi.ServicesClient.Components;
    using AtomicTorch.CBND.GameApi.ServicesClient.Rendering;
    using AtomicTorch.GameEngine.Common.Helpers;
    using AtomicTorch.GameEngine.Common.Primitives;

    public class ItemWeaponMobPragmiumQueenNova : ProtoItemMobWeaponNova
    {
        public override double DamageApplyDelay => 1.05;

        public override double FireAnimationDuration => 1.3;

        public override double FireInterval => 3.0;

        public override (float min, float max) SoundPresetWeaponDistance
            => (15, 45);

        public override (float min, float max) SoundPresetWeaponDistance3DSpread
            => (10, 35);

        public override string WeaponAttachmentName => "Head";

        public override void ClientOnWeaponShot(ICharacter character)
        {
            ClientTimersSystem.AddAction(this.DamageApplyDelay - 0.11667,
                                         () => this.ClientCreateBlastwave(character));

            ClientTimersSystem.AddAction(this.DamageApplyDelay - 0.11667,
                                         ClientCreateScreenShakes);
        }

        public override void ServerOnObjectHitByNova(
            IWorldObject damagedObject,
            double damageApplied,
            WeaponFinalCache weaponFinalCache)
        {
            // daze damaged players
            if (damagedObject is ICharacter character
                && !character.IsNpc
                && damageApplied > 1)
            {
                character.ServerAddStatusEffect<StatusEffectDazed>(intensity: 1.0);
            }
        }

        public override bool SharedOnFire(ICharacter character, WeaponState weaponState)
        {
            base.SharedOnFire(character, weaponState);

            // spawn minions after a nova attack
            (character.ProtoGameObject as MobBossPragmiumQueen)
                ?.ServerTrySpawnMinions(character);

            return true;
        }

        protected override void PrepareProtoWeapon(
            out IEnumerable<IProtoItemAmmo> compatibleAmmoProtos,
            ref DamageDescription overrideDamageDescription)
        {
            // no ammo used
            compatibleAmmoProtos = null;

            var damageDistribution = new DamageDistribution()
                .Set(DamageType.Cold, 1.0);

            overrideDamageDescription = new DamageDescription(
                damageValue: 40,
                armorPiercingCoef: 0.1,
                finalDamageMultiplier: 1.25,
                rangeMax: 20.5,
                damageDistribution: damageDistribution);
        }

        protected override ReadOnlySoundPreset<WeaponSound> PrepareSoundPresetWeapon()
        {
            return new SoundPreset<WeaponSound>(customDistance: (15, 45),
                                                customDistance3DSpread: (10, 35))
                .Add(WeaponSound.Shot, "Skeletons/PragmiumQueen/Weapon/ShotNova");
        }

        protected override float SharedGetFireYOffset(ICharacter character)
        {
            return 0;
        }

        private static void ClientCreateScreenShakes()
        {
            const float shakesDuration = 0.4f,
                        shakesDistanceMin = 0.9f,
                        shakesDistanceMax = 1.1f;
            ClientComponentCameraScreenShakes.AddRandomShakes(duration: shakesDuration,
                                                              worldDistanceMin: -shakesDistanceMin,
                                                              worldDistanceMax: shakesDistanceMax);
        }

        private void ClientCreateBlastwave(ICharacter character)
        {
            // add blast wave
            var rangeMax = 4 + 2 * this.OverrideDamageDescription.RangeMax;

            const double blastAnimationDuration = 1.0;
            var blastWaveColor = Color.FromRgb(0x99, 0xDD, 0xFF);
            var blastSize = new Size2F(rangeMax, rangeMax);
            var blastwaveSizeFrom = 2 * new Size2F(128, 128);
            var blastwaveSizeTo = 128 * blastSize;

            var blastSceneObject = Client.Scene.CreateSceneObject(
                "Temp Nova",
                character.Position + (0, this.SharedGetFireYOffset(character)));

            blastSceneObject.Destroy(delay: blastAnimationDuration);

            var blastSpriteRenderer = Client.Rendering.CreateSpriteRenderer(blastSceneObject,
                                                                            new TextureResource("FX/ExplosionBlast"),
                                                                            drawOrder: DrawOrder.Light,
                                                                            spritePivotPoint: (0.5, 0.5));
            blastSpriteRenderer.BlendMode = BlendMode.AlphaBlendPremultiplied;

            // animate blast wave
            ClientComponentGenericAnimationHelper.Setup(
                blastSceneObject,
                blastAnimationDuration,
                updateCallback: alpha =>
                                {
                                    var blastwaveAlpha = (byte)(byte.MaxValue * (1 - alpha));
                                    blastSpriteRenderer.Color = blastWaveColor.WithAlpha(blastwaveAlpha);

                                    var sizeX = MathHelper.Lerp(blastwaveSizeFrom.X,
                                                                blastwaveSizeTo.X,
                                                                alpha);
                                    var sizeY = MathHelper.Lerp(blastwaveSizeFrom.Y,
                                                                blastwaveSizeTo.Y,
                                                                alpha);
                                    blastSpriteRenderer.Size = (sizeX, sizeY);
                                });
        }
    }
}