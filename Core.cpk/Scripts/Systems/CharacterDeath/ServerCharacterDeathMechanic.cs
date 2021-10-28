namespace AtomicTorch.CBND.CoreMod.Systems.CharacterDeath
{
    using System;
    using AtomicTorch.CBND.CoreMod.Characters;
    using AtomicTorch.CBND.CoreMod.Characters.Player;
    using AtomicTorch.CBND.CoreMod.Skills;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Loot;
    using AtomicTorch.CBND.CoreMod.Systems.CharacterDamageTrackingSystem;
    using AtomicTorch.CBND.CoreMod.Systems.CharacterDespawnSystem;
    using AtomicTorch.CBND.CoreMod.Systems.CharacterRespawn;
    using AtomicTorch.CBND.CoreMod.Systems.Crafting;
    using AtomicTorch.CBND.CoreMod.Systems.Droplists;
    using AtomicTorch.CBND.CoreMod.Systems.NewbieProtection;
    using AtomicTorch.CBND.CoreMod.Systems.PvE;
    using AtomicTorch.CBND.CoreMod.Systems.ServerTimers;
    using AtomicTorch.CBND.CoreMod.Systems.WorldObjectClaim;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.Items;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.ServicesServer;

    public class ServerCharacterDeathMechanic
    {
        private const double PlayerTeleportToGraveyardDelaySeconds = 10;

        private static readonly IItemsServerService ServerItems = Api.IsServer
                                                                      ? Api.Server.Items
                                                                      : null;

        private static readonly IWorldServerService ServerWorld = Api.IsServer
                                                                      ? Api.Server.World
                                                                      : null;

        public delegate void DelegateCharacterKilled(ICharacter attackerCharacter, ICharacter targetCharacter);

        public static event Action<ICharacter> CharacterDeath;

        public static event DelegateCharacterKilled CharacterKilled;

        public static void OnCharacterDeath(ICharacter deadCharacter)
        {
            var publicState = deadCharacter.GetPublicState<ICharacterPublicState>();
            if (!publicState.IsDead)
            {
                publicState.CurrentStats.ServerSetHealthCurrent(0);
                return;
            }

            // recreate physics (as dead character doesn't have any physics)
            deadCharacter.ProtoCharacter.SharedCreatePhysics(deadCharacter);

            if (deadCharacter.ProtoCharacter is IProtoCharacterMob protoCharacterMob)
            {
                Api.SafeInvoke(() => CharacterDeath?.Invoke(deadCharacter));
                protoCharacterMob.ServerOnDeath(deadCharacter);
                return;
            }

            // player character death
            // remember the death position (useful for the respawn)
            var privateState = PlayerCharacter.GetPrivateState(deadCharacter);
            privateState.LastDeathPosition = deadCharacter.TilePosition;
            privateState.LastDeathTime = Api.Server.Game.FrameTime;
            ServerTimersSystem.AddAction(
                delaySeconds: PlayerTeleportToGraveyardDelaySeconds,
                () =>
                {
                    if (!publicState.IsDead)
                    {
                        // player has already respawned
                        return;
                    }

                    CharacterDespawnSystem.ServerTeleportPlayerCharacterToServiceArea(deadCharacter);
                    CharacterRespawnSystem.ServerRemoveStatusEffectsOnRespawn(deadCharacter);
                });

            var isPvPdeath = CharacterDamageTrackingSystem.ServerGetPvPdamagePercent(deadCharacter)
                             >= 0.5;
            // register death (required even if the player is not a newbie)
            NewbieProtectionSystem.ServerRegisterDeath(deadCharacter,
                                                       isPvPdeath,
                                                       out var shouldSufferDeathConsequences);

            if (shouldSufferDeathConsequences)
            {
                DropPlayerLoot(deadCharacter);
            }
            else
            {
                Api.Logger.Important("Player character is dead - newbie PvP case, no loot drop or other consequences",
                                     deadCharacter);
            }

            Api.SafeInvoke(() => CharacterDeath?.Invoke(deadCharacter));
        }

        public static void OnCharacterInitialize(ICharacter character)
        {
            if (character.IsNpc)
            {
                return;
            }

            if (PlayerCharacter.GetPublicState(character).IsDead)
            {
                CharacterDespawnSystem.ServerTeleportPlayerCharacterToServiceArea(character);
                CharacterRespawnSystem.ServerRemoveStatusEffectsOnRespawn(character);
            }
            else if (PlayerCharacter.GetPrivateState(character).IsDespawned)
            {
                CharacterDespawnSystem.ServerTeleportPlayerCharacterToServiceArea(character);
            }
        }

        public static void OnCharacterKilled(
            ICharacter attackerCharacter,
            ICharacter targetCharacter,
            IProtoSkill protoSkill)
        {
            if (attackerCharacter is null
                || targetCharacter is null)
            {
                return;
            }

            Api.Logger.Info("Killed " + targetCharacter, attackerCharacter);
            Api.SafeInvoke(
                () => CharacterKilled?.Invoke(attackerCharacter, targetCharacter));

            if (attackerCharacter.IsNpc
                || !targetCharacter.IsNpc)
            {
                return;
            }

            var playerCharacterSkills = attackerCharacter.SharedGetSkills();

            // give hunting skill experience for mob kill
            var huntXP = SkillHunting.ExperienceForKill;
            huntXP *= ((IProtoCharacterMob)targetCharacter.ProtoGameObject).MobKillExperienceMultiplier;
            if (huntXP > 0)
            {
                playerCharacterSkills.ServerAddSkillExperience<SkillHunting>(huntXP);
            }

            if (protoSkill is ProtoSkillWeapons protoSkillWeapon)
            {
                // give weapon experience for kill
                protoSkillWeapon.ServerOnKill(playerCharacterSkills, killedCharacter: targetCharacter);
            }

            if (!WorldObjectClaimSystem.SharedIsEnabled)
            {
                return;
            }

            // try claim the corpse for the attacker player
            using var tempListCorpses = Api.Shared.GetTempList<IStaticWorldObject>();
            Api.GetProtoEntity<ObjectCorpse>()
               .GetAllGameObjects(tempListCorpses.AsList());
            foreach (var worldObjectCorpse in tempListCorpses.AsList())
            {
                if (targetCharacter.Id
                    == ObjectCorpse.GetPublicState(worldObjectCorpse).DeadCharacterId)
                {
                    WorldObjectClaimSystem.ServerTryClaim(worldObjectCorpse,
                                                          attackerCharacter,
                                                          WorldObjectClaimDuration.CreatureCorpse);
                    return;
                }
            }
        }

        private static void DropPlayerLoot(ICharacter character)
        {
            var containerEquipment = character.SharedGetPlayerContainerEquipment();
            var containerHand = character.SharedGetPlayerContainerHand();
            var containerHotbar = character.SharedGetPlayerContainerHotbar();
            var containerInventory = character.SharedGetPlayerContainerInventory();

            if (PveSystem.ServerIsPvE)
            {
                // don't drop player loot on death in PvE
                Api.Logger.Important("Player character is dead - reduce durability for the equipped items", character);

                // process items and drop loot
                ProcessContainerItemsOnDeathInPvE(isEquipmentContainer: true,
                                                  fromContainer: containerEquipment);

                ProcessContainerItemsOnDeathInPvE(isEquipmentContainer: false,
                                                  fromContainer: containerHand);

                ProcessContainerItemsOnDeathInPvE(isEquipmentContainer: false,
                                                  fromContainer: containerHotbar);

                ProcessContainerItemsOnDeathInPvE(isEquipmentContainer: false,
                                                  fromContainer: containerInventory);
                return;
            }

            Api.Logger.Important("Player character is dead - drop loot", character);

            CraftingMechanics.ServerCancelCraftingQueue(character);

            var characterContainersOccupiedSlotsCount = containerEquipment.OccupiedSlotsCount
                                                        + containerHand.OccupiedSlotsCount
                                                        + containerHotbar.OccupiedSlotsCount
                                                        + containerInventory.OccupiedSlotsCount;

            if (characterContainersOccupiedSlotsCount == 0)
            {
                Api.Logger.Important("No need to drop loot for dead character (no items to drop)", character);
                return;
            }

            var lootContainer = ObjectPlayerLootContainer.ServerTryCreateLootContainer(character);
            if (lootContainer is null)
            {
                Api.Logger.Error("Unable to drop loot for dead character", character);
                return;
            }

            // set slots count matching the total occupied slots count
            ServerItems.SetSlotsCount(
                lootContainer,
                (byte)characterContainersOccupiedSlotsCount);

            // process items and drop loot
            ProcessContainerItemsOnDeathInPvP(isEquipmentContainer: true,
                                              fromContainer: containerEquipment,
                                              toContainer: lootContainer);

            ProcessContainerItemsOnDeathInPvP(isEquipmentContainer: false,
                                              fromContainer: containerHand,
                                              toContainer: lootContainer);

            ProcessContainerItemsOnDeathInPvP(isEquipmentContainer: false,
                                              fromContainer: containerHotbar,
                                              toContainer: lootContainer);

            ProcessContainerItemsOnDeathInPvP(isEquipmentContainer: false,
                                              fromContainer: containerInventory,
                                              toContainer: lootContainer);

            if (lootContainer.OccupiedSlotsCount <= 0)
            {
                // nothing dropped, destroy the just spawned loot container
                ServerWorld.DestroyObject((IWorldObject)lootContainer.Owner);
                return;
            }

            // set exact slots count
            ServerItems.SetSlotsCount(
                lootContainer,
                (byte)lootContainer.OccupiedSlotsCount);

            SharedLootDropNotifyHelper.ServerOnLootDropped(lootContainer);
            // please note - no need to notify player about the dropped loot, it's automatically done by ClientDroppedItemsNotificationsManager
        }

        private static void ProcessContainerItemsOnDeathInPvE(bool isEquipmentContainer, IItemsContainer fromContainer)
        {
            // decrease durability for all equipped items (which will be dropped as the full loot)
            using var tempList = Api.Shared.WrapInTempList(fromContainer.Items);
            foreach (var item in tempList.AsList())
            {
                item.ProtoItem.ServerOnCharacterDeath(item,
                                                      isEquipped: isEquipmentContainer,
                                                      out _);
            }
        }

        private static void ProcessContainerItemsOnDeathInPvP(
            bool isEquipmentContainer,
            IItemsContainer fromContainer,
            IItemsContainer toContainer)
        {
            // decrease durability for all equipped items (which will be dropped as the full loot)
            using var tempList = Api.Shared.WrapInTempList(fromContainer.Items);
            foreach (var item in tempList.AsList())
            {
                item.ProtoItem.ServerOnCharacterDeath(item,
                                                      isEquipped: isEquipmentContainer,
                                                      out var shouldDrop);
                if (shouldDrop
                    && !item.IsDestroyed)
                {
                    ServerItems.MoveOrSwapItem(item, toContainer, out _);
                }
            }
        }
    }
}