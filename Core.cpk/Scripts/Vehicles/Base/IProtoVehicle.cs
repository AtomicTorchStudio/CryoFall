namespace AtomicTorch.CBND.CoreMod.Vehicles
{
    using System.Collections.Generic;
    using AtomicTorch.CBND.CoreMod.Characters;
    using AtomicTorch.CBND.CoreMod.StaticObjects;
    using AtomicTorch.CBND.CoreMod.Systems;
    using AtomicTorch.CBND.CoreMod.Systems.Weapons;
    using AtomicTorch.CBND.CoreMod.Systems.WorldObjectOwners;
    using AtomicTorch.CBND.CoreMod.Technologies;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.Items;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Resources;
    using AtomicTorch.CBND.GameApi.Scripting.ClientComponents;
    using AtomicTorch.CBND.GameApi.ServicesClient.Components;

    public interface IProtoVehicle : IProtoDynamicWorldObject, IProtoObjectWithOwnersList, IInteractableProtoWorldObject
    {
        uint BuildRequiredElectricityAmount { get; }

        IReadOnlyList<ProtoItemWithCount> BuildRequiredItems { get; }

        byte CargoItemsSlotsCount { get; }

        string Description { get; }

        ushort EnergyUsePerSecondIdle { get; }

        ushort EnergyUsePerSecondMoving { get; }

        ITextureResource Icon { get; }

        bool IsAllowCreatureDamageWhenNoPilot { get; }

        bool IsHeavyVehicle { get; }

        bool IsPlayersHotbarAndEquipmentItemsAllowed { get; }

        IReadOnlyList<TechNode> ListedInTechNodes { get; }

        ITextureResource MapIcon { get; }

        uint RepairRequiredElectricityAmount { get; }

        IReadOnlyList<ProtoItemWithCount> RepairStageRequiredItems { get; }

        int RepairStagesCount { get; }

        SoundResource SoundResourceLightsToggle { get; }

        SoundResource SoundResourceVehicleDismount { get; }

        SoundResource SoundResourceVehicleMount { get; }

        double VehicleWorldHeight { get; }

        void ClientOnVehicleDismounted(IDynamicWorldObject vehicle);

        void ClientRequestBuild();

        void ClientRequestRepair();

        void ClientSetupSkeleton(
            IDynamicWorldObject vehicle,
            IProtoCharacterSkeleton protoSkeleton,
            IComponentSkeleton skeletonRenderer,
            List<IClientComponent> skeletonComponents);

        void ClientToggleLight();

        void ClientTrySetLightsActiveState(IDynamicWorldObject vehicle, bool setIsActive);

        void PrepareProtoSetLinkWithTechNode(TechNode techNode);

        void ServerOnBuilt(IDynamicWorldObject worldObject, ICharacter byCharacter);

        void ServerOnCharacterEnterVehicle(IDynamicWorldObject vehicle, ICharacter character);

        void ServerOnCharacterExitVehicle(IDynamicWorldObject vehicle, ICharacter character);

        void ServerOnPilotDamage(
            WeaponFinalCache weaponCache,
            IDynamicWorldObject vehicle,
            ICharacter pilotCharacter,
            double damageApplied);

        void ServerOnRepair(IDynamicWorldObject worldObject, ICharacter byCharacter);

        void ServerRefreshEnergyMax(IDynamicWorldObject vehicle);

        void SharedApplyInput(
            IDynamicWorldObject vehicle,
            ICharacter character,
            PlayerCharacterPrivateState characterPrivateState,
            PlayerCharacterPublicState characterPublicState);

        IItemsContainer SharedGetHotbarItemsContainer(IDynamicWorldObject vehicle);

        double SharedGetMoveSpeedMultiplier(IDynamicWorldObject vehicle, ICharacter characterPilot);

        void SharedGetSkeletonProto(
            IDynamicWorldObject gameObject,
            out IProtoCharacterSkeleton protoSkeleton,
            out double scale);

        bool SharedIsTechUnlocked(ICharacter character, bool allowIfAdmin = true);

        VehicleCanBuildCheckResult SharedPlayerCanBuild(ICharacter character);

        VehicleCanRepairCheckResult SharedPlayerCanRepairInVehicleAssemblyBay(ICharacter character);

        bool SharedPlayerHasRequiredItemsToBuild(ICharacter character, bool allowIfAdmin = true);

        bool SharedPlayerHasRequiredItemsToRepair(ICharacter character, bool allowIfAdmin = true);
    }
}