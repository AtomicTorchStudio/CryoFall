namespace AtomicTorch.CBND.CoreMod.StaticObjects.Loot
{
    using AtomicTorch.CBND.CoreMod.Helpers.Client;
    using AtomicTorch.CBND.CoreMod.Systems.NewbieProtection;
    using AtomicTorch.CBND.CoreMod.Systems.Notifications;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Scripting.Network;

    public class ObjectPlayerLootContainerProtected : ObjectPlayerLootContainer
    {
        public override string ClientGetTitle(IStaticWorldObject worldObject)
        {
            var ownerName = GetPublicState(worldObject).OwnerName;
            if (ownerName == ClientCurrentCharacterHelper.Character?.Name)
            {
                return MessageLootFromCurrentPlayer;
            }

            return string.Format("({0})\n{1}",
                                 NewbieProtectionSystem.Title_NewbieProtection,
                                 string.Format(MessageFormatLootFromAnotherPlayer, ownerName));
        }

        public override bool SharedCanInteract(ICharacter character, IStaticWorldObject worldObject, bool writeToLog)
        {
            if (!base.SharedCanInteract(character, worldObject, writeToLog))
            {
                return false;
            }

            if (GetPublicState(worldObject).OwnerName == character.Name)
            {
                return true;
            }

            if (writeToLog)
            {
                if (IsClient)
                {
                    this.ClientShowProtectedBagNotification();
                }
                else
                {
                    this.CallClient(character,
                                    _ => _.ClientRemote_ShowProtectedBagNotification());
                }
            }

            return false;
        }

        private void ClientRemote_ShowProtectedBagNotification()
        {
            this.ClientShowProtectedBagNotification();
        }

        private void ClientShowProtectedBagNotification()
        {
            NotificationSystem.ClientShowNotification(
                title: NewbieProtectionSystem.Notification_LootBagUnderProtection,
                icon: this.Icon);
        }
    }
}