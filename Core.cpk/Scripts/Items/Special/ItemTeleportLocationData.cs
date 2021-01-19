namespace AtomicTorch.CBND.CoreMod.Items.Special
{
    using AtomicTorch.CBND.CoreMod.StaticObjects.Misc;

    public class ItemTeleportLocationData : ProtoItemTeleportUnlock<ObjectAlienTeleport>
    {
        public override string Description =>
            "Contains records of coordinates and navigation frequency for one of many teleport locations.";

        public override string Name => "Teleport location data";
    }
}