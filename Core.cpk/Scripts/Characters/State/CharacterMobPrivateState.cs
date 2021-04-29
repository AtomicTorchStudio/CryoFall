namespace AtomicTorch.CBND.CoreMod.Characters
{
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.State;
    using AtomicTorch.GameEngine.Common.Primitives;

    public class CharacterMobPrivateState : BaseCharacterPrivateState
    {
        [TempOnly]
        public double AttackRange { get; set; }

        [TempOnly]
        public ICharacter CurrentAggroCharacter { get; set; }

        [TempOnly]
        public double CurrentAggroTimeRemains { get; set; }

        [TempOnly]
        public ICharacter CurrentTargetCharacter { get; set; }

        [TempOnly]
        public bool IsRetreating { get; set; }

        [TempOnly]
        public double LastFleeSoundTime { get; set; }

        [TempOnly]
        public Vector2Ushort SpawnPosition { get; set; }

        /// <summary>
        /// This timer is incremented when the mob is too far from the spawn location.
        /// As it reaches a certain high number the mob should be despawned.
        /// </summary>
        [TempOnly]
        public double TimerDespawn { get; set; }
    }
}