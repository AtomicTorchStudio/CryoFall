namespace AtomicTorch.CBND.CoreMod.Systems.Weapons
{
    using System.Collections.Generic;
    using AtomicTorch.CBND.CoreMod.Characters;
    using AtomicTorch.CBND.CoreMod.Characters.Player;
    using AtomicTorch.CBND.CoreMod.CharacterStatusEffects.Debuffs;
    using AtomicTorch.CBND.CoreMod.Helpers.Client;
    using AtomicTorch.CBND.CoreMod.Items.Weapons;
    using AtomicTorch.CBND.CoreMod.Skills;
    using AtomicTorch.CBND.CoreMod.Systems.Physics;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.Items;
    using AtomicTorch.CBND.GameApi.Data.State;
    using AtomicTorch.CBND.GameApi.Data.Weapons;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.Scripting.Network;
    using AtomicTorch.GameEngine.Common.Helpers;
    using AtomicTorch.GameEngine.Common.Primitives;

    /// <summary>
    /// System to process firing/reloading/switching ammo of the weapons.
    /// </summary>
    public class WeaponSystem : ProtoSystem<WeaponSystem>
    {
        private const string RemoteCallSequenceGroupCharacterFiring = "CharacterWeaponFiringSequence";

        public override string Name => "Weapon system logic.";

        public static void ClientChangeWeaponFiringMode(bool isFiring)
        {
            var character = Client.Characters.CurrentPlayerCharacter;
            var state = PlayerCharacter.GetPrivateState(character).WeaponState;

            if (state.SharedGetInputIsFiring()
                == isFiring)
            {
                // no need to change the firing input mode
                return;
            }

            state.SetInputIsFiring(isFiring);

            uint shotsDone = 0;
            if (!isFiring)
            {
                // stopping firing
                shotsDone = state.ShotsDone;
                if (SharedShouldFireMore(state))
                {
                    // assume we will attempt fire a shot more
                    var ammoConsumptionPerShot = state.ActiveProtoWeapon.AmmoConsumptionPerShot;
                    if (ammoConsumptionPerShot > 0)
                    {
                        var ammoCountAvailable = state.ActiveItemWeapon?
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
            Instance.CallServer(_ => _.ServerRemote_SetWeaponFiringMode(isFiring, shotsDone));
        }

        public static void RebuildWeaponCache(
            ICharacter character,
            WeaponState weaponState)
        {
            DamageDescription damageDescription = null;
            var item = weaponState.ActiveItemWeapon;
            var protoItem = weaponState.ActiveProtoWeapon;
            if (protoItem == null)
            {
                return;
            }

            if (protoItem.OverrideDamageDescription != null)
            {
                damageDescription = protoItem.OverrideDamageDescription;
            }
            else if (item != null)
            {
                var weaponPrivateState = item.GetPrivateState<WeaponPrivateState>();
                damageDescription = weaponPrivateState.CurrentProtoItemAmmo?.DamageDescription;
            }

            weaponState.WeaponCache = new WeaponFinalCache(
                character,
                character.SharedGetFinalStatsCache(),
                item,
                weaponState.ActiveProtoWeapon,
                damageDescription);
        }

        public static void SharedUpdateCurrentWeapon(
            ICharacter character,
            WeaponState state,
            double deltaTime)
        {
            var protoWeapon = state.ActiveProtoWeapon;
            if (protoWeapon == null)
            {
                return;
            }

            if (state.CooldownSecondsRemains > 0)
            {
                // decrease cooldown
                state.CooldownSecondsRemains -= deltaTime;
            }

            if (!state.IsFiring)
            {
                WeaponAmmoSystem.SharedUpdateReloading(state, character, ref deltaTime);
            }

            if (deltaTime <= 0)
            {
                // the weapon reloading process is consumed the whole delta time
                return;
            }

            if (state.SharedGetInputIsFiring()
                && !character.IsOnline)
            {
                state.SetInputIsFiring(false);
            }

            if (state.SharedGetInputIsFiring()
                && StatusEffectDazed.SharedIsCharacterDazed(character,
                                                            StatusEffectDazed.NotificationCannotAttackWhileDazed))
            {
                state.SetInputIsFiring(false);
            }

            // check ammo (if applicable to this weapon prototype)
            var canFire = protoWeapon.SharedCanFire(character, state);
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

                // the character is not firing
                // reset delay for the next shot (it will be set when firing starts next time)
                state.DamageApplyDelaySecondsRemains = 0;
                return;
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
                state.DamageApplyDelaySecondsRemains = protoWeapon.DamageApplyDelay;
                SharedCallOnWeaponShot(character);
            }

            // decrease the remaining time to the damage application
            state.DamageApplyDelaySecondsRemains -= deltaTime;

            if (state.DamageApplyDelaySecondsRemains > 0)
            {
                // firing delay not completed
                return;
            }

            // firing delay completed
            state.ShotsDone++;
            //Logger.Dev("Weapon fired, shots done: " + state.ShotsDone);
            SharedFireWeapon(character, state.ActiveItemWeapon, protoWeapon, state);
            state.CooldownSecondsRemains += protoWeapon.FireInterval - protoWeapon.DamageApplyDelay;

            if (!protoWeapon.IsLoopedAttackAnimation)
            {
                // we don't want to stuck this animation in the last frame
                // that's fix for the issue:
                // "Fix extended animation "stuck" issue for mobs (like limbs stuck in the end position and movement animation appears broken)"
                state.IsEventWeaponStartSent = false;
            }
        }

        private static void ServerCheckFiredShotsMismatch(WeaponState state, ICharacter character)
        {
            var ammoConsumptionPerShot = state.ActiveProtoWeapon.AmmoConsumptionPerShot;
            if (ammoConsumptionPerShot == 0)
            {
                // weapon doesn't use any ammo - no problem with possible desync
                return;
            }

            var requestedShotsCount = state.ServerLastClientReportedShotsDoneCount;
            if (!requestedShotsCount.HasValue)
            {
                return;
            }

            var extraShotsDone = (int)(state.ShotsDone - (long)requestedShotsCount.Value);
            state.ServerLastClientReportedShotsDoneCount = null;

            if (extraShotsDone == 0)
            {
                return;
            }

            if (extraShotsDone < 0)
            {
                // should never happen as server should fire as much as client requested, always
                return;
            }

            var itemWeapon = state.ActiveItemWeapon;
            if (itemWeapon == null)
            {
                return;
            }

            //Logger.Dev($"Shots count mismatch: requested={requestedShotsCount} actualShotsDone={state.ShotsDone}");

            Instance.CallClient(character,
                                _ => _.ClientRemote_FixAmmoCount(itemWeapon, extraShotsDone));
        }

        private static void SharedCallOnWeaponFinished(WeaponState state, ICharacter character)
        {
            if (IsServer)
            {
                ServerCheckFiredShotsMismatch(state, character);
            }

            state.IsEventWeaponStartSent = false;

            if (IsClient)
            {
                // finished firing weapon on Client-side
                WeaponSystemClientDisplay.OnWeaponFinished(character);
            }
            else // if this is Server
            {
                // notify other clients about finished firing weapon
                using (var scopedBy = Api.Shared.GetTempList<ICharacter>())
                {
                    Server.World.GetScopedByPlayers(character, scopedBy);
                    Instance.CallClient(
                        scopedBy,
                        _ => _.ClientRemote_OnWeaponFinished(character));
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
                WeaponSystemClientDisplay.OnWeaponInputStop(character);
            }
            else // if this is Server
            {
                // notify other clients about finished firing weapon
                using (var scopedBy = Api.Shared.GetTempList<ICharacter>())
                {
                    Server.World.GetScopedByPlayers(character, scopedBy);
                    Instance.CallClient(
                        scopedBy,
                        _ => _.ClientRemote_OnWeaponInputStop(character));
                }
            }
        }

        private static void SharedCallOnWeaponShot(ICharacter character)
        {
            if (IsClient)
            {
                // start firing weapon on Client-side
                WeaponSystemClientDisplay.OnWeaponShot(character);
            }
            else // if IsServer
            {
                using (var scopedBy = Api.Shared.GetTempList<ICharacter>())
                {
                    Server.World.GetScopedByPlayers(character, scopedBy);
                    Instance.CallClient(scopedBy,
                                        _ => _.ClientRemote_OnWeaponShot(character));
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
                WeaponSystemClientDisplay.OnWeaponStart(character);
            }
            else // if IsServer
            {
                using (var scopedBy = Api.Shared.GetTempList<ICharacter>())
                {
                    Server.World.GetScopedByPlayers(character, scopedBy);
                    Instance.CallClient(scopedBy,
                                        _ => _.ClientRemote_OnWeaponStart(character));
                }
            }
        }

        private static void SharedFireWeapon(
            ICharacter character,
            IItem weaponItem,
            IProtoItemWeapon protoWeapon,
            WeaponState weaponState)
        {
            protoWeapon.SharedOnFire(character, weaponState);

            var playerCharacterSkills = character.SharedGetSkills();
            var protoWeaponSkill = playerCharacterSkills != null
                                       ? protoWeapon.WeaponSkillProto
                                       : null;
            if (IsServer)
            {
                // give experience for shot
                protoWeaponSkill?.ServerOnShot(playerCharacterSkills);
            }

            var weaponCache = weaponState.WeaponCache;
            if (weaponCache == null)
            {
                // calculate new weapon cache
                RebuildWeaponCache(character, weaponState);
                weaponCache = weaponState.WeaponCache;
            }

            // raycast possible victims
            var fromPosition = character.Position
                               + (0, character.ProtoCharacter.CharacterWorldWeaponOffset);

            var toPosition = fromPosition
                             + new Vector2D(weaponCache.RangeMax, 0)
                                 .RotateRad(character.ProtoCharacter.SharedGetRotationAngleRad(character));

            var collisionGroup = protoWeapon is IProtoItemWeaponMelee
                                     ? CollisionGroups.HitboxMelee
                                     : CollisionGroups.HitboxRanged;

            using (var lineTestResults = character.PhysicsBody.PhysicsSpace.TestLine(
                fromPosition: fromPosition,
                toPosition: toPosition,
                collisionGroup: collisionGroup))
            {
                var damageMultiplier = 1d;
                var isMeleeWeapon = protoWeapon is IProtoItemWeaponMelee;
                var hitObjects = new List<WeaponHitData>(isMeleeWeapon ? 1 : lineTestResults.Count);

                foreach (var testResult in lineTestResults)
                {
                    var testResultPhysicsBody = testResult.PhysicsBody;
                    var attackedProtoTile = testResultPhysicsBody.AssociatedProtoTile;
                    if (attackedProtoTile != null)
                    {
                        if (attackedProtoTile.Kind != TileKind.Solid)
                        {
                            // non-solid obstacle - skip
                            continue;
                        }

                        // tile on the way - blocking damage ray
                        break;
                    }

                    var damagedObject = testResultPhysicsBody.AssociatedWorldObject;
                    if (damagedObject == character)
                    {
                        // ignore collision with self
                        continue;
                    }

                    if (!(damagedObject.ProtoGameObject is IDamageableProtoWorldObject damageableProto))
                    {
                        // shoot through this object
                        continue;
                    }

                    if (!damageableProto.SharedOnDamage(
                            weaponCache,
                            damagedObject,
                            damageMultiplier,
                            out var obstacleBlockDamageCoef,
                            out var damageApplied))
                    {
                        // not hit
                        continue;
                    }

                    if (IsServer)
                    {
                        weaponCache.ProtoWeapon
                                   .ServerOnDamageApplied(weaponCache.Weapon, character, damagedObject, damageApplied);

                        if (damageApplied > 0
                            && protoWeaponSkill != null)
                        {
                            // give experience for damage
                            protoWeaponSkill.ServerOnDamageApplied(playerCharacterSkills, damagedObject, damageApplied);

                            if (damagedObject is ICharacter damagedCharacter
                                && damagedCharacter.GetPublicState<ICharacterPublicState>().CurrentStats.HealthCurrent
                                <= 0)
                            {
                                // give weapon experience for kill
                                Logger.Info("Killed " + damagedCharacter, character);
                                protoWeaponSkill.ServerOnKill(playerCharacterSkills, killedCharacter: damagedCharacter);

                                if (damagedCharacter.ProtoCharacter is ProtoCharacterMob protoMob)
                                {
                                    // give hunting skill experience for mob kill
                                    var experience = SkillHunting.ExperienceForKill;
                                    experience *= protoMob.MobKillExperienceMultiplier;
                                    if (experience > 0)
                                    {
                                        playerCharacterSkills.ServerAddSkillExperience<SkillHunting>(experience);
                                    }
                                }
                            }
                        }
                    }

                    if (obstacleBlockDamageCoef < 0
                        || obstacleBlockDamageCoef > 1)
                    {
                        Logger.Error(
                            "Obstacle block damage coefficient should be >= 0 and <= 1 - wrong calculation by "
                            + damageableProto);
                        break;
                    }

                    //var hitPosition = testResultPhysicsBody.Position + testResult.Penetration;
                    hitObjects.Add(new WeaponHitData(damagedObject)); //, hitPosition));

                    if (isMeleeWeapon)
                    {
                        // currently melee weapon could attack only one object on the ray
                        break;
                    }

                    damageMultiplier = damageMultiplier * (1.0 - obstacleBlockDamageCoef);
                    if (damageMultiplier <= 0)
                    {
                        // target blocked the damage ray
                        break;
                    }
                }

                if (hitObjects.Count > 0)
                {
                    if (IsClient)
                    {
                        // display weapon shot on Client-side
                        WeaponSystemClientDisplay.OnWeaponHit(protoWeapon, hitObjects);
                    }
                    else // if server
                    {
                        // display damages on clients in scope of every damaged object
                        using (var scopedBy = Api.Shared.GetTempList<ICharacter>())
                        {
                            foreach (var hitObject in hitObjects)
                            {
                                if (hitObject.WorldObject.IsDestroyed)
                                {
                                    continue;
                                }

                                Server.World.GetScopedByPlayers(hitObject.WorldObject, scopedBy);
                                scopedBy.Remove(character);
                                if (scopedBy.Count == 0)
                                {
                                    continue;
                                }

                                Instance.CallClient(scopedBy,
                                                    _ => _.ClientRemote_OnWeaponHit(protoWeapon, hitObject));
                                scopedBy.Clear();
                            }
                        }
                    }
                }

                if (IsServer)
                {
                    protoWeapon.ServerOnShot(character, weaponItem, protoWeapon, hitObjects);
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
                && IsServer
                && state.ServerLastClientReportedShotsDoneCount.HasValue)
            {
                // cannot stop firing if not all the ammo are fired yet
                if (state.ShotsDone < state.ServerLastClientReportedShotsDoneCount)
                {
                    // let's spend all the remaining ammo before stopping firing
                    canStopFiring = false;
                    //Logger.Dev("Not all shots done yet, delay stopping firing: shotsDone="
                    //           + state.ShotsDone
                    //           + " requiresShotsDone="
                    //           + state.ServerLastClientReportedShotsDoneCount);
                }
            }

            return !canStopFiring;
        }

        // in case server fired more ammo than the client we can fix this here
        [RemoteCallSettings(DeliveryMode.ReliableOrdered)]
        private void ClientRemote_FixAmmoCount(IItem itemWeapon, int extraShotsDone)
        {
            var ammoConsumptionPerShot = ((IProtoItemWeapon)itemWeapon.ProtoItem).AmmoConsumptionPerShot;
            var deltaAmmo = -extraShotsDone * ammoConsumptionPerShot;

            var weaponPrivateState = itemWeapon.GetPrivateState<WeaponPrivateState>();
            //Logger.Dev("Client correcting ammo count for weapon by server request: "
            //           + itemWeapon
            //           + ". Current ammo count: "
            //           + weaponPrivateState.AmmoCount
            //           + ". Delta ammo (correction): "
            //           + deltaAmmo);

            weaponPrivateState.AmmoCount = (ushort)MathHelper.Clamp(
                weaponPrivateState.AmmoCount + deltaAmmo,
                0,
                ushort.MaxValue);

            var state = ClientCurrentCharacterHelper.PrivateState.WeaponState;
            state.ShotsDone = (uint)MathHelper.Clamp(
                state.ShotsDone + extraShotsDone,
                0,
                ushort.MaxValue);
        }

        [RemoteCallSettings(
            DeliveryMode.ReliableSequenced,
            maxCallsPerSecond: 60,
            keyArgIndex: 0,
            groupName: RemoteCallSequenceGroupCharacterFiring)]
        private void ClientRemote_OnWeaponFinished(ICharacter whoFires)
        {
            WeaponSystemClientDisplay.OnWeaponFinished(whoFires);
        }

        [RemoteCallSettings(DeliveryMode.Unreliable, maxCallsPerSecond: 60)]
        private void ClientRemote_OnWeaponHit(IProtoItemWeapon protoWeapon, WeaponHitData hitObject)
        {
            using (var tempList = Api.Shared.WrapObjectInTempList(hitObject))
            {
                WeaponSystemClientDisplay.OnWeaponHit(protoWeapon, tempList);
            }
        }

        [RemoteCallSettings(
            DeliveryMode.ReliableSequenced,
            maxCallsPerSecond: 60,
            keyArgIndex: 0,
            groupName: RemoteCallSequenceGroupCharacterFiring)]
        private void ClientRemote_OnWeaponInputStop(ICharacter whoFires)
        {
            WeaponSystemClientDisplay.OnWeaponInputStop(whoFires);
        }

        [RemoteCallSettings(DeliveryMode.Unreliable, maxCallsPerSecond: 60)]
        private void ClientRemote_OnWeaponShot(ICharacter whoFires)
        {
            WeaponSystemClientDisplay.OnWeaponShot(whoFires);
        }

        [RemoteCallSettings(
            DeliveryMode.ReliableSequenced,
            maxCallsPerSecond: 60,
            keyArgIndex: 0,
            groupName: RemoteCallSequenceGroupCharacterFiring)]
        private void ClientRemote_OnWeaponStart(ICharacter whoFires)
        {
            WeaponSystemClientDisplay.OnWeaponStart(whoFires);
        }

        [RemoteCallSettings(DeliveryMode.ReliableSequenced, maxCallsPerSecond: 120)]
        private void ServerRemote_SetWeaponFiringMode(
            bool isFiring,
            uint clientShotsDone)
        {
            var character = ServerRemoteContext.Character;
            var weaponState = PlayerCharacter.GetPrivateState(character).WeaponState;

            //Logger.Dev(isFiring
            //               ? "SetWeaponFiringMode: firing!"
            //               : $"SetWeaponFiringMode: stop firing! Shots done: {clientShotsDone}");

            weaponState.SetInputIsFiring(isFiring,
                                         clientShotsDone);
        }
    }
}