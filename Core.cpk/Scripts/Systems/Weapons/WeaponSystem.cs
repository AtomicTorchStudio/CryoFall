namespace AtomicTorch.CBND.CoreMod.Systems.Weapons
{
    using System;
    using System.Collections.Generic;
    using AtomicTorch.CBND.CoreMod.Characters;
    using AtomicTorch.CBND.CoreMod.Characters.Player;
    using AtomicTorch.CBND.CoreMod.CharacterStatusEffects.Neutral;
    using AtomicTorch.CBND.CoreMod.Helpers.Physics;
    using AtomicTorch.CBND.CoreMod.Helpers.Primitives;
    using AtomicTorch.CBND.CoreMod.Items.Ammo;
    using AtomicTorch.CBND.CoreMod.Items.Devices;
    using AtomicTorch.CBND.CoreMod.Items.Weapons;
    using AtomicTorch.CBND.CoreMod.Skills;
    using AtomicTorch.CBND.CoreMod.Stats;
    using AtomicTorch.CBND.CoreMod.Systems.Party;
    using AtomicTorch.CBND.CoreMod.Systems.Physics;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.Items;
    using AtomicTorch.CBND.GameApi.Data.Physics;
    using AtomicTorch.CBND.GameApi.Data.State;
    using AtomicTorch.CBND.GameApi.Data.Weapons;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.Scripting.Network;
    using AtomicTorch.CBND.GameApi.ServicesClient;
    using AtomicTorch.GameEngine.Common.DataStructures;
    using AtomicTorch.GameEngine.Common.Extensions;
    using AtomicTorch.GameEngine.Common.Helpers;
    using AtomicTorch.GameEngine.Common.Primitives;

    /// <summary>
    /// System to process weapon states (firing, cooldown, etc).
    /// </summary>
    public class WeaponSystem : ProtoSystem<WeaponSystem>
    {
        private const string RemoteCallSequenceGroupCharacterFiring = "CharacterWeaponFiringSequence";

        private static readonly ISharedApi Shared = Api.Shared;

        public override string Name => "Weapon system logic.";

        public static void ClientChangeWeaponFiringMode(bool isFiring)
        {
            var character = Client.Characters.CurrentPlayerCharacter;
            var state = PlayerCharacter.GetPrivateState(character).WeaponState;

            if (state.ProtoWeapon is null)
            {
                // player cannot use default attack when on a hoverboard
                return;
            }

            if (state.SharedGetInputIsFiring()
                == isFiring)
            {
                // no need to change the firing input mode
                return;
            }

            state.SharedSetInputIsFiring(isFiring);

            var shotsDone = state.ShotsDone;
            if (!isFiring)
            {
                // stopping firing
                if (SharedShouldFireMore(state))
                {
                    // assume we will attempt fire a shot more
                    var ammoConsumptionPerShot = state.ProtoWeapon.AmmoConsumptionPerShot;
                    if (ammoConsumptionPerShot > 0)
                    {
                        var ammoCountAvailable = state.ItemWeapon?
                                                      .GetPrivateState<WeaponPrivateState>()
                                                      .AmmoCount;

                        if (ammoCountAvailable.HasValue
                            && ammoCountAvailable.Value < ammoConsumptionPerShot)
                        {
                            // cannot shot more ammo than have loaded
                            ammoConsumptionPerShot = ammoCountAvailable.Value;
                        }
                    }

                    if (ammoConsumptionPerShot < 1)
                    {
                        ammoConsumptionPerShot = 1;
                    }

                    shotsDone += ammoConsumptionPerShot;
                }
            }

            // set firing mode on server
            state.ProtoWeapon.ClientOnFireModChanged(isFiring, shotsDone);
            //Logger.Dev(isFiring
            //               ? "SetWeaponFiringMode: firing!"
            //               : $"SetWeaponFiringMode: stop firing! Shots done: {shotsDone}");
        }

        /// <summary>
        /// Adjust rotation angle only if a player character has a laser sight item equipped.
        /// </summary>
        public static double? ClientGetCorrectedRotationAngleForWeaponAim(
            ICharacter character,
            ViewOrientation viewOrientation,
            double currentRotationAngleRad)
        {
            if (character.IsNpc)
            {
                return null;
            }

            var clientState = character.GetClientState<BaseCharacterClientState>();
            if (clientState.SkeletonRenderer is null
                || !clientState.SkeletonRenderer.IsReady)
            {
                return null;
            }

            var characterPublicState = character.GetPublicState<PlayerCharacterPublicState>();
            if (characterPublicState.IsDead
                || !characterPublicState.IsOnline
                || !(characterPublicState.SelectedItemWeaponProto
                         is IProtoItemWeaponRanged protoWeaponRanged))
            {
                return null;
            }

            IItem itemSight = null;
            foreach (var item in characterPublicState.ContainerEquipment.Items)
            {
                if (item.ProtoItem is ItemLaserSight)
                {
                    itemSight = item;
                    break;
                }
            }

            if (itemSight is null)
            {
                return null;
            }

            var rangeMax = character.IsCurrentClientCharacter
                               ? ItemLaserSight.SharedGetCurrentRangeMax(characterPublicState)
                               : ItemLaserSight.GetPublicState(itemSight).MaxRange;

            if (rangeMax <= 0.5)
            {
                // no weapon selected or a too close-range weapon
                return null;
            }

            CastLine(character,
                     null,
                     rangeMax,
                     currentRotationAngleRad,
                     protoWeaponRanged.CollisionGroup,
                     out var toPosition,
                     out var hitPosition);

            toPosition = hitPosition ?? toPosition;

            var originalSourcePosition = ((IProtoCharacterCore)character.ProtoCharacter)
                .SharedGetWeaponFireWorldPosition(character, isMeleeWeapon: false);
            var adjustedSourcePosition = originalSourcePosition + GetSourcePositionOffset();

            //ClientComponentPhysicsSpaceVisualizer.DrawGizmo(
            //    new LineSegmentShape(adjustedSourcePosition, toPosition),
            //    lifetime: 0.05);

            var vectorSourceToTarget = originalSourcePosition - toPosition;
            var vectorAdjustedSourceToTarget = adjustedSourcePosition - toPosition;
            if (vectorSourceToTarget.Length < 0.5
                || vectorAdjustedSourceToTarget.Length < 0.5)
            {
                // too close
                return null;
            }

            var adjustedRotationAngleRad = Math.Abs(Math.PI
                                                    + Math.Atan2(vectorAdjustedSourceToTarget.Y,
                                                                 vectorAdjustedSourceToTarget.X));

            // for testing purposes this code could be uncommented to disable the feature when Ctrl key is held
            if (character.IsCurrentClientCharacter
                && Client.Input.IsKeyHeld(InputKey.Control))
            {
                return null;
            }

            return adjustedRotationAngleRad;

            Vector2D GetSourcePositionOffset()
            {
                // we cannot use current animation state here as it's not reliable
                // and animation doesn't support inverse kinematics for aiming
                // so let's just approximate the muzzle position, it should be good enough

                var muzzleWorldOffset = protoWeaponRanged.MuzzleFlashDescription.TextureScreenOffset
                                        / ScriptingConstants.TileSizeVirtualPixels;

                if (viewOrientation.IsLeft)
                {
                    // flip Y axis
                    muzzleWorldOffset = (muzzleWorldOffset.X, -muzzleWorldOffset.Y);
                }

                return muzzleWorldOffset.RotateRad(currentRotationAngleRad);
            }

            static void CastLine(
                ICharacter character,
                Vector2D? customTargetPosition,
                double rangeMax,
                double currentRotationAngleRad,
                CollisionGroup collisionGroup,
                out Vector2D toPosition,
                out Vector2D? hitPosition)
            {
                hitPosition = null;

                SharedCastLine(character,
                               isMeleeWeapon: false,
                               rangeMax,
                               currentRotationAngleRad,
                               customTargetPosition: customTargetPosition,
                               fireSpreadAngleOffsetDeg: 0,
                               collisionGroup: collisionGroup,
                               toPosition: out toPosition,
                               tempLineTestResults: out var tempLineTestResults,
                               sendDebugEvent: false);

                var currentCharacterVehicle = character.SharedGetCurrentVehicle();

                using (tempLineTestResults)
                {
                    foreach (var testResult in tempLineTestResults.AsList())
                    {
                        var worldObject = testResult.PhysicsBody.AssociatedWorldObject;
                        if (ReferenceEquals(worldObject, character))
                        {
                            continue;
                        }

                        if (currentCharacterVehicle is not null
                            && ReferenceEquals(worldObject, currentCharacterVehicle))
                        {
                            continue;
                        }

                        hitPosition = testResult.PhysicsBody.Position;

                        if (worldObject is not null)
                        {
                            hitPosition += SharedOffsetHitWorldPositionCloserToObjectCenter(
                                worldObject,
                                worldObject.ProtoWorldObject,
                                hitPoint: testResult.Penetration,
                                isRangedWeapon: true);
                        }
                        else
                        {
                            hitPosition += testResult.Penetration;
                        }

                        break;
                    }
                }
            }
        }

        public static DamageDescription GetCurrentDamageDescription(
            IItem item,
            IProtoItemWeapon protoItem,
            out IProtoItemAmmo protoAmmo)
        {
            DamageDescription damageDescription = null;

            protoAmmo = null;
            if (item is not null)
            {
                var weaponPrivateState = item.GetPrivateState<WeaponPrivateState>();
                protoAmmo = weaponPrivateState.CurrentProtoItemAmmo;
            }

            if (protoItem.OverrideDamageDescription is not null)
            {
                damageDescription = protoItem.OverrideDamageDescription;
            }
            else if (protoAmmo is IAmmoWithCustomWeaponCacheDamageDescription customAmmo)
            {
                damageDescription = customAmmo.DamageDescriptionForWeaponCache;
            }
            else if (protoAmmo is not null)
            {
                damageDescription = protoAmmo.DamageDescription;
            }

            return damageDescription;
        }

        public static void SharedCastLine(
            ICharacter character,
            bool isMeleeWeapon,
            double rangeMax,
            double characterRotationAngleRad,
            Vector2D? customTargetPosition,
            double fireSpreadAngleOffsetDeg,
            CollisionGroup collisionGroup,
            out Vector2D toPosition,
            out ITempList<TestResult> tempLineTestResults,
            bool sendDebugEvent)
        {
            var fromPosition = ((IProtoCharacterCore)character.ProtoCharacter)
                .SharedGetWeaponFireWorldPosition(character, isMeleeWeapon);

            if (customTargetPosition.HasValue)
            {
                var direction = customTargetPosition.Value - fromPosition;
                // ensure the max range is not exceeded
                direction = direction.ClampMagnitude(rangeMax);
                toPosition = fromPosition + direction;
            }
            else
            {
                characterRotationAngleRad += fireSpreadAngleOffsetDeg * MathConstants.DegToRad;

                toPosition = fromPosition
                             + new Vector2D(rangeMax, 0)
                                 .RotateRad(characterRotationAngleRad);
            }

            tempLineTestResults = character.PhysicsBody.PhysicsSpace.TestLine(
                fromPosition: fromPosition,
                toPosition: toPosition,
                collisionGroup: collisionGroup,
                sendDebugEvent: sendDebugEvent);
        }

        public static bool SharedHasTileObstacle(
            Vector2D fromPosition,
            byte characterTileHeight,
            IWorldObject targetObject,
            Vector2D targetPosition)
        {
            var physicsSpace = targetObject.PhysicsBody.PhysicsSpace;
            bool anyCliffIsAnObstacle;
            switch (targetObject)
            {
                case IDynamicWorldObject dynamicWorldObject:
                    anyCliffIsAnObstacle = characterTileHeight != dynamicWorldObject.Tile.Height;
                    break;

                case IStaticWorldObject staticWorldObject:
                    anyCliffIsAnObstacle = characterTileHeight != staticWorldObject.OccupiedTile.Height;
                    break;

                default:
                    anyCliffIsAnObstacle = true;
                    break;
            }

            return SharedHasTileObstacle(fromPosition,
                                         characterTileHeight,
                                         targetPosition,
                                         physicsSpace,
                                         anyCliffIsAnObstacle);
        }

        // Ensure that player can hit objects only on the same height level
        // and can fire through over the pits (the cliffs of the lower heights).
        public static bool SharedHasTileObstacle(
            Vector2D fromPosition,
            byte characterTileHeight,
            Vector2D targetPosition,
            IPhysicsSpace physicsSpace,
            bool anyCliffIsAnObstacle)
        {
            using var testResults = physicsSpace.TestLine(
                fromPosition,
                targetPosition,
                collisionGroup: CollisionGroups.Default,
                sendDebugEvent: false);

            foreach (var testResult in testResults.AsList())
            {
                var testResultPhysicsBody = testResult.PhysicsBody;
                var protoTile = testResultPhysicsBody.AssociatedProtoTile;

                if (protoTile is null
                    || protoTile.Kind != TileKind.Solid)
                {
                    continue;
                }

                var testResultPosition = testResultPhysicsBody.Position;
                var attackedTile = IsServer
                                       ? Server.World.GetTile((Vector2Ushort)testResultPosition)
                                       : Client.World.GetTile((Vector2Ushort)testResultPosition);
                if (attackedTile.IsSlope)
                {
                    // slope is not an obstacle
                    continue;
                }

                // found collision with a cliff
                if (anyCliffIsAnObstacle)
                {
                    return true;
                }

                if (attackedTile.Height >= characterTileHeight)
                {
                    return true; // cliff to higher tile is always an obstacle
                }
            }

            return false; // no obstacles
        }

        public static Vector2D SharedOffsetHitWorldPositionCloserToObjectCenter(
            IWorldObject worldObject,
            IProtoWorldObject protoWorldObject,
            Vector2D hitPoint,
            bool isRangedWeapon)
        {
            var objectCenterPosition = worldObject?.PhysicsBody?.CalculateCenterOffsetForCollisionGroup(
                                           isRangedWeapon
                                               ? CollisionGroups.HitboxRanged
                                               : CollisionGroups.HitboxMelee)
                                       ?? protoWorldObject.SharedGetObjectCenterWorldOffset(worldObject);
            var coef = isRangedWeapon ? 0.2 : 0.5;
            var offset = coef * (objectCenterPosition - hitPoint);
            offset = offset.ClampMagnitude(0.5); // don't offset more than 0.5 tiles
            hitPoint += offset;
            return hitPoint;
        }

        public static Vector2D SharedOffsetHitWorldPositionCloserToTileHitboxCenter(
            IPhysicsBody tilePhysicsBody,
            Vector2D hitPoint,
            bool isRangedWeapon)
        {
            var objectCenterPosition = tilePhysicsBody.CalculateCenterOffsetForCollisionGroup(
                                           isRangedWeapon
                                               ? CollisionGroups.HitboxRanged
                                               : CollisionGroups.HitboxMelee)
                                       ?? tilePhysicsBody.CenterOffset;

            var coef = isRangedWeapon ? 0.5 : 0.6;
            var offset = coef * (objectCenterPosition - hitPoint);
            hitPoint += offset;
            return hitPoint;
        }

        public static void SharedRebuildWeaponCache(
            ICharacter character,
            WeaponState weaponState)
        {
            var protoItem = weaponState.ProtoWeapon;
            if (protoItem is null)
            {
                return;
            }

            var damageDescription = GetCurrentDamageDescription(weaponState.ItemWeapon,
                                                                protoItem,
                                                                out var protoAmmo);

            weaponState.WeaponCache = new WeaponFinalCache(
                character,
                character.SharedGetFinalStatsCache(),
                weaponState.ItemWeapon,
                weaponState.ProtoWeapon,
                protoAmmo,
                damageDescription);
        }

        public static void SharedUpdateCurrentWeapon(
            ICharacter character,
            WeaponState state,
            double deltaTime)
        {
            var protoWeapon = state.ProtoWeapon;
            if (protoWeapon is null)
            {
                return;
            }

            if (deltaTime > 0.4)
            {
                // too large delta time probably due to a frame skip
                deltaTime = 0.4;
            }

            if (state.CooldownSecondsRemains > 0)
            {
                state.CooldownSecondsRemains -= deltaTime;
                if (state.CooldownSecondsRemains < -0.2)
                {
                    // clamp the remaining cooldown in case of a frame skip
                    state.CooldownSecondsRemains = -0.2;
                }
            }

            if (state.ReadySecondsRemains > 0)
            {
                state.ReadySecondsRemains -= deltaTime;
            }

            if (state.FirePatternCooldownSecondsRemains > 0)
            {
                state.FirePatternCooldownSecondsRemains -= deltaTime;

                if (state.FirePatternCooldownSecondsRemains <= 0)
                {
                    state.FirePatternCurrentShotNumber = 0;
                }
            }

            // TODO: restore this condition when we redo UI countdown animation for ViewModelHotbarItemWeaponOverlayControl.ReloadDurationSeconds
            //if (state.CooldownSecondsRemains <= 0)
            //{
            WeaponAmmoSystem.SharedUpdateReloading(state,
                                                   character,
                                                   deltaTime,
                                                   out var isReloadingNow);
            //}

            if (Api.IsServer
                && !character.ServerIsOnline
                && state.SharedGetInputIsFiring())
            {
                state.SharedSetInputIsFiring(false);
                state.ClearFiringStateData();
            }

            // check ammo (if applicable to this weapon prototype)
            var canFire = (Api.IsClient || character.ServerIsOnline)
                          && !isReloadingNow
                          && protoWeapon.SharedCanFire(character, state);

            // perform the perk check to ensure scenarios when player cannot fire (such as a medical cooldown)
            if (canFire
                && !character.IsNpc
                && state.SharedGetInputIsFiring()
                && protoWeapon is IProtoItemWeaponRanged
                && character.SharedHasPerk(StatName.PerkCannotAttack))
            {
                // stop using weapon item
                canFire = false;

                if (IsServer)
                {
                    state.SharedSetInputIsFiring(false);
                }
                else
                {
                    state.ProtoWeapon.ClientItemUseFinish(state.ItemWeapon);
                    StatusEffectMedicalCooldown.ClientShowCooldownNotification();
                }
            }

            if (state.CooldownSecondsRemains > 0)
            {
                // firing cooldown is not completed
                if (!state.SharedGetInputIsFiring()
                    && state.IsEventWeaponStartSent)
                {
                    // not firing anymore
                    SharedCallOnWeaponInputStop(state, character);
                }

                return;
            }

            var wasFiring = state.IsFiring;
            if (!state.IsFiring)
            {
                state.IsFiring = state.SharedGetInputIsFiring();
            }
            else // if IsFiring
            {
                if (!SharedShouldFireMore(state))
                {
                    state.IsFiring = state.SharedGetInputIsFiring();
                }
            }

            if (!canFire)
            {
                // cannot fire (no ammo, etc)
                state.IsFiring = false;
            }

            if (!state.IsFiring)
            {
                if (wasFiring)
                {
                    // just stopped firing
                    SharedCallOnWeaponFinished(state, character);
                }

                if (IsServer && !state.SharedGetInputIsFiring())
                {
                    ServerCheckFiredShotsMismatch(state, character);
                }

                // the character is not firing
                // reset delay for the next shot (it will be set when firing starts next time)
                state.DamageApplyDelaySecondsRemains = 0;
                return;
            }

            if (state.WeaponCache is null)
            {
                SharedRebuildWeaponCache(character, state);
            }

            // let's process what happens when we're in the firing mode
            if (!state.IsEventWeaponStartSent)
            {
                // started firing
                SharedCallOnWeaponStart(state, character);
            }

            if (state.DamageApplyDelaySecondsRemains <= 0)
            {
                // initialize delay to next shot
                state.DamageApplyDelaySecondsRemains =
                    Shared.RoundDurationByServerFrameDuration(protoWeapon.DamageApplyDelay);

                SharedCallOnWeaponShot(character, protoWeapon);
            }

            // decrease the remaining time to the damage application
            state.DamageApplyDelaySecondsRemains -= deltaTime;

            if (state.DamageApplyDelaySecondsRemains > 0)
            {
                // firing delay not completed
                return;
            }

            // firing delay completed
            if (SharedFireWeapon(character, state.ItemWeapon, protoWeapon, state))
            {
                state.ShotsDone++;
                /*Logger.Dev(
                    string.Format("Weapon fired, shots done: {0} ({1}) Ammo available: {2}",
                                  state.ShotsDone,
                                  state.ItemWeapon?.GetPrivateState<WeaponPrivateState>().CurrentProtoItemAmmo
                                       ?.ShortId
                                  ?? "<no ammo>",
                                  state.ItemWeapon?.GetPrivateState<WeaponPrivateState>().AmmoCount ?? 0));*/
            }

            var cooldownDuration = Shared.RoundDurationByServerFrameDuration(protoWeapon.FireInterval)
                                   - Shared.RoundDurationByServerFrameDuration(protoWeapon.DamageApplyDelay);

            //Logger.Dev($"Cooldown adding: {cooldownDuration} for {protoWeapon}");

            state.CooldownSecondsRemains += cooldownDuration;

            if (!protoWeapon.IsLoopedAttackAnimation)
            {
                // we don't want to stuck this animation in the last frame
                // that's fix for the issue:
                // "Fix extended animation "stuck" issue for mobs (like limbs stuck in the end position and movement animation appears broken)"
                state.IsEventWeaponStartSent = false;
            }
        }

        [RemoteCallSettings(DeliveryMode.ReliableSequenced, timeInterval: 1 / 120.0)]
        public void ServerRemote_SetWeaponFiringMode(
            bool isFiring,
            uint clientShotsDone)
        {
            var character = ServerRemoteContext.Character;
            var weaponState = PlayerCharacter.GetPrivateState(character).WeaponState;

            //Logger.Dev(isFiring
            //               ? "SetWeaponFiringMode: firing!"
            //               : $"SetWeaponFiringMode: stop firing! Shots done: {clientShotsDone}");

            weaponState.SharedSetInputIsFiring(isFiring);
            weaponState.ServerLastClientReportedShotsDoneCount = clientShotsDone;
        }

        protected override void PrepareSystem()
        {
            if (IsServer)
            {
                Server.Characters.PlayerOnlineStateChanged += ServerPlayerOnlineStateChangedHandler;
            }
        }

        private static Vector2D GetShotEndPosition(
            bool damageRayStoppedOnLastHit,
            List<WeaponHitData> hitObjects,
            Vector2D toPosition,
            bool isRangedWeapon)
        {
            if (!damageRayStoppedOnLastHit)
            {
                return toPosition;
            }

            var hitData = hitObjects[hitObjects.Count - 1];
            var hitPoint = hitData.HitPoint.ToVector2D();
            if (hitData.IsCliffsHit)
            {
                return hitData.FallbackTilePosition.ToVector2D() + hitData.HitPoint;
            }

            var worldObject = hitData.WorldObject;
            hitPoint = SharedOffsetHitWorldPositionCloserToObjectCenter(
                worldObject,
                worldObject.ProtoWorldObject,
                hitPoint,
                isRangedWeapon);

            var hitWorldPosition = worldObject switch
            {
                IDynamicWorldObject dynamicWorldObject => dynamicWorldObject.Position,
                _                                      => worldObject.TilePosition.ToVector2D(),
            };
            return hitWorldPosition + hitPoint;
        }

        private static void ServerCheckFiredShotsMismatch(WeaponState state, ICharacter character)
        {
            var itemWeapon = state.ItemWeapon;
            if (itemWeapon is null)
            {
                return;
            }

            var ammoConsumptionPerShot = state.ProtoWeapon.AmmoConsumptionPerShot;
            if (ammoConsumptionPerShot == 0)
            {
                // weapon doesn't use any ammo - no problem with possible desync
                return;
            }

            var actualShotsDone = state.ShotsDone;
            var requestedShotsCount = state.ServerLastClientReportedShotsDoneCount;
            var extraShotsDone = (int)(actualShotsDone - (long)requestedShotsCount);
            // that's correct! even though logical it should be vice versa
            state.ShotsDone = state.ServerLastClientReportedShotsDoneCount;

            if (extraShotsDone == 0)
            {
                return;
            }

            if (extraShotsDone < 0)
            {
                // should never happen as server should fire as much as client requested, always
                return;
            }

            //Logger.Dev($"Shots count mismatch: requested={requestedShotsCount} actualShotsDone={actualShotsDone}",
            //           character);
            var currentProtoItemAmmo = itemWeapon.GetPrivateState<WeaponPrivateState>()
                                                 .CurrentProtoItemAmmo;
            Instance.CallClient(character,
                                _ => _.ClientRemote_FixAmmoCount(itemWeapon, currentProtoItemAmmo, extraShotsDone));
        }

        private static void ServerPlayerOnlineStateChangedHandler(ICharacter playerCharacter, bool isOnline)
        {
            var weaponState = playerCharacter.GetPrivateState<PlayerCharacterPrivateState>().WeaponState;
            weaponState?.ClearFiringStateData();
        }

        private static void SharedCallOnWeaponFinished(WeaponState state, ICharacter character)
        {
            state.IsEventWeaponStartSent = false;

            if (IsClient)
            {
                // finished firing weapon on Client-side
                WeaponSystemClientDisplay.ClientOnWeaponFinished(character);
            }
            else // if this is Server
            {
                // notify other clients about finished firing weapon
                using var scopedBy = Shared.GetTempList<ICharacter>();
                Server.World.GetScopedByPlayers(character, scopedBy);
                Instance.CallClient(scopedBy.AsList(),
                                    _ => _.ClientRemote_OnWeaponFinished(character));
            }
        }

        private static void SharedCallOnWeaponHitOrTrace(
            ICharacter firingCharacter,
            IProtoItemWeapon protoWeapon,
            IProtoItemAmmo protoAmmo,
            Vector2D endPosition,
            List<WeaponHitData> hitObjects,
            bool endsWithHit)
        {
            if (IsClient)
            {
                // display weapon shot on Client-side
                WeaponSystemClientDisplay.ClientOnWeaponHitOrTrace(firingCharacter,
                                                                   protoWeapon,
                                                                   protoAmmo,
                                                                   firingCharacter.ProtoCharacter,
                                                                   firingCharacter.Position.ToVector2Ushort(),
                                                                   hitObjects,
                                                                   endPosition,
                                                                   endsWithHit);
            }
            else // if server
            {
                // display damages on clients in scope of every damaged object
                var observers = new HashSet<ICharacter>();
                using var tempList = Shared.GetTempList<ICharacter>();
                Server.World.GetScopedByPlayers(firingCharacter, tempList);
                observers.AddRange(tempList.AsList());

                foreach (var hitObject in hitObjects)
                {
                    if (hitObject.IsCliffsHit
                        || hitObject.WorldObject.IsDestroyed)
                    {
                        continue;
                    }

                    if (hitObject.WorldObject is ICharacter damagedCharacter
                        && !damagedCharacter.IsNpc)
                    {
                        // notify the damaged character
                        observers.Add(damagedCharacter);
                    }

                    Server.World.GetScopedByPlayers(hitObject.WorldObject, tempList);
                    tempList.Clear();
                    observers.AddRange(tempList.AsList());
                }

                // add all observers within the sound radius (so they can not only hear but also see the traces)
                var eventNetworkRadius = (byte)Math.Max(
                    15,
                    Math.Ceiling(protoWeapon.SoundPresetWeaponDistance.max));

                tempList.Clear();
                Server.World.GetCharactersInRadius(firingCharacter.TilePosition,
                                                   tempList,
                                                   radius: eventNetworkRadius,
                                                   onlyPlayers: true);
                observers.AddRange(tempList.AsList());

                // don't notify the attacking character
                observers.Remove(firingCharacter);

                if (observers.Count > 0)
                {
                    Instance.CallClient(observers,
                                        _ => _.ClientRemote_OnWeaponHitOrTrace(firingCharacter,
                                                                               protoWeapon,
                                                                               protoAmmo,
                                                                               firingCharacter.ProtoCharacter,
                                                                               firingCharacter
                                                                                   .Position.ToVector2Ushort(),
                                                                               hitObjects.ToArray(),
                                                                               endPosition,
                                                                               endsWithHit));
                }
            }
        }

        private static void SharedCallOnWeaponInputStop(WeaponState state, ICharacter character)
        {
            Api.Assert(state.IsEventWeaponStartSent, "Firing event must be set");
            state.IsEventWeaponStartSent = false;

            if (IsClient)
            {
                // finished firing weapon on Client-side
                WeaponSystemClientDisplay.ClientOnWeaponInputStop(character);
            }
            else // if this is Server
            {
                // notify other clients about finished firing weapon
                using var scopedBy = Shared.GetTempList<ICharacter>();
                Server.World.GetScopedByPlayers(character, scopedBy);
                Instance.CallClient(
                    scopedBy.AsList(),
                    _ => _.ClientRemote_OnWeaponInputStop(character));
            }
        }

        private static void SharedCallOnWeaponShot(
            ICharacter character,
            IProtoItemWeapon protoWeapon)
        {
            if (IsClient)
            {
                // start firing weapon on Client-side
                WeaponSystemClientDisplay.ClientOnWeaponShot(character,
                                                             partyId:
                                                             0, // not relevant here as it's the current player firing the weapon
                                                             protoWeapon: protoWeapon,
                                                             protoCharacter: character.ProtoCharacter,
                                                             fallbackPosition: character.Position.ToVector2Ushort());
            }
            else // if IsServer
            {
                using var observers = Shared.GetTempList<ICharacter>();
                var eventNetworkRadius = (byte)Math.Max(
                    15,
                    Math.Ceiling(protoWeapon.SoundPresetWeaponDistance.max));

                Server.World.GetCharactersInRadius(character.TilePosition,
                                                   observers,
                                                   radius: eventNetworkRadius,
                                                   onlyPlayers: true);
                observers.Remove(character);

                if (observers.Count > 0)
                {
                    var partyId = PartySystem.ServerGetParty(character)?.Id ?? 0;

                    Instance.CallClient(observers.AsList(),
                                        _ => _.ClientRemote_OnWeaponShot(character,
                                                                         partyId,
                                                                         protoWeapon,
                                                                         character.ProtoCharacter,
                                                                         character.Position.ToVector2Ushort()));
                }
            }
        }

        private static void SharedCallOnWeaponStart(WeaponState state, ICharacter character)
        {
            Api.Assert(!state.IsEventWeaponStartSent, "Firing event must be not set");
            state.IsEventWeaponStartSent = true;

            if (IsClient)
            {
                // start firing weapon on Client-side
                WeaponSystemClientDisplay.ClientOnWeaponStart(character);
            }
            else // if IsServer
            {
                using var scopedBy = Shared.GetTempList<ICharacter>();
                Server.World.GetScopedByPlayers(character, scopedBy);
                Instance.CallClient(scopedBy.AsList(),
                                    _ => _.ClientRemote_OnWeaponStart(character));
            }
        }

        private static bool SharedFireWeapon(
            ICharacter character,
            IItem weaponItem,
            IProtoItemWeapon protoWeapon,
            WeaponState weaponState)
        {
            if (!protoWeapon.SharedOnFire(character, weaponState))
            {
                return false;
            }

            var playerCharacterSkills = character.SharedGetSkills();
            var protoWeaponSkill = playerCharacterSkills is not null
                                       ? protoWeapon.WeaponSkillProto
                                       : null;
            if (IsServer)
            {
                protoWeaponSkill?.ServerOnShot(playerCharacterSkills); // give experience for shot
            }

            var weaponCache = weaponState.WeaponCache;
            if (weaponCache is null)
            {
                SharedRebuildWeaponCache(character, weaponState);
                weaponCache = weaponState.WeaponCache;
            }

            var characterCurrentVehicle = character.IsNpc
                                              ? null
                                              : character.SharedGetCurrentVehicle();

            var isMeleeWeapon = protoWeapon is IProtoItemWeaponMelee;
            var fireSpreadAngleOffsetDeg = protoWeapon.SharedUpdateAndGetFirePatternCurrentSpreadAngleDeg(weaponState);

            // ReSharper disable once CompareOfFloatsByEqualityOperator
            if (fireSpreadAngleOffsetDeg != 0)
            {
                // Select a random angle within the spread angle (with emphasis on the max spread angle).
                // This way it's not possible to predict the weapon spread angle
                // so no cheats for spread compensation are possible.
                // A discrepancy with the server is guaranteed but as weapons with spread angle
                // are usually automatic it's fine (most shots will hit and deal damage).
                fireSpreadAngleOffsetDeg = Math.Abs(fireSpreadAngleOffsetDeg)
                                           * Math.Pow(RandomHelper.NextDouble(), 0.25)
                                           * (RandomHelper.Next(0, 2) == 0
                                                  ? -1
                                                  : 1);
            }

            using var allHitObjects = Shared.GetTempList<IWorldObject>();
            var shotsPerFire = weaponCache.FireScatterPreset.ProjectileAngleOffets;
            foreach (var angleOffsetDeg in shotsPerFire)
            {
                SharedShotWeaponHitscan(character,
                                        protoWeapon,
                                        weaponCache,
                                        weaponState.CustomTargetPosition,
                                        fireSpreadAngleOffsetDeg + angleOffsetDeg,
                                        isMeleeWeapon,
                                        characterCurrentVehicle,
                                        protoWeaponSkill,
                                        playerCharacterSkills,
                                        allHitObjects);
            }

            if (IsServer)
            {
                protoWeapon.ServerOnShot(character, weaponItem, protoWeapon, allHitObjects.AsList());
            }

            return true;
        }

        private static void SharedShotWeaponHitscan(
            ICharacter character,
            IProtoItemWeapon protoWeapon,
            WeaponFinalCache weaponCache,
            Vector2D? customTargetPosition,
            double fireSpreadAngleOffsetDeg,
            bool isMeleeWeapon,
            IDynamicWorldObject characterCurrentVehicle,
            ProtoSkillWeapons protoWeaponSkill,
            PlayerCharacterSkills playerCharacterSkills,
            ITempList<IWorldObject> allHitObjects)
        {
            var collisionGroup = protoWeapon.CollisionGroup;
            var characterRotationAngleRad = ((IProtoCharacterCore)character.ProtoCharacter)
                .SharedGetRotationAngleRad(character);

            SharedCastLine(character,
                           isMeleeWeapon: weaponCache.ProtoWeapon is IProtoItemWeaponMelee,
                           weaponCache.RangeMax,
                           characterRotationAngleRad,
                           customTargetPosition,
                           fireSpreadAngleOffsetDeg,
                           collisionGroup,
                           out var toPosition,
                           out var tempLineTestResults,
                           sendDebugEvent: true);

            using (tempLineTestResults)
            {
                var damageMultiplier = 1d;
                var hitObjects = new List<WeaponHitData>(isMeleeWeapon ? 1 : tempLineTestResults.Count);
                var characterTileHeight = character.Tile.Height;

                if (IsClient
                    || Api.IsEditor)
                {
                    SharedEditorPhysicsDebugger.SharedVisualizeTestResults(tempLineTestResults, collisionGroup);
                }

                var isDamageRayStopped = false;
                foreach (var testResult in tempLineTestResults.AsList())
                {
                    var testResultPhysicsBody = testResult.PhysicsBody;
                    var attackedProtoTile = testResultPhysicsBody.AssociatedProtoTile;
                    if (attackedProtoTile is not null)
                    {
                        if (attackedProtoTile.Kind != TileKind.Solid)
                        {
                            // non-solid obstacle - skip
                            continue;
                        }

                        var attackedTile = IsServer
                                               ? Server.World.GetTile((Vector2Ushort)testResultPhysicsBody.Position)
                                               : Client.World.GetTile((Vector2Ushort)testResultPhysicsBody.Position);

                        if (attackedTile.Height < characterTileHeight)
                        {
                            // attacked tile is below - ignore it
                            continue;
                        }

                        // tile on the way - blocking damage ray
                        isDamageRayStopped = true;
                        var hitData = new WeaponHitData(testResult.PhysicsBody.Position
                                                        + SharedOffsetHitWorldPositionCloserToTileHitboxCenter(
                                                            testResultPhysicsBody,
                                                            testResult.Penetration,
                                                            isRangedWeapon: !isMeleeWeapon));
                        hitObjects.Add(hitData);

                        weaponCache.ProtoWeapon
                                   .SharedOnHit(weaponCache,
                                                null,
                                                0,
                                                hitData,
                                                out _);
                        break;
                    }

                    var damagedObject = testResultPhysicsBody.AssociatedWorldObject;
                    if (ReferenceEquals(damagedObject,    character)
                        || ReferenceEquals(damagedObject, characterCurrentVehicle))
                    {
                        // ignore collision with self
                        continue;
                    }

                    if (!(damagedObject.ProtoGameObject is IDamageableProtoWorldObject damageableProto))
                    {
                        // shoot through this object
                        continue;
                    }

                    // don't allow damage is there is no direct line of sight on physical colliders layer between the two objects
                    if (SharedHasTileObstacle(character.Position,
                                              characterTileHeight,
                                              damagedObject,
                                              targetPosition: testResult.PhysicsBody.Position
                                                              + testResult.PhysicsBody.CenterOffset))
                    {
                        continue;
                    }

                    using (CharacterDamageContext.Create(attackerCharacter: character,
                                                         damagedObject as ICharacter,
                                                         protoWeaponSkill))
                    {
                        if (!damageableProto.SharedOnDamage(
                                weaponCache,
                                damagedObject,
                                damageMultiplier,
                                damagePostMultiplier: 1.0,
                                out var obstacleBlockDamageCoef,
                                out var damageApplied))
                        {
                            // not hit
                            continue;
                        }

                        var hitData = new WeaponHitData(damagedObject,
                                                        testResult.Penetration.ToVector2F());
                        weaponCache.ProtoWeapon
                                   .SharedOnHit(weaponCache,
                                                damagedObject,
                                                damageApplied,
                                                hitData,
                                                out var isDamageStop);

                        if (isDamageStop)
                        {
                            obstacleBlockDamageCoef = 1;
                        }

                        if (IsServer
                            && damageApplied > 0)
                        {
                            // give experience for damage
                            protoWeaponSkill?.ServerOnDamageApplied(playerCharacterSkills,
                                                                    damagedObject,
                                                                    damageApplied);
                        }

                        if (obstacleBlockDamageCoef < 0
                            || obstacleBlockDamageCoef > 1)
                        {
                            Logger.Error(
                                "Obstacle block damage coefficient should be >= 0 and <= 1 - wrong calculation by "
                                + damageableProto);
                            break;
                        }

                        hitObjects.Add(hitData);

                        if (isMeleeWeapon)
                        {
                            // currently melee weapon could attack only one object on the ray
                            isDamageRayStopped = true;
                            break;
                        }

                        damageMultiplier *= 1.0 - obstacleBlockDamageCoef;
                        if (damageMultiplier <= 0)
                        {
                            // target blocked the damage ray
                            isDamageRayStopped = true;
                            break;
                        }
                    }
                }

                var shotEndPosition = GetShotEndPosition(isDamageRayStopped,
                                                         hitObjects,
                                                         toPosition,
                                                         isRangedWeapon: !isMeleeWeapon);
                if (hitObjects.Count == 0)
                {
                    protoWeapon.SharedOnMiss(weaponCache,
                                             shotEndPosition);
                }

                SharedCallOnWeaponHitOrTrace(character,
                                             protoWeapon,
                                             weaponCache.ProtoAmmo,
                                             shotEndPosition,
                                             hitObjects,
                                             endsWithHit: isDamageRayStopped);

                foreach (var entry in hitObjects)
                {
                    if (!entry.IsCliffsHit
                        && !allHitObjects.Contains(entry.WorldObject))
                    {
                        allHitObjects.Add(entry.WorldObject);
                    }
                }
            }
        }

        private static bool SharedShouldFireMore(WeaponState state)
        {
            if (!state.IsFiring)
            {
                return false;
            }

            // is firing delay completed?
            var canStopFiring = state.DamageApplyDelaySecondsRemains <= 0;

            if (canStopFiring
                && Api.IsServer
                && state.ShotsDone < state.ServerLastClientReportedShotsDoneCount)
            {
                // cannot stop firing if not all the ammo are fired yet
                // let's spend all the remaining ammo before stopping firing
                canStopFiring = false;
                //Logger.Dev("Not all shots done yet, delay stopping firing: shotsDone="
                //           + state.ShotsDone
                //           + " requiresShotsDone="
                //           + state.ServerLastClientReportedShotsDoneCount);
            }

            return !canStopFiring;
        }

        // in case server fired more ammo than the client we can fix this here
        [RemoteCallSettings(DeliveryMode.ReliableOrdered)]
        private void ClientRemote_FixAmmoCount(
            IItem itemWeapon,
            IProtoItemAmmo currentProtoItemAmmo,
            int extraShotsDone)
        {
            var ammoConsumptionPerShot = ((IProtoItemWeapon)itemWeapon.ProtoItem).AmmoConsumptionPerShot;
            var deltaAmmo = -extraShotsDone * ammoConsumptionPerShot;

            var weaponPrivateState = itemWeapon.GetPrivateState<WeaponPrivateState>();
            if (!ReferenceEquals(weaponPrivateState.CurrentProtoItemAmmo, currentProtoItemAmmo))
            {
                return;
            }

            var previousAmmoCount = weaponPrivateState.AmmoCount;
            weaponPrivateState.SetAmmoCount(
                (ushort)MathHelper.Clamp(
                    previousAmmoCount + deltaAmmo,
                    0,
                    ushort.MaxValue));

            //Logger.Dev(
            //    $"Client correcting loaded ammo count for weapon by server request: {itemWeapon}. New ammo count {weaponPrivateState.AmmoCount}. Previous ammo count: {previousAmmoCount}. Delta ammo (correction): {deltaAmmo}");
        }

        [RemoteCallSettings(
            DeliveryMode.ReliableSequenced,
            timeInterval: 1 / 60.0,
            keyArgIndex: 0,
            groupName: RemoteCallSequenceGroupCharacterFiring)]
        private void ClientRemote_OnWeaponFinished(ICharacter whoFires)
        {
            if (whoFires is null
                || !whoFires.IsInitialized)
            {
                return;
            }

            WeaponSystemClientDisplay.ClientOnWeaponFinished(whoFires);
        }

        [RemoteCallSettings(DeliveryMode.Unreliable, timeInterval: 1 / 120.0)]
        private void ClientRemote_OnWeaponHitOrTrace(
            ICharacter firingCharacter,
            IProtoItemWeapon protoWeapon,
            IProtoItemAmmo protoAmmo,
            IProtoCharacter protoCharacter,
            Vector2Ushort fallbackCharacterPosition,
            WeaponHitData[] hitObjects,
            Vector2D endPosition,
            bool endsWithHit)
        {
            WeaponSystemClientDisplay.ClientOnWeaponHitOrTrace(firingCharacter,
                                                               protoWeapon,
                                                               protoAmmo,
                                                               protoCharacter,
                                                               fallbackCharacterPosition,
                                                               hitObjects,
                                                               endPosition,
                                                               endsWithHit);
        }

        [RemoteCallSettings(
            DeliveryMode.ReliableSequenced,
            timeInterval: 1 / 60.0,
            keyArgIndex: 0,
            groupName: RemoteCallSequenceGroupCharacterFiring)]
        private void ClientRemote_OnWeaponInputStop(ICharacter whoFires)
        {
            if (whoFires is null
                || !whoFires.IsInitialized)
            {
                return;
            }

            WeaponSystemClientDisplay.ClientOnWeaponInputStop(whoFires);
        }

        [RemoteCallSettings(DeliveryMode.Unreliable, timeInterval: 1 / 60.0)]
        private void ClientRemote_OnWeaponShot(
            ICharacter whoFires,
            uint partyId,
            IProtoItemWeapon protoWeapon,
            IProtoCharacter fallbackProtoCharacter,
            Vector2Ushort fallbackPosition)
        {
            if (whoFires is not null
                && !whoFires.IsInitialized)
            {
                whoFires = null;
            }

            WeaponSystemClientDisplay.ClientOnWeaponShot(whoFires,
                                                         partyId,
                                                         protoWeapon,
                                                         fallbackProtoCharacter,
                                                         fallbackPosition);
        }

        [RemoteCallSettings(
            DeliveryMode.ReliableSequenced,
            timeInterval: 1 / 60.0,
            keyArgIndex: 0,
            groupName: RemoteCallSequenceGroupCharacterFiring)]
        private void ClientRemote_OnWeaponStart(ICharacter whoFires)
        {
            if (whoFires is null
                || !whoFires.IsInitialized)
            {
                return;
            }

            WeaponSystemClientDisplay.ClientOnWeaponStart(whoFires);
        }
    }
}