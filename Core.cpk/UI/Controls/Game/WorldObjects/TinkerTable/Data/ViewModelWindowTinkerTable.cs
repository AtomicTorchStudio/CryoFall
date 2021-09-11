namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.WorldObjects.TinkerTable.Data
{
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Misc;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.CBND.GameApi.Data.World;

    public class ViewModelWindowTinkerTable : BaseViewModel
    {
        public ViewModelWindowTinkerTable(
            IStaticWorldObject tinkerTableObject,
            ObjectTinkerTable.PrivateState privateState)
        {
            this.ViewModelWindowTinkerTableTabRepair
                = new ViewModelWindowTinkerTableTabRepair(tinkerTableObject, privateState);

            this.ViewModelWindowTinkerTableTabDisassembly
                = new ViewModelWindowTinkerTableTabDisassembly(tinkerTableObject, privateState);
        }

        public ViewModelWindowTinkerTableTabDisassembly ViewModelWindowTinkerTableTabDisassembly { get; }

        public ViewModelWindowTinkerTableTabRepair ViewModelWindowTinkerTableTabRepair { get; }
    }
}