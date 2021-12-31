namespace AtomicTorch.CBND.CoreMod.Systems.ItemExplosive
{
    using AtomicTorch.CBND.CoreMod.Items.Explosives;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.Items;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.GameEngine.Common.Primitives;

    public class ItemExplosiveActionState
        : BaseSystemActionState<
            ItemExplosiveSystem,
            ItemExplosiveRequest,
            ItemExplosiveActionState,
            ItemExplosiveActionPublicState>
    {
        public readonly IItem ItemExplosive;

        public readonly Vector2Ushort TargetPosition;

        public ItemExplosiveActionState(
            ICharacter character,
            Vector2Ushort targetPosition,
            double durationSeconds,
            IItem itemExplosive)
            : base(character, null, durationSeconds)
        {
            this.TargetPosition = targetPosition;
            this.ItemExplosive = itemExplosive;
        }

        public override bool IsBlockingMovement => true;

        public override bool IsDisplayingProgress => true;

        public override void SharedUpdate(double deltaTime)
        {
            base.SharedUpdate(deltaTime);

            ((IProtoItemExplosive)this.ItemExplosive.ProtoItem)
                .SharedValidatePlacement(this.Character,
                                         this.TargetPosition,
                                         logErrors: true,
                                         canPlace: out var canPlace,
                                         isTooFar: out var isTooFar,
                                         errorMessage: out _);
            if (!canPlace || isTooFar)
            {
                this.AbortAction();
            }
        }

        protected override void OnCompletedOrCancelled()
        {
            base.OnCompletedOrCancelled();

            if (Api.IsServer)
            {
                return;
            }

            var protoItemExplosive = ((IProtoItemExplosive)this.ItemExplosive.ProtoItem);
            protoItemExplosive.ClientOnActionCompleted(
                this.ItemExplosive,
                this.IsCancelled);
        }

        protected override void SetupPublicState(ItemExplosiveActionPublicState state)
        {
            base.SetupPublicState(state);
            state.ProtoItemExplosive = (IProtoItemExplosive)this.ItemExplosive.ProtoItem;
            state.TargetPosition = this.TargetPosition;
        }
    }
}