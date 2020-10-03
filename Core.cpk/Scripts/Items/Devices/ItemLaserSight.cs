namespace AtomicTorch.CBND.CoreMod.Items.Devices
{
    using System.Collections.Generic;
    using AtomicTorch.CBND.CoreMod.Characters;
    using AtomicTorch.CBND.CoreMod.Characters.Player;
    using AtomicTorch.CBND.CoreMod.ClientComponents.ItemLaserSight;
    using AtomicTorch.CBND.CoreMod.Items.Equipment;
    using AtomicTorch.CBND.CoreMod.Items.Weapons;
    using AtomicTorch.CBND.CoreMod.Systems.Weapons;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.Items;
    using AtomicTorch.CBND.GameApi.Data.State;
    using AtomicTorch.CBND.GameApi.Scripting.ClientComponents;
    using AtomicTorch.CBND.GameApi.ServicesClient.Components;

    public class ItemLaserSight
        : ProtoItemEquipmentDevice
            <ItemWithDurabilityPrivateState,
                ItemLaserSight.PublicState,
                EmptyClientState>
    {
        public override string Description =>
            "Small laser target designator used to make aiming at long range easier.";

        public override uint DurabilityMax => 100;

        public override string Name => "Laser sight";

        public override bool OnlySingleDeviceOfThisProtoAppliesEffect => true;

        public override double ServerUpdateIntervalSeconds => 0.5;

        public static double SharedGetCurrentRangeMax(
            ICharacterPublicState characterPublicState)
        {
            if (!(characterPublicState.SelectedItemWeaponProto is IProtoItemWeaponRanged protoWeaponRanged))
            {
                return 0;
            }

            var damageDescription = WeaponSystem.GetCurrentDamageDescription(characterPublicState.SelectedItem,
                                                                             protoWeaponRanged,
                                                                             out _);

            return damageDescription is null
                       ? 0
                       : damageDescription.RangeMax * protoWeaponRanged.RangeMultiplier;
        }

        public override void ClientSetupSkeleton(
            IItem item,
            ICharacter character,
            IComponentSkeleton skeletonRenderer,
            List<IClientComponent> skeletonComponents,
            bool isPreview)
        {
            var componentSight = ComponentCharacterLaserSight.TryCreateComponent(character, GetPublicState(item));
            if (componentSight is not null)
            {
                skeletonComponents.Add(componentSight);
            }

            base.ClientSetupSkeleton(item, character, skeletonRenderer, skeletonComponents, isPreview);
        }

        protected override void ServerUpdate(ServerUpdateData data)
        {
            var item = data.GameObject;
            var publicState = data.PublicState;

            var itemOwnerCharacter = item.Container?.OwnerAsCharacter;
            if (itemOwnerCharacter is null
                || !itemOwnerCharacter.ServerIsOnline
                || item.Container != itemOwnerCharacter.SharedGetPlayerContainerEquipment())
            {
                publicState.MaxRange = 0;
                return;
            }

            var characterPublicState = itemOwnerCharacter.GetPublicState<ICharacterPublicState>();
            publicState.MaxRange = (float)SharedGetCurrentRangeMax(characterPublicState);
        }

        public class PublicState : BasePublicState
        {
            [SyncToClient]
            public float MaxRange { get; set; }
        }
    }
}