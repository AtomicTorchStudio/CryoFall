namespace AtomicTorch.CBND.CoreMod.SoundPresets
{
    using AtomicTorch.CBND.GameApi;

    [RemoteEnum]
    public enum CharacterSound
    {
        LoopIdle,

        LoopWalk,

        LoopRun,

        DamageTaken,

        /// <summary>
        /// Please note that this sound should be present only in CharacterPreset.
        /// It cannot be overridden by equipment.
        /// </summary>
        Death,

        Aggression,

        Flee,

        Surprise
    }
}