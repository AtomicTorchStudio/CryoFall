namespace AtomicTorch.CBND.CoreMod.Systems.CharacterStyle
{
    using AtomicTorch.CBND.CoreMod.Characters;
    using AtomicTorch.CBND.CoreMod.Characters.Player;
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

        private void ServerRemote_ChangeStyle(CharacterHumanFaceStyle style, bool isMale)
        {
            var character = ServerRemoteContext.Character;
            var publicState = PlayerCharacter.GetPublicState(character);
            publicState.IsMale = isMale;
            publicState.FaceStyle = style;
        }

        private void ServerRemote_SetHeadEquipmentVisibility(bool isHeadEquipmentHiddenForSelfAndPartyMembers)
        {
            var character = ServerRemoteContext.Character;
            var publicState = PlayerCharacter.GetPublicState(character);
            publicState.IsHeadEquipmentHiddenForSelfAndPartyMembers = isHeadEquipmentHiddenForSelfAndPartyMembers;
        }
    }
}