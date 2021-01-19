namespace AtomicTorch.CBND.CoreMod.Items.Equipment
{
    using System;
    using AtomicTorch.CBND.CoreMod.Characters;
    using AtomicTorch.CBND.CoreMod.Characters.Player;
    using AtomicTorch.CBND.CoreMod.SoundPresets;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Explosives.Bombs;
    using AtomicTorch.CBND.CoreMod.Systems.LandClaim;
    using AtomicTorch.CBND.CoreMod.Systems.RaidingProtection;
    using AtomicTorch.CBND.CoreMod.Systems.Weapons;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.Items;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.Scripting.Network;
    using AtomicTorch.GameEngine.Common.DataStructures;
    using AtomicTorch.GameEngine.Common.Primitives;

    public class ItemExplosivesArmor : ProtoItemEquipmentArmor
    {
        public static ObjectBombPrimitive ProtoObjectBomb
            => Api.GetProtoEntity<ObjectBombPrimitive>();

        public override string Description =>
            "Special vest fitted with explosives. Automatically detonates after death of the wearer as a means of revenge.";

        public override uint DurabilityMax => 500;

        public override ObjectMaterial Material => ObjectMaterial.SoftTissues;

        public override string Name => "Revenge vest";

        public override void ServerOnCharacterDeath(IItem item, bool isEquipped, out bool shouldDrop)
        {
            if (!isEquipped)
            {
                base.ServerOnCharacterDeath(item,
                                            isEquipped: false,
                                            out shouldDrop);
                return;
            }

            shouldDrop = false;
            this.ServerExplode(item);
        }

        public override void ServerOnItemBrokeAndDestroyed(IItem item, IItemsContainer container, byte slotId)
        {
            if (container.OwnerAsCharacter is not null
                && container == container.OwnerAsCharacter.SharedGetPlayerContainerEquipment())
            {
                // the equipped item is broken - explode immediately
                this.ServerExplode(item);
            }
        }

        protected override void ClientFillSlotAttachmentSources(ITempList<string> folders)
        {
            base.ClientFillSlotAttachmentSources(folders);
            // add generic pants
            folders.Add("Characters/Equipment/GenericPants");
        }

        protected override void PrepareDefense(DefenseDescription defense)
        {
            defense.Set(
                impact: 0.20,
                kinetic: 0.20,
                explosion: 0.10,
                heat: 0.15,
                cold: 0.15,
                chemical: 0.10,
                radiation: 0.10,
                psi: 0.0);
        }

        private void ClientRemote_OnExplosion(
            Vector2D explosionWorldPosition)
        {
            var protoBomb = ProtoObjectBomb;
            SharedExplosionHelper.ClientExplode(explosionWorldPosition,
                                                protoBomb.ExplosionPreset,
                                                protoBomb.VolumeExplosion);
        }

        private void ServerExplode(IItem item)
        {
            Server.Items.DestroyItem(item);

            // explode a primitive bomb in player position
            var character = item.Container.OwnerAsCharacter;
            if (character is null)
            {
                return;
            }

            var characterPublicState = character.GetPublicState<PlayerCharacterPublicState>();
            if (!characterPublicState.IsDead)
            {
                // ensure the wearer is killed by the explosion
                characterPublicState.CurrentStats.ServerSetHealthCurrent(0);
            }

            var targetPosition = character.Position;

            var protoBomb = ProtoObjectBomb;
            SharedExplosionHelper.ServerExplode(
                character: character,
                protoExplosive: protoBomb,
                protoWeapon: null,
                explosionPreset: protoBomb.ExplosionPreset,
                epicenterPosition: targetPosition,
                damageDescriptionCharacters: protoBomb.DamageDescriptionCharacters,
                physicsSpace: Server.World.GetPhysicsSpace(),
                executeExplosionCallback: protoBomb.ServerExecuteExplosion);

            // notify all characters about the explosion
            using var charactersObserving = Api.Shared.GetTempList<ICharacter>();
            const byte explosionEventRadius = 40;

            Server.World.GetCharactersInRadius(targetPosition.ToVector2Ushort(),
                                               charactersObserving,
                                               radius: explosionEventRadius,
                                               onlyPlayers: true);

            this.CallClient(charactersObserving.AsList(),
                            _ => _.ClientRemote_OnExplosion(targetPosition));

            // activate the raidblock if possible (the code is similar to ProtoObjectEplosive)
            var explosionRadius = (int)Math.Ceiling(protoBomb.DamageRadius);
            var bounds = new RectangleInt(x: (int)Math.Round(character.Position.X - explosionRadius),
                                          y: (int)Math.Round(character.Position.Y - explosionRadius),
                                          width: 2 * explosionRadius,
                                          height: 2 * explosionRadius);

            if (RaidingProtectionSystem.SharedCanRaid(bounds,
                                                      showClientNotification: false))
            {
                // try activate the raidblock
                LandClaimSystem.ServerOnRaid(bounds, character);
            }
            else
            {
                // Raiding is not possible now due to raiding window
                // Find if there is any land claim and in that case notify nearby players
                // that the damage to objects there was not applied.
                if (LandClaimSystem.SharedIsLandClaimedByAnyone(bounds))
                {
                    using var tempPlayers = Api.Shared.GetTempList<ICharacter>();
                    Server.World.GetScopedByPlayers(character, tempPlayers);
                    RaidingProtectionSystem.ServerNotifyShowNotificationRaidingNotAvailableNow(
                        tempPlayers.AsList());
                }
            }
        }
    }
}