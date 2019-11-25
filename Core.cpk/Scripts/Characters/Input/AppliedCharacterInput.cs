namespace AtomicTorch.CBND.CoreMod.Characters.Input
{
    using System;
    using AtomicTorch.CBND.GameApi.Data.State;
    using AtomicTorch.CBND.GameApi.Data.State.NetSync;
    using AtomicTorch.GameEngine.Common.Primitives;
    using static GameApi.Data.State.SyncToClientReceivers;

    public class AppliedCharacterInput : BaseNetObject
    {
        [NonSerialized]
        private float? rotationAngleRadUncompressed;

        [SyncToClient(
            receivers: ScopePlayers,
            deliveryMode: DeliveryMode.ReliableSequenced,
            maxUpdatesPerSecond: 20)]
        [TempOnly]
        public CharacterMoveModes MoveModes { get; set; }

        [SyncToClient(
            receivers: ScopePlayers,
            deliveryMode: DeliveryMode.ReliableSequenced,
            // TODO: we need to use less updates per second and enable client interpolation for that
            maxUpdatesPerSecond: 20,
            // TODO: we need to compress it much more (single byte?)
            networkDataType: typeof(float))]
        [TempOnly]
        public double MoveSpeed { get; private set; }

        public float RotationAngleDeg
        {
            get
            {
                if (this.rotationAngleRadUncompressed.HasValue)
                {
                    return this.rotationAngleRadUncompressed.Value * MathConstants.RadToDeg;
                }

                return DecompressAngle(this.RotationAngleRadCompressed);
            }
        }

        public float RotationAngleRad
        {
            get
            {
                if (this.rotationAngleRadUncompressed != null)
                {
                    return this.rotationAngleRadUncompressed.Value;
                }

                return this.RotationAngleDeg * MathConstants.DegToRad;
            }
        }

        [SyncToClient(
            receivers: ScopePlayers,
            deliveryMode: DeliveryMode.ReliableSequenced,
            // TODO: we need to use less updates per second and enable client interpolation for that
            maxUpdatesPerSecond: 40)]
        [TempOnly]
        public byte RotationAngleRadCompressed { get; private set; }

        public static byte CompressAngle(float angleRead)
        {
            return (byte)(angleRead * MathConstants.RadToDeg / 360.0 * byte.MaxValue);
        }

        public static float DecompressAngle(byte angleCompressed)
        {
            return angleCompressed / (float)byte.MaxValue * 360;
        }

        public void Set(CharacterInput input, double moveSpeed)
        {
            this.MoveModes = input.MoveModes;
            this.rotationAngleRadUncompressed = input.RotationAngleRad;
            this.RotationAngleRadCompressed = CompressAngle(input.RotationAngleRad);
            this.MoveSpeed = moveSpeed;
        }
    }
}