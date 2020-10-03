namespace AtomicTorch.CBND.CoreMod.Characters
{
    using System;

    [Serializable]
    public readonly struct CharacterHumanFaceStyle
    {
        public readonly string BottomId;

        public readonly string FaceId;

        public readonly string HairColorId;

        public readonly string HairId;

        public readonly string SkinToneId;

        public readonly string TopId;

        public CharacterHumanFaceStyle(
            string faceId,
            string topId,
            string bottomId,
            string hairId,
            string skinToneId,
            string hairColorId)
        {
            this.FaceId = faceId;
            this.TopId = topId;
            this.BottomId = bottomId;
            this.HairId = hairId;
            this.SkinToneId = skinToneId;
            this.HairColorId = hairColorId;
        }

        public CharacterHumanFaceStyle EmptyStringsToNulls()
        {
            return new CharacterHumanFaceStyle(
                faceId: this.FaceId,
                topId: this.TopId,
                bottomId: this.BottomId,
                hairId: ToNullIfEmpty(this.HairId),
                skinToneId: ToNullIfEmpty(this.SkinToneId),
                hairColorId: ToNullIfEmpty(this.HairColorId));

            static string ToNullIfEmpty(string str)
                => string.IsNullOrEmpty(str)
                       ? null
                       : str;
        }
    }
}