namespace AtomicTorch.CBND.CoreMod.Helpers.Client
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using AtomicTorch.CBND.CoreMod.Characters;
    using AtomicTorch.CBND.CoreMod.Characters.Input;
    using AtomicTorch.CBND.CoreMod.CharacterSkeletons;
    using AtomicTorch.CBND.CoreMod.ClientComponents.Timer;
    using AtomicTorch.CBND.CoreMod.Helpers.Primitives;
    using AtomicTorch.CBND.CoreMod.SoundPresets;
    using AtomicTorch.CBND.CoreMod.Systems.Weapons;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.ServicesClient.Components;
    using AtomicTorch.GameEngine.Common.Helpers;
    using static GameEngine.Common.Primitives.MathConstants;

    [SuppressMessage("ReSharper", "CanExtractXamlLocalizableStringCSharp")]
    public static class ClientCharacterAnimationHelper
    {
        /// <summary>
        /// Remove corpse rendering after this timeout.
        /// </summary>
        private const double CorpseTimeoutSeconds = 10;

        private const double RotationAngleRadInterpolationRateCurrentCharacter = 70;

        private const double RotationAngleRadInterpolationRateRemoteCharacter = 50;

        public static void ClientUpdateAnimation(
            ICharacter character,
            BaseCharacterClientState clientState,
            ICharacterPublicState publicState)
        {
            var skeletonRenderer = clientState.SkeletonRenderer;
            if (skeletonRenderer == null)
            {
                return;
            }

            var activeWeaponProto = publicState.CurrentItemWeaponProto;

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
                    "Idle",
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

                // character just died
                // play death sound
                protoSkeleton.PlaySound(CharacterSound.Death, character);
                clientState.SoundEmitterLoopCharacter.Stop();
                clientState.SoundEmitterLoopMovemement.Stop();

                // hide skeleton after timeout
                ClientComponentTimersManager.AddAction(
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
                    rendererShadow.IsEnabled = false;
                }

                return;
            }

            skeletonRenderer.IsEnabled = true;
            rendererShadow.IsEnabled = true;

            var appliedInput = publicState.AppliedInput;
            var rotationAngleRad = GetCurrentRotationAngleRadInterpolated(character,
                                                                          clientState,
                                                                          appliedInput);

            GetCurrentAnimationSetting(
                protoSkeleton,
                appliedInput.MoveModes,
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

            var currentSkeleton = viewOrientation.IsUp && protoSkeleton.SkeletonResourceBack != null
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
                var activeAnimationName =
                    skeletonRenderer.GetLatestAddedAnimationName(trackIndex: AnimationTrackIndexes.Primary);
                if (newAnimationName != activeAnimationName
                    && newAnimationStarterName != activeAnimationName)
                {
                    //Api.Logger.WriteDev(
                    //	newAnimationStarterName != null
                    //	? $"Changing move animation: {activeAnimationName}->{newAnimationStarterName}->{newAnimationName}"
                    //	: $"Changing move animation: {activeAnimationName}->{newAnimationName}");

                    var hasStarterAnimation = newAnimationStarterName != null;
                    if (hasStarterAnimation)
                    {
                        // add starter animation
                        skeletonRenderer.SetAnimation(
                            trackIndex: AnimationTrackIndexes.Primary,
                            animationName: newAnimationStarterName,
                            isLooped: false);

                        // add looped animation
                        skeletonRenderer.AddAnimation(
                            trackIndex: AnimationTrackIndexes.Primary,
                            animationName: newAnimationName,
                            isLooped: true);
                    }
                    else if (newAnimationName == "Idle"
                             && skeletonRenderer.GetCurrentAnimationName(trackIndex: AnimationTrackIndexes.Primary)
                                                .EndsWith("Start"))
                    {
                        // going into idle when playing a start animation - allow to finish it!
                        // remove queued entries
                        skeletonRenderer.RemoveAnimationTrackNextEntries(trackIndex: AnimationTrackIndexes.Primary);

                        // add looped idle animation
                        skeletonRenderer.AddAnimation(
                            trackIndex: AnimationTrackIndexes.Primary,
                            animationName: newAnimationName,
                            isLooped: true);
                    }
                    else
                    {
                        // set looped animation
                        skeletonRenderer.SetAnimation(
                            trackIndex: AnimationTrackIndexes.Primary,
                            animationName: newAnimationName,
                            isLooped: true);
                    }
                }
            }

            if (appliedInput.MoveModes != CharacterMoveModes.None)
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

            if (activeWeaponProto != null)
            {
                clientState.HasWeaponAnimationAssigned = true;
                clientState.LastAimCoef = aimCoef;

                var aimingAnimationName = activeWeaponProto.CharacterAnimationAimingName;
                if (aimingAnimationName != null)
                {
                    //Api.Logger.WriteDev(
                    //    $"Setting aiming animation: {aimingAnimationName} timePercents: {aimCoef:F2}");
                    skeletonRenderer.SetAnimationFrame(
                        trackIndex: AnimationTrackIndexes.ItemAiming,
                        animationName: aimingAnimationName,
                        timePositionPercents: aimCoef);
                }
                else
                {
                    skeletonRenderer.RemoveAnimationTrack(AnimationTrackIndexes.ItemAiming);
                }

                WeaponSystemClientDisplay.RefreshCurrentAttackAnimation(character);
            }
            else
            {
                clientState.HasWeaponAnimationAssigned = false;
            }

            SetLoopSounds(character, clientState, publicState.AppliedInput, protoSkeleton, isIdle);
        }

        public static void GetAimingOrientation(
            IProtoCharacterSkeleton protoSkeleton,
            double angleRad,
            ViewOrientation lastViewOrientation,
            out ViewOrientation viewOrientation,
            out float aimCoef)
        {
            //const float thresholdUpToDownFlipDeg = 0;

            var angleDeg = angleRad * RadToDeg;
            viewOrientation = new ViewOrientation(
                isUp: IsOrientedUp(protoSkeleton, angleDeg),
                isLeft: IsLeftHalfOfCircle(angleDeg));

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
                        if (angleDeg > 180 - protoSkeleton.OrientationThresholdDownToUpFlipDeg)
                        {
                            // keep down-left orientation
                            viewOrientation.IsUp = false;
                        }
                    }
                    else // if view orientation was down-right, but now up-right
                    {
                        if (angleDeg < protoSkeleton.OrientationThresholdDownToUpFlipDeg)
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
                        if (angleDeg > 90 - protoSkeleton.OrientationThresholdUpHorizontalFlipDeg)
                        {
                            // keep up-left orientation
                            viewOrientation.IsLeft = true;
                        }
                    }
                    else
                    {
                        // if view orientation was right-up, but now left-up
                        if (angleDeg < 90 + protoSkeleton.OrientationThresholdUpHorizontalFlipDeg)
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
                        if (angleDeg < 270 + protoSkeleton.OrientationThresholdDownHorizontalFlipDeg
                            && angleDeg > 270)
                        {
                            // keep down-left orientation
                            viewOrientation.IsLeft = true;
                        }
                    }
                    else
                    {
                        // if view orientation was right-down, but now left-down
                        if (angleDeg > 270 - protoSkeleton.OrientationThresholdDownHorizontalFlipDeg
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

        public static void GetCurrentAnimationSetting(
            IProtoCharacterSkeleton protoSkeleton,
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
            GetAimingOrientation(protoSkeleton, angle, lastViewOrientation, out viewOrientation, out aimCoef);

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
            double rotationAngleRad = appliedInput.RotationAngleRad;
            if (!clientState.LastInterpolatedRotationAngleRad.HasValue)
            {
                // current character or first time - simply use current angle
                clientState.LastInterpolatedRotationAngleRad = rotationAngleRad;
                return rotationAngleRad;
            }

            var oldAngle = clientState.LastInterpolatedRotationAngleRad.Value;

            if (Math.Abs(MathHelper.GetShortestAngleDist(oldAngle, rotationAngleRad))
                < Math.PI / 2)
            {
                // small difference - allow to interpolate
                // smooth interpolation for remote players is required to better deal with the network update rate
                // for local player it's much more fast and just improves overall smoothness of the character rotation
                var rate = character.IsCurrentClientCharacter
                               ? RotationAngleRadInterpolationRateCurrentCharacter
                               : RotationAngleRadInterpolationRateRemoteCharacter;

                rotationAngleRad = MathHelper.LerpAngle(
                    oldAngle,
                    rotationAngleRad,
                    Api.Client.Core.DeltaTime,
                    rate);
            }

            clientState.LastInterpolatedRotationAngleRad = rotationAngleRad;
            return rotationAngleRad;
        }

        private static bool IsOrientedUp(IProtoCharacterSkeleton protoSkeleton, double angleDeg)
        {
            // we consider upper half-circle as starting from "extra angle" to 180-"extra angle" degrees, not 0 to 180
            var extraAngle = protoSkeleton.OrientationDownExtraAngle;
            return angleDeg > extraAngle
                   && angleDeg < 180.0 - extraAngle;
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
            clientState.SoundEmitterLoopCharacter.NextSoundResource = soundPresetCharacter.GetSound(soundKey);
            clientState.SoundEmitterLoopMovemement.NextSoundResource = soundPresetMovement.GetSound(soundKey);

            // ensure the sounds are played
            clientState.SoundEmitterLoopCharacter.Play();
            clientState.SoundEmitterLoopMovemement.Play();
        }
    }
}