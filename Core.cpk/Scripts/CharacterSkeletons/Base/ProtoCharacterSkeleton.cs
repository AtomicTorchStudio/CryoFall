namespace AtomicTorch.CBND.CoreMod.CharacterSkeletons
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Windows.Media;
    using AtomicTorch.CBND.CoreMod.Characters.Input;
    using AtomicTorch.CBND.CoreMod.Characters.Player;
    using AtomicTorch.CBND.CoreMod.Helpers.Client;
    using AtomicTorch.CBND.CoreMod.Helpers.Primitives;
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
    using AtomicTorch.GameEngine.Common.Primitives;
    using JetBrains.Annotations;

    public abstract class ProtoCharacterSkeleton : ProtoEntity, IProtoCharacterSkeleton
    {
        protected ProtoCharacterSkeleton()
        {
            this.Name = this.GetType().Name;
        }

        /// <summary>
        /// Move speed at the default scale, in world units (tiles) per second.
        /// </summary>
        public abstract double DefaultMoveSpeed { get; }

        public abstract bool HasMoveStartAnimations { get; }

        public virtual bool HasStaticAttackAnimations { get; } = true;

        public virtual Vector2D InventoryOffset => Vector2D.Zero;

        public virtual double InventoryScale => 1;

        public override string Name { get; }

        public abstract float OrientationDownExtraAngle { get; }

        public abstract float OrientationThresholdDownHorizontalFlipDeg { get; }

        public abstract float OrientationThresholdDownToUpFlipDeg { get; }

        public abstract float OrientationThresholdUpHorizontalFlipDeg { get; }

        public abstract SkeletonResource SkeletonResourceBack { get; }

        public abstract SkeletonResource SkeletonResourceFront { get; }

        public abstract string SlotNameItemInHand { get; }

        public ReadOnlySoundPreset<CharacterSound> SoundPresetCharacter { get; private set; }

        public ReadOnlySoundPreset<GroundSoundMaterial> SoundPresetFootsteps { get; private set; }

        public ReadOnlySoundPreset<CharacterSound> SoundPresetMovement { get; private set; }

        public ReadOnlySoundPreset<WeaponSound> SoundPresetWeapon { get; private set; }

        public virtual SoundResource SoundResourceAimingProcess { get; }

        public virtual double SpeedMultiplier => 1;

        public virtual double WorldScale => 1;

        protected virtual RangeDouble FootstepsPitchVariationRange { get; }
            = new RangeDouble(0.95, 1.05);

        protected virtual RangeDouble FootstepsVolumeVariationRange { get; }
            = new RangeDouble(0.85, 1.0);

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

            if (rendererShadow is null)
            {
                return null;
            }

            rendererShadow.Color = Color.FromArgb(0xAA, 0x00, 0x00, 0x00);

            rendererShadow.DrawOrder = DrawOrder.Shadow;
            this.ClientSetupShadowRenderer(rendererShadow, scaleMultiplier);
            return rendererShadow;
        }

        public virtual void ClientGetAimingOrientation(
            [CanBeNull] ICharacter character,
            double angleRad,
            ViewOrientation lastViewOrientation,
            out ViewOrientation viewOrientation,
            out float aimCoef)
        {
            var angleDeg = angleRad * MathConstants.RadToDeg;
            viewOrientation = new ViewOrientation(
                isUp: this.ClientIsOrientedUp(angleDeg),
                isLeft: ClientCharacterAnimationHelper.IsLeftHalfOfCircle(angleDeg));

            if (viewOrientation.IsLeft == lastViewOrientation.IsLeft
                && viewOrientation.IsUp != lastViewOrientation.IsUp)
            {
                // switched up/down
                if (lastViewOrientation.IsUp)
                {
                    // COMMENTED OUT - we don't need tolerance for keeping the up orientation
                    //// if view orientation was up, but now down
                    //if (lastViewOrientation.IsLeft)
                    //{
                    //	// if view orientation was up-left, but now down-left
                    //	if (angleDeg < 90 + toleranceUpVerticalFlipDeg)
                    //	{
                    //		// keep up-left orientation
                    //		viewOrientation.IsUp = true;
                    //	}
                    //}
                    //else // if view orientation was up-right, but now down-right
                    //{
                    //	if (angleDeg > 90 - toleranceUpVerticalFlipDeg)
                    //	{
                    //		// keep up-right orientation
                    //		viewOrientation.IsUp = true;
                    //	}
                    //}
                }
                else
                {
                    // if view orientation was down, but now up
                    if (lastViewOrientation.IsLeft)
                    {
                        // if view orientation was down-left, but now up-left
                        if (angleDeg > 180 - this.OrientationThresholdDownToUpFlipDeg)
                        {
                            // keep down-left orientation
                            viewOrientation.IsUp = false;
                        }
                    }
                    else // if view orientation was down-right, but now up-right
                    {
                        if (angleDeg < this.OrientationThresholdDownToUpFlipDeg)
                        {
                            // keep down-right orientation
                            viewOrientation.IsUp = false;
                        }
                    }
                }
            }
            else if (viewOrientation.IsLeft != lastViewOrientation.IsLeft
                     && viewOrientation.IsUp == lastViewOrientation.IsUp)
            {
                // switched left/right
                if (lastViewOrientation.IsUp)
                {
                    if (lastViewOrientation.IsLeft)
                    {
                        // if view orientation was left-up, but now right-up
                        if (angleDeg > 90 - this.OrientationThresholdUpHorizontalFlipDeg)
                        {
                            // keep up-left orientation
                            viewOrientation.IsLeft = true;
                        }
                    }
                    else
                    {
                        // if view orientation was right-up, but now left-up
                        if (angleDeg < 90 + this.OrientationThresholdUpHorizontalFlipDeg)
                        {
                            // keep up-right orientation
                            viewOrientation.IsLeft = false;
                        }
                    }
                }
                else
                {
                    if (lastViewOrientation.IsLeft)
                    {
                        // if view orientation was left-down, but now right-down
                        if (angleDeg < 270 + this.OrientationThresholdDownHorizontalFlipDeg
                            && angleDeg > 270)
                        {
                            // keep down-left orientation
                            viewOrientation.IsLeft = true;
                        }
                    }
                    else
                    {
                        // if view orientation was right-down, but now left-down
                        if (angleDeg > 270 - this.OrientationThresholdDownHorizontalFlipDeg
                            && angleDeg < 270)
                        {
                            // keep down-right orientation
                            viewOrientation.IsLeft = false;
                        }
                    }
                }
            }

            // let's calculate aim coef
            var aimAngle = angleDeg;
            if (viewOrientation.IsUp)
            {
                // offset angle
                aimAngle += 45;
                // calculated angle between 0 and 270 degrees
                aimAngle %= 360;
                // calculate coef (from 0 to 2)
                aimCoef = (float)(aimAngle / 180);

                if (viewOrientation.IsLeft)
                {
                    // if left orientation, aimCoef values will be from 1 to 2
                    // remap them to values from 1 to 0 respectively
                    aimCoef = 1.5f - aimCoef;
                }
            }
            else // if oriented down
            {
                if (aimAngle < 90)
                {
                    // it means we're in first quarter, extend aimAngle
                    // on 360 degrees to keep coordinates continuum (from 180*3/4 to 180*(2+(1/4)))
                    aimAngle += 360;
                }

                // offset angle
                aimAngle -= 45;
                // calculated angle between 0 and 270 degrees
                aimAngle = 360 - aimAngle;
                // calculate coef (from 0 to 2)
                aimCoef = (float)(aimAngle / 180);

                if (viewOrientation.IsLeft)
                {
                    // if left orientation, aimCoef values will be from 1 to 2
                    // remap them to values from 1 to 0 respectively
                    aimCoef = 1.5f - aimCoef;
                }

                // invert coefficient
                aimCoef = 1f - aimCoef;
            }

            //Api.Logger.WriteDev(
            //    $"AngleDeg: {angleDeg:F2}. Aiming coef: {aimCoef:F2}. Current view data: isUp={viewOrientation.IsUp} isLeft={viewOrientation.IsLeft}");
        }

        public virtual void ClientResetItemInHand(IComponentSkeleton skeletonRenderer)
        {
            skeletonRenderer.SetAttachment(this.SlotNameItemInHand, attachmentName: null);
        }

        public virtual void ClientSetupItemInHand(
            IComponentSkeleton skeletonRenderer,
            string attachmentName,
            TextureResource textureResource)
        {
            skeletonRenderer.SetAttachmentSprite(this.SlotNameItemInHand, attachmentName, textureResource);
            skeletonRenderer.SetAttachment(this.SlotNameItemInHand, attachmentName);
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

        public void GetCurrentAnimationSetting(
            ICharacter character,
            CharacterMoveModes moveModes,
            double angle,
            ViewOrientation lastViewOrientation,
            out string starterAnimationName,
            out string currentAnimationName,
            out DrawMode currentDrawMode,
            out float aimCoef,
            out ViewOrientation viewOrientation,
            out bool isIdle)
        {
            this.ClientGetAimingOrientation(character,
                                            angle,
                                            lastViewOrientation,
                                            out viewOrientation,
                                            out aimCoef);

            currentDrawMode = viewOrientation.IsLeft ? DrawMode.Default : DrawMode.FlipHorizontally;

            isIdle = moveModes == CharacterMoveModes.None;
            if (isIdle)
            {
                starterAnimationName = null;
                currentAnimationName = "Idle";
                return;
            }

            if ((moveModes & (CharacterMoveModes.Left | CharacterMoveModes.Right))
                != CharacterMoveModes.None)
            {
                if (viewOrientation.IsLeft && (moveModes & CharacterMoveModes.Left) != 0
                    || !viewOrientation.IsLeft && (moveModes & CharacterMoveModes.Right) != 0)
                {
                    starterAnimationName = "RunSideStart";
                    currentAnimationName = "RunSide";
                }
                else
                {
                    starterAnimationName = "RunSideBackwardStart";
                    currentAnimationName = "RunSideBackward";
                }
            }
            else
            {
                // going up or down
                var isMoveUp = (moveModes & CharacterMoveModes.Up) != 0;
                starterAnimationName = isMoveUp ? "RunUpStart" : "RunDownStart";
                currentAnimationName = isMoveUp ? "RunUp" : "RunDown";
            }
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
                        case IProtoItemEquipmentArmor chest:
                            soundPresetCharacter = chest.SoundPresetMovementOverride;
                            break;

                        case IProtoItemEquipmentHead head:
                            soundPresetMovement = head.SoundPresetCharacterOverride;
                            break;
                    }
                }
            }

            soundPresetCharacter ??= this.SoundPresetCharacter;
            soundPresetMovement ??= this.SoundPresetMovement;

            return (soundPresetCharacter, soundPresetMovement);
        }

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

        protected bool ClientIsOrientedUp(double angleDeg)
        {
            // we consider upper half-circle as starting from "extra angle" to 180-"extra angle" degrees, not 0 to 180
            var extraAngle = this.OrientationDownExtraAngle;
            return angleDeg > extraAngle
                   && angleDeg < 180.0 - extraAngle;
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

        protected virtual ReadOnlySoundPreset<CharacterSound> PrepareSoundPresetCharacter()
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

            if (protoGroundObject is not null)
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
            if (protoCharacter is PlayerCharacter
                && PlayerCharacter.GetPublicState(character).CurrentVehicle is null)
            {
                // try get movement sound preset override
                foreach (var item in character.SharedGetPlayerContainerEquipment().Items)
                {
                    if (item.ProtoItem is IProtoItemEquipmentArmor protoItemEquipmentArmor)
                    {
                        soundPresetMovement = protoItemEquipmentArmor.SoundPresetFootstepsOverride;
                        break;
                    }
                }
            }

            var protoSkeleton = (ProtoCharacterSkeleton)protoCharacter.ClientGetCurrentProtoSkeleton(character);
            if (soundPresetMovement is null)
            {
                // movement sound preset is not overridden - use from this skeleton
                soundPresetMovement = protoSkeleton.SoundPresetFootsteps;
            }

            // use some pitch variation
            var pitch = RandomHelper.Range(protoSkeleton.FootstepsPitchVariationRange.From,
                                           protoSkeleton.FootstepsPitchVariationRange.To);

            var volume = protoSkeleton.VolumeFootsteps;
            // apply some volume variation
            volume *= RandomHelper.Range(protoSkeleton.FootstepsVolumeVariationRange.From,
                                         protoSkeleton.FootstepsVolumeVariationRange.To);
            // apply constant volume multiplier
            volume *= SoundConstants.VolumeFootstepsMultiplier;

            soundPresetMovement.PlaySound(
                groundSoundMaterial,
                character,
                volume: (float)volume,
                pitch: (float)pitch);
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