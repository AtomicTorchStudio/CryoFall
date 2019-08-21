namespace AtomicTorch.CBND.CoreMod.CharacterSkeletons
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Windows.Media;
    using AtomicTorch.CBND.CoreMod.Characters.Player;
    using AtomicTorch.CBND.CoreMod.Helpers.Client;
    using AtomicTorch.CBND.CoreMod.Items.Equipment;
    using AtomicTorch.CBND.CoreMod.SoundPresets;
    using AtomicTorch.CBND.CoreMod.StaticObjects;
    using AtomicTorch.CBND.CoreMod.Systems.Physics;
    using AtomicTorch.CBND.CoreMod.Tiles;
    using AtomicTorch.CBND.GameApi.Data;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.Physics;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Resources;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.ServicesClient.Components;
    using AtomicTorch.GameEngine.Common.Helpers;

    public abstract class ProtoCharacterSkeleton : ProtoEntity, IProtoCharacterSkeleton
    {
        protected ProtoCharacterSkeleton()
        {
            this.Name = this.GetType().Name;
        }

        // move speed as the default scale, in world unites (tiles) per second
        public virtual double DefaultMoveSpeed => 1.0;

        public abstract bool HasMoveStartAnimations { get; }

        public virtual bool HasStaticAttackAnimations { get; } = true;

        public override string Name { get; }

        public abstract float OrientationDownExtraAngle { get; }

        public abstract float OrientationThresholdDownHorizontalFlipDeg { get; }

        public abstract float OrientationThresholdDownToUpFlipDeg { get; }

        public abstract float OrientationThresholdUpHorizontalFlipDeg { get; }

        public abstract SkeletonResource SkeletonResourceBack { get; }

        public abstract SkeletonResource SkeletonResourceFront { get; }

        public ReadOnlySoundPreset<CharacterSound> SoundPresetCharacter { get; private set; }

        public ReadOnlySoundPreset<GroundSoundMaterial> SoundPresetFootsteps { get; private set; }

        public ReadOnlySoundPreset<CharacterSound> SoundPresetMovement { get; private set; }

        public ReadOnlySoundPreset<WeaponSound> SoundPresetWeapon { get; private set; }

        public virtual double SpeedMultiplier => 1;

        public virtual double WorldScale => 1;

        protected abstract string SoundsFolderPath { get; }

        /// <summary>
        /// Please note this property is multiplied on <see cref="SoundConstants.VolumeFootstepsMultiplier" />.
        /// </summary>
        protected virtual double VolumeFootsteps => 1;

        public IComponentSpriteRenderer ClientCreateShadowRenderer(IWorldObject worldObject, double scaleMultiplier)
        {
            var rendererShadow = ClientShadowHelper.AddShadowRenderer(
                worldObject,
                scaleMultiplier: (float)scaleMultiplier);

            if (rendererShadow == null)
            {
                return null;
            }

            rendererShadow.Color = Color.FromArgb(0xAA, 0x00, 0x00, 0x00);

            rendererShadow.DrawOrder = DrawOrder.Shadow;
            this.ClientSetupShadowRenderer(rendererShadow, scaleMultiplier);
            return rendererShadow;
        }

        public abstract void ClientSetupShadowRenderer(IComponentSpriteRenderer shadowRenderer, double scaleMultiplier);

        public virtual void CreatePhysics(IPhysicsBody physicsBody)
        {
            // default skeleton physics (for test only, must be overridden in inherited classes)
            var radius = 0.2;

            physicsBody.AddShapeCircle(
                radius,
                center: (-radius / 2, 0));

            // melee hitbox (circle)
            physicsBody.AddShapeCircle(
                radius * 1.5,
                center: (-radius / 2, radius / 1.5),
                group: CollisionGroups.HitboxMelee);

            // ranged hitbox (circle)
            physicsBody.AddShapeCircle(
                radius * 2,
                center: (-radius / 2, radius),
                group: CollisionGroups.HitboxRanged);
        }

        public
            (ReadOnlySoundPreset<CharacterSound> soundPresetCharacter,
            ReadOnlySoundPreset<CharacterSound> soundPresetMovement) GetSoundPresets(ICharacter character)
        {
            ReadOnlySoundPreset<CharacterSound> soundPresetCharacter = null;
            ReadOnlySoundPreset<CharacterSound> soundPresetMovement = null;

            var protoCharacter = character.ProtoCharacter;
            if (protoCharacter is PlayerCharacter)
            {
                // try get sound preset overrides from the character equipment
                foreach (var item in character.SharedGetPlayerContainerEquipment()
                                              .Items)
                {
                    switch (item.ProtoItem)
                    {
                        case IProtoItemEquipmentChest chest:
                            soundPresetCharacter = chest.SoundPresetMovementOverride;
                            break;

                        case IProtoItemEquipmentHead head:
                            soundPresetMovement = head.SoundPresetCharacterOverride;
                            break;
                    }
                }
            }

            if (soundPresetCharacter == null)
            {
                soundPresetCharacter = this.SoundPresetCharacter;
            }

            if (soundPresetMovement == null)
            {
                soundPresetMovement = this.SoundPresetMovement;
            }

            return (soundPresetCharacter, soundPresetMovement);
        }

        [SuppressMessage("ReSharper", "CanExtractXamlLocalizableStringCSharp")]
        public virtual void OnSkeletonCreated(IComponentSkeleton skeleton)
        {
            skeleton.AnimationEvent += SkeletonOnAnimationEventFootstep;
        }

        public void PlaySound(CharacterSound soundKey, ICharacter character)
        {
            var (soundPresetCharacter, soundPresetMovement) = this.GetSoundPresets(character);
            soundPresetCharacter.PlaySound(soundKey, character);
            soundPresetMovement.PlaySound(soundKey, character);
        }

        protected sealed override void PrepareProto()
        {
            base.PrepareProto();

            this.SoundPresetFootsteps = this.PrepareSoundPresetFootsteps();
            this.SoundPresetCharacter = this.PrepareSoundPresetCharacter();
            this.SoundPresetMovement = this.PrepareSoundPresetMovement();
            this.SoundPresetWeapon = this.PrepareSoundPresetWeapon();
            this.PrepareProtoCharacterSkeleton();
        }

        protected virtual void PrepareProtoCharacterSkeleton()
        {
        }

        protected virtual ReadOnlySoundPreset<GroundSoundMaterial> PrepareSoundPresetFootsteps()
        {
            var preset = SoundPreset.CreateFromFolder<GroundSoundMaterial>(
                this.SoundsFolderPath + "/Footsteps/",
                throwExceptionIfNoFilesFound: false);
            this.VerifySoundPreset(preset);
            return preset;
        }

        protected void VerifySoundPreset<TSoundKey>(ReadOnlySoundPreset<TSoundKey> preset)
            where TSoundKey : struct, Enum
        {
            if (preset.SoundsCount > 0)
            {
                return;
            }

            Logger.Error(
                $"No {typeof(TSoundKey).Name} sounds found for skeleton {this.ShortId}. Sounds should be located at \"{ContentPaths.Sounds + this.SoundsFolderPath}\" (determined by property DefaultSoundsFolder of this class).");
        }

        [SuppressMessage("ReSharper", "CanExtractXamlLocalizableStringCSharp")]
        private static void SkeletonOnAnimationEventFootstep(
            ICharacter character,
            IComponentSkeleton skeleton,
            SkeletonEventData e)
        {
            if (e.EventName != "Footstep")
            {
                return;
            }

            // play footstep sound
            GroundSoundMaterial groundSoundMaterial;

            var tile = character.Tile;

            // find the latest proto ground object with sound
            IProtoObjectWithGroundSoundMaterial protoGroundObject = null;
            foreach (var worldObject in tile.StaticObjects)
            {
                if (worldObject.ProtoStaticWorldObject is IProtoObjectWithGroundSoundMaterial proto)
                {
                    protoGroundObject = proto;
                }
            }

            if (protoGroundObject != null)
            {
                // get sound material of proto ground object
                groundSoundMaterial = protoGroundObject.SharedGetGroundSoundMaterial();
            }
            else
            {
                // get sound material of proto tile
                var protoTile = (ProtoTile)tile.ProtoTile;
                groundSoundMaterial = protoTile.GroundSoundMaterial;
            }

            ReadOnlySoundPreset<GroundSoundMaterial> soundPresetMovement = null;
            var protoCharacter = character.ProtoCharacter;
            if (protoCharacter is PlayerCharacter)
            {
                // try get movement sound preset override
                foreach (var item in character.SharedGetPlayerContainerEquipment().Items)
                {
                    if (item.ProtoItem is IProtoItemEquipmentLegs protoItemEquipmentLegs)
                    {
                        soundPresetMovement = protoItemEquipmentLegs.SoundPresetFootstepsOverride;
                        break;
                    }
                }
            }

            var protoSkeleton = (ProtoCharacterSkeleton)protoCharacter.ClientGetCurrentProtoSkeleton(character);
            if (soundPresetMovement == null)
            {
                // movement sound preset is not overridden - use from this skeleton
                soundPresetMovement = protoSkeleton.SoundPresetFootsteps;
            }

            // use some pitch variation
            var pitch = RandomHelper.Range(0.95f, 1.05f);

            var volume = protoSkeleton.VolumeFootsteps;
            // apply some volume variation
            volume *= RandomHelper.Range(0.85f, 1.0f);
            // apply constant volume multiplier
            volume *= SoundConstants.VolumeFootstepsMultiplier;

            soundPresetMovement.PlaySound(
                groundSoundMaterial,
                character,
                volume: (float)volume,
                pitch: pitch);
        }

        private ReadOnlySoundPreset<CharacterSound> PrepareSoundPresetCharacter()
        {
            if (!Api.Shared.IsFolderExists(ContentPaths.Sounds + this.SoundsFolderPath))
            {
                throw new Exception("Sounds folder for " + this + " doesn't exist");
            }

            var preset = SoundPreset.CreateFromFolder<CharacterSound>(
                this.SoundsFolderPath + "/Character/",
                throwExceptionIfNoFilesFound: false);
            //this.VerifySoundPreset(preset);
            return preset;
        }

        private ReadOnlySoundPreset<CharacterSound> PrepareSoundPresetMovement()
        {
            var preset = SoundPreset.CreateFromFolder<CharacterSound>(
                this.SoundsFolderPath + "/Movement/",
                throwExceptionIfNoFilesFound: false);
            //this.VerifySoundPreset(preset);
            return preset;
        }

        private ReadOnlySoundPreset<WeaponSound> PrepareSoundPresetWeapon()
        {
            var preset = SoundPreset.CreateFromFolder<WeaponSound>(
                this.SoundsFolderPath + "/Weapon/",
                throwExceptionIfNoFilesFound: false);
            //this.VerifySoundPreset(preset);
            return preset;
        }
    }
}