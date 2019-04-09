namespace AtomicTorch.CBND.CoreMod.Characters
{
    using System;
    using JetBrains.Annotations;

    [Serializable]
    public struct CharacterHumanFaceStyle
    {
        public readonly string BottomId;

        public readonly string FaceId;

        public readonly string HairId;

        public readonly string TopId;

        public CharacterHumanFaceStyle(string faceId, string topId, string bottomId, [CanBeNull] string hairId)
        {
            this.FaceId = faceId;
            this.TopId = topId;
            this.BottomId = bottomId;
            this.HairId = hairId;
        }
    }
}