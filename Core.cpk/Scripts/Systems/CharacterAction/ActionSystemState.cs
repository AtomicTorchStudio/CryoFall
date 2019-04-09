namespace AtomicTorch.CBND.CoreMod.Systems
{
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Scripting;

    public abstract class ActionSystemState<TSystem, TActionRequest, TActionState, TPublicActionState>
        : BaseActionState<TPublicActionState>
        where TSystem : ProtoActionSystem<TSystem, TActionRequest, TActionState, TPublicActionState>, new()
        where TActionRequest : IActionRequest
        where TActionState : ActionSystemState<TSystem, TActionRequest, TActionState, TPublicActionState>
        where TPublicActionState : BasePublicActionState, new()
    {
        private double timeRemainsSeconds;

        protected ActionSystemState(
            ICharacter character,
            IWorldObject targetWorldObject,
            double durationSeconds)
            : base(character)
        {
            if (Api.IsClient)
            {
                // Add ping to all client action durations.
                // Otherwise the client will not see immediately the result of the action
                // - the client will receive it only after RTT (ping) time.
                // TODO: currently it's possible to cancel action earlier and start a new one, but completed action result will come from server - which looks like a bug to player
                durationSeconds += Api.Client.CurrentGame.PingGameSeconds;
            }

            this.DurationSeconds = durationSeconds;
            this.TargetWorldObject = targetWorldObject;
            this.timeRemainsSeconds = durationSeconds;
        }

        public double DurationSeconds { get; }

        public TActionRequest Request { get; internal set; }

        public sealed override IWorldObject TargetWorldObject { get; }

        public double TimeRemainsSeconds => this.timeRemainsSeconds;

        private static TSystem System { get; } = Api.GetProtoEntity<TSystem>();

        public override void SharedUpdate(double deltaTime)
        {
            this.timeRemainsSeconds -= deltaTime;
            if (this.timeRemainsSeconds > 0)
            {
                var progress = (this.DurationSeconds - this.timeRemainsSeconds) / this.DurationSeconds;
                this.ProgressPercents = 100 * progress;
                return;
            }

            // completed!
            this.SetCompleted(isCancelled: false);
            this.ProgressPercents = 100;
            System.SharedOnActionCompleted((TActionState)this);
        }

        protected override void AbortAction()
        {
            System.SharedAbortAction((TActionState)this);
        }
    }
}