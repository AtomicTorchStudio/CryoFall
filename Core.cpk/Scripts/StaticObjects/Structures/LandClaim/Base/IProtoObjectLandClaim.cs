namespace AtomicTorch.CBND.CoreMod.StaticObjects.Structures.LandClaim
{
    using System;
    using AtomicTorch.CBND.CoreMod.Systems.Construction;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.World;

    public interface IProtoObjectLandClaim : IProtoObjectStructure
    {
        TimeSpan DestructionTimeout { get; }

        ushort LandClaimGraceAreaPaddingSizeOneDirection { get; }

        ushort LandClaimSize { get; }

        ushort LandClaimWithGraceAreaSize { get; }

        byte SafeItemsSlotsCount { get; }

        void ClientUpgrade(IStaticWorldObject worldObjectLandClaim, IProtoObjectLandClaim upgradeStructure);

        bool SharedCanEditOwners(IStaticWorldObject worldObject, ICharacter byOwner);

        ObjectLandClaimCanUpgradeCheckResult SharedCanUpgrade(
            IStaticWorldObject worldObjectLandClaim,
            IProtoObjectLandClaim protoUpgradedLandClaim,
            ICharacter character,
            out IConstructionUpgradeEntryReadOnly upgradeEntry,
            bool writeErrors = true);
    }
}