namespace AtomicTorch.CBND.CoreMod.Characters
{
    using AtomicTorch.CBND.GameApi.Data.Items;
    using AtomicTorch.CBND.GameApi.Resources;

    public struct CharacterHeadSpriteData
    {
        public readonly CharacterHumanFaceStyle FaceStyle;

        public readonly IItem HeadEquipment;

        public readonly SkeletonResource SkeletonResource;

        public CharacterHeadSpriteData(
            CharacterHumanFaceStyle faceStyle,
            IItem headEquipment,
            SkeletonResource skeletonResource)
        {
            this.SkeletonResource = skeletonResource;
            this.FaceStyle = faceStyle;
            this.HeadEquipment = headEquipment;
        }
    }
}