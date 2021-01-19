namespace AtomicTorch.CBND.CoreMod
{
    /// <summary>
    /// There are some special constants that are used to fine-tune the gameplay.
    /// Please do not modify in a server-only or client-only mod otherwise it will lead to
    /// discrepancies in simulation. Could be modified only in a client+server mod.
    /// </summary>
    public static class GameplayConstants
    {
        public const double CharacterMoveSpeedMultiplier = 1.1;

        public const double VehicleMoveSpeedMultiplier = 1.1;
    }
}