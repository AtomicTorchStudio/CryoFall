namespace AtomicTorch.CBND.CoreMod.Systems.CharacterDeath
{
    using System;
    using AtomicTorch.CBND.CoreMod.Characters;
    using AtomicTorch.CBND.CoreMod.Characters.Player;
    using AtomicTorch.CBND.CoreMod.Skills;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Loot;
    using AtomicTorch.CBND.CoreMod.Systems.CharacterDamageTrackingSystem;
    using AtomicTorch.CBND.CoreMod.Systems.CharacterRespawn;
    using AtomicTorch.CBND.CoreMod.Systems.Crafting;
    using AtomicTorch.CBND.CoreMod.Systems.Droplists;
    using AtomicTorch.CBND.CoreMod.Systems.NewbieProtection;
    using AtomicTorch.CBND.CoreMod.Systems.PvE;
    using AtomicTorch.CBND.CoreMod.Systems.ServerTimers;
    using AtomicTorch.CBND.CoreMod.Systems.VehicleSystem;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.Items;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.ServicesServer;
    using AtomicTorch.GameEngine.Common.Primitives;

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

        /// <summary>
        /// Moves the character to the "graveyard" so a respawn will be required on login.
        /// No penalty in items or "weakened" status effect.
        /// </summary>
        public static void DespawnCharacter(ICharacter character)
        {
            var publicState = character.GetPublicState<ICharacterPublicState>();
            if (publicState.IsDead)
            {
                return;
            }

            VehicleSystem.ServerCharacterExitCurrentVehicle(character, force: true);

            var privateState = PlayerCharacter.GetPrivateState(character);
            CharacterDamageTrackingSystem.ServerClearStats(character);
            privateState.IsDespawned = true;
            Api.Logger.Important("Character despawned", character);

            // we have to set the dead flag to stop game mechanics from working
            // but on the respawn player should not lose anything
            publicState.IsDead = true;
            // recreate physics (as dead/despawned character doesn't have any physics)
            character.ProtoCharacter.SharedCreatePhysics(character);

            TeleportDeadPlayerCharacterToGraveyard(character);

            privateState.LastDeathPosition = Vector2Ushort.Zero;
            privateState.LastDeathTime = null;
        }

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

            Action<ICharacter> onCharacterDeath;
            if (deadCharacter.ProtoCharacter is IProtoCharacterMob protoCharacterMob)
            {
                onCharacterDeath = CharacterDeath;
                if (onCharacterDeath != null)
                {
                    Api.SafeInvoke(() => onCharacterDeath(deadCharacter));
                }

                protoCharacterMob.ServerOnDeath(deadCharacter);
                return;
            }

            // player character death
            // remember the death position (useful for the respawn)
            var privateState = PlayerCharacter.GetPrivateState(deadCharacter);
            privateState.LastDeathPosition = deadCharacter.TilePosition;
            privateState.LastDeathTime = Api.Server.Game.FrameTime;
            ServerTimersSystem.AddAction(delaySeconds: PlayerTeleportToGraveyardDelaySeconds,
                                         () => TeleportDeadPlayerCharacterToGraveyard(deadCharacter));

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

            onCharacterDeath = CharacterDeath;
            if (onCharacterDeath != null)
            {
                Api.SafeInvoke(() => onCharacterDeath(deadCharacter));
            }
        }

        public static void OnCharacterInitialize(ICharacter character)
        {
            if (character.IsNpc)
            {
                return;
            }

            var publicState = PlayerCharacter.GetPublicState(character);
            if (publicState.IsDead)
            {
                TeleportDeadPlayerCharacterToGraveyard(character);
                return;
            }

            var privateState = PlayerCharacter.GetPrivateState(character);
            if (privateState.IsDespawned)
            {
                // it will move character to the graveyard
                DespawnCharacter(character);
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
        }

        public static Vector2Ushort ServerGetGraveyardPosition()
        {
            var world = ServerWorld;
            var worldBounds = world.WorldBounds;

            // teleport to bottom right corner of the map
            var position = new Vector2Ushort((ushort)(worldBounds.Offset.X + worldBounds.Size.X - 1),
                                             (ushort)(worldBounds.Offset.Y + 1));
            return position;
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
            if (lootContainer == null)
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

        // "Graveyard" is a technical area in the bottom right corner of the map.
        private static void TeleportDeadPlayerCharacterToGraveyard(ICharacter character)
        {
            if (character.IsNpc)
            {
                // only player characters are teleported to graveyard
                return;
            }

            var publicState = character.GetPublicState<ICharacterPublicState>();
            if (!publicState.IsDead)
            {
                // player has been respawned
                return;
            }

            VehicleSystem.ServerCharacterExitCurrentVehicle(character, force: true);

            // disable the visual scope so the player cannot not see anyone and nobody could see the player
            Api.Server.Characters.SetViewScopeMode(character, isEnabled: false);
            var graveyardPosition = ServerGetGraveyardPosition();
            if (character.TilePosition != graveyardPosition)
            {
                ServerWorld.SetPosition(character, (Vector2D)graveyardPosition);
            }

            CharacterRespawnSystem.ServerRemoveInvalidStatusEffects(character);
        }
    }
}