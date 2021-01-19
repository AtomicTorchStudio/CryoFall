namespace AtomicTorch.CBND.CoreMod.Systems.CharacterStyle
{
    using AtomicTorch.CBND.CoreMod.Characters;
    using AtomicTorch.CBND.CoreMod.Characters.Player;
    using AtomicTorch.CBND.CoreMod.Systems.CharacterRespawn;
    using AtomicTorch.CBND.GameApi.Data.State;
    using AtomicTorch.CBND.GameApi.Scripting.Network;

    public class CharacterStyleSystem : ProtoSystem<CharacterStyleSystem>
    {
        public override string Name => "Character style system";

        public static void ClientChangeStyle(CharacterHumanFaceStyle style, bool isMale)
        {
            Instance.CallServer(_ => _.ServerRemote_ChangeStyle(style, isMale));
        }

        public static void ClientSetHeadEquipmentVisibility(bool isHeadEquipmentHiddenForSelfAndPartyMembers)
        {
            Instance.CallServer(_ => _.ServerRemote_SetHeadEquipmentVisibility(
                                    isHeadEquipmentHiddenForSelfAndPartyMembers));
        }

        [RemoteCallSettings(DeliveryMode.ReliableSequenced, timeInterval: 5)]
        private void ServerRemote_ChangeStyle(CharacterHumanFaceStyle style, bool isMale)
        {
            var character = ServerRemoteContext.Character;

            style = style.EmptyStringsToNulls();
            if (!SharedCharacterFaceStylesProvider.GetForGender(isMale)
                                                  .SharedIsValidFaceStyle(style))
            {
                Logger.Warning("An invalid face style received");
                return;
            }

            var publicState = PlayerCharacter.GetPublicState(character);
            publicState.IsMale = isMale;
            publicState.FaceStyle = style;

            var privateState = PlayerCharacter.GetPrivateState(character);
            if (privateState.IsAppearanceSelected)
            {
                Logger.Info("Character appearance changed");
                return;
            }

            privateState.IsAppearanceSelected = true;
            Logger.Info("Character appearance selected - spawning in the world");
            CharacterRespawnSystem.ServerOnCharacterAppearanceSelected(character);
        }

        [RemoteCallSettings(DeliveryMode.ReliableSequenced, timeInterval: 1)]
        private void ServerRemote_SetHeadEquipmentVisibility(bool isHeadEquipmentHiddenForSelfAndPartyMembers)
        {
            var character = ServerRemoteContext.Character;
            var publicState = PlayerCharacter.GetPublicState(character);
            publicState.IsHeadEquipmentHiddenForSelfAndPartyMembers = isHeadEquipmentHiddenForSelfAndPartyMembers;
        }
    }
}