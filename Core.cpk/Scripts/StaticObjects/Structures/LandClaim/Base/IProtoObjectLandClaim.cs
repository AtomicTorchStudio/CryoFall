namespace AtomicTorch.CBND.CoreMod.StaticObjects.Structures.LandClaim
{
    using System;
    using AtomicTorch.CBND.CoreMod.Systems.Construction;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.World;

    public interface IProtoObjectLandClaim : IProtoObjectStructure
    {
        TimeSpan DecayDelayDuration { get; }

        TimeSpan DestructionTimeout { get; }

        ushort LandClaimGraceAreaPaddingSizeOneDirection { get; }

        ushort LandClaimSize { get; }

        byte LandClaimTier { get; }

        ushort LandClaimWithGraceAreaSize { get; }

        double ShieldProtectionDuration { get; }

        double ShieldProtectionTotalElectricityCost { get; }

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