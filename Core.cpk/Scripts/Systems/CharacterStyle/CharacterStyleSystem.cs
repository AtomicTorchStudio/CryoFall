namespace AtomicTorch.CBND.CoreMod.Systems.CharacterStyle
{
    using System;
    using AtomicTorch.CBND.CoreMod.Characters;
    using AtomicTorch.CBND.CoreMod.Characters.Player;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.State;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.Scripting.Network;

    public class CharacterStyleSystem : ProtoSystem<CharacterStyleSystem>
    {
        public static event Action<ICharacter> ServerCharacterAppearanceSelected;

        public static void ClientChangeStyle(CharacterHumanFaceStyle style, bool isMale)
        {
            Instance.CallServer(_ => _.ServerRemote_ChangeStyle(style, isMale));
        }

        public static void ClientSetHeadEquipmentVisibility(bool isHeadEquipmentHiddenForSelfAndPartyMembers)
        {
            Instance.CallServer(_ => _.ServerRemote_SetHeadEquipmentVisibility(
                                    isHeadEquipmentHiddenForSelfAndPartyMembers));
        }

        public static void ServerSetStyle(ICharacter character, CharacterHumanFaceStyle style, bool isMale)
        {
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

            Api.SafeInvoke(() => ServerCharacterAppearanceSelected?.Invoke(character));
        }

        [RemoteCallSettings(DeliveryMode.ReliableSequenced, timeInterval: 5)]
        private void ServerRemote_ChangeStyle(CharacterHumanFaceStyle style, bool isMale)
        {
            ServerSetStyle(ServerRemoteContext.Character, style, isMale);
        }

        [RemoteCallSettings(DeliveryMode.ReliableSequenced, timeInterval: 0.667)]
        private void ServerRemote_SetHeadEquipmentVisibility(bool isHeadEquipmentHiddenForSelfAndPartyMembers)
        {
            var character = ServerRemoteContext.Character;
            var publicState = PlayerCharacter.GetPublicState(character);
            publicState.IsHeadEquipmentHiddenForSelfAndPartyMembers = isHeadEquipmentHiddenForSelfAndPartyMembers;
        }
    }
}