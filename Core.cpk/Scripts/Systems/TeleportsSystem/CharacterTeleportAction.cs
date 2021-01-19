namespace AtomicTorch.CBND.CoreMod.Systems.TeleportsSystem
{
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.World;

    public class CharacterTeleportAction : BaseActionState<CharacterTeleportAction.PublicState>
    {
        public CharacterTeleportAction(ICharacter character)
            : base(character)
        {
        }

        public override bool IsBlocksMovement => true;

        public override IWorldObject TargetWorldObject => null;

        public override void SharedUpdate(double deltaTime)
        {
        }

        protected override void AbortAction()
        {
        }

        public class PublicState : BasePublicActionState
        {
            protected override void ClientOnCompleted()
            {
            }

            protected override void ClientOnStart()
            {
            }
        }
    }
}