namespace AtomicTorch.CBND.CoreMod.SoundPresets
{
    using AtomicTorch.CBND.GameApi;

    [RemoteEnum]
    public enum WeaponSound
    {
        /// <summary>
        /// Sound played when the weapon is fired (for melee weapon - when the attack animation is started).
        /// </summary>
        Shot,

        /// <summary>
        /// Reload sound like replacing a magazine.
        /// </summary>
        Reload,

        /// <summary>
        /// Reload finish sound like cocking a gun.
        /// </summary>
        ReloadFinished,

        /// <summary>
        /// Could be played for certain weapons that have "prepare" cycle, such as gatling gun spinning up or laser heating up
        /// before shooting.
        /// </summary>
        Start,

        /// <summary>
        /// Could be played for certain weapons that have "cooldown" cycle that happens after shooting.
        /// </summary>
        Stop,

        /// <summary>
        /// Sound is played when there is no ammo left in the weapon (reloading is needed) or when the player is completely out of
        /// ammo.
        /// </summary>
        Empty
    }
}