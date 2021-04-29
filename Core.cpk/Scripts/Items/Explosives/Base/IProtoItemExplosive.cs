namespace AtomicTorch.CBND.CoreMod.Items.Explosives
{
    using System;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Explosives;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.Items;
    using AtomicTorch.GameEngine.Common.Primitives;

    public interface IProtoItemExplosive : IProtoItem
    {
        double DeployDistanceMax { get; }

        TimeSpan DeployDuration { get; }

        IProtoObjectExplosive ObjectExplosiveProto { get; }

        void ClientOnActionCompleted(IItem item, bool isCancelled);

        void ServerOnUseActionFinished(ICharacter character, IItem item, Vector2Ushort targetPosition);

        void SharedValidatePlacement(
            ICharacter character,
            Vector2Ushort targetPosition,
            bool logErrors,
            out bool canPlace,
            out bool isTooFar,
            out string errorMessage);
    }
}