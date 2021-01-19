namespace AtomicTorch.CBND.CoreMod.Items.Tools.Pickaxes
{
    using System.Collections.Generic;
    using System.Windows.Media;
    using AtomicTorch.CBND.CoreMod.ClientComponents.FX;
    using AtomicTorch.CBND.CoreMod.Items.Ammo;
    using AtomicTorch.CBND.CoreMod.Items.Weapons;
    using AtomicTorch.CBND.CoreMod.SoundPresets;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Minerals;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Vegetation.Trees;
    using AtomicTorch.CBND.CoreMod.Systems.Weapons;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Extensions;
    using AtomicTorch.CBND.GameApi.Resources;
    using AtomicTorch.CBND.GameApi.ServicesClient.Components;
    using AtomicTorch.CBND.GameApi.ServicesClient.Rendering;
    using AtomicTorch.GameEngine.Common.Helpers;
    using AtomicTorch.GameEngine.Common.Primitives;
    using static SoundPresets.ObjectMaterial;

    public class ItemHammerPragmium : ProtoItemToolPickaxe, IProtoItemToolWoodcutting
    {
        private readonly ReadOnlySoundPreset<ObjectMaterial> HammerPragmiumHitSoundPreset
            = new SoundPreset<ObjectMaterial>(MaterialHitsSoundPresets.HitSoundDistancePreset)
              .Add(SoftTissues, "Hit/Special/HammerPragmium")
              .Add(HardTissues, "Hit/Special/HammerPragmium")
              .Add(SolidGround, "Hit/Special/HammerPragmium")
              .Add(Vegetation,  "Hit/Special/HammerPragmium")
              .Add(Wood,        "Hit/Special/HammerPragmium")
              .Add(Stone,       "Hit/Special/HammerPragmium")
              .Add(Metal,       "Hit/Special/HammerPragmium")
              .Add(Glass,       "Hit/Special/HammerPragmium");

        public override double DamageApplyDelay => 0.15;

        public override double DamageToMinerals => 5000;

        public override double DamageToNonMinerals => 25;

        public double DamageToTree => 5000;

        public override string Description =>
            "This fragile pragmium hammer uses molecular resonance mechanics, making it much faster than conventional alternatives.";

        // It's useless against buildings but we don't want to decrease
        // its durability further as it's very fragile anyway.
        public override double DurabilityDecreaseMultiplierWhenHittingBuildings => 1;

        public override uint DurabilityMax => 150;

        public override double FireAnimationDuration => 0.6;

        public override string Name => "Pragmium hammer";

        public override void ClientOnWeaponHitOrTrace(
            ICharacter firingCharacter,
            Vector2D worldPositionSource,
            IProtoItemWeapon protoWeapon,
            IProtoItemAmmo protoAmmo,
            IProtoCharacter protoCharacter,
            in Vector2Ushort fallbackCharacterPosition,
            IReadOnlyList<WeaponHitData> hitObjects,
            in Vector2D endPosition,
            bool endsWithHit)
        {
            var drawEffect = false;
            foreach (var hitObj in hitObjects)
            {
                if (SharedIsValidTargetForQuickMining(hitObj.FallbackProtoWorldObject))
                {
                    drawEffect = true;
                    break;
                }
            }

            if (!drawEffect)
            {
                return;
            }

            // add blast wave
            const double blastAnimationDuration = 0.3;
            var blastWaveColor = Color.FromRgb(0xBB, 0xDD, 0xFF);
            var blastwaveSizeFrom = 128 * 0.25 * new Size2F(3, 2);
            var blastwaveSizeTo = 128 * 1.25 * new Size2F(3,   2);

            var blastSceneObject = Client.Scene.CreateSceneObject("Temp hit", endPosition);

            blastSceneObject.Destroy(delay: blastAnimationDuration);

            var blastSpriteRenderer = Client.Rendering.CreateSpriteRenderer(
                blastSceneObject,
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

        public override void ClientPlayWeaponHitSound(
            IWorldObject hitWorldObject,
            IProtoWorldObject protoWorldObject,
            WeaponFireScatterPreset fireScatterPreset,
            ObjectMaterial objectMaterial,
            Vector2D worldObjectPosition)
        {
            // apply some volume variation
            var volume = SoundConstants.VolumeHit;
            volume *= RandomHelper.Range(0.8f, 1.0f);
            var pitch = RandomHelper.Range(0.95f, 1.05f);

            var soundPresetHit = SharedIsValidTargetForQuickMining(protoWorldObject)
                                     ? this.HammerPragmiumHitSoundPreset
                                     : this.SoundPresetHit;

            if (hitWorldObject is not null)
            {
                soundPresetHit.PlaySound(
                    objectMaterial,
                    hitWorldObject,
                    volume: volume,
                    pitch: pitch);
            }
            else
            {
                soundPresetHit.PlaySound(
                    objectMaterial,
                    protoWorldObject,
                    worldPosition: worldObjectPosition,
                    volume: volume,
                    pitch: pitch);
            }
        }

        public override double ServerGetDamageToMineral(IStaticWorldObject targetObject)
        {
            return SharedIsValidTargetForQuickMining(targetObject.ProtoStaticWorldObject)
                       ? this.DamageToMinerals
                       : this.DamageToNonMinerals;
        }

        public double ServerGetDamageToTree(IStaticWorldObject targetObject)
        {
            return SharedIsValidTargetForQuickMining(targetObject.ProtoStaticWorldObject)
                       ? this.DamageToTree
                       : this.DamageToNonMinerals;
        }

        private static bool SharedIsValidTargetForQuickMining(IProtoWorldObject protoWorldObject)
        {
            return protoWorldObject switch
            {
                IProtoObjectTree                                                      => true,
                IProtoObjectMineral protoMineral when protoMineral.IsAllowQuickMining => true,
                _                                                                     => false
            };
        }
    }
}