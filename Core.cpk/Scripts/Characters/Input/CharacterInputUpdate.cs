namespace AtomicTorch.CBND.CoreMod.Characters.Input
{
    using AtomicTorch.CBND.GameApi.Scripting.Network;
    using AtomicTorch.GameEngine.Common.Primitives;

    public readonly struct CharacterInputUpdate : IRemoteCallParameter
    {
        public readonly CharacterMoveModes MoveModes;

        private readonly ushort rotationAngleCompressed;

        public CharacterInputUpdate(
            CharacterMoveModes moveModes,
            float rotationAngleRad)
        {
            this.MoveModes = moveModes;
            this.rotationAngleCompressed = (ushort)(rotationAngleRad
                                                    % MathConstants.DoublePI
                                                    * MathConstants.RadToDeg
                                                    / 360
                                                    * ushort.MaxValue);
        }

        public double RotationAngleDeg => (double)this.rotationAngleCompressed / ushort.MaxValue * 360;

        public float RotationAngleRad => (float)(this.RotationAngleDeg * MathConstants.DegToRad);

        public override string ToString()
        {
            // ReSharper disable once CanExtractXamlLocalizableStringCSharp
            return string.Format("mode={0} angle={1:F1} deg", this.MoveModes, this.RotationAngleDeg);
        }
    }
}