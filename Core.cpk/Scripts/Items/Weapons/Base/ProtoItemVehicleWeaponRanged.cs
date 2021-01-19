namespace AtomicTorch.CBND.CoreMod.Items.Weapons
{
    using System.Collections.Generic;
    using AtomicTorch.CBND.CoreMod.Vehicles;
    using AtomicTorch.GameEngine.Common.Extensions;

    public abstract class ProtoItemVehicleWeaponRanged
        : ProtoItemWeaponRanged,
          IProtoItemVehicleWeapon
    {
        public abstract VehicleWeaponHardpoint WeaponHardpoint { get; }

        protected override void PrepareHints(List<string> hints)
        {
            base.PrepareHints(hints);

            hints.Add(string.Format(ItemHints.VehicleWeapon_Format,
                                    this.WeaponHardpoint.GetDescription()));
        }
    }
}