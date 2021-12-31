namespace AtomicTorch.CBND.CoreMod.Characters
{
    using AtomicTorch.CBND.CoreMod.Items.Equipment;
    using AtomicTorch.CBND.GameApi.Data.Items;
    using AtomicTorch.CBND.GameApi.Resources;

    public readonly struct CharacterHeadSpriteData
    {
        public readonly CharacterHumanFaceStyle FaceStyle;

        public readonly IItem HeadEquipmentItem;

        public readonly IProtoItemEquipmentHead HeadEquipmentItemProto;

        public readonly SkeletonResource SkeletonResource;

        public CharacterHeadSpriteData(
            CharacterHumanFaceStyle faceStyle,
            IItem headEquipmentItem,
            IProtoItemEquipmentHead headEquipmentItemProto,
            SkeletonResource skeletonResource)
        {
            this.SkeletonResource = skeletonResource;
            this.FaceStyle = faceStyle;
            this.HeadEquipmentItem = headEquipmentItem;
            this.HeadEquipmentItemProto = headEquipmentItemProto;
        }
    }
}