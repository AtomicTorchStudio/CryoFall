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
    using AtomicTorch.CBND.CoreMod.Systems.Faction;
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

        public static void ClientAddHitSparks(
            IReadOnlyWeaponHitSparksPreset hitSparksPreset,
            WeaponHitData hitData,
            IWorldObject hitWorldObject,
            IProtoWorldObject protoWorldObject,
            Vector2D worldObjectPosition,
            int projectilesCount,
            ObjectMaterial objectMaterial,
            bool randomizeHitPointOffset,
            bool randomRotation,
            double? rotationAngleRad,
            DrawOrder drawOrder,
            double scale = 1.0,
            double animationFrameDuration = 2 / 60.0)
        {
            var sceneObject = Api.Client.Scene.CreateSceneObject("Temp_HitSparks");
            sceneObject.Position = worldObjectPosition;
            var hitPoint = hitData.HitPoint.ToVector2D();

            var sparksEntry = hitSparksPreset.GetForMaterial(objectMaterial);
            if (!hitData.IsCliffsHit
                && randomizeHitPointOffset)
            {
                // move hitpoint a bit closer to the center of the object
                hitPoint = WeaponSystem.SharedOffsetHitWorldPositionCloserToObjectCenter(
                    hitWorldObject,
                    protoWorldObject,
                    hitPoint,
                    isRangedWeapon: randomizeHitPointOffset);
            }

            if (projectilesCount == 1
                && randomizeHitPointOffset
                && sparksEntry.AllowRandomizedHitPointOffset)
            {
                // randomize hitpoint a bit by adding a little random offset
                var maxOffsetDistance = 0.2;
                var range = maxOffsetDistance * RandomHelper.NextDouble();
                var angleRad = 2 * Math.PI * RandomHelper.NextDouble();
                var randomOffset = new Vector2D(range * Math.Cos(angleRad),
                                                range * Math.Sin(angleRad));

                hitPoint += randomOffset;
            }

            var componentSpriteRender = Api.Client.Rendering.CreateSpriteRenderer(
                sceneObject,
                positionOffset: hitPoint,
                spritePivotPoint: (0.5, sparksEntry.PivotY),
                drawOrder: drawOrder);
            componentSpriteRender.DrawOrderOffsetY = -hitPoint.Y;
            componentSpriteRender.Scale = (float)scale * Math.Pow(1.0 / projectilesCount, 0.35);

            if (sparksEntry.UseScreenBlending)
            {
                componentSpriteRender.BlendMode = BlendMode.Screen;
            }

            if (randomRotation)
            {
                componentSpriteRender.RotationAngleRad = (float)(RandomHelper.NextDouble() * 2 * Math.PI);
            }
            else if (rotationAngleRad.HasValue)
            {
                componentSpriteRender.RotationAngleRad = (float)(rotationAngleRad.Value - Math.PI / 2);
            }

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

        public static void ClientCreateMuzzleFlash(
            IProtoItemWeaponRanged protoWeapon,
            ICharacter characterWorldObject,
            IComponentSkeleton skeletonRenderer)
        {
            if (protoWeapon?.MuzzleFlashDescription.TextureAtlas is null)
            {
                // no muzzle flash defined for this weapon
                return;
            }

            // get scene object (of character) to attach the components to
            var sceneObject = characterWorldObject.ClientSceneObject;
            sceneObject.AddComponent<ClientComponentMuzzleFlash>()
                       .Setup(characterWorldObject, skeletonRenderer, protoWeapon);
        }

        public static void ClientOnWeaponFinished(ICharacter character)
        {
            if (character is null
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

            var weaponProto = SharedGetCharacterCurrentWeaponProto(character);
            weaponProto.SoundPresetWeapon.PlaySound(
                WeaponSound.Stop,
                character,
                volume: SoundConstants.VolumeWeapon);

            // ensure that the weapon is stopped too
            ClientOnWeaponInputStop(character);
        }

        public static void ClientOnWeaponHitOrTrace(
            ICharacter firingCharacter,
            IProtoItemWeapon protoWeapon,
            IProtoItemAmmo protoAmmo,
            IProtoCharacter protoCharacter,
            Vector2Ushort fallbackCharacterPosition,
            IReadOnlyList<WeaponHitData> hitObjects,
            Vector2D endPosition,
            bool endsWithHit)
        {
            if (firingCharacter is not null
                && !firingCharacter.IsInitialized)
            {
                firingCharacter = null;
            }

            var weaponTracePreset = protoWeapon.FireTracePreset
                                    ?? protoAmmo?.FireTracePreset;
            var worldPositionSource = SharedCalculateWeaponShotWorldPositon(
                firingCharacter,
                protoWeapon,
                protoCharacter,
                fallbackCharacterPosition.ToVector2D(),
                hasTrace: weaponTracePreset?.HasTrace ?? false);

            protoWeapon.ClientOnWeaponHitOrTrace(firingCharacter,
                                                 worldPositionSource,
                                                 protoWeapon,
                                                 protoAmmo,
                                                 protoCharacter,
                                                 fallbackCharacterPosition,
                                                 hitObjects,
                                                 endPosition,
                                                 endsWithHit);

            if (weaponTracePreset?.HasTrace ?? false)
            {
                ComponentWeaponTrace.Create(weaponTracePreset,
                                            worldPositionSource,
                                            endPosition,
                                            hasHit: endsWithHit,
                                            lastHitData: hitObjects.LastOrDefault(t => t.WorldObject is not null));
            }

            foreach (var hitData in hitObjects)
            {
                var hitWorldObject = hitData.WorldObject;
                if (hitWorldObject is not null
                    && !hitWorldObject.IsInitialized)
                {
                    hitWorldObject = null;
                }

                var protoWorldObject = hitData.FallbackProtoWorldObject;

                double delay;
                {
                    var worldObjectPosition = CalculateWorldObjectPosition(hitWorldObject, hitData);
                    delay = weaponTracePreset?.HasTrace ?? false
                                ? SharedCalculateTimeToHit(weaponTracePreset,
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

                        var fireScatterPreset = protoAmmo?.OverrideFireScatterPreset
                                                ?? protoWeapon.FireScatterPreset;
                        var projectilesCount = fireScatterPreset.ProjectileAngleOffets.Length;

                        var objectMaterial = hitData.FallbackObjectMaterial;
                        if (hitWorldObject is ICharacter hitCharacter
                            && hitCharacter.IsInitialized)
                        {
                            objectMaterial = ((IProtoCharacterCore)hitCharacter.ProtoCharacter)
                                .SharedGetObjectMaterialForCharacter(hitCharacter);
                        }

                        protoWeapon.ClientPlayWeaponHitSound(hitWorldObject,
                                                             protoWorldObject,
                                                             fireScatterPreset,
                                                             objectMaterial,
                                                             worldObjectPosition);

                        if (weaponTracePreset is null)
                        {
                            return;
                        }

                        var randomRotation = !weaponTracePreset.HasTrace;
                        double? angleRad = null;
                        if (!randomRotation)
                        {
                            var deltaPos = endPosition - worldPositionSource;
                            ComponentWeaponTrace.CalculateAngleAndDirection(deltaPos,
                                                                            out var angleRadLocal,
                                                                            out _);
                            angleRad = angleRadLocal;
                        }

                        ClientAddHitSparks(weaponTracePreset.HitSparksPreset,
                                           hitData,
                                           hitWorldObject,
                                           protoWorldObject,
                                           worldObjectPosition,
                                           projectilesCount,
                                           objectMaterial,
                                           randomizeHitPointOffset: !weaponTracePreset.HasTrace,
                                           randomRotation: randomRotation,
                                           rotationAngleRad: angleRad,
                                           drawOrder: weaponTracePreset.DrawHitSparksAsLight
                                                          ? DrawOrder.Light
                                                          : DrawOrder.Default);
                    });

                static Vector2D CalculateWorldObjectPosition(IWorldObject worldObject, WeaponHitData hitData)
                {
                    return worldObject switch
                    {
                        IDynamicWorldObject dynamicWorldObject => dynamicWorldObject.Position,
                        IStaticWorldObject                     => worldObject.TilePosition.ToVector2D(),
                        _                                      => hitData.FallbackTilePosition.ToVector2D()
                    };
                }
            }
        }

        public static void ClientOnWeaponInputStop(ICharacter character)
        {
            if (character is null
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
                if (skeletonRenderer.GetCurrentAnimationName(trackIndex) is null)
                {
                    return;
                }

                skeletonRenderer.SetAnimationLoopMode(trackIndex: trackIndex, isLooped: false);
                skeletonRenderer.RemoveAnimationTrackNextEntries(trackIndex);
                skeletonRenderer.AddEmptyAnimation(trackIndex: trackIndex);
            }
        }

        public static void ClientOnWeaponShot(
            ICharacter character,
            uint partyId,
            string clanTag,
            IProtoItemWeapon protoWeapon,
            IProtoCharacter protoCharacter,
            Vector2Ushort fallbackPosition)
        {
            if (character is not null
                && !character.IsInitialized)
            {
                character = null;
            }

            var position = character?.Position ?? fallbackPosition.ToVector2D();
            position += (0, protoCharacter.CharacterWorldWeaponOffsetRanged);

            var isByPartyMember = PartySystem.ClientIsCurrentParty(partyId);
            var isCurrentOrAllyFaction = FactionSystem.ClientIsCurrentOrAllyFaction(clanTag);
            ClientSoundCueManager.OnSoundEvent(
                position,
                isFriendly: isByPartyMember
                            || isCurrentOrAllyFaction);

            float volume = SoundConstants.VolumeWeapon;
            volume *= protoWeapon.ShotVolumeMultiplier;
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
                if (character is not null
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

                if (characterSkeleton is null)
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

            if (emitter is not null)
            {
                var distance = protoWeapon.SoundPresetWeaponDistance;
                emitter.CustomMinDistance = distance.min;
                emitter.CustomMaxDistance = distance.max;

                var distance3DSpread = protoWeapon.SoundPresetWeaponDistance3DSpread;
                emitter.CustomMinDistance3DSpread = distance3DSpread.min;
                emitter.CustomMaxDistance3DSpread = distance3DSpread.max;
            }

            if (character is not null
                && ReferenceEquals(protoWeapon, SharedGetCharacterCurrentWeaponProto(character)))
            {
                protoWeapon.ClientOnWeaponShot(character);
            }
        }

        public static void ClientOnWeaponStart(ICharacter character)
        {
            if (character is null
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

            var weapon = SharedGetCharacterCurrentWeaponProto(character);
            weapon.SoundPresetWeapon.PlaySound(
                WeaponSound.Start,
                character,
                volume: SoundConstants.VolumeWeapon);

            var fireAnimationName = weapon.GetCharacterAnimationNameFire(character);
            if (fireAnimationName is null)
            {
                return;
            }

            ClientSetFiringAnimation(
                weapon,
                clientState.SkeletonRenderer,
                AnimationTrackIndexes.ItemFiring,
                fireAnimationName,
                weapon.FireAnimationDuration,
                weapon.FireInterval,
                mixWithCurrent: false,
                isLooped: weapon.IsLoopedAttackAnimation);

            ClientRefreshCurrentAttackAnimation(character);
        }

        public static void ClientRefreshCurrentAttackAnimation(ICharacter character)
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

            var weapon = SharedGetCharacterCurrentWeaponProto(character);
            var firingAnimationName = weapon.GetCharacterAnimationNameFire(character);
            if (firingAnimationName is null)
            {
                // no animation needed
                return;
            }

            if (weapon.IsLoopedAttackAnimation)
            {
                var trackIndexFiring = AnimationTrackIndexes.ItemFiring;

                var currentAnimationName = skeletonRenderer.GetCurrentAnimationName(
                    trackIndexFiring,
                    getLatestAddedAnimation: false);
                var lastFiringAnimationName = skeletonRenderer.GetLatestAddedAnimationName(
                    trackIndexFiring);

                if (firingAnimationName == currentAnimationName
                    && firingAnimationName != lastFiringAnimationName)
                {
                    // stop future scheduled attack animations and restore the loop
                    //Api.Logger.Dev($"Will keep the current firing animation: {firingAnimationName}");
                    skeletonRenderer.RemoveAnimationTrackNextEntries(trackIndexFiring);
                    skeletonRenderer.SetAnimationLoopMode(trackIndexFiring, isLooped: true);
                }
                else if (firingAnimationName != lastFiringAnimationName)
                {
                    //Api.Logger.Dev($"replace firing animation with the proper one: {lastFiringAnimationName} -> {firingAnimationName}");
                    ClientSetFiringAnimation(
                        weapon,
                        skeletonRenderer,
                        trackIndexFiring,
                        firingAnimationName,
                        weapon.FireAnimationDuration,
                        weapon.FireInterval,
                        mixWithCurrent: true,
                        isLooped: true);
                }
            }

            if (!isCharacterMoving
                && protoSkeleton.HasStaticAttackAnimations)
            {
                ClientRefreshStaticAttackAnimation(skeletonRenderer, weapon);
            }
        }

        public static void ClientSetRecoilAnimation(
            ICharacter character,
            IProtoItemWeaponRanged weaponProto,
            IComponentSkeleton skeletonRenderer)
        {
            var sceneObject = character.ClientSceneObject;
            var componentRecoil = sceneObject.FindComponent<ClientComponentCharacterWeaponRecoilAnimation>()
                                  ?? sceneObject.AddComponent<ClientComponentCharacterWeaponRecoilAnimation>();
            componentRecoil.StartRecoil(character, weaponProto, skeletonRenderer);
        }

        public static double SharedCalculateTimeToHit(
            WeaponFireTracePreset weaponTracePreset,
            Vector2D worldPositionSource,
            Vector2D endPosition)
        {
            if (weaponTracePreset is null)
            {
                // no weapon trace for this weapon
                return 0;
            }

            var deltaPos = endPosition - worldPositionSource;
            var fireDistance = deltaPos.Length;
            // hit happens when the end of the weapon trace sprite touches the target, so cut the distance accordingly

            fireDistance -= weaponTracePreset.TraceWorldLength;
            fireDistance = Math.Max(0, fireDistance);
            return SharedCalculateTimeToHit(fireDistance, weaponTracePreset);
        }

        public static double SharedCalculateTimeToHit(double fireDistance, WeaponFireTracePreset weaponTracePreset)
        {
            return fireDistance / weaponTracePreset.TraceSpeed;
        }

        public static Vector2D SharedCalculateWeaponShotWorldPositon(
            ICharacter character,
            IProtoItemWeapon protoWeapon,
            IProtoCharacter protoCharacter,
            Vector2D fallbackPosition,
            bool hasTrace)
        {
            Vector2D worldPositionSource;
            if (character is not null
                && character.IsInitialized)
            {
                if (Api.IsClient
                    && (hasTrace || protoWeapon is IProtoItemWeaponRanged)
                    && character.GetClientState<BaseCharacterClientState>().SkeletonRenderer is { } skeletonRenderer)
                {
                    var protoSkeleton = character.GetClientState<BaseCharacterClientState>().CurrentProtoSkeleton;
                    var slotName = protoSkeleton.SlotNameItemInHand;
                    var slotOffset = skeletonRenderer.GetSlotScreenOffset(slotName);
                    var muzzleFlashTextureOffset = Vector2F.Zero;

                    if (protoWeapon is IProtoItemWeaponRanged protoItemWeaponRanged)
                    {
                        muzzleFlashTextureOffset =
                            (protoItemWeaponRanged.MuzzleFlashDescription.TextureScreenOffset
                             / skeletonRenderer.GetSlotScreenScale(slotName)).ToVector2F();
                    }

                    var boneWorldPosition = skeletonRenderer.TransformSlotPosition(
                        slotName,
                        slotOffset + muzzleFlashTextureOffset,
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

        // Synchronize static firing animation with the firing animation
        private static void ClientRefreshStaticAttackAnimation(
            IComponentSkeleton skeletonRenderer,
            IProtoItemWeapon weapon)
        {
            // ReSharper disable once CanExtractXamlLocalizableStringCSharp
            var currentAnimationName = skeletonRenderer.GetCurrentAnimationName(
                AnimationTrackIndexes.ItemFiring,
                getLatestAddedAnimation: false);

            if (currentAnimationName is null)
            {
                // no current firing animation - no need the static animation
                return;
            }

            var trackIndexStaticFiring = AnimationTrackIndexes.ItemFiringStatic;
            var staticFiringAnimationName = currentAnimationName + "_Static";
            var currentStaticAnimationName = skeletonRenderer.GetCurrentAnimationName(
                trackIndexStaticFiring,
                getLatestAddedAnimation: false);

            if (currentStaticAnimationName == staticFiringAnimationName)
            {
                return;
            }

            skeletonRenderer.RemoveAnimationTrack(trackIndexStaticFiring);

            // need to update the static attack animation
            ClientSetFiringAnimation(
                weapon,
                skeletonRenderer,
                trackIndexStaticFiring,
                staticFiringAnimationName,
                weapon.FireAnimationDuration,
                weapon.FireInterval,
                mixWithCurrent: false,
                isLooped: weapon.IsLoopedAttackAnimation);

            // synchronize static with non-static animation tracks
            skeletonRenderer.SetAnimationTime(
                trackIndexStaticFiring,
                skeletonRenderer.GetAnimationTime(AnimationTrackIndexes.ItemFiring));
        }

        private static void ClientSetFiringAnimation(
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

                skeletonRenderer.RemoveAnimationTrack(trackIndex);
            }
            else if (currentFireAnimation is not null)
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

        private static IProtoItemWeapon SharedGetCharacterCurrentWeaponProto(ICharacter character)
        {
            return character.GetPublicState<ICharacterPublicState>().SelectedItemWeaponProto;
        }
    }
}