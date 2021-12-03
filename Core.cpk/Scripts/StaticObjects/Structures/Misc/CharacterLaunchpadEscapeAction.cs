namespace AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Misc
{
    using AtomicTorch.CBND.CoreMod.Characters.Player;
    using AtomicTorch.CBND.CoreMod.Systems;
    using AtomicTorch.CBND.CoreMod.Systems.CharacterDespawnSystem;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Scripting;

    public class CharacterLaunchpadEscapeAction : BaseActionState<CharacterLaunchpadEscapeAction.PublicState>
    {
        private double timeRemains;

        public CharacterLaunchpadEscapeAction(ICharacter character, double duration)
            : base(character)
        {
            this.timeRemains = duration;
        }

        public override bool IsBlockingActions => true;

        public override bool IsBlockingMovement => true;

        public override bool IsDisplayingProgress => false;

        public override IWorldObject TargetWorldObject => null;

        public override void SharedUpdate(double deltaTime)
        {
            this.timeRemains -= deltaTime;
            if (this.timeRemains > 0)
            {
                return;
            }

            PlayerCharacter.GetPrivateState(this.Character).SetCurrentActionState(null);

            if (Api.IsServer)
            {
                CharacterDespawnSystem.DespawnCharacter(this.Character);
            }
        }

        protected override void AbortAction()
        {
        }

        protected override void SharedOnStart()
        {
            // recreate physics (as despawned character doesn't have any physics)
            this.Character.ProtoCharacter.SharedCreatePhysics(this.Character);
        }

        public class PublicState : BasePublicActionState
        {
            protected override void ClientOnCompleted()
            {
                // restore physics
                this.Character.ProtoCharacter.SharedCreatePhysics(this.Character);
            }

            protected override void ClientOnStart()
            {
                // recreate physics (as despawned character doesn't have any physics)
                this.Character.ProtoCharacter.SharedCreatePhysics(this.Character);
            }
        }
    }
}