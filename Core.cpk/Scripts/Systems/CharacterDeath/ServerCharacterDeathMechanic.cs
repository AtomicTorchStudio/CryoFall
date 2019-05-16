namespace AtomicTorch.CBND.CoreMod.Systems.CharacterDeath
{
    using System;
    using AtomicTorch.CBND.CoreMod.Characters;
    using AtomicTorch.CBND.CoreMod.Characters.Player;
    using AtomicTorch.CBND.CoreMod.Helpers.Client;
    using AtomicTorch.CBND.CoreMod.Items.Weapons;
    using AtomicTorch.CBND.CoreMod.Skills;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Loot;
    using AtomicTorch.CBND.CoreMod.Stats;
    using AtomicTorch.CBND.CoreMod.Systems.CharacterDamageTrackingSystem;
    using AtomicTorch.CBND.CoreMod.Systems.Crafting;
    using AtomicTorch.CBND.CoreMod.Systems.Droplists;
    using AtomicTorch.CBND.CoreMod.Systems.NewbieProtection;
    using AtomicTorch.CBND.CoreMod.Systems.PvE;
    using AtomicTorch.CBND.CoreMod.Systems.ServerTimers;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.Items;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.ServicesServer;
    using AtomicTorch.GameEngine.Common.Helpers;
    using AtomicTorch.GameEngine.Common.Primitives;

    public class ServerCharacterDeathMechanic
    {
        private const double PlayerTeleportToGraveyardDelaySeconds = 10;

        private static readonly IItemsServerService ServerItemsService = Api.IsServer
                                                                             ? Api.Server.Items
                                                                             : null;

        public delegate void DelegateCharacterKilled(ICharacter attackerCharacter, ICharacter targetCharacter);

        public static event Action<ICharacter> CharacterDeath;

        public static event DelegateCharacterKilled CharacterKilled;

        /// <summary>
        /// Moves the character to the "graveyard" so a respawn will be required on login.
        /// No penalty in items or LP loss.
        /// </summary>
        public static void DespawnCharacter(ICharacter character)
        {
            var publicState = character.GetPublicState<ICharacterPublicState>();
            if (publicState.IsDead)
            {
                return;
            }

            var privateState = PlayerCharacter.GetPrivateState(character);
            CharacterDamageTrackingSystem.ServerClearStats(character);
            privateState.IsDespawned = true;
            Api.Logger.Important("Character despawned", character);

            // we have to set the dead flag to stop game mechanics from working
            // but on the respawn player should not lose anything
            publicState.IsDead = true;
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
            if (!(deadCharacter.ProtoCharacter is PlayerCharacter))
            {
                onCharacterDeath = CharacterDeath;
                if (onCharacterDeath != null)
                {
                    Api.SafeInvoke(() => onCharacterDeath(deadCharacter));
                }

                ReplaceMobWithCorpse(deadCharacter);
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
                                                       out var shouldDropLootAndLoseLP);

            if (shouldDropLootAndLoseLP)
            {
                DeductPlayerLearningPoints(deadCharacter);
                DropPlayerLoot(deadCharacter);
            }
            else
            {
                Api.Logger.Important("Player character is dead - newbie PvP case, no loot drop", deadCharacter);
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
            ICharacter targetCharacter,
            ICharacter attackerCharacter,
            IItem weapon,
            IProtoItemWeapon protoWeapon)
        {
            Api.Logger.Important(
                $"Character killed: {targetCharacter} by {attackerCharacter} with {weapon?.ToString() ?? protoWeapon?.ToString()}");

            Api.SafeInvoke(
                () => CharacterKilled?.Invoke(attackerCharacter, targetCharacter));
        }

        public static ushort SharedGetLearningPointsRetainedAfterDeath(ICharacter character)
        {
            return (ushort)MathHelper.Clamp(
                character.SharedGetFinalStatValue(StatName.LearningPointsRetainedAfterDeath),
                0,
                ushort.MaxValue);
        }

        private static void DeductPlayerLearningPoints(ICharacter character)
        {
            // reset learning points
            var technologies = character.SharedGetTechnologies();
            technologies.ServerResetLearningPointsRemainder();

            var learningPointsRetainedAfterDeath = SharedGetLearningPointsRetainedAfterDeath(character);
            var lostLp = technologies.LearningPoints - learningPointsRetainedAfterDeath;
            if (lostLp > 0)
            {
                technologies.ServerSetLearningPoints(learningPointsRetainedAfterDeath);
                character.ServerAddSkillExperience<SkillLearning>(
                    lostLp * SkillLearning.ExperienceAddedPerLPLost);
            }
        }

        private static void DropPlayerLoot(ICharacter character)
        {
            if (PveSystem.ServerIsPvE)
            {
                // don't drop player loot on death in PvE
                return;
            }

            Api.Logger.Important("Player character is dead - drop loot", character);

            CraftingMechanics.ServerCancelCraftingQueue(character);

            var containerEquipment = character.SharedGetPlayerContainerEquipment();
            var containerHand = character.SharedGetPlayerContainerHand();
            var containerHotbar = character.SharedGetPlayerContainerHotbar();
            var containerInventory = character.SharedGetPlayerContainerInventory();

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
            ServerItemsService.SetSlotsCount(
                lootContainer,
                (byte)characterContainersOccupiedSlotsCount);

            // drop loot
            ProcessContainerItemsOnDeath(isEquipmentContainer: true,
                                         fromContainer: containerEquipment,
                                         toContainer: lootContainer);

            ProcessContainerItemsOnDeath(isEquipmentContainer: false,
                                         fromContainer: containerHand,
                                         toContainer: lootContainer);

            ProcessContainerItemsOnDeath(isEquipmentContainer: false,
                                         fromContainer: containerHotbar,
                                         toContainer: lootContainer);

            ProcessContainerItemsOnDeath(isEquipmentContainer: false,
                                         fromContainer: containerInventory,
                                         toContainer: lootContainer);

            if (lootContainer.OccupiedSlotsCount <= 0)
            {
                // nothing dropped, destroy the just spawned loot container
                Api.Server.World.DestroyObject((IWorldObject)lootContainer.Owner);
                return;
            }

            // set exact slots count
            ServerItemsService.SetSlotsCount(
                lootContainer,
                (byte)lootContainer.OccupiedSlotsCount);

            SharedLootDropNotifyHelper.ServerOnLootDropped(lootContainer);
            // please note - no need to notify player about the dropped loot, it's automatically done by ClientDroppedItemsNotificationsManager
        }

        private static void ProcessContainerItemsOnDeath(
            bool isEquipmentContainer,
            IItemsContainer fromContainer,
            IItemsContainer toContainer)
        {
            // decrease durability for all equipped items (which will be dropped as the full loot)
            using (var tempList = Api.Shared.WrapInTempList(fromContainer.Items))
            {
                foreach (var item in tempList)
                {
                    item.ProtoItem.ServerOnCharacterDeath(item,
                                                          isEquipped: isEquipmentContainer,
                                                          out var shouldDrop);
                    if (shouldDrop
                        && !item.IsDestroyed)
                    {
                        ServerItemsService.MoveOrSwapItem(item, toContainer, out _);
                    }
                }
            }
        }

        private static void ReplaceMobWithCorpse(ICharacter deadCharacter)
        {
            var position = deadCharacter.Position;
            var rotationAngleRad = deadCharacter.GetPublicState<ICharacterPublicState>().AppliedInput.RotationAngleRad;
            var isLeftOrientation = ClientCharacterAnimationHelper.IsLeftHalfOfCircle(
                angleDeg: rotationAngleRad * MathConstants.RadToDeg);
            var isFlippedHorizontally = !isLeftOrientation;

            var tilePosition = position.ToVector2Ushort();

            Api.Server.World.DestroyObject(deadCharacter);
            var objectCorpse = Api.Server.World.CreateStaticWorldObject<ObjectCorpse>(tilePosition);
            ObjectCorpse.ServerSetupCorpse(objectCorpse,
                                           (IProtoCharacterMob)deadCharacter.ProtoCharacter,
                                           (Vector2F)(position - tilePosition.ToVector2D()),
                                           isFlippedHorizontally: isFlippedHorizontally);
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

            // disable the visual scope so the player cannot not see anyone and nobody could see the player
            Api.Server.Characters.SetViewScopeMode(character, isEnabled: false);
            var world = Api.Server.World;
            var worldBounds = world.WorldBounds;

            // teleport to bottom right corner of the map
            var position = new Vector2Ushort((ushort)(worldBounds.Offset.X + worldBounds.Size.X - 1),
                                             (ushort)(worldBounds.Offset.Y + 1));
            if (character.TilePosition != position)
            {
                world.SetPosition(character, (Vector2D)position);
            }
        }
    }
}