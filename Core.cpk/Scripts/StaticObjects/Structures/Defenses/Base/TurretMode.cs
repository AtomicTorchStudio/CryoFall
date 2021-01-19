namespace AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Defenses
{
    using System.ComponentModel;
    using AtomicTorch.CBND.GameApi;

    [RemoteEnum]
    public enum TurretMode : byte
    {
        [Description("Disabled")]
        Disabled = 0,

        [Description("Attack enemies, return fire to neutrals")]
        AttackHostile = 10,

        [Description("Attack enemies and all trespassers")]
        AttackEnemiesAndTrespassers = 20
    }
}