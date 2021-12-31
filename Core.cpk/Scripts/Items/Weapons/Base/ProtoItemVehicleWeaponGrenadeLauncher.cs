namespace AtomicTorch.CBND.CoreMod.Items.Weapons
{
    using System.Collections.Generic;
    using AtomicTorch.CBND.CoreMod.Systems.Notifications;
    using AtomicTorch.CBND.CoreMod.UI;
    using AtomicTorch.CBND.CoreMod.Vehicles;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.Items;
    using AtomicTorch.GameEngine.Common.Extensions;

    public abstract class ProtoItemVehicleWeaponGrenadeLauncher
        : ProtoItemWeaponGrenadeLauncher,
          IProtoItemVehicleWeapon
    {
        public override bool IsSkinnable => false;

        public abstract VehicleWeaponHardpoint WeaponHardpoint { get; }

        public override bool SharedCanSelect(IItem item, ICharacter character, bool isAlreadySelected, bool isByPlayer)
        {
            if (IsClient
                && !isAlreadySelected
                && isByPlayer)
            {
                NotificationSystem.ClientShowNotification(
                    this.Name,
                    CoreStrings.Vehicle_Mech_NotificationWeaponNeedsInstallationOnMech,
                    NotificationColor.Bad,
                    item.ProtoItem.Icon);
            }

            return false;
        }

        protected override void PrepareHints(List<string> hints)
        {
            base.PrepareHints(hints);

            hints.Add(string.Format(ItemHints.VehicleWeapon_Format,
                                    this.WeaponHardpoint.GetDescription()));
        }
    }
}