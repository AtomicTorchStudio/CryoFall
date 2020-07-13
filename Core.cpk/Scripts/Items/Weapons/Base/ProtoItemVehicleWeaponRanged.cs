namespace AtomicTorch.CBND.CoreMod.Items.Weapons
{
    using System.Collections.Generic;
    using System.Windows;
    using AtomicTorch.CBND.CoreMod.UI;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.Items.Controls.Tooltips;
    using AtomicTorch.CBND.CoreMod.Vehicles;
    using AtomicTorch.CBND.GameApi.Data.Items;
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