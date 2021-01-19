namespace AtomicTorch.CBND.CoreMod.Helpers.Client
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using AtomicTorch.CBND.CoreMod.Characters;
    using AtomicTorch.CBND.CoreMod.Characters.Input;
    using AtomicTorch.CBND.CoreMod.CharacterSkeletons;
    using AtomicTorch.CBND.CoreMod.SoundPresets;
    using AtomicTorch.CBND.CoreMod.Systems.Weapons;
    using AtomicTorch.CBND.CoreMod.Vehicles;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.GameEngine.Common.Helpers;

    [SuppressMessage("ReSharper", "CanExtractXamlLocalizableStringCSharp")]
    public static class ClientCharacterAnimationHelper
    {
        /// <summary>
        /// Remove corpse rendering after this timeout.
        /// </summary>
        private const double CorpseTimeoutSeconds = 10;

        private const double RotationAngleRadInterpolationRateCurrentCharacter = 70;

        // Adjusted for mobs with 0.1s server update interval.
        private const double RotationAngleRadInterpolationRateRemoteCharacterNpc = 15;

        private const double RotationAngleRadInterpolationRateRemoteCharacterPlayer = 50;

        public static void ClientUpdateAnimation(
            ICharacter character,
            BaseCharacterClientState clientState,
            ICharacterPublicState publicState)
        {
            var skeletonRenderer = clientState.SkeletonRenderer;
            if (skeletonRenderer is null)
            {
                return;
            }

            var activeWeaponProto = publicState.SelectedItemWeaponProto;

            var protoSkeleton = clientState.CurrentProtoSkeleton;
            var rendererShadow = clientState.RendererShadow;
            var wasDead = clientState.IsDead;

            clientState.IsDead = publicState.IsDead;

            if (publicState.IsDead)
            {
                if (skeletonRenderer.GetCurrentAnimationName(AnimationTrackIndexes.Primary)
                    == "Death")
                {
                    // already in death animation
                    return;
                }

                // character was not dead on client and now become dead
                skeletonRenderer.ResetSkeleton();

                skeletonRenderer.SelectCurrentSkeleton(
                    protoSkeleton.SkeletonResourceFront,
                    protoSkeleton.DefaultAnimationName,
                    isLooped: false);

                skeletonRenderer.SetAnimation(AnimationTrackIndexes.Primary, "Death", isLooped: false);

                if (!wasDead.HasValue)
                {
                    // character entered scope and it's dead
                    skeletonRenderer.SetAnimationTime(0, 10000);
                    // hide skeleton completely
                    HideBody();
                    return;
                }

                // character just died (was in another animation state)
                if (!character.IsNpc)
                {
                    // play death sound (please note - NPC death sound is played by network event only)
                    protoSkeleton.PlaySound(CharacterSound.Death, character);
                }

                clientState.SoundEmitterLoopCharacter.Stop();
                clientState.SoundEmitterLoopMovemement.Stop();

                // hide skeleton after timeout
                ClientTimersSystem.AddAction(
                    CorpseTimeoutSeconds,
                    HideBody);

                void HideBody()
                {
                    if (!publicState.IsDead)
                    {
                        // the character has been respawned
                        return;
                    }

                    skeletonRenderer.IsEnabled = false;
                    if (rendererShadow is not null)
                    {
                        rendererShadow.IsEnabled = false;
                    }
                }

                return;
            }

            skeletonRenderer.IsEnabled = true;
            if (rendererShadow is not null)
            {
                rendererShadow.IsEnabled = true;
            }

            var appliedInput = publicState.AppliedInput;
            var rotationAngleRad = GetCurrentRotationAngleRadInterpolated(character,
                                                                          clientState,
                                                                          appliedInput);

            var moveModes = appliedInput.MoveModes;
            if (publicState is PlayerCharacterPublicState playerCharacterPublicState
                && playerCharacterPublicState.CurrentVehicle is { } vehicle)
            {
                var protoVehicle = (IProtoVehicle)vehicle.ProtoGameObject;
                protoVehicle.SharedGetSkeletonProto(vehicle, out var vehicleSkeleton, out _);
                if (vehicleSkeleton is null)
                {
                    // this vehicle doesn't provide any skeleton so player is simply displayed as standing idle inside it
                    moveModes = CharacterMoveModes.None;
                }
            }

            protoSkeleton.GetCurrentAnimationSetting(
                character,
                moveModes,
                rotationAngleRad,
                clientState.LastViewOrientation,
                out var newAnimationStarterName,
                out var newAnimationName,
                out var currentDrawMode,
                out var aimCoef,
                out var viewOrientation,
                out var isIdle);

            clientState.LastViewOrientation = viewOrientation;

            // TODO: consider adding new field - HasBackwardAnimations
            if (!protoSkeleton.HasMoveStartAnimations)
            {
                newAnimationStarterName = null;
                if (newAnimationName == "RunSideBackward")
                {
                    newAnimationName = "RunSide";
                }
            }

            var currentSkeleton = viewOrientation.IsUp && protoSkeleton.SkeletonResourceBack is not null
                                      ? protoSkeleton.SkeletonResourceBack
                                      : protoSkeleton.SkeletonResourceFront;

            if (skeletonRenderer.CurrentSkeleton != currentSkeleton)
            {
                // switch skeleton completely
                skeletonRenderer.SelectCurrentSkeleton(
                    currentSkeleton,
                    // please note: no starter animation in that case!
                    animationName: newAnimationName,
                    isLooped: true);
            }
            else
            {
                var activeAnimationName = skeletonRenderer.GetLatestAddedAnimationName(
                    trackIndex: AnimationTrackIndexes.Primary);

                if (newAnimationName != activeAnimationName
                    && newAnimationStarterName != activeAnimationName)
                {
                    //Api.Logger.WriteDev(
                    //	newAnimationStarterName is not null
                    //	? $"Changing move animation: {activeAnimationName}->{newAnimationStarterName}->{newAnimationName}"
                    //	: $"Changing move animation: {activeAnimationName}->{newAnimationName}");

                    skeletonRenderer.RemoveAnimationTrackNextEntries(
                        trackIndex: AnimationTrackIndexes.Primary);

                    var currentAnimationName = skeletonRenderer.GetCurrentAnimationName(
                        trackIndex: AnimationTrackIndexes.Primary,
                        getLatestAddedAnimation: false);

                    var hasStarterAnimation = newAnimationStarterName is not null;
                    if (newAnimationName != "Idle"
                        && hasStarterAnimation)
                    {
                        // add starter animation
                        skeletonRenderer.SetAnimationLoopMode(AnimationTrackIndexes.Primary,
                                                              isLooped: false);

                        if (newAnimationStarterName != currentAnimationName)
                        {
                            if (currentAnimationName == "Idle")
                            {
                                //Api.Logger.Dev("Adding starter animation: "
                                //               + newAnimationStarterName
                                //               + " (current animation is "
                                //               + currentAnimationName
                                //               + ")");

                                skeletonRenderer.SetAnimation(
                                    trackIndex: AnimationTrackIndexes.Primary,
                                    animationName: newAnimationStarterName,
                                    isLooped: false);
                            }
                            else
                            {
                                //Api.Logger.Dev("No starter animation will be added. Current animation: "
                                //               + currentAnimationName
                                //               + " starter name: "
                                //               + newAnimationStarterName);
                                //skeletonRenderer.AddAnimation(
                                //    trackIndex: AnimationTrackIndexes.Primary,
                                //    animationName: newAnimationStarterName,
                                //    isLooped: false);
                            }
                        }
                        else
                        {
                            //Api.Logger.Dev("No need to add starter animation: "
                            //               + newAnimationStarterName
                            //               + " (current animation is "
                            //               + currentAnimationName
                            //               + ")");
                        }

                        // add looped animation
                        skeletonRenderer.AddAnimation(
                            trackIndex: AnimationTrackIndexes.Primary,
                            animationName: newAnimationName,
                            isLooped: true);
                    }
                    else
                    {
                        if (currentAnimationName is not null
                            && currentAnimationName.EndsWith("Start"))
                        {
                            //Api.Logger.Dev("Adding new animation + Abort animation after START animation: "
                            //               + newAnimationName
                            //               + " (current animation is "
                            //               + currentAnimationName
                            //               + ")");

                            // going into idle when playing a start animation - allow to finish it!

                            // remove queued entries
                            skeletonRenderer.RemoveAnimationTrackNextEntries(
                                trackIndex: AnimationTrackIndexes.Primary);

                            // add abort animation
                            skeletonRenderer.AddAnimation(
                                trackIndex: AnimationTrackIndexes.Primary,
                                animationName: currentAnimationName + "Abort",
                                isLooped: false);

                            // add looped animation
                            skeletonRenderer.AddAnimation(
                                trackIndex: AnimationTrackIndexes.Primary,
                                animationName: newAnimationName,
                                isLooped: true);
                        }
                        else if (currentAnimationName is not null
                                 && currentAnimationName.EndsWith("Abort"))
                        {
                            //Api.Logger.Dev("Adding new animation after Abort animation: "
                            //               + newAnimationName
                            //               + " (current animation is "
                            //               + currentAnimationName
                            //               + ")");

                            // remove queued entries
                            skeletonRenderer.RemoveAnimationTrackNextEntries(
                                trackIndex: AnimationTrackIndexes.Primary);

                            // add end animation
                            skeletonRenderer.AddAnimation(
                                trackIndex: AnimationTrackIndexes.Primary,
                                animationName: currentAnimationName + "Abort",
                                isLooped: false);

                            // add looped animation
                            skeletonRenderer.AddAnimation(
                                trackIndex: AnimationTrackIndexes.Primary,
                                animationName: newAnimationName,
                                isLooped: true);
                        }
                        else
                        {
                            //Api.Logger.Dev("Setting new animation: "
                            //               + newAnimationName
                            //               + " (current animation is "
                            //               + currentAnimationName
                            //               + ")");

                            // set looped animation (reset any current animation)
                            skeletonRenderer.SetAnimation(
                                trackIndex: AnimationTrackIndexes.Primary,
                                animationName: newAnimationName,
                                isLooped: true);
                        }
                    }
                }
            }

            if (moveModes != CharacterMoveModes.None)
            {
                // moving mode
                var animationSpeedMultiplier = appliedInput.MoveSpeed
                                               / (protoSkeleton.DefaultMoveSpeed
                                                  * clientState.CurrentProtoSkeletonScaleMultiplier);

                skeletonRenderer.SetAnimationSpeed(
                    trackIndex: AnimationTrackIndexes.Primary,
                    speedMultiliper: (float)animationSpeedMultiplier);

                // moving - remove animation for static firing
                skeletonRenderer.RemoveAnimationTrack(AnimationTrackIndexes.ItemFiringStatic);
            }
            else
            {
                skeletonRenderer.SetAnimationSpeed(
                    trackIndex: AnimationTrackIndexes.Primary,
                    speedMultiliper: 1.0f);
            }

            skeletonRenderer.DrawMode = currentDrawMode;

            if (activeWeaponProto is not null)
            {
                clientState.HasWeaponAnimationAssigned = true;
                clientState.LastAimCoef = aimCoef;

                var aimingAnimationName = activeWeaponProto.CharacterAnimationAimingName;
                if (aimingAnimationName is not null)
                {
                    //Api.Logger.WriteDev(
                    //    $"Setting aiming animation: {aimingAnimationName} timePercents: {aimCoef:F2}");
                    skeletonRenderer.SetAnimationFrame(
                        trackIndex: AnimationTrackIndexes.ItemAiming,
                        animationName: aimingAnimationName,
                        timePositionFraction: aimCoef);
                }
                else
                {
                    skeletonRenderer.RemoveAnimationTrack(AnimationTrackIndexes.ItemAiming);
                }

                WeaponSystemClientDisplay.ClientRefreshCurrentAttackAnimation(character);
            }
            else
            {
                clientState.HasWeaponAnimationAssigned = false;
            }

            SetLoopSounds(character, clientState, publicState.AppliedInput, protoSkeleton, isIdle);
        }

        public static bool IsLeftHalfOfCircle(double angleDeg)
        {
            return angleDeg > 90
                   && angleDeg <= 270;
        }

        private static double GetCurrentRotationAngleRadInterpolated(
            ICharacter character,
            BaseCharacterClientState clientState,
            AppliedCharacterInput appliedInput)
        {
            var isCurrentClientCharacter = character.IsCurrentClientCharacter;

            double rotationAngleRad = appliedInput.RotationAngleRad;
            rotationAngleRad = WeaponSystem.ClientGetCorrectedRotationAngleForWeaponAim(character,
                                   clientState.LastViewOrientation,
                                   rotationAngleRad)
                               ?? rotationAngleRad;

            if (clientState.LastInterpolatedRotationAngleRad.HasValue)
            {
                InterpolateAngle(clientState.LastInterpolatedRotationAngleRad.Value,
                                 ref rotationAngleRad,
                                 isCurrentClientCharacter,
                                 character.IsNpc);
            }

            clientState.LastInterpolatedRotationAngleRad = rotationAngleRad;
            return rotationAngleRad;
        }

        private static void InterpolateAngle(
            double previousAngleRad,
            ref double newAngleRad,
            bool isCurrentClientCharacter,
            bool isNpc)
        {
            if (Math.Abs(MathHelper.GetShortestAngleDist(previousAngleRad, newAngleRad))
                >= Math.PI / 2)
            {
                // too big difference
                return;
            }

            // small difference - allow to interpolate
            // smooth interpolation for remote players is required to better deal with the network update rate
            // for local player it's much more fast and just improves overall smoothness of the character rotation
            var rate = isCurrentClientCharacter
                           ? RotationAngleRadInterpolationRateCurrentCharacter
                           : isNpc
                               ? RotationAngleRadInterpolationRateRemoteCharacterNpc
                               : RotationAngleRadInterpolationRateRemoteCharacterPlayer;

            newAngleRad = MathHelper.LerpAngle(
                previousAngleRad,
                newAngleRad,
                Api.Client.Core.DeltaTime,
                rate);
        }

        private static void SetLoopSounds(
            ICharacter character,
            BaseCharacterClientState clientState,
            AppliedCharacterInput appliedInput,
            ProtoCharacterSkeleton protoSkeleton,
            bool isIdle)
        {
            CharacterSound soundKey;
            if (isIdle)
            {
                soundKey = CharacterSound.LoopIdle;
            }
            else
            {
                var isRunningMode = (appliedInput.MoveModes & CharacterMoveModes.ModifierRun) != 0;
                soundKey = isRunningMode ? CharacterSound.LoopRun : CharacterSound.LoopWalk;
            }

            var (soundPresetCharacter, soundPresetMovement) = protoSkeleton.GetSoundPresets(character);
            if (clientState.SoundEmitterLoopCharacter is not null)
            {
                clientState.SoundEmitterLoopCharacter.NextSoundResource = soundPresetCharacter.GetSound(soundKey);
                clientState.SoundEmitterLoopCharacter.Play();
            }

            if (clientState.SoundEmitterLoopMovemement is not null)
            {
                clientState.SoundEmitterLoopMovemement.NextSoundResource = soundPresetMovement.GetSound(soundKey);
                clientState.SoundEmitterLoopMovemement.Play();
            }
        }
    }
}