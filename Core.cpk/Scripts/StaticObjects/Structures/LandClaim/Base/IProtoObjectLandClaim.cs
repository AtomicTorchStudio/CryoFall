namespace AtomicTorch.CBND.CoreMod.StaticObjects.Structures.LandClaim
{
    using System;
    using AtomicTorch.CBND.CoreMod.Systems.Construction;
    using AtomicTorch.CBND.CoreMod.Systems.WorldObjectOwners;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.World;

    public interface IProtoObjectLandClaim : IProtoObjectWithOwnersList
    {
        TimeSpan DestructionTimeout { get; }

        ushort LandClaimSize { get; }

        byte SafeItemsSlotsCount { get; }

        void ClientUpgrade(IStaticWorldObject worldObjectLandClaim, IProtoObjectLandClaim upgradeStructure);

        ObjectLandClaimCanUpgradeCheckResult SharedCanUpgrade(
            IStaticWorldObject worldObjectLandClaim,
            IProtoObjectLandClaim protoUpgradedLandClaim,
            ICharacter character,
            out IConstructionUpgradeEntryReadOnly upgradeEntry,
            bool writeErrors = true);
    }
}