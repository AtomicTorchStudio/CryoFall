namespace AtomicTorch.CBND.CoreMod.Items.Weapons.MobWeapons
{
    using System.Collections.Generic;
    using System.Windows.Media;
    using AtomicTorch.CBND.CoreMod.ClientComponents.FX;
    using AtomicTorch.CBND.CoreMod.Items.Ammo;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.Weapons;
    using AtomicTorch.CBND.GameApi.Extensions;
    using AtomicTorch.CBND.GameApi.Resources;
    using AtomicTorch.CBND.GameApi.ServicesClient.Components;
    using AtomicTorch.CBND.GameApi.ServicesClient.Rendering;
    using AtomicTorch.GameEngine.Common.Helpers;
    using AtomicTorch.GameEngine.Common.Primitives;

    public class ItemWeaponMobFloaterNova : ProtoItemMobWeaponNova
    {
        public override double DamageApplyDelay => 0.4;

        public override double FireAnimationDuration => 1.0;

        public override double FireInterval => 2.0;

        public override (float min, float max) SoundPresetWeaponDistance
            => (6, 45);

        public override string WeaponAttachmentName => "Head";

        public override void ClientOnWeaponShot(ICharacter character)
        {
            ClientTimersSystem.AddAction(this.DamageApplyDelay - 0.11667,
                                         () => this.ClientCreateBlastwave(character));
        }

        public override string GetCharacterAnimationNameFire(ICharacter character)
        {
            return "Attack";
        }

        protected override void PrepareProtoWeapon(
            out IEnumerable<IProtoItemAmmo> compatibleAmmoProtos,
            ref DamageDescription overrideDamageDescription)
        {
            // no ammo used
            compatibleAmmoProtos = null;

            var damageDistribution = new DamageDistribution()
                .Set(DamageType.Chemical, 1.0);

            overrideDamageDescription = new DamageDescription(
                damageValue: 15,
                armorPiercingCoef: 0.2,
                finalDamageMultiplier: 1,
                rangeMax: 8.5,
                damageDistribution: damageDistribution);
        }

        private void ClientCreateBlastwave(ICharacter character)
        {
            // add blast wave
            var rangeMax = 4 + 2 * this.OverrideDamageDescription.RangeMax;

            const double blastAnimationDuration = 0.6;
            var blastWaveColor = Color.FromRgb(0xCC, 0xEE, 0xAA);
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