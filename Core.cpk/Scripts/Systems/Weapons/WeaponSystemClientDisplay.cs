namespace AtomicTorch.CBND.CoreMod.Systems.Weapons
{
    using System;
    using System.Collections.Generic;
    using AtomicTorch.CBND.CoreMod.Characters;
    using AtomicTorch.CBND.CoreMod.Characters.Input;
    using AtomicTorch.CBND.CoreMod.Helpers.Client;
    using AtomicTorch.CBND.CoreMod.Items.Weapons;
    using AtomicTorch.CBND.CoreMod.SoundPresets;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.ServicesClient.Components;
    using AtomicTorch.GameEngine.Common.Helpers;

    public static class WeaponSystemClientDisplay
    {
        public static void OnWeaponFinished(ICharacter character)
        {
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

        public static void OnWeaponHit(IProtoItemWeapon protoWeapon, IReadOnlyList<WeaponHitData> hitObjects)
        {
            foreach (var hitData in hitObjects)
            {
                var worldObject = hitData.WorldObject;
                if (worldObject?.ProtoGameObject == null)
                {
                    // no such object in current context
                    if (Api.IsEditor)
                    {
                        Api.Logger.Error("Unknown world object on OnWeaponHit(): " + worldObject);
                    }
                    else
                    {
                        Api.Logger.Warning("Unknown world object on OnWeaponHit(): " + worldObject);
                    }

                    continue;
                }

                var protoWorldObject = worldObject.ProtoWorldObject;
                var objectSoundMaterial = protoWorldObject.SharedGetObjectSoundMaterial();

                var volume = SoundConstants.VolumeHit;
                // apply some volume variation
                volume *= RandomHelper.Range(0.8f, 1.0f);

                protoWeapon.SoundPresetHit.PlaySound(
                    objectSoundMaterial,
                    worldObject,
                    volume: volume,
                    pitch: RandomHelper.Range(0.95f, 1.05f));
            }
        }

        public static void OnWeaponInputStop(ICharacter character)
        {
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

        public static void OnWeaponShot(ICharacter character)
        {
            if (character?.ProtoGameObject == null)
            {
                if (Api.IsEditor)
                {
                    Api.Logger.Error("Unknown character on OnWeaponShot(): " + character);
                }
                else
                {
                    Api.Logger.Warning("Unknown character on OnWeaponShot(): " + character);
                }

                return;
            }

            var clientState = character.GetClientState<BaseCharacterClientState>();
            if (!clientState.HasWeaponAnimationAssigned)
            {
                return;
            }

            const float volume = SoundConstants.VolumeWeapon;
            var pitch = RandomHelper.Range(0.95f, 1.05f);

            var skeletonRenderer = clientState.SkeletonRenderer;
            var weapon = GetCharacterCurrentWeaponProto(character);

            IComponentSoundEmitter emitter;
            if (weapon.SoundPresetWeapon.HasSound(WeaponSound.Shot))
            {
                // play shot sound from weapon
                weapon.SoundPresetWeapon.PlaySound(WeaponSound.Shot,
                                                   character,
                                                   out emitter,
                                                   volume: volume,
                                                   pitch: pitch);
            }
            else
            {
                // play sounds from the skeleton instead
                var characterSkeleton = clientState.CurrentProtoSkeleton;
                if (!characterSkeleton.SoundPresetWeapon.PlaySound(WeaponSound.Shot,
                                                                   character,
                                                                   out emitter,
                                                                   volume))
                {
                    // no method returned true
                    // fallback to the default weapon sound (if there is no, it will be logged into the audio log)
                    weapon.SoundPresetWeapon.PlaySound(WeaponSound.Shot,
                                                       character,
                                                       out emitter,
                                                       volume: volume,
                                                       pitch: pitch);
                }
            }

            if (emitter != null)
            {
                var distance = weapon.SoundPresetWeaponDistance;
                emitter.CustomMinDistance = distance.min;
                emitter.CustomMaxDistance = distance.max;
            }

            if (weapon is IProtoItemWeaponRanged rangedWeapon)
            {
                CreateMuzzleFlash(rangedWeapon, character, skeletonRenderer);

                var recoilAnimationName = rangedWeapon.CharacterAnimationAimingRecoilName;
                if (recoilAnimationName != null
                    && rangedWeapon.CharacterAnimationAimingRecoilPower > 0
                    && rangedWeapon.CharacterAnimationAimingRecoilDuration > 0)
                {
                    SetRecoilAnimation(
                        character,
                        rangedWeapon,
                        skeletonRenderer);
                }
            }
        }

        public static void OnWeaponStart(ICharacter character)
        {
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
            var sceneObject = Api.Client.Scene.GetSceneObject(characterWorldObject);
            sceneObject.AddComponent<ClientComponentMuzzleFlash>()
                       .Setup(characterWorldObject, skeletonRenderer, protoWeapon);
        }

        private static IProtoItemWeapon GetCharacterCurrentWeaponProto(ICharacter character)
        {
            return character.GetPublicState<ICharacterPublicState>().CurrentItemWeaponProto;
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
            var sceneObject = Api.Client.Scene.GetSceneObject(character);
            var componentRecoil = sceneObject.FindComponent<ClientComponentCharacterWeaponRecoilAnimation>()
                                  ?? sceneObject.AddComponent<ClientComponentCharacterWeaponRecoilAnimation>();
            componentRecoil.StartRecoil(character, weaponProto, skeletonRenderer);
        }
    }
}