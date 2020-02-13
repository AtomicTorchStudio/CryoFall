namespace AtomicTorch.CBND.CoreMod.Systems.Weapons
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using AtomicTorch.CBND.CoreMod.Characters;
    using AtomicTorch.CBND.CoreMod.Characters.Input;
    using AtomicTorch.CBND.CoreMod.CharacterSkeletons;
    using AtomicTorch.CBND.CoreMod.ClientComponents.FX;
    using AtomicTorch.CBND.CoreMod.ClientComponents.Rendering;
    using AtomicTorch.CBND.CoreMod.ClientComponents.Rendering.Lighting;
    using AtomicTorch.CBND.CoreMod.Helpers.Client;
    using AtomicTorch.CBND.CoreMod.Items.Ammo;
    using AtomicTorch.CBND.CoreMod.Items.Weapons;
    using AtomicTorch.CBND.CoreMod.SoundPresets;
    using AtomicTorch.CBND.CoreMod.Systems.Party;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.WorldObjects.SoundCue;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.ServicesClient.Components;
    using AtomicTorch.CBND.GameApi.ServicesClient.Rendering;
    using AtomicTorch.GameEngine.Common.Helpers;
    using AtomicTorch.GameEngine.Common.Primitives;

    public static class WeaponSystemClientDisplay
    {
        private static readonly ISharedApi Shared = Api.Shared;

        public static void OnWeaponFinished(ICharacter character)
        {
            if (character == null
                || !character.IsInitialized)
            {
                return;
            }

            var clientState = character.GetClientState<BaseCharacterClientState>();
            if (!clientState.HasWeaponAnimationAssigned
                && !clientState.IsWeaponFiringAnimationActive)
            {
                return;
            }

            var weaponProto = GetCharacterCurrentWeaponProto(character);
            weaponProto.SoundPresetWeapon.PlaySound(
                WeaponSound.Stop,
                character,
                volume: SoundConstants.VolumeWeapon);

            // ensure that the weapon is stopped too
            OnWeaponInputStop(character);
        }

        public static void OnWeaponHitOrTrace(
            ICharacter firingCharacter,
            IProtoItemWeapon protoWeapon,
            IProtoItemAmmo protoAmmo,
            IProtoCharacter protoCharacter,
            Vector2Ushort fallbackCharacterPosition,
            IReadOnlyList<WeaponHitData> hitObjects,
            Vector2D endPosition,
            bool endsWithHit)
        {
            if (firingCharacter != null
                && !firingCharacter.IsInitialized)
            {
                firingCharacter = null;
            }

            var worldPositionSource = CalculateWeaponShotWorldPositon(firingCharacter,
                                                                      protoWeapon,
                                                                      protoCharacter,
                                                                      fallbackCharacterPosition.ToVector2D());

            var isRangedWeapon = protoWeapon is IProtoItemWeaponRanged;
            var weaponTracePreset = protoWeapon.FireTracePreset
                                    ?? protoAmmo?.FireTracePreset;
            if (isRangedWeapon)
            {
                ComponentWeaponTrace.Create(weaponTracePreset,
                                            worldPositionSource,
                                            endPosition,
                                            hasHit: endsWithHit,
                                            lastHitData: hitObjects.LastOrDefault(t => t.WorldObject != null));
            }

            foreach (var hitData in hitObjects)
            {
                var hitWorldObject = hitData.WorldObject;
                if (hitWorldObject != null
                    && !hitWorldObject.IsInitialized)
                {
                    hitWorldObject = null;
                }

                var protoWorldObject = hitData.FallbackProtoWorldObject;

                double delay;
                {
                    var worldObjectPosition = CalculateWorldObjectPosition(hitWorldObject, hitData);
                    delay = isRangedWeapon
                                ? ComponentWeaponTrace.CalculateTimeToHit(weaponTracePreset,
                                                                          worldPositionSource: worldPositionSource,
                                                                          endPosition: worldObjectPosition
                                                                                       + hitData.HitPoint.ToVector2D())
                                : 0;
                }

                ClientTimersSystem.AddAction(
                    delay,
                    () =>
                    {
                        // re-calculate the world object position
                        var worldObjectPosition = CalculateWorldObjectPosition(hitWorldObject, hitData);

                        var volume = SoundConstants.VolumeHit;
                        // apply some volume variation
                        volume *= RandomHelper.Range(0.8f, 1.0f);
                        var pitch = RandomHelper.Range(0.95f, 1.05f);

                        // adjust volume proportionally to the number of scattered projects per fire
                        var fireScatterPreset = protoAmmo?.OverrideFireScatterPreset
                                                ?? protoWeapon.FireScatterPreset;
                        var projectilesCount = fireScatterPreset.ProjectileAngleOffets.Length;
                        volume *= (float)Math.Pow(1.0 / projectilesCount, 0.35);

                        var objectMaterial = hitData.FallbackObjectMaterial;
                        if (hitWorldObject is ICharacter hitCharacter
                            && hitCharacter.IsInitialized)
                        {
                            objectMaterial = ((IProtoCharacterCore)hitCharacter.ProtoCharacter)
                                .SharedGetObjectMaterialForCharacter(hitCharacter);
                        }

                        if (hitWorldObject != null)
                        {
                            protoWeapon.SoundPresetHit.PlaySound(
                                objectMaterial,
                                hitWorldObject,
                                volume: volume,
                                pitch: pitch);
                        }
                        else
                        {
                            protoWeapon.SoundPresetHit.PlaySound(
                                objectMaterial,
                                protoWorldObject,
                                worldPosition: worldObjectPosition,
                                volume: volume,
                                pitch: pitch);
                        }

                        if (weaponTracePreset != null)
                        {
                            AddHitSparks(weaponTracePreset.HitSparksPreset,
                                         hitData,
                                         hitWorldObject,
                                         protoWorldObject,
                                         worldObjectPosition,
                                         projectilesCount,
                                         objectMaterial,
                                         isRangedWeapon);
                        }
                    });

                static Vector2D CalculateWorldObjectPosition(IWorldObject worldObject, WeaponHitData hitData)
                {
                    return worldObject switch
                    {
                        IDynamicWorldObject dynamicWorldObject => dynamicWorldObject.Position,
                        IStaticWorldObject _                   => worldObject.TilePosition.ToVector2D(),
                        _                                      => hitData.FallbackTilePosition.ToVector2D()
                    };
                }
            }
        }

        public static void OnWeaponInputStop(ICharacter character)
        {
            if (character == null
                || !character.IsInitialized)
            {
                return;
            }

            var clientState = character.GetClientState<BaseCharacterClientState>();
            if (!clientState.HasWeaponAnimationAssigned
                && !clientState.IsWeaponFiringAnimationActive)
            {
                return;
            }

            clientState.IsWeaponFiringAnimationActive = false;

            // break animation loops (if any)
            var skeletonRenderer = clientState.SkeletonRenderer;
            StopAnimation(AnimationTrackIndexes.ItemFiring);
            StopAnimation(AnimationTrackIndexes.ItemFiringStatic);

            void StopAnimation(byte trackIndex)
            {
                if (skeletonRenderer.GetCurrentAnimationName(trackIndex) == null)
                {
                    return;
                }

                skeletonRenderer.SetAnimationLoopMode(trackIndex: trackIndex, isLooped: false);
                skeletonRenderer.RemoveAnimationTrackNextEntries(trackIndex);
                skeletonRenderer.AddEmptyAnimation(trackIndex: trackIndex);
            }
        }

        public static void OnWeaponShot(
            ICharacter character,
            uint partyId,
            IProtoItemWeapon protoWeapon,
            IProtoCharacter protoCharacter,
            Vector2Ushort fallbackPosition)
        {
            if (character != null
                && !character.IsInitialized)
            {
                character = null;
            }

            var position = character?.Position ?? fallbackPosition.ToVector2D();
            position += (0, protoCharacter.CharacterWorldWeaponOffsetRanged);

            ClientSoundCueManager.OnSoundEvent(position,
                                               isPartyMember: partyId > 0
                                                              && partyId == PartySystem.ClientCurrentParty?.Id);

            const float volume = SoundConstants.VolumeWeapon;
            var pitch = RandomHelper.Range(0.95f, 1.05f);

            IComponentSoundEmitter emitter;
            var soundPresetWeapon = protoWeapon.SoundPresetWeapon;
            if (soundPresetWeapon.HasSound(WeaponSound.Shot))
            {
                // play shot sound from weapon
                soundPresetWeapon.PlaySound(WeaponSound.Shot,
                                            protoWorldObject: protoCharacter,
                                            worldPosition: position,
                                            out emitter,
                                            volume: volume,
                                            pitch: pitch);
            }
            else
            {
                // play sounds from the skeleton instead
                ProtoCharacterSkeleton characterSkeleton = null;
                if (character != null
                    && character.IsInitialized)
                {
                    var clientState = character.GetClientState<BaseCharacterClientState>();
                    if (clientState.HasWeaponAnimationAssigned)
                    {
                        characterSkeleton = clientState.CurrentProtoSkeleton;
                    }
                }
                else
                {
                    protoCharacter.SharedGetSkeletonProto(character: null,
                                                          out var characterSkeleton1,
                                                          out _);
                    characterSkeleton = (ProtoCharacterSkeleton)characterSkeleton1;
                }

                if (characterSkeleton == null)
                {
                    emitter = null;
                }
                else
                {
                    if (!characterSkeleton.SoundPresetWeapon.PlaySound(WeaponSound.Shot,
                                                                       protoWorldObject: protoCharacter,
                                                                       worldPosition: position,
                                                                       out emitter,
                                                                       volume))
                    {
                        // no method returned true
                        // fallback to the default weapon sound (if there is no, it will be logged into the audio log)
                        soundPresetWeapon.PlaySound(WeaponSound.Shot,
                                                    protoWorldObject: protoCharacter,
                                                    worldPosition: position,
                                                    out emitter,
                                                    volume: volume,
                                                    pitch: pitch);
                    }
                }
            }

            if (emitter != null)
            {
                var distance = protoWeapon.SoundPresetWeaponDistance;
                emitter.CustomMinDistance = distance.min;
                emitter.CustomMaxDistance = distance.max;
            }

            if (protoWeapon is IProtoItemWeaponRanged rangedWeapon
                && character != null
                && ReferenceEquals(protoWeapon, GetCharacterCurrentWeaponProto(character)))
            {
                // add muzzle flash
                var clientState = character.GetClientState<BaseCharacterClientState>();
                if (!clientState.HasWeaponAnimationAssigned)
                {
                    return;
                }

                var skeletonRenderer = clientState.SkeletonRenderer;
                CreateMuzzleFlash(rangedWeapon, character, skeletonRenderer);

                var recoilAnimationName = rangedWeapon.CharacterAnimationAimingRecoilName;
                if (recoilAnimationName != null
                    && rangedWeapon.CharacterAnimationAimingRecoilPower > 0
                    && rangedWeapon.CharacterAnimationAimingRecoilDuration > 0)
                {
                    SetRecoilAnimation(character, rangedWeapon, skeletonRenderer);
                }
            }
        }

        public static void OnWeaponStart(ICharacter character)
        {
            if (character == null
                || !character.IsInitialized)
            {
                return;
            }

            var clientState = character.GetClientState<BaseCharacterClientState>();
            if (!clientState.HasWeaponAnimationAssigned)
            {
                return;
            }

            clientState.IsWeaponFiringAnimationActive = true;

            var weapon = GetCharacterCurrentWeaponProto(character);
            weapon.SoundPresetWeapon.PlaySound(
                WeaponSound.Start,
                character,
                volume: SoundConstants.VolumeWeapon);

            var fireAnimationName = weapon.GetCharacterAnimationNameFire(character);
            if (fireAnimationName == null)
            {
                return;
            }

            SetFiringAnimation(
                weapon,
                clientState.SkeletonRenderer,
                AnimationTrackIndexes.ItemFiring,
                fireAnimationName,
                weapon.FireAnimationDuration,
                weapon.FireInterval,
                mixWithCurrent: false,
                isLooped: weapon.IsLoopedAttackAnimation);

            RefreshCurrentAttackAnimation(character);
        }

        public static void RefreshCurrentAttackAnimation(ICharacter character)
        {
            var clientState = character.GetClientState<BaseCharacterClientState>();
            var protoSkeleton = clientState.CurrentProtoSkeleton;
            var skeletonRenderer = clientState.SkeletonRenderer;

            // determine whether the character is staying or moving
            var input = character.GetPublicState<ICharacterPublicState>().AppliedInput;
            var isCharacterMoving = input.MoveModes != CharacterMoveModes.None;
            if (isCharacterMoving)
            {
                // character is moving - remove attack animation static part
                skeletonRenderer.RemoveAnimationTrack(AnimationTrackIndexes.ItemFiringStatic);
            }

            if (!clientState.HasWeaponAnimationAssigned
                || !clientState.IsWeaponFiringAnimationActive)
            {
                return;
            }

            var weapon = GetCharacterCurrentWeaponProto(character);
            var firingAnimationName = weapon.GetCharacterAnimationNameFire(character);
            if (firingAnimationName == null)
            {
                // no animation needed
                return;
            }

            if (weapon.IsLoopedAttackAnimation)
            {
                var lastFiringAnimationName = skeletonRenderer.GetLatestAddedAnimationName(
                    AnimationTrackIndexes.ItemFiring);

                if (!skeletonRenderer.IsLooped(AnimationTrackIndexes.ItemFiring))
                {
                    // weapon stopped firing or already scheduled the next attack animation
                }
                else if (firingAnimationName != lastFiringAnimationName)
                {
                    // replace animation with the proper one
                    SetFiringAnimation(
                        weapon,
                        skeletonRenderer,
                        AnimationTrackIndexes.ItemFiring,
                        firingAnimationName,
                        weapon.FireAnimationDuration,
                        weapon.FireInterval,
                        mixWithCurrent: true,
                        isLooped: weapon.IsLoopedAttackAnimation);
                }
            }

            if (!isCharacterMoving
                && protoSkeleton.HasStaticAttackAnimations)
            {
                RefreshStaticAttackAnimation(skeletonRenderer, weapon, firingAnimationName);
            }
        }

        private static void AddHitSparks(
            IReadOnlyWeaponHitSparksPreset hitSparksPreset,
            WeaponHitData hitData,
            IWorldObject hitWorldObject,
            IProtoWorldObject protoWorldObject,
            Vector2D worldObjectPosition,
            int projectilesCount,
            ObjectMaterial objectMaterial,
            bool isRangedWeapon)
        {
            var sceneObject = Api.Client.Scene.CreateSceneObject("Temp_HitSparks");
            sceneObject.Position = worldObjectPosition;
            var hitPoint = hitData.HitPoint.ToVector2D();

            if (!hitData.IsCliffsHit)
            {
                // move hitpoint a bit closer to the center of the object
                hitPoint = WeaponSystem.SharedOffsetHitWorldPositionCloserToObjectCenter(
                    hitWorldObject,
                    protoWorldObject,
                    hitPoint,
                    isRangedWeapon);
            }

            if (projectilesCount == 1
                && !isRangedWeapon)
            {
                // randomize hitpoint a bit by adding a little random offset
                var maxOffsetDistance = 0.2;
                var range = maxOffsetDistance * RandomHelper.NextDouble();
                var angleRad = 2 * Math.PI * RandomHelper.NextDouble();
                var randomOffset = new Vector2D(range * Math.Cos(angleRad),
                                                range * Math.Sin(angleRad));

                hitPoint += randomOffset;
            }

            var sparksEntry = hitSparksPreset.GetForMaterial(objectMaterial);
            var componentSpriteRender = Api.Client.Rendering.CreateSpriteRenderer(
                sceneObject,
                positionOffset: hitPoint,
                spritePivotPoint: (0.5, 0.225),
                drawOrder: isRangedWeapon
                               ? DrawOrder.Light
                               : DrawOrder.Default);
            componentSpriteRender.DrawOrderOffsetY = -hitPoint.Y;
            componentSpriteRender.Scale = (float)Math.Pow(1.0 / projectilesCount, 0.35);

            if (sparksEntry.UseScreenBlending)
            {
                componentSpriteRender.BlendMode = BlendMode.Screen;
            }

            if (!isRangedWeapon)
            {
                componentSpriteRender.RotationAngleRad = (float)(RandomHelper.NextDouble() * 2 * Math.PI);
            }

            const double animationFrameDuration = 1 / 30.0;
            var componentAnimator = sceneObject.AddComponent<ClientComponentSpriteSheetAnimator>();
            var hitSparksEntry = sparksEntry;
            componentAnimator.Setup(
                componentSpriteRender,
                hitSparksEntry.SpriteSheetAnimationFrames,
                frameDurationSeconds: animationFrameDuration,
                isLooped: false);

            var totalAnimationDuration = animationFrameDuration * componentAnimator.FramesCount;
            var totalDurationWithLight = 0.15 + totalAnimationDuration;
            if (hitSparksEntry.LightColor.HasValue)
            {
                // create light spot (even for melee weapons)
                var lightSource = ClientLighting.CreateLightSourceSpot(
                    sceneObject,
                    color: hitSparksEntry.LightColor.Value,
                    spritePivotPoint: (0.5, 0.5),
                    size: 7,
                    // we don't want to display nickname/healthbar for the firing character, it's too quick anyway
                    logicalSize: 0,
                    positionOffset: hitPoint);

                ClientComponentOneShotLightAnimation.Setup(lightSource, totalDurationWithLight);
            }

            componentSpriteRender.Destroy(totalAnimationDuration);
            componentAnimator.Destroy(totalAnimationDuration);

            sceneObject.Destroy(totalDurationWithLight);
        }

        private static Vector2D CalculateWeaponShotWorldPositon(
            ICharacter character,
            IProtoItemWeapon protoWeapon,
            IProtoCharacter protoCharacter,
            Vector2D fallbackPosition)
        {
            Vector2D worldPositionSource;
            if (character != null
                && character.IsInitialized)
            {
                if (character.GetClientState<BaseCharacterClientState>().SkeletonRenderer is {} skeletonRenderer
                    && protoWeapon is IProtoItemWeaponRanged protoItemWeaponRanged)
                {
                    var protoSkeleton = character.GetClientState<BaseCharacterClientState>().CurrentProtoSkeleton;
                    var slotName = protoSkeleton.SlotNameItemInHand;
                    var weaponSlotScreenOffset = skeletonRenderer.GetSlotScreenOffset(attachmentName: slotName);
                    var muzzleFlashTextureOffset = protoItemWeaponRanged.MuzzleFlashDescription.TextureScreenOffset;
                    var boneWorldPosition = skeletonRenderer.TransformSlotPosition(
                        slotName,
                        weaponSlotScreenOffset + (Vector2F)muzzleFlashTextureOffset,
                        out _);
                    worldPositionSource = boneWorldPosition;
                }
                else
                {
                    worldPositionSource = character.Position;
                    worldPositionSource += (0, protoCharacter.CharacterWorldWeaponOffsetRanged);
                }
            }
            else
            {
                worldPositionSource = fallbackPosition;
                worldPositionSource += (0, protoCharacter.CharacterWorldWeaponOffsetRanged);
            }

            return worldPositionSource;
        }

        private static void CreateMuzzleFlash(
            IProtoItemWeaponRanged protoWeapon,
            ICharacter characterWorldObject,
            IComponentSkeleton skeletonRenderer)
        {
            if (protoWeapon == null)
            {
                // not a ranged weapon
                return;
            }

            // get scene object (of character) to attach the components to
            var sceneObject = characterWorldObject.ClientSceneObject;
            sceneObject.AddComponent<ClientComponentMuzzleFlash>()
                       .Setup(characterWorldObject, skeletonRenderer, protoWeapon);
        }

        private static IProtoItemWeapon GetCharacterCurrentWeaponProto(ICharacter character)
        {
            return character.GetPublicState<ICharacterPublicState>().SelectedItemWeaponProto;
        }

        private static void RefreshStaticAttackAnimation(
            IComponentSkeleton skeletonRenderer,
            IProtoItemWeapon weapon,
            string firingAnimationName)
        {
            // ReSharper disable once CanExtractXamlLocalizableStringCSharp
            var staticFiringAnimationName = firingAnimationName + "_Static";
            var lastStaticAnimationName = skeletonRenderer.GetLatestAddedAnimationName(
                AnimationTrackIndexes.ItemFiringStatic);

            if (lastStaticAnimationName == staticFiringAnimationName)
            {
                return;
            }

            if (skeletonRenderer.GetLatestAddedAnimationName(AnimationTrackIndexes.ItemFiring)
                == null)
            {
                // no current firing animation - no need the static animation
                return;
            }

            // need to update the static attack animation
            SetFiringAnimation(
                weapon,
                skeletonRenderer,
                AnimationTrackIndexes.ItemFiringStatic,
                staticFiringAnimationName,
                weapon.FireAnimationDuration,
                weapon.FireInterval,
                mixWithCurrent: true,
                isLooped: weapon.IsLoopedAttackAnimation);

            // synchronize static with non-static animation tracks
            skeletonRenderer.SetAnimationTime(
                AnimationTrackIndexes.ItemFiringStatic,
                skeletonRenderer.GetAnimationTime(AnimationTrackIndexes.ItemFiring));
        }

        private static void SetFiringAnimation(
            IProtoItemWeapon weaponProto,
            IComponentSkeleton skeletonRenderer,
            byte trackIndex,
            string fireAnimationName,
            double fireAnimationDuration,
            double fireInterval,
            bool mixWithCurrent,
            bool isLooped)
        {
            fireAnimationDuration = Shared.RoundDurationByServerFrameDuration(fireAnimationDuration);
            fireInterval = Shared.RoundDurationByServerFrameDuration(fireInterval);

            var currentFireAnimation = skeletonRenderer.GetCurrentAnimationName(trackIndex);
            if (currentFireAnimation == fireAnimationName)
            {
                // the same firing animation is already playing
                Api.Logger.Warning(
                    "Will overwrite current attack animation: "
                    + currentFireAnimation
                    + " - usually it means that the DamageApplyDelaySeconds+FireIntervalSeconds is lower than the attack animation duration for "
                    + weaponProto
                    + " (they must be matching perfectly).");
            }

            if (currentFireAnimation != null)
            {
                if (mixWithCurrent)
                {
                    skeletonRenderer.SetAnimationLoopMode(trackIndex, isLooped: false);
                    skeletonRenderer.RemoveAnimationTrackNextEntries(trackIndex);
                }
                else
                {
                    skeletonRenderer.RemoveAnimationTrack(trackIndex);
                }
            }

            // cooldown is a padding duration which makes animation "stuck" on the last frame for the specified duration
            var cooldownDuration = isLooped
                                       // in looped animation we need to match its total duration to fire interval by using cooldown
                                       ? Math.Max(0, fireInterval - fireAnimationDuration)
                                       : 0; // non-looped animation no cooldown is necessary

            skeletonRenderer.AddAnimation(
                trackIndex,
                animationName: fireAnimationName,
                isLooped: isLooped,
                customDuration: (float)fireAnimationDuration,
                cooldownDuration: (float)cooldownDuration);
        }

        private static void SetRecoilAnimation(
            ICharacter character,
            IProtoItemWeaponRanged weaponProto,
            IComponentSkeleton skeletonRenderer)
        {
            var sceneObject = character.ClientSceneObject;
            var componentRecoil = sceneObject.FindComponent<ClientComponentCharacterWeaponRecoilAnimation>()
                                  ?? sceneObject.AddComponent<ClientComponentCharacterWeaponRecoilAnimation>();
            componentRecoil.StartRecoil(character, weaponProto, skeletonRenderer);
        }
    }
}