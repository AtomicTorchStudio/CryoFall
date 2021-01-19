namespace AtomicTorch.CBND.CoreMod.Characters.Turrets
{
    using AtomicTorch.CBND.GameApi.Data.Items;
    using AtomicTorch.CBND.GameApi.Data.State;
    using AtomicTorch.CBND.GameApi.Data.World;

    // All properties of this class are provided by the turret object during its initialization.
    public class CharacterTurretPrivateState : CharacterMobPrivateState
    {
        [TempOnly]
        public IItem CurrentAmmoItem { get; set; }

        [TempOnly]
        public IStaticWorldObject ObjectTurret { get; set; }
    }
}